using KAGTools.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KAGTools.Helpers
{
    public static class FileHelper
    {
        // Application
        public static string CommonDir = Path.Combine("..", "common");
        public static string SettingsPath = Path.Combine(CommonDir, "settings.json");
        public static string LogPath = Path.Combine(CommonDir, "log.txt");

        // KAG directories/files
        public static string KagDir => App.Settings.KagDirectory;
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

        public static void ReadConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            ReadWriteConfigProperties(filePath, false, configProperties);
        }

        public static void WriteConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            ReadWriteConfigProperties(filePath, true, configProperties);
        }

        private static void ReadWriteConfigProperties(string filePath, bool writePropertiesToFile, params BaseConfigProperty[] configProperties)
        {
            var configPropertyList = new List<BaseConfigProperty>(configProperties);

            string[] lines = File.ReadAllLines(filePath);
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
                File.WriteAllLines(filePath, lines);
            }
        }

        public static List<string> GetActiveModNames()
        {
            string[] lines = File.ReadAllLines(ModsConfigPath);
            return lines.Where((line) => !line.StartsWith("#")).ToList();
        }

        public static IEnumerable<Mod> GetMods(bool activeOnly = false)
        {
            List<string> activeModNames = GetActiveModNames();

            foreach (string dir in Directory.EnumerateDirectories(ModsDir))
            {
                Mod mod = new Mod(dir);
                mod.IsActive = activeModNames.Contains(mod.Name);

                if (mod.IsActive || !activeOnly)
                {
                    yield return mod;
                }
            }
        }

        public static void SetActiveMods(Mod[] mods)
        {
            string[] comments = File.ReadAllLines(ModsConfigPath).Where((line) => line.StartsWith("#")).ToArray();
            List<string> lines = new List<string>(comments);
            lines.AddRange(mods.Select((mod) => mod.Name));
            File.WriteAllLines(ModsConfigPath, lines.ToArray());
        }

        public static bool IsValidPath(string path)
        {
            Regex regex = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
            return !regex.IsMatch(path);
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            // Copy folders
            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
            }

            // Copy files
            foreach (string newPath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourceDir, destDir), true);
            }
        }

        public static string FindFirstFile(string dir, string fileName, bool includeSubs = true)
        {
            string[] filePaths = FindFiles(dir, fileName, includeSubs);
            if (filePaths.Length == 0) return null;
            return filePaths[0];
        }

        public static string[] FindFiles(string dir, string fileName, bool includeSubs = true)
        {
            return Directory.GetFiles(dir, fileName, includeSubs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public static string FindGamemodeOfMod(string modDir)
        {
            string gamemodeConfigPath = FindFirstFile(modDir, "gamemode.cfg");
            if (gamemodeConfigPath != null)
            {
                var gamemodeProperty = new StringConfigProperty("gamemode_name", null);
                ReadConfigProperties(gamemodeConfigPath, gamemodeProperty);

                if (!string.IsNullOrEmpty(gamemodeProperty.Value))
                {
                    return gamemodeProperty.Value;
                }
            }

            return null;
        }

        public static List<ManualItem> GetManualFunctions(string fileName, bool findTypes = false)
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
    }
}
