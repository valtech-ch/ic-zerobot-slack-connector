using SlackBot;
using SlackBot.Models;
using SlackBot.Responders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace ICSlackBot.Logic.Controllers
{
    /// <summary>
    /// Controller class for the Slack bot.
    /// </summary>
    public class SlackBotController
    {
        #region vars
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Bot bot = null;		
        private readonly List<DateTime> connectionTimes = new List<DateTime>();
        public ConfigController cfg;

        public bool isBotConnected = false;
        private Timer reconnectTimer = null;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackBotController" /> class and loads the config parameters
        /// </summary>
        /// <param name="cfg">The CFG.</param>
        public SlackBotController(ConfigController cfg)
        {
            this.cfg = cfg;                                
        }

        /// <summary>
        /// Initializes this instance and starts the Bot
        /// </summary>
        /// <returns></returns>
        public void InitBot()
        {
            logger.Info("Starting bot");
            bot = new Bot(cfg.Get("Slack", "ApiToken", "xxx"), cfg.Get("Slack", "InfoChannel", "@dany"));

            logger.Info("Set aliasses");
            bot.Aliases = cfg.Get("Slack", "Aliasses", "Jarvis,Scotty,James,Amy,Ava,Eve,Vici,Viki").Split(new char[] { ',', ';' }).ToList();

            logger.Info("Add Events");
            try
            {
                logger.Info("Add Responders");
                LoadResponders();
                AddServiceResponces();
            }
            catch (Exception ee)
            {
                logger.Error("Error loading responders: " + ee.Message + Environment.NewLine + ee.StackTrace);
            }

            bot.ConnectionStatusChanged += bot_ConnectionStatusChanged;

            Connect();

            bot.MessageReceived += Bot_MessageReceived;
            logger.Info("Bot ready.");
        }

        private void Connect() { 
            try
            {
                logger.Info("Connecting bot");
                bot.Connect();
            }
            catch (Exception ee)
            {
                logger.Error("Error connection bot: " + ee.Message);
            }
        }

        /// <summary>
        /// Shutdowns this instance and send a message to inform the channel
        /// </summary>
        /// <returns></returns>
        public async Task Shutdown()
        {
            if (null != bot)
            {
                logger.Info("Shutting down");
                await SendMessage(cfg.Get("Slack", "InfoChannel", "@dany"), "Shutting down, bye bye");
                bot.Disconnect();
                reconnectTimer.Dispose();
            }
        }

        #region Events
        /// <summary>
        /// Executed when a message is received. Used to controll the Bot.
        /// </summary>
        public event EventHandlers.BotEvents.SlackControllerMessageReceivedEventHandler onMessageReceived;
        private void Bot_MessageReceived(ResponseContext context)
        {
            HandleServerActions(context).Wait();

            if (context.Message.User != null && !string.IsNullOrEmpty(context.Message.User.ID))
            {
                string userName = context.Message.User.ID;
                if (context.UserNameCache.ContainsKey(context.Message.User.ID))
                {
                    userName = context.UserNameCache[context.Message.User.ID];
                }

                if (context != null && context.Message != null && context.Message.ChatHub != null)
                {
                    onMessageReceived(userName, context.Message.Text, context.Message.ChatHub.Name);
                }
            }
            else
            {
                logger.WarnFormat("Username is empty and on message received could not be processed for message {0}", context.Message.Text);
            }
        }

        /// <summary>
        /// Bot_s the connection status changed. start a timer to test connection and reconnects as soon as possible.
        /// </summary>
        /// <param name="isConnected">if set to <c>true</c> [is connected].</param>
        private void bot_ConnectionStatusChanged(bool isConnected)
        {
            isBotConnected = isConnected;

            if (isConnected)
            {
                logger.Info(string.Format("Bot connected as {0} with id {1} on team {2}", bot.UserName, bot.UserID, bot.TeamName));
                connectionTimes.Add(DateTime.Now);

                if (connectionTimes.Count > 20)
                {
                    connectionTimes.RemoveAt(0);
                }
                if (reconnectTimer != null)
                {
                    reconnectTimer.Dispose();
                }
                RaiseSlackBotConnected();
            }
            else
            {
                RaiseSlackBotDisconnected();
                Thread.Sleep(10000);
                Connect();
            }
        }
        #endregion

        #region Bot config
        /// <summary>
        /// Loads all responders
        /// </summary>
        private void LoadResponders()
        {
            bot.Responders.Clear();

            foreach (string cls in cfg.Get("Common", "Responder", string.Empty).Split(new char[] { ';', ',' }))
            {
                try
                {
                    bot.Responders.Add((IResponder)Activator.CreateInstance(Type.GetType(cls), this));
                    logger.InfoFormat("Responder {0} loaded", cls);
                }
                catch (Exception eee)
                {
                    logger.Error("Error loading responder with name : " + cls + Environment.NewLine + eee.Message + Environment.NewLine + eee.StackTrace);
                }
            }
        }
        /// <summary>
        /// Load Simple responders
        /// </summary>
        private void AddServiceResponces()
        {
            bot
                .RespondsTo("(there|alive|ready|status)", true)
                .With("Sure, dude!")
                .With("I'll see what I can do, tough one.")
                .With("I'll try. No promises, though!")
                .Description("*status*: checks if the bot is still connected")
                .IfBotIsMentioned();
            
            List<string> commands = new List<string>();
            foreach (IResponder resp in bot.Responders)
            {
                commands.Add(resp.GetCommandDescription());
            }
            bot
                .RespondsTo("commands")
                .With("- " + string.Join(Environment.NewLine+"- ", commands))
                .Description("*commands*: Get the list of possibl commands")
                .IfBotIsMentioned();

            bot
                .RespondsTo("modules")
                .With(string.Join(Environment.NewLine, bot.Responders))
                .Description("*modules*: Get the list of active modules. Must be sent as direct message.")
                .IsDM();

            bot
                .RespondsTo("reconnects")
                .With(string.Join(Environment.NewLine, connectionTimes))
                .Description("*reconnects*: Get the list of reconnect times.")
                .IsDM();
        }

        /// <summary>
        /// Handles the actions sent directly to the bot a private message.
        /// Used to controll the bot.
        /// </summary>
        /// <param name="context">The context.</param>
        private async Task HandleServerActions(ResponseContext context)
        {
            if (context != null &&
                context.Message != null &&
                context.Message.ChatHub != null &&
                context.Message.ChatHub.Type == SlackChatHubType.DM &&
                !string.IsNullOrEmpty(context.Message.Text))
            {
                if (Regex.IsMatch(context.Message.Text, @"\breload responders\b", RegexOptions.IgnoreCase))
                {
                    LoadResponders();
                    await SendMessage(context.Message.ChatHub.Name, "Responders reloaded\n" + string.Join(Environment.NewLine, bot.Responders));
                }

                if (Regex.IsMatch(context.Message.Text, @"\breload config\b", RegexOptions.IgnoreCase))
                {
                    cfg.LoadConfig();
                    await SendMessage(context.Message.ChatHub.Name, "Config file reloaded");
                }
            }
        }

        #endregion

        #region Helpers

        #region SendMessages
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async Task SendMessage(string where, string message)
        {
            await SendMessage(where, message, null);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="where">The where.</param>
        /// <param name="message">The message.</param>
        /// <param name="slas">The slas.</param>
        /// <returns></returns>
        public async Task SendMessage(string where, string message, List<SlackAttachment> slas)
        {
            if (null != bot)
            {
                SlackChatHub sch = GetChannel(where);
                if (sch != null)
                {
                    BotMessage msg = new BotMessage() { Text = message, ChatHub = sch } ;
                    if (null != slas && slas.Count > 0)
                    {
                        msg.Attachments = slas;
                    }
                    await SendMessage(msg);
                }
            }
        }
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        public async Task SendMessage(BotMessage msg)
        {
            if (null != bot && msg.ChatHub != null && !string.IsNullOrEmpty(msg.Text))
            {
                await bot.Say(msg);
            }
        }
        #endregion

        /// <summary>
        /// Formats the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public string FormatMessage(string message)
        {
            string pattern = @"<@(\w+)>";
            Regex rgx = new Regex(pattern);
            foreach (Match match in rgx.Matches(message))
            {
                string user = match.Value.Substring(2, match.Value.Length - 3);
                message = message.Replace(match.Value, "@" + GetUserNameFromUID(user));
            }

            return message;
        }

        /// <summary>
        /// Gets the channel.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public SlackChatHub GetChannel(string target)
        {
            if (null != bot)
            {
                List<SlackChatHub> targets = bot.ConnectedChannels.ToList();

                if (target.Contains("@"))
                {
                    targets = bot.ConnectedDMs.ToList();
                }

                target = target.ToLower();

                SlackChatHub sch = targets.Find(x => x.Name == target);

                return sch;
            }

            return null;
        }

        /// <summary>
        /// Gets the user name from uid.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns></returns>
        public string GetUserNameFromUID(string uid)
        {
            return bot.GetUserName(uid);
        }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when the slack bot is disconnected (no more ping possible to rtm server).
        /// </summary>
        public event EventHandlers.BotEvents.SlackControllerDisconnectEventHandler onSlackBotDisconnected;
        private void RaiseSlackBotDisconnected()
        {
            if (onSlackBotDisconnected != null)
            {
                onSlackBotDisconnected();
            }
        }

        /// <summary>
        /// Occurs when the Slack bot is connected.
        /// </summary>
        public event EventHandlers.BotEvents.SlackControllerConnectedEventHandler onSlackBotConnected;
        private void RaiseSlackBotConnected()
        {
            if (onSlackBotConnected != null)
            {
                onSlackBotConnected();
            }
        }
        #endregion
    }
}