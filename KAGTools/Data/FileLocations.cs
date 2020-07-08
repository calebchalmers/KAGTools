namespace KAGTools.Data
{
    public class FileLocations
    {
        // King Arthur's Gold
        public string KagDirectory { get; set; }
        public string ModsDirectory { get; set; }
        public string ManualDirectory { get; set; }
        public string AutoConfigPath { get; set; }
        public string StartupConfigPath { get; set; }
        public string ModsConfigPath { get; set; }
        public string KagExecutablePath { get; set; }

        // Auto-start scripts
        public string SoloAutoStartScriptPath { get; set; }
        public string ClientAutoStartScriptPath { get; set; }
        public string ServerAutoStartScriptPath { get; set; }
    }
}
