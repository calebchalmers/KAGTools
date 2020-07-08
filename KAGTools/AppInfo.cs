using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace KAGTools
{
    public static class AppInfo
    {
        public static string ExeName { get; }
        public static string Title { get; }
        public static string Version { get; }

        static AppInfo()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            ExeName = Path.GetFileName(exePath);

            var fileVersionInfo = FileVersionInfo.GetVersionInfo(exePath);
            Title = fileVersionInfo.ProductName;
            Version = fileVersionInfo.FileVersion;
        }
    }
}
