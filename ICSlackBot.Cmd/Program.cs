namespace ICSlackBot.Cmd
{
    using ICSlackBot;
    using System;
    /// <summary>
    /// Static command line
    /// </summary>
    class Program
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Main call. 
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            logger.Info("Start command line tool");
            try
            {
                logger.Info("Run bot");
                RunBot();
                logger.Info("Stop bot");
            }
            catch (Exception ee)
            {
                logger.Error("CMD ERROR: "+ ee.Message);
                Console.ReadKey();
            }            
        }

        /// <summary>
        /// Runs the bot until ESC is pressed in the console.
        /// </summary>
        static void RunBot()
        {
            MainProgram mainProg = new MainProgram();

            do { } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            mainProg.Shutdown();
        }
    }
}
