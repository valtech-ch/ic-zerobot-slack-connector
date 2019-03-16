using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ICZeroBotSlack.Logic.Helpers
{
    /// <summary>
    /// Class to handle config files
    /// </summary>
    public sealed class Configurator
    {
        #region Declarations
        private DataTable Settings; 
        private string fileName = "";

        /// <summary>
        /// The file Types
        /// </summary>
        public enum FileType //this specifies if the file is an xml or an ini
        {
            Ini, Xml
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="Configurator"/> class. Creates the settings
        /// </summary>
        public Configurator()
        {
            initializeDataTable();
        }

        /// <summary>
        /// loads settings from a file (xml or ini)
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="ft">The ft.</param>
        public void LoadFromFile(string file, FileType ft)
        {
            fileName = Path.GetFullPath(file); //saves the filename for future use

            if (ft == FileType.Ini)
                LoadFromIni();
            else
                LoadFromXml();
        }

        /// <summary>
        /// Adds a new setting to the table
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <param name="Key">The key.</param>
        /// <param name="Value">The value.</param>
        /// <param name="OverwriteExisting">if set to <c>true</c> [overwrite existing].</param>
        public void AddValue(string Category, string Key, string Value, bool OverwriteExisting)
        {
            if (OverwriteExisting)
            {
                foreach (DataRow row in Settings.Rows.Cast<DataRow>().Where(row => (string)row[0] == Category && (string)row[1] == Key))
                {
                    row[2] = Value;
                    return;
                }

                Settings.Rows.Add(Category, Key, Value);
            }
            else
                Settings.Rows.Add(Category, Key, Value);
        }

        /// <summary>
        /// Gets a value or returns a default value
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <param name="Key">The key.</param>
        /// <param name="DefaultValue">The default value.</param>
        /// <returns></returns>
        public string GetValue(string Category, string Key, string DefaultValue)
        {
            List<string> rets = new List<string>();
            string cat = Category.ToLower();
            string key = Key.ToLower();
            foreach (DataRow row in Settings.Rows.Cast<DataRow>().Where(row => (string)row[0].ToString().ToLower() == cat && (string)row[1].ToString().ToLower() == key))
            {
                rets.Add((string)row[2]);
            }

            return string.Join(";", rets);
        }

        /// <summary>
        /// Gets the keys from a given section
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <returns></returns>
        public List<string> GetKeys(string Category)
        {
            List<string> rets = new List<string>();
            string cat = Category.ToLower();
            foreach (DataRow row in Settings.Rows.Cast<DataRow>().Where(row => (string)row[0].ToString().ToLower() == cat))
            {
                rets.Add((string)row[1]);
            }
            return rets;
        }


        /// <summary>
        /// Saves the file to the previously loaded file
        /// </summary>
        /// <param name="ft">The ft.</param>
        /// <exception cref="System.IO.FileNotFoundException">The file name was not previously defined</exception>
        public void Save(FileType ft) 
        {
            //sorts the table for saving

            if (fileName == "") throw new FileNotFoundException("The file name was not previously defined");

            DataView dv = Settings.DefaultView;
            dv.Sort = "Category asc";
            DataTable sortedDT = dv.ToTable();

            if (ft == FileType.Xml)
                sortedDT.WriteXml(fileName);
            else
            {
                StreamWriter sw = new StreamWriter(fileName);

                string lastCategory = "";

                foreach (DataRow row in sortedDT.Rows)
                {
                    if ((string)row[0] != lastCategory)
                    {
                        lastCategory = (string)row[0];
                        sw.WriteLine("[" + lastCategory + "]");
                    }

                    sw.WriteLine((string)row[1] + "=" + (string)row[2]);
                }

                sw.Close();
            }
        }

        /// <summary>
        /// Saves the file to a file
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="ft">The ft.</param>
        public void Save(string file, FileType ft) 
        {
            fileName = Path.GetFullPath(file); //saves the filename for future use

            Save(ft);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads settings from ini
        /// </summary>
        private void LoadFromIni() 
        {
            if (!File.Exists(fileName)) return;

            StreamReader sr = new StreamReader(fileName); //stream reader that will read the settings

            string currentCategory = ""; //holds the category we're at

            while (!sr.EndOfStream) //goes through the file
            {
                string currentLine = sr.ReadLine(); //reads the current file

                if (currentLine.Length < 3) continue; //checks that the line is usable

                if (currentLine.StartsWith("[") && currentLine.EndsWith("]")) //checks if the line is a category marker
                {
                    currentCategory = currentLine.Substring(1, currentLine.Length - 2);
                    continue;
                }

                if (!currentLine.Contains("=")) continue; //or an actual setting

                string currentKey = currentLine.Substring(0, currentLine.IndexOf("=", StringComparison.Ordinal));

                string currentValue = currentLine.Substring(currentLine.IndexOf("=", StringComparison.Ordinal) + 1);

                AddValue(currentCategory, currentKey, currentValue, false);
            }

            sr.Close(); //closes the stream
        }

        /// <summary>
        /// Loads the settings from an xml file
        /// </summary>
        private void LoadFromXml() 
        {
            Settings.ReadXml(fileName);
        }

        /// <summary>
        /// Re-initializes the table with the proper columns
        /// </summary>
        private void initializeDataTable()
        {
            Settings = new DataTable { TableName = "Settings" };

            Settings.Columns.Add("Category", typeof(string));
            Settings.Columns.Add("SettingKey", typeof(string));
            Settings.Columns.Add("SettingsValue", typeof(string));
        }

        #endregion

    }
}
