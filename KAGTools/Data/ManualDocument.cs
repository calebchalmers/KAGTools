using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class ManualDocument
    {
        public string Name { get; }
        public bool HasTypes { get; }
        public List<ManualItem> Items { get; }
        public Action OpenSourceFile { get; }

        public ManualDocument(string name, bool hasTypes, List<ManualItem> items, Action openSourceFile)
        {
            Name = name;
            HasTypes = hasTypes;
            Items = items;
            OpenSourceFile = openSourceFile;
        }
    }
}
