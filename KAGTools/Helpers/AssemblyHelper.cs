using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace KAGTools.Helpers
{
    public static class AssemblyHelper
    {
        public static FileVersionInfo _fileVersionInfo = null;

        static AssemblyHelper()
        {
            _fileVersionInfo = FileVersionInfo.GetVersionInfo(AppFilePath);
        }

        public static FileVersionInfo FileVersionInfo
        {
            get => _fileVersionInfo;
        }

        public static string AppName
        {
            get => FileVersionInfo.ProductName;
        }

        public static string AppFileName
        {
            get => Path.GetFileName(AppFilePath);
        }

        public static string AppFilePath
        {
            get => Assembly.GetEntryAssembly().Location;
        }

        public static string Version
        {
            get => FileVersionInfo.ProductVersion;
        }
    }
}
