using KAGTools.Data;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Services
{
    public class JsonSettingsService<T>
    {
        public T Settings { get; set; }
        public string FilePath { get; set; }

        public JsonSettingsService(string filePath)
        {
            FilePath = filePath;
        }

        public void Load()
        {
            Log.Information("Loading user settings");

            string json = "{}";

            if (File.Exists(FilePath))
            {
                try
                {
                    json = File.ReadAllText(FilePath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to read user settings file: {Path}", FilePath);
                }
            }
            else
            {
                Log.Warning("User settings file not found: {Path}", FilePath);
            }

            var serializerSettings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Populate
            };

            Settings = JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        public void Save()
        {
            Log.Information("Saving user settings");

            var serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                FloatFormatHandling = FloatFormatHandling.String
            };

            string json = JsonConvert.SerializeObject(Settings, serializerSettings);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write user settings to disk: {Path}", FilePath);
            }
        }
    }
}
