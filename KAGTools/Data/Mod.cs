using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KAGTools.Helpers;

namespace KAGTools.Data
{
    public class Mod
    {
        private string _gamemode = null;

        public string Name { get; set; }
        public string Directory { get; set; }
        public bool IsActive { get; set; }

        public string Gamemode
        {
            get
            {
                if(_gamemode == null)
                {
                    EvaluateGamemode();
                }

                return _gamemode;
            }
        }

        public Mod(string dir, bool isActive = false)
        {
            Directory = dir;
            Name = new DirectoryInfo(dir).Name;
            IsActive = isActive;
        }

        public override string ToString()
        {
            return Name;
        }

        public void EvaluateGamemode()
        {
            _gamemode = "N/A";

            string gamemodeConfigPath = FileHelper.FindFirstFile(Directory, "gamemode.cfg");
            if (gamemodeConfigPath != null)
            {
                var gamemodeProperty = new StringConfigProperty("gamemode_name", null);
                FileHelper.ReadConfigProperties(gamemodeConfigPath, gamemodeProperty);

                if (!string.IsNullOrEmpty(gamemodeProperty.Value))
                {
                    _gamemode = gamemodeProperty.Value;
                }
            }
        }
    }
}
