﻿using System;
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
        public static string KagDir { get { return Properties.Settings.Default.KagDirectory; } }
        public static string ScreenshotsDir { get { return Path.Combine(KagDir, "Screenshots"); } }
        public static string ModsDir { get { return Path.Combine(KagDir, "Mods"); } }
        public static string StartupConfigPath { get { return Path.Combine(KagDir, "startup_config.cfg"); } }
        public static string ModsConfigPath { get { return Path.Combine(KagDir, "mods.cfg"); } }
        public static string AutoConfigPath { get { return Path.Combine(KagDir, "autoconfig.cfg"); } }
        public static string RunLocalhostPath { get { return Path.Combine(KagDir, "runlocalhost.bat"); } }
        public static string RunDedicatedServerPath { get { return Path.Combine(KagDir, "dedicatedserver.bat"); } }
        public static string KAGExecutablePath { get { return Path.Combine(KagDir, "KAG.exe"); } }
        public static string ClientLocalhostScriptPath { get { return Path.GetFullPath(@"Resources\client_localhost.as"); } }

        public static void GetConfigInfo(string filePath, params ConfigProperty[] properties)
        {
            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = Regex.Replace(lines[i], @"\s+", "");
                string[] args = line.Split('=');
                if (args.Length > 1)
                {
                    string rawvalue = args[1];

                    int commentIndex = rawvalue.IndexOf('#');
                    if (commentIndex != -1)
                    {
                        rawvalue = rawvalue.Remove(commentIndex);
                    }

                    //Debug.WriteLine(rawvalue);

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

                        if (property is ConfigPropertyBoolean)
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

            foreach (string dir in Directory.EnumerateDirectories(ModsDir))
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
            string[] filePaths = Directory.GetFiles(dir, fileName, includeSubs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            return filePaths;
        }
    }
}
