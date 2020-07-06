using KAGTools.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KAGTools.Services
{
    public class ConfigService
    {
        public ConfigService()
        {

        }

        public bool ReadConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            Log.Information("Reading config file: {FilePath}", filePath);
            return ReadWriteConfigProperties(filePath, false, configProperties);
        }

        public bool WriteConfigProperties(string filePath, params BaseConfigProperty[] configProperties)
        {
            Log.Information("Writing config file: {FilePath}", filePath);
            return ReadWriteConfigProperties(filePath, true, configProperties);
        }

        private bool ReadWriteConfigProperties(string filePath, bool writePropertiesToFile, params BaseConfigProperty[] configProperties)
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

        public string FindGamemodeOfMod(Mod mod)
        {
            string gamemodeConfigPath;

            try
            {
                gamemodeConfigPath = Directory.EnumerateFiles(mod.Directory, "gamemode.cfg", SearchOption.AllDirectories).FirstOrDefault();
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
    }
}