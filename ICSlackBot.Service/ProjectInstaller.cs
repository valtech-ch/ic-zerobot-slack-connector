using System.ComponentModel;

namespace ICSlackBot
{
    /// <summary>
    /// To Install the Service local for tests, use installutil __yourproject__.exe
    /// </summary>
    /// <seealso cref="System.Configuration.Install.Installer" />
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// 
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
