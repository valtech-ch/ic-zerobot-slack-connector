using SlackBot.Models;
using SlackBot.Responders;
using ICSlackBot.Logic.Controllers;

namespace ICSlackBot.Logic.Responders
{
    /// <summary>
    /// Allows to check in at a specific location
    /// </summary>
    public class TaskResponder : IResponder
    {

        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SlackBotController botController;

        public TaskResponder(SlackBotController botController)
        {
            this.botController = botController;
        }

        public TaskResponder() : this(null) { }

        /// <summary>
        /// Determines whether this instance can respond the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public bool CanRespond(ResponseContext context)
        {
            return true;
        }

        /// <summary>
        /// Answers with a message containing links to the comics selected
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public BotMessage GetResponse(ResponseContext context)
        {
            BotMessage botMessage = new BotMessage() { Text = "Yeeeyyy, we answer" };
            return botMessage;
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
