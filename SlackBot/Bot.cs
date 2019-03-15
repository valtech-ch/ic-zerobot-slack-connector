using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SlackBot.EventHandlers;
using SlackBot.Models;
using SlackBot.Responders;
using SlackBot.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using SlackBot.BotHelpers;
using System.Threading;

namespace SlackBot
{
    /// <summary>
    /// Controlls the Bot actions as RTM connection to the Slack Interface
    /// </summary>
    public class Bot
    {
        #region Private properties
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string _BotNameRegex;
        private string BotNameRegex
        {
            get 
            {
                // only build the regex if we're connected - if we're not connected we won't know our bot's name or user ID
                if (_BotNameRegex == string.Empty && IsConnected) {
                    _BotNameRegex = new BotNameRegexComposer().ComposeFor(UserName, UserID, Aliases);
                }

                return _BotNameRegex;
            }
            set { _BotNameRegex = value; }
        }

        private Dictionary<string, string> UserNameCache = null;
        private WebSocket WebSocket = null;
        private Timer pinger = null;
        #endregion

        #region Public properties
        private IReadOnlyList<string> _Aliases;
        public IReadOnlyList<string> Aliases
        {
            get { return _Aliases; }
            set
            {
                _Aliases = value;
                BotNameRegex = string.Empty;
            }
        }
        public List<IResponder> Responders { get; private set; }
        
        public IReadOnlyList<SlackChatHub> ConnectedChannels
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.Channel).ToList(); }
        }

        public IReadOnlyList<SlackChatHub> ConnectedDMs
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.DM).ToList(); }
        }

        public IReadOnlyList<SlackChatHub> ConnectedGroups
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.Group).ToList(); }
        }

        public IReadOnlyList<SlackChatHub> ConnectedBots
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.Bot).ToList(); }
        }

        public IReadOnlyDictionary<string, SlackChatHub> ConnectedHubs { get; private set; }

        public bool IsConnected 
        {
            get { return ConnectedSince != null; }
        }

        private DateTime? _ConnectedSince = null;
        public DateTime? ConnectedSince
        {
            get { return _ConnectedSince; }
            set
            {
                _ConnectedSince = value;
                
            }
        }

        public Dictionary<string, object> ResponseContext { get; private set; }
        public string SlackKey { get; private set; }
        public string ErrorMessageChannel { get; private set; }
        public string TeamID { get; private set; }
        public string TeamName { get; private set; }
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        private SlackChatHub errorChannel { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        /// <param name="slackKey">The slack key.</param>
        /// <param name="errorMessageChannel">The errorMessageChannel.</param>
        public Bot(string slackKey, string errorMessageChannel)
        {
            // get the books ready
            Aliases = new List<string>();
            ResponseContext = new Dictionary<string, object>();
            Responders = new List<IResponder>();
            UserNameCache = new Dictionary<string, string>();
            this.SlackKey = slackKey;
            this.ErrorMessageChannel = errorMessageChannel;
        }

        /// <summary>
        /// Connects the bot with the specified slack key.
        /// </summary>
        /// <returns></returns>
        public async Task Connect()
        {
            try
            {
                if (WebSocket != null && WebSocket.IsAlive)
                {
                    WebSocket.OnOpen -= WebSocket_OnOpen;
                    WebSocket.OnMessage -= WebSocket_OnMessage;
                    WebSocket.OnError -= WebSocket_OnError;
                    WebSocket.OnClose -= WebSocket_OnClose;
                    WebSocket.Close();
                }

                // kill the regex for our bot's name - we'll rebuild it upon request with some of the info we get here
                BotNameRegex = string.Empty;

                WebClient client = new WebClient();

                string json = await client.GetResponse("https://slack.com/api/rtm.start", WebClient.RequestMethod.Post, "token", this.SlackKey);
                JObject jData = JObject.Parse(json);

                TeamID = jData["team"]["id"].Value<string>();
                TeamName = jData["team"]["name"].Value<string>();
                UserID = jData["self"]["id"].Value<string>();
                UserName = jData["self"]["name"].Value<string>();
                string webSocketUrl = jData["url"].Value<string>();

                UserNameCache.Clear();
                foreach (JObject userObject in jData["users"])
                {
                    UserNameCache.Add(userObject["id"].Value<string>(), userObject["name"].Value<string>());
                }

                // load the channels, groups, and DMs that margie's in
                Dictionary<string, SlackChatHub> hubs = new Dictionary<string, SlackChatHub>();
                ConnectedHubs = hubs;

                // channelz
                if (jData["channels"] != null)
                {
                    foreach (JObject channelData in jData["channels"])
                    {
                        if (!channelData["is_archived"].Value<bool>() && channelData["is_member"].Value<bool>())
                        {
                            SlackChatHub channel = new SlackChatHub()
                            {
                                ID = channelData["id"].Value<string>(),
                                Name = "#" + channelData["name"].Value<string>(),
                                Type = SlackChatHubType.Channel
                            };
                            hubs.Add(channel.ID, channel);
                        }
                    }
                }

                // groupz
                if (jData["groups"] != null)
                {
                    foreach (JObject groupData in jData["groups"])
                    {
                        if (!groupData["is_archived"].Value<bool>() && groupData["members"].Values<string>().Contains(UserID))
                        {
                            SlackChatHub group = new SlackChatHub()
                            {
                                ID = groupData["id"].Value<string>(),
                                Name = groupData["name"].Value<string>(),
                                Type = SlackChatHubType.Group
                            };
                            hubs.Add(group.ID, group);
                        }
                    }
                }

                // dmz
                if (jData["ims"] != null)
                {
                    foreach (JObject dmData in jData["ims"])
                    {
                        string userID = dmData["user"].Value<string>();
                        SlackChatHub dm = new SlackChatHub()
                        {
                            ID = dmData["id"].Value<string>(),
                            Name = "@" + (UserNameCache.ContainsKey(userID) ? UserNameCache[userID] : userID),
                            Type = SlackChatHubType.DM
                        };
                        hubs.Add(dm.ID, dm);
                    }
                }

                // dmz
                if (jData["bots"] != null)
                {
                    foreach (JObject dmData in jData["bots"])
                    {
                        if (!dmData["deleted"].Value<bool>())
                        {
                            string userID = dmData["id"].Value<string>();
                            SlackChatHub cbot = new SlackChatHub()
                            {
                                ID = dmData["id"].Value<string>(),
                                Name = "@" + (UserNameCache.ContainsKey(userID) ? UserNameCache[userID] : userID),
                                Type = SlackChatHubType.Bot
                            };
                            hubs.Add(cbot.ID, cbot);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(this.ErrorMessageChannel) && ConnectedHubs.ContainsKey(this.ErrorMessageChannel))
                {
                    errorChannel = ConnectedHubs[this.ErrorMessageChannel];
                }

                WebSocket = new WebSocket(webSocketUrl);
                WebSocket.OnOpen += WebSocket_OnConnect;
                WebSocket.OnMessage += WebSocket_OnMessage;
                //WebSocket.OnError += WebSocket_OnError;
                //WebSocket.OnClose += WebSocket_OnClose;

                WebSocket.Connect();

                pinger = new Timer(TimerCallback, null, 0, 15000);
            }
            catch (Exception ee)
            {
                Disconnect();
            }            
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        private void TimerCallback(Object o)
        {
            if (WebSocket.IsAlive)
            {
                if (!WebSocket.Ping())
                {
                    logger.Info("Bot:Ping went wrong. Bot Disconnecting.");
                    Disconnect();
                }
            }
            else
            {
                logger.Debug("Bot:Socket missing. Bot Disconnecting.");
                Disconnect();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebSocket_OnConnect(object sender, EventArgs e)
        {
            logger.Debug("Bot:WebSocket connected.");
            ConnectedSince = DateTime.Now;
            RaiseConnectionStatusChanged();
        }

        private void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            ListenTo(e.Data).GetAwaiter();
        }
        /// <summary>
        /// Handles the OnError event of the WebSocket control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ErrorEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            logger.Error("Bot:WebSocket Error. What now?");
            Disconnect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            logger.Info("Bot:WebSocket closed. Disconnecting");
            Disconnect();
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            TeamID = null;
            TeamName = null;
            UserID = null;
            UserName = null;

            if (WebSocket != null && WebSocket.IsAlive)
            {
                WebSocket.Close();
            }
            
            if (pinger != null)
            {
                pinger.Dispose();
            }

            logger.Info("Bot:Disconnected");
            ConnectedSince = null;
            RaiseConnectionStatusChanged();
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        public string GetUserName(string userID)
        {
            if (UserNameCache.ContainsKey(userID)) {
                return UserNameCache[userID];
            }

            return null;
        }

        /// <summary>
        /// Listens to events sent from the rtm Interface. Uses a simple web socket to get the requests
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        private async Task ListenTo(string json)
        {
            ResponseContext context = null;
            JObject jObject = JObject.Parse(json);

            if (jObject["type"] != null && jObject["type"].Value<string>() == "message")
            {
                string channelID = jObject["channel"].Value<string>();

                    SlackMessage message = new SlackMessage()
                    {
                        ChatHub = null,
                        RawData = json,
                        Attachments = new List<SlackAttachment>()
                    };

                    if (ConnectedHubs.ContainsKey(channelID))
                    {
                        message.ChatHub = ConnectedHubs[channelID];
                    }
                    else
                    {
                        message.ChatHub = SlackChatHub.FromID(channelID);
                        List<SlackChatHub> hubs = new List<SlackChatHub>();
                        hubs.AddRange(ConnectedHubs.Values);
                        hubs.Add(message.ChatHub);
                    }

                try
                {
                    message.Text = (jObject["text"] != null ? jObject["text"].Value<string>() : string.Empty);
                    
                    message.User = new SlackUser();
                    if (jObject["user"] != null)
                    {
                        WebClient client = new WebClient();
                        string responseJson = await client.GetResponse("https://slack.com/api/users.info?", WebClient.RequestMethod.Post, "token", this.SlackKey, "user", jObject["user"].Value<string>());
                        JObject jData = JObject.Parse(responseJson);

                        message.User.ID = jObject["user"].Value<string>();
                        message.User.Name = jData["user"]["real_name"].Value<string>();
                        message.User.ImageUrl = jData["user"]["profile"]["image_192"].Value<string>();
                    }
                    if (jObject["bot_id"] != null && jObject["username"] != null)
                    {
                        message.User.ID = jObject["bot_id"].Value<string>();
                        message.User.IsBot = true;
                        message.User.Name = jObject["username"].Value<string>();
                        if (!string.IsNullOrEmpty(message.User.ID) && !UserNameCache.ContainsKey(message.User.ID))
                        {
                            UserNameCache.Add(message.User.ID, message.User.Name);
                        }
                    }

                    if (jObject["attachments"] != null)
                    {
                        List<string> msg = new List<string>();

                        foreach (JObject attachment in jObject["attachments"])
                        {
                            SlackAttachment sla = new SlackAttachment()
                            {
                                Fallback = attachment["fallback"] != null ? attachment["fallback"].Value<string>() : string.Empty,
                                ColorHex = attachment["color"] != null ? attachment["color"].Value<string>(): string.Empty,
                                Text = attachment["text"] != null ? attachment["text"].Value<string>() : string.Empty,
                                Title = attachment["title"] != null? attachment["title"].Value<string>(): string.Empty,
                            };
                            message.Attachments.Add(sla);
                            msg.Add(sla.Title);
                            msg.Add(sla.Text);
                        }

                        if (string.IsNullOrEmpty(message.Text))
                        {
                            message.Text = string.Empty;
                        }

                        message.Text += string.Join(Environment.NewLine, msg);
                    }

                    // check to see if bot has been mentioned
                    if (message.Text != null && 
                        Regex.IsMatch(message.Text, BotNameRegex, RegexOptions.IgnoreCase))
                    {
                        message.MentionsBot = true;
                    }

                    context = new ResponseContext()
                    {
                        BotHasResponded = false,
                        BotUserID = UserID,
                        BotUserName = UserName,
                        Message = message,
                        TeamID = this.TeamID,
                        UserNameCache = new ReadOnlyDictionary<string, string>(this.UserNameCache),
                    };

                    // if the end dev has added any static entries to the ResponseContext collection of Bot, add them to the context being passed to the responders.
                    if (ResponseContext != null)
                    {
                        foreach (string key in ResponseContext.Keys)
                        {
                            context.Set(key, ResponseContext[key]);
                        }
                    }
                }
                catch (Exception ee)
                {
                    BotMessage errorMessage = new BotMessage() { ChatHub = message.ChatHub, Text = "ERROR parsing message : " + ee.Message + " " + ee.StackTrace };
                    logger.Error(errorMessage.Text);
                    if (errorChannel != null)
                    {
                        //TODO
                        //await Say(errorMessage);
                    }
                }

                try
                {
                    // Avoid to answer to yourself
                    if (context != null && context.Message != null && context.Message.Text != null && 
                        !string.IsNullOrEmpty(context.Message.User.ID) && context.Message.User.ID != context.BotUserID)
                    {
                        foreach (IResponder responder in Responders)
                        {
                            if (responder.CanRespond(context))
                            {
                                await Say(responder.GetResponse(context), context);
                                context.BotHasResponded = true;
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    BotMessage errorMessage = new BotMessage() { ChatHub = message.ChatHub, Text = "ERROR in responder: " + ee.Message + " " + ee.StackTrace };
                    logger.Error(errorMessage.Text);
                    if (errorChannel != null)
                    {
                        //TODO
                        //await Say(errorMessage);
                    }
                }

                try
                {
                    RaiseMessageReceived(context);
                }
                catch (Exception ee)
                {
                    BotMessage errorMessage = new BotMessage() { ChatHub = message.ChatHub, Text = "ERROR with Message received event: " + ee.Message + " " + ee.StackTrace };
                    logger.Error(errorMessage.Text);
                    if (errorChannel != null)
                    {
                        //TODO
                        //await Say(errorMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Says the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async Task Say(BotMessage message)
        {
            await Say(message, null);
        }

        /// <summary>
        /// Says the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">When calling the Say() method, the message parameter must have its ChatHub property set.</exception>
        private async Task Say(BotMessage message, ResponseContext context)
        {
            string chatHubID = null;

            if(message.ChatHub != null) {
                chatHubID = message.ChatHub.ID;
            }
            else if(context != null && context.Message.ChatHub != null) {
                chatHubID = context.Message.ChatHub.ID;
            }

            if(chatHubID != null) {
                WebClient client = new WebClient();

                List<string> values = new List<string>() {
                    "token", this.SlackKey,
                    "channel", chatHubID,
                    "text", message.Text,
                    "as_user", "true"
                };

                if (message.Attachments.Count > 0) {
                    values.Add("attachments");
                    values.Add(JsonConvert.SerializeObject(message.Attachments));
                }

                await client.GetResponse(
                    "https://slack.com/api/chat.postMessage",
                    WebClient.RequestMethod.Post,
                    values.ToArray()
                );
            }
            else {
                string errorMsg = "When calling the Say() method, the message parameter must have its ChatHub property set.";
                logger.Error(errorMsg);
                throw new ArgumentException(errorMsg);
            }
        }

        #region Events
        /// <summary>
        /// Occurs when [connection status changed].
        /// </summary>
        public event MargieConnectionStatusChangedEventHandler ConnectionStatusChanged;
        private void RaiseConnectionStatusChanged()
        {
            if (ConnectionStatusChanged != null) {
                ConnectionStatusChanged(IsConnected);
            }
        }

        /// <summary>
        /// Occurs when [message received].
        /// </summary>
        public event MargieMessageReceivedEventHandler MessageReceived;
        private void RaiseMessageReceived(ResponseContext context)
        {
            if (MessageReceived != null)
            {
                MessageReceived(context);
            }
        }

        #endregion
    }


}