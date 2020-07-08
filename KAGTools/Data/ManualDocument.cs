namespace KAGTools.Data
{
    public class ManualDocument
    {
        public string Name { get; }
        public string Path { get; }
        public bool HasTypes { get; }

        public ManualDocument(string name, string path, bool hasTypes = false)
        {
            Name = name;
            Path = path;
            HasTypes = hasTypes;
        }
    }
}
