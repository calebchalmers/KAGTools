using KAGTools.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KAGTools.Services
{
    public class ModsService
    {
        public string ModsDirectory { get; set; }
        public string ModsConfigPath { get; set; }

        public ModsService(string modsDirectory, string modsConfigPath)
        {
            ModsDirectory = modsDirectory;
            ModsConfigPath = modsConfigPath;
        }

        public IEnumerable<Mod> EnumerateAllMods()
        {
            IEnumerable<string> modDirectories;

            try
            {
                modDirectories = Directory.EnumerateDirectories(ModsDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to enumerate mods folder");
                yield break;
            }

            List<string> activeModNames = ReadActiveModNames().ToList();

            foreach (string modDirectory in modDirectories)
            {
                string modName = new DirectoryInfo(modDirectory).Name;
                bool modIsActive = false;

                // If this mod is active, mark it as such and remove it from the list to improve efficiency
                if (activeModNames.Remove(modName))
                {
                    modIsActive = true;
                }

                yield return new Mod(modName, modDirectory, modIsActive);
            }
        }

        public IEnumerable<Mod> EnumerateActiveMods()
        {
            IEnumerable<string> activeModNames = ReadActiveModNames();

            foreach (string modName in activeModNames)
            {
                string modDirectory = Path.Combine(ModsDirectory, modName);

                if (Directory.Exists(modDirectory))
                {
                    yield return new Mod(modName, modDirectory, true);
                }
            }
        }

        private IEnumerable<string> ReadActiveModNames()
        {
            if (!File.Exists(ModsConfigPath))
            {
                Log.Error("Could not find mods config file");
                yield break;
            }

            string line;
            using (StreamReader reader = new StreamReader(ModsConfigPath))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        yield return line;
                    }
                }
            }
        }

        public bool WriteActiveMods(IEnumerable<Mod> activeMods)
        {
            try
            {
                string[] comments = File.ReadAllLines(ModsConfigPath).Where((line) => line.StartsWith("#")).ToArray();
                List<string> lines = new List<string>(comments);
                lines.AddRange(activeMods.Select(mod => mod.Name));
                File.WriteAllLines(ModsConfigPath, lines.ToArray());
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update mods config");
                return false;
            }
        }
    }
}
