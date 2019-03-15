using ICSlackBot.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICSlackBot.Logic.Controllers
{
    /// <summary>
    /// Controlls the config files
    /// </summary>
    public class ConfigController
    {
        #region vars
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Configurator config = null;
        private string location = "";
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigController"/> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public ConfigController(string location)
        {
            this.location = location;
        }

        /// <summary>
        /// Loads the configuration file and saves the Data in the __see cref="ConfigParams"/__ object
        /// Create a default file if none can be found
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unable to load config file</exception>
        public bool LoadConfig()
        {
            string cfgFile = Path.Combine(location, @"config.ini");

            config = new Configurator();

            if (!File.Exists(cfgFile))
            {
                logger.Error("Log file missing. Creating empty config file and abording.");
                config.AddValue("Slack", "ApiToken", "xxx", true);
                config.AddValue("Slack", "Aliasses", "Jarvis,Scotty,James,Amy,Ava,Eve,Vici,Viki", true);
                config.AddValue("Slack", "QAXmlFile", @"resources\QA.xml", true);

                config.Save(cfgFile, Configurator.FileType.Ini);
                throw new Exception("Unable to load config file");
            }

            logger.Info("Loading confile file");
            config.LoadFromFile(cfgFile, Configurator.FileType.Ini);
            
            return true;
        }

        /// <summary>
        /// Saves the configuration file as Ini format and reloads the whole config file
        /// </summary>
        public void Save()
        {
            config.Save(Configurator.FileType.Ini);
            LoadConfig();
        }


        /// <summary>
        /// Gets the config value based on category and key from the loaded config file
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string Get(string section, string key, string defaultValue)
        {
            return config.GetValue(section, key, defaultValue);
        }

        /// <summary>
        /// Gets the keys from section.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <returns></returns>
        public List<string> GetKeysFromSection(string section)
        {
            return config.GetKeys(section);
        }
        
        /// <summary>
        /// Sets a config value
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Set(string section, string key, string value)
        {
            config.AddValue(section, key, value, true);
        }
    }
}
