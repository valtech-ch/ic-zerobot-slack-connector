using SlackBot.Models;
using SlackBot.Responders;
using ICSlackBot.Logic.Controllers;
using ICSlackBot.Logic.Connectors;
using ICSlackBot.Logic.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ICSlackBot.Logic.Responders
{
    /// <summary>
    /// Allows to check in at a specific location
    /// </summary>
    public class TaskResponder : IResponder
    {
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SlackBotController _botController;
        private IcZeroBotConnector _icZeroBotConnector;

        public TaskResponder(SlackBotController botController)
        {
            _botController = botController;
            _icZeroBotConnector = new IcZeroBotConnector(botController.cfg.Get("ZeroBot", "BusinessLogicEndPoint", "http://ic-zerobot.infocentric.ch/api"));
        }

        public TaskResponder() : this(null) { }

        /// <summary>
        /// Determines whether this instance can respond the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.ChatHub.Type == SlackChatHubType.DM;
        }

        /// <summary>
        /// Answers with a message containing links to the comics selected
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public BotMessage GetResponse(ResponseContext context)
        {
            BotMessage botMessage = new BotMessage() { Text = "Didn't work :(" };
            try
            {
                //TODO: add switch logic depending on what the bot should to
                if (true)
                {
                    botMessage.Text = CreateTask(context.Message);
                }
            }
            catch (Exception ee)
            {
                botMessage.Text += $" {ee.Message}";
            }
            return botMessage;
        }

        /// <summary>
        /// Creates the task.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns></returns>
        private string CreateTask(SlackMessage msg)
        {
            IcZeroBotTask task = new IcZeroBotTask()
            {
                Title = msg.Text.Replace("create task ", ""),
                Description = msg.Text.Replace("create task ", ""),
                Creator = msg.User?.Name ?? "Unidentified user. Probably new."
            };

            List<byte[]> atta = new List<byte[]>();

            dynamic d = JObject.Parse(msg.RawData);

            if (d.files != null && d.files.Count > 0)
            {
                var webClient = new WebClient();
                webClient.Headers.Add("Authorization", $"Bearer #{this._botController.cfg.Get("Slack", "ApiToken", "xxx")}");

                foreach (dynamic f in d.files)
                {
                    byte[] img = webClient.DownloadData(f.url_private.ToString());
                    atta.Add(img);
                }
            }

            if (atta.Any())
            {
                task.Attachements = atta;
            }

            string uid = _icZeroBotConnector.CreateTask(task);

            return $"Yeeeyyy, task created ${uid}";
        }


        /// <summary>
        /// Gets the command description.
        /// </summary>
        /// <returns></returns>
        public string GetCommandDescription()
        {
            return "*task*";
        }
    
    }
}
