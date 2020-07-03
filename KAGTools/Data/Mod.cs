using System.IO;

namespace KAGTools.Data
{
    public class Mod
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public bool? IsActive { get; set; }

        public Mod(string dir, bool? isActive = false)
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
