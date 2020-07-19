using KAGTools.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KAGTools.Services
{
    public class ModsService : IModsService
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
            if (!Directory.Exists(ModsDirectory))
            {
                Log.Warning("Could not find mods folder");
                yield break;
            }

            IEnumerable<string> modDirectories;

            try
            {
                modDirectories = Directory.EnumerateDirectories(ModsDirectory);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                Log.Error(ex, "Failed to enumerate mods folder");
                yield break;
            }

            List<string> activeModNames = ReadActiveModNames();

            foreach (string modDirectory in modDirectories)
            {
                string modName = new DirectoryInfo(modDirectory).Name;
                bool? modIsActive = activeModNames?.Remove(modName);

                yield return new Mod(modName, modDirectory, modIsActive);
            }
        }

        public IEnumerable<Mod> EnumerateActiveMods()
        {
            List<string> activeModNames = ReadActiveModNames();

            if(activeModNames == null)
            {
                yield break;
            }

            foreach (string modName in activeModNames)
            {
                string modDirectory = Path.Combine(ModsDirectory, modName);

                if (Directory.Exists(modDirectory))
                {
                    yield return new Mod(modName, modDirectory, true);
                }
            }
        }

        private List<string> ReadActiveModNames()
        {
            if (!File.Exists(ModsConfigPath))
            {
                Log.Error("Could not find mods config file");
                return null;
            }

            List<string> activeModNames = new List<string>();

            try
            {
                string line;
                using (StreamReader reader = new StreamReader(ModsConfigPath))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!line.StartsWith("#"))
                        {
                            activeModNames.Add(line);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Log.Error(ex, "Failed to read mods config file");
                return null;
            }

            return activeModNames;
        }

        public bool WriteActiveMods(IEnumerable<Mod> activeMods)
        {
            if(!File.Exists(ModsConfigPath))
            {
                Log.Error("Could not find mods config file");
                return false;
            }

            try
            {
                var comments = File.ReadAllLines(ModsConfigPath).Where(line => line.StartsWith("#"));
                var lines = comments.Concat(activeMods.Select(mod => mod.Name));
                File.WriteAllLines(ModsConfigPath, lines);
                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                Log.Error(ex, "Failed to update mods config");
                return false;
            }
        }

        public Mod CreateNewMod(string name)
        {
            try
            {
                string directory = Path.Combine(ModsDirectory, name);
                Directory.CreateDirectory(directory);
                return new Mod(name, directory, true);
            }
            catch (Exception ex) when (ex is PathTooLongException || ex is UnauthorizedAccessException || ex is IOException)
            {
                Log.Error(ex, "Failed to create new mod: {Name}", name);
                return null;
            }
        }
    }
}
