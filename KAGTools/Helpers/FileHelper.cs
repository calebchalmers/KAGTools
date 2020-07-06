using KAGTools.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KAGTools.Helpers
{
    public static class FileHelper
    {
        // Application
        public static string CommonDir = Path.Combine("..", "common");
        public static string SettingsPath = Path.Combine(CommonDir, "settings.json");
        public static string LogPath = Path.Combine(CommonDir, "log.txt");

        // KAG directories/files
        public static string KagDir { get; set; } // Must be set on startup
        public static string ScreenshotsDir => Path.Combine(KagDir, "Screenshots");
        public static string ModsDir => Path.Combine(KagDir, "Mods");
        public static string StartupConfigPath => Path.Combine(KagDir, "startup_config.cfg");
        public static string ModsConfigPath => Path.Combine(KagDir, "mods.cfg");
        public static string AutoConfigPath => Path.Combine(KagDir, "autoconfig.cfg");
        public static string RunLocalhostPath => Path.Combine(KagDir, "runlocalhost.bat");
        public static string RunDedicatedServerPath => Path.Combine(KagDir, "dedicatedserver.bat");
        public static string KagExecutablePath => Path.Combine(KagDir, "KAG.exe");

        // Manual
        public static string ManualDir => Path.Combine(KagDir, "Manual", "interface");
        public static string ManualObjectsPath => Path.Combine(ManualDir, "Objects.txt");
        public static string ManualFunctionsPath => Path.Combine(ManualDir, "Functions.txt");
        public static string ManualHooksPath => Path.Combine(ManualDir, "Hooks.txt");
        public static string ManualEnumsPath => Path.Combine(ManualDir, "Enums.txt");
        public static string ManualVariablesPath => Path.Combine(ManualDir, "Variables.txt");
        public static string ManualTypeDefsPath => Path.Combine(ManualDir, "TypeDefs.txt");

        private const int ManualHeaderLineCount = 3;
        private const char ManualIndentCharacter = '\t';

        // Auto-Start scripts
        public static string ClientAutoStartScriptPath = Path.GetFullPath(@"Resources\client_autostart.as");
        public static string ServerAutoStartScriptPath = Path.GetFullPath(@"Resources\server_autostart.as");
        public static string SoloAutoStartScriptPath = Path.GetFullPath(@"Resources\solo_autostart.as");

        public static bool ReadConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            Log.Information("Reading config file: {FilePath}", filePath);
            return ReadWriteConfigProperties(filePath, false, configProperties);
        }

        public static bool WriteConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            Log.Information("Writing config file: {FilePath}", filePath);
            return ReadWriteConfigProperties(filePath, true, configProperties);
        }

        private static bool ReadWriteConfigProperties(string filePath, bool writePropertiesToFile, params BaseConfigProperty[] configProperties)
        {
            if (!File.Exists(filePath))
            {
                Log.Error("Could not find config file: {FilePath}", filePath);
                return false;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read config file: {FilePath}", filePath);
                return false;
            }

            var configPropertyList = new List<BaseConfigProperty>(configProperties);

            for (int i = 0; i < lines.Length; i++)
            {
                if (configPropertyList.Count == 0)
                    break;

                string line = lines[i];
                string lineNoComment = line;
                string comment = null;

                int commentStartIndex = line.IndexOf('#');
                if (commentStartIndex != -1)
                {
                    lineNoComment = lineNoComment.Remove(commentStartIndex);
                    comment = line.Substring(commentStartIndex);
                }

                int splitIndex = lineNoComment.IndexOf('=');
                if (splitIndex == -1) continue;

                string propertyName = lineNoComment.Substring(0, splitIndex).Trim();
                string propertyValue = lineNoComment.Substring(splitIndex + 1).Trim();

                var targetPropertyIndex = configPropertyList.FindIndex(p => p.Name == propertyName);
                if (targetPropertyIndex != -1)
                {
                    var targetProperty = configPropertyList[targetPropertyIndex];

                    if (writePropertiesToFile)
                    {
                        if (targetProperty.Value != propertyValue)
                        {
                            lines[i] = $"{propertyName} = {targetProperty.Value}" + (comment != null ? $" {comment}" : "");
                        }
                    }
                    else
                    {
                        targetProperty.Value = propertyValue;
                    }

                    configPropertyList.RemoveAt(targetPropertyIndex);
                }
            }

            if (writePropertiesToFile)
            {
                try
                {
                    File.WriteAllLines(filePath, lines);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to write config file: {FilePath}", filePath);
                    return false;
                }
            }

            return true;
        }

        public static List<string> GetActiveModNames()
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(ModsConfigPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read mods config file");
                return null;
            }
            return lines.Where((line) => !line.StartsWith("#")).ToList();
        }

        public static List<Mod> GetMods(bool activeOnly = false)
        {
            IEnumerable<string> dirs;
            try
            {
                dirs = Directory.EnumerateDirectories(ModsDir);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enumerate mods folder");
                return null;
            }

            List<string> activeModNames = GetActiveModNames();
            List<Mod> modList = new List<Mod>();

            foreach (string dir in dirs)
            {
                Mod mod = new Mod(new DirectoryInfo(dir).Name, dir);
                mod.IsActive = activeModNames?.Contains(mod.Name);

                if (!activeOnly || mod.IsActive == true)
                {
                    modList.Add(mod);
                }
            }
            return modList;
        }

        public static bool SetActiveMods(Mod[] mods)
        {
            try
            {
                string[] comments = File.ReadAllLines(ModsConfigPath).Where((line) => line.StartsWith("#")).ToArray();
                List<string> lines = new List<string>(comments);
                lines.AddRange(mods.Select((mod) => mod.Name));
                File.WriteAllLines(ModsConfigPath, lines.ToArray());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update mods config");
                return false;
            }
        }

        private static string FindFirstFile(string dir, string fileName, bool includeSubs = true)
        {
            string[] filePaths = Directory.GetFiles(dir, fileName, SearchOption.AllDirectories);
            return filePaths.Length > 0 ? filePaths[0] : null;
        }

        public static string FindGamemodeOfMod(string modDir)
        {
            string gamemodeConfigPath;

            try
            {
                gamemodeConfigPath = FindFirstFile(modDir, "gamemode.cfg");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed searching for gamemode config");
                return null;
            }

            if (gamemodeConfigPath == null)
            {
                return null;
            }

            // If we found a gamemode config then read the gamemode name
            var gamemodeProperty = new StringConfigProperty("gamemode_name", null);
            ReadConfigProperties(gamemodeConfigPath, gamemodeProperty);
            return gamemodeProperty.Value;
        }

        public static List<ManualItem> GetManualFunctions(string fileName, bool findTypes = false)
        {
            if (!File.Exists(fileName))
            {
                Log.Error("Could not find manual file: {FileName}", fileName);
                return null;
            }

            try
            {
                List<ManualItem> items = new List<ManualItem>();

                using (StreamReader reader = new StreamReader(fileName))
                {
                    string line = "";

                    for (int i = 0; i < ManualHeaderLineCount; i++) // Remove manual header
                    {
                        reader.ReadLine();
                    }

                    string lastType = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                            continue;

                        if (findTypes)
                        {
                            if (!line.StartsWith(ManualIndentCharacter.ToString())) // Is a class declaration
                            {
                                lastType = line;
                                continue;
                            }
                        }

                        string info = line.TrimStart(ManualIndentCharacter); // Trim indent characters
                        items.Add(new ManualItem(lastType, info));
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to read manual file: {FileName}", fileName);
                return null;
            }
        }
    }
}
