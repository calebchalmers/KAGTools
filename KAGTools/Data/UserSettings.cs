using KAGTools.Helpers;
using Newtonsoft.Json;
using Serilog;
using Serilog.Formatting.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace KAGTools.Data
{
    public class UserSettings
    {
        #region Settings Definitions
        // Application
        [DefaultValue("")]
        public string KagDirectory { get; set; }

        [DefaultValue(false)]
        public bool UsePreReleases { get; set; }

        // Main window
        [DefaultValue(double.NaN)]
        public double Left { get; set; }

        [DefaultValue(double.NaN)]
        public double Top { get; set; }

        [DefaultValue(0)]
        public int RunTypeIndex { get; set; }

        // Manual window
        [DefaultValue(500)]
        public double ManualWindowWidth { get; set; }

        [DefaultValue(600)]
        public double ManualWindowHeight { get; set; }

        [DefaultValue(double.NaN)]
        public double ManualWindowTop { get; set; }

        [DefaultValue(double.NaN)]
        public double ManualWindowLeft { get; set; }
        #endregion

        public static UserSettings Load(string path)
        {
            Log.Information("Loading user settings");

            string json = "{}";
            if (File.Exists(path))
            {
                try
                {
                    json = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to read user settings file: {Path}", path);
                }
            }
            else
            {
                Log.Warning("User settings file not found: {Path}", path);
            }

            var serializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };

            return JsonConvert.DeserializeObject<UserSettings>(json, serializerSettings);
        }

        public void Save(string path)
        {
            Log.Information("Saving user settings");

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                FloatFormatHandling = FloatFormatHandling.String
            };

            string json = JsonConvert.SerializeObject(this, serializerSettings);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write user settings to disk: {Path}", path);
            }
        }
    }
}
