using KAGTools.Data;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;

namespace KAGTools.Helpers
{
    public static class SettingsHelper
    {
        private static JsonSerializerSettings SettingsSerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Populate,
            Formatting = Formatting.Indented,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double
        };

        public static Settings Load()
        {
            Log.Information("Loading settings");

            string json = "{}";
            if (File.Exists(FileHelper.SettingsPath))
            {
                try
                {
                    json = File.ReadAllText(FileHelper.SettingsPath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to read settings file");
                }
            }
            else
            {
                Log.Information("No settings file found");
            }
            return JsonConvert.DeserializeObject<Settings>(json, SettingsSerializerSettings);
        }

        public static void Save(Settings settings)
        {
            Log.Information("Saving settings");

            string json = JsonConvert.SerializeObject(settings, SettingsSerializerSettings);
            try
            {
                Directory.CreateDirectory(FileHelper.CommonDir);
                File.WriteAllText(FileHelper.SettingsPath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write settings to disk");
            }
        }
    }
}
