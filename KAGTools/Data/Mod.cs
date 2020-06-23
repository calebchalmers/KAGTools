using KAGTools.Helpers;
using System.IO;

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
                if (_gamemode == null)
                {
                    _gamemode = FileHelper.FindGamemodeOfMod(Directory);
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
    }
}
