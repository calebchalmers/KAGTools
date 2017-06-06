using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KAGTools
{
    public static class FileHelper
    {
        public static string KagDir
        {
            get
            {
                return Properties.Settings.Default.KagDirectory;
            }
        }

        public static string ScreenshotsDir
        {
            get
            {
                return Path.Combine(KagDir, "Screenshots");
            }
        }

        public static string ModsDir
        {
            get
            {
                return Path.Combine(KagDir, "Mods");
            }
        }

        public static string StartupConfigPath
        {
            get
            {
                return Path.Combine(KagDir, "startup_config.cfg");
            }
        }

        public static string ModsConfigPath
        {
            get
            {
                return Path.Combine(KagDir, "mods.cfg");
            }
        }

        public static string AutoConfigPath
        {
            get
            {
                return Path.Combine(KagDir, "autoconfig.cfg");
            }
        }

        public static string RunLocalhostPath
        {
            get
            {
                return Path.Combine(KagDir, "runlocalhost.bat");
            }
        }

        /*public static void GetStartupInfo(ref int width, ref int height, ref bool fullscreen)
        {
            string[] lines = File.ReadAllLines(StartupConfigPath);
            foreach (string line in lines)
            {
                string[] args = line.Split('=');
                if (args.Length > 1)
                {
                    string value = args[1];
                    if (line.StartsWith("Window.Width"))
                    {
                        int.TryParse(value, out width);
                    }
                    else if (line.StartsWith("Window.Height"))
                    {
                        int.TryParse(value, out height);
                    }
                    else if (line.StartsWith("Fullscreen"))
                    {
                        if (value == "0")
                            fullscreen = false;
                        else if (value == "1")
                            fullscreen = true;
                    }
                }
            }
        }

        public static void SetStartupInfo(int width, int height, bool fullscreen)
        {
            string[] lines = File.ReadAllLines(StartupConfigPath);
            for(int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("Window.Width"))
                {
                    lines[i] = "Window.Width=" + width;
                }
                else if (line.StartsWith("Window.Height"))
                {
                    lines[i] = "Window.Height=" + height;
                }
                else if (line.StartsWith("Fullscreen"))
                {
                    lines[i] = "Fullscreen=" + (fullscreen ? "1" : "0");
                }
            }
            File.WriteAllLines(StartupConfigPath, lines);
        }
        public static void GetAutoconfigInfo(ref string gamemode)
        {
            string[] lines = File.ReadAllLines(StartupConfigPath);
            foreach (string line in lines)
            {
                string[] args = line.Split('=');
                if (args.Length > 1)
                {
                    string value = args[1];
                    if (line.StartsWith("sv_gamemode"))
                    {
                        gamemode = value;
                    }
                }
            }
        }

        public static void SetAutoconfigInfo(string gamemode)
        {
            string[] lines = File.ReadAllLines(StartupConfigPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("sv_gamemode"))
                {
                    lines[i] = "sv_gamemode = " + gamemode;
                }
            }
            File.WriteAllLines(StartupConfigPath, lines);
        }*/

        public static void GetConfigInfo(string filePath, params ConfigProperty[] properties)
        {
            string[] lines = File.ReadAllLines(filePath);
            for(int i = 0; i < lines.Length; i++)
            {
                string line = Regex.Replace(lines[i], @"\s+", "");
                string[] args = line.Split('=');
                if (args.Length > 1)
                {
                    string rawvalue = args[1];

                    int commentIndex = rawvalue.IndexOf('#');
                    if(commentIndex != -1)
                    {
                        rawvalue = rawvalue.Remove(commentIndex);
                    }

                    Debug.WriteLine(rawvalue);

                    foreach (var property in properties)
                    {
                        if (line.StartsWith(property.PropertyName))
                        {
                            object value = rawvalue;

                            if (property is ConfigPropertyDouble)
                            {
                                double result = 0;
                                double.TryParse(rawvalue, out result);
                                value = result;
                            }
                            else if (property is ConfigPropertyBoolean)
                            {
                                value = rawvalue == "1";
                            }

                            property.Value = value;
                        }
                    }
                }
            }
        }

        public static void SetConfigInfo(string filePath, params ConfigProperty[] properties)
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string rawline = lines[i];
                string line = Regex.Replace(rawline, @"\s+", "");

                foreach (var property in properties)
                {
                    if (line.StartsWith(property.PropertyName))
                    {
                        string value = property.Value.ToString();

                        if(property is ConfigPropertyBoolean)
                        {
                            value = (bool)property.Value ? "1" : "0";
                        }

                        string comment = "";
                        int commentIndex = rawline.IndexOf('#');
                        if (commentIndex != -1)
                        {
                            comment = rawline.Substring(commentIndex);
                        }

                        lines[i] = property.PropertyName + " = " + value + "\t" + comment;
                    }
                }
            }
            File.WriteAllLines(filePath, lines);
        }

        public static List<string> GetActiveModNames()
        {
            string[] lines = File.ReadAllLines(ModsConfigPath);
            return lines.Where((line) => !line.StartsWith("#")).ToList();
        }

        public static IEnumerable<Mod> GetMods()
        {
            List<string> activeModNames = GetActiveModNames();

            foreach(string dir in Directory.EnumerateDirectories(ModsDir))
            {
                Mod mod = new Mod(dir);
                mod.IsActive = activeModNames.Contains(mod.Name);
                yield return mod;
            }
        }

        public static void SetActiveMods(Mod[] mods)
        {
            string[] comments = File.ReadAllLines(ModsConfigPath).Where((line) => line.StartsWith("#")).ToArray();
            List<string> lines = new List<string>(comments);
            lines.AddRange(mods.Select((mod) => mod.Name));
            File.WriteAllLines(ModsConfigPath, lines.ToArray());
        }
    }
}
