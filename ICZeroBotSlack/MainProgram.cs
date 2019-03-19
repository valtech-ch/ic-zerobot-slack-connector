using System;
using log4net;
using ICZeroBotSlack.Logic.Controllers;
using ICSlackBot.API;

/// <summary>
/// Main Namespace
/// </summary>
namespace ICZeroBotSlack
{
    /// <summary>
    /// Main Class of the programm
    /// </summary>
    public class MainProgram
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SlackBotController _slackBot = null;
        private ApiBase _api = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainProgram"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public MainProgram()
        {
            log4net.Config.XmlConfigurator.Configure();
            string location = AppDomain.CurrentDomain.BaseDirectory;

            _logger.Info("Starting main app from location " + location);
            ConfigController cfg = new ConfigController(location);

            if (cfg.LoadConfig())
            {
                _logger.Info("Config loaded, starting modules");
                _slackBot = new SlackBotController(cfg);
                _slackBot.InitBot();

                _api = new ApiBase(new Uri(cfg.Get("API", "Host", "http://localhost:50231/")));

                _slackBot.onMessageReceived += _slackBot_onMessageReceived;
                _logger.Info("Ready");
            }
            else
            {
                _logger.Warn("Unable to load config file");
            }
        }

        /// <summary>
        /// _slacks the bot_on message received.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="message">The message.</param>
        /// <param name="channel">The channel.</param>
        private void _slackBot_onMessageReceived(string from, string message, string channel)
        {
            string msg = _slackBot.FormatMessage(message);
        }


        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            _logger.Info("Stoping application");
            if (_api != null)
            {
                _api.Stop();
            }
            if (_slackBot != null)
            {
                _slackBot.Shutdown();
            }
        }
    }
}
