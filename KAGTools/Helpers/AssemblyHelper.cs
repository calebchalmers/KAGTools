using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Helpers
{
    public static class AssemblyHelper
    {
        public static FileVersionInfo _fileVersionInfo = null;

        public static FileVersionInfo FileVersionInfo
        {
            get
            {
                if(_fileVersionInfo == null)
                {
                    _fileVersionInfo = FileVersionInfo.GetVersionInfo(AppFilePath);
                }
                return _fileVersionInfo;
            }
        }

        public static string AppName
        {
            get
            {
                return FileVersionInfo.ProductName;
            }
        }

        public static string AppFileName
        {
            get
            {
                return Path.GetFileName(AppFilePath);
            }
        }

        public static string AppFilePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }
    }
}
