using log4net;
using System.ServiceProcess;
using System.Timers;

namespace ICZeroBotSlack.Service
{
    /// <summary>
    /// IC Zero Bot Slack Service. The bot starts and stops via Windows service
    /// </summary>
    public partial class ICZeroBotSlackService : ServiceBase
    {
        #region vars
        private MainProgram main;
        private Timer startTimer = null;
        private static readonly ILog logger = LogManager.GetLogger(typeof(ICZeroBotSlackService));
        #endregion

        #region Init and start
        /// <summary>
        /// Initializes a new instance of the <see cref="ICZeroBotSlackService"/> class. Starts also the log.
        /// </summary>
        public ICZeroBotSlackService()
        {
            log4net.Config.XmlConfigurator.Configure();

            logger.Debug("Service initialized");
            InitializeComponent();
        }

        /// <summary>
        /// Called when the service starts. Starts directly the Init function in Release mode or deplays the start for debugging
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected override void OnStart(string[] args)
        {
            logger.Info("Service started");
#if DEBUG
            startTimer = new Timer();
            startTimer.Interval = 10000;
            startTimer.Elapsed += time_Elapsed;
            startTimer.Start();
            logger.Info("Debug mode, delayed init");
#else
            Init();
#endif
        }

        /// <summary>
        /// Called when the service stops
        /// </summary>
        protected override void OnStop()
        {
            if (main != null)
            {
                main.Shutdown();
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the time control for the init function
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        void time_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            startTimer.Stop();
            Init();
        }
        #endregion

        /// <summary>
        /// Starts the real part of the service and launches the Hartbeat
        /// </summary>
        protected void Init() {
            main = new MainProgram();
        }
    }
}
