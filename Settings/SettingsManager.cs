using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GMDCGiphyPlugin.Settings
{
    public class SettingsManager
    {
        public SettingsManager(string databaseName)
        {
            this.DatabaseName = databaseName;
            this.LoadDatabase();
        }

        private string DatabaseName
        {
            get;
            set;
        }

        public GMDCGiphyPlugin.Settings.Settings Settings
        {
            get;
            set;
        } = new Settings();

        public void LoadDatabase()
        {
            if (File.Exists(this.DatabaseName))
            {
                string json = File.ReadAllText(this.DatabaseName);
                if (json == "")
                {
                    SaveDatabase();
                }
                JsonConvert.PopulateObject(json, this);
            }
        }

        public void SaveDatabase()
        {
            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(this.DatabaseName))
            {
                JsonSerializer serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                };

                serializer.Serialize(file, this);
            }
        }
    }
}
