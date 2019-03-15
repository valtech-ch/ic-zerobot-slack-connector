namespace ICSlackBot.Service
{
    using System.ServiceProcess;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ICSlackBotService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
