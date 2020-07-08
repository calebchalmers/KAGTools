namespace KAGTools.Data
{
    public class Mod
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public bool? IsActive { get; set; }

        public Mod(string name, string directory, bool? isActive = false)
        {
            Name = name;
            Directory = directory;
            IsActive = isActive;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
