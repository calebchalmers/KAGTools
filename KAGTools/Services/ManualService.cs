using KAGTools.Data;
using Serilog;
using System.Collections.Generic;
using System.IO;

namespace KAGTools.Services
{
    public class ManualService : IManualService
    {
        private readonly int ManualHeaderLineCount = 3;
        private readonly char ManualIndentCharacter = '\t';

        public string ManualDirectory { get; set; }
        public ManualDocument[] ManualDocuments { get; set; }

        public ManualService(string manualDirectory)
        {
            ManualDirectory = manualDirectory;
            ManualDocuments = new ManualDocument[]
            {
                new ManualDocument("Objects", Path.Combine(ManualDirectory, "Objects.txt"), true),
                new ManualDocument("Functions", Path.Combine(ManualDirectory, "Functions.txt"), false),
                new ManualDocument("Hooks", Path.Combine(ManualDirectory, "Hooks.txt"), false),
                new ManualDocument("Enums", Path.Combine(ManualDirectory, "Enums.txt"), true),
                new ManualDocument("Variables", Path.Combine(ManualDirectory, "Variables.txt"), false),
                new ManualDocument("TypeDefs", Path.Combine(ManualDirectory, "TypeDefs.txt"), false)
            };
        }

        public IEnumerable<ManualItem> EnumerateManualDocument(ManualDocument document)
        {
            if (!File.Exists(document.Path))
            {
                Log.Error("Could not find manual file: {FileName}", document.Path);
                yield break;
            }

            using (StreamReader reader = new StreamReader(document.Path))
            {
                for (int i = 0; i < ManualHeaderLineCount; i++) // Remove manual header
                {
                    reader.ReadLine();
                }

                string lastType = null;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (line[0] == ManualIndentCharacter)
                    {
                        yield return new ManualItem(lastType, line.Substring(1));
                    }
                    else // Is a type declaration
                    {
                        if (document.HasTypes)
                        {
                            lastType = line;
                        }
                        else
                        {
                            yield return new ManualItem(lastType, line);
                        }
                    }
                }
            }
        }
    }
}
