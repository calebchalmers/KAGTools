using Newtonsoft.Json;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;

namespace KAGTools.Data
{
    public class UserSettings
    {
        // Application
        [DefaultValue("")]
        public string KagDirectory;

        [DefaultValue(false)]
        public bool UsePreReleases;

        // Main window
        [DefaultValue(double.NaN)]
        public double Left;

        [DefaultValue(double.NaN)]
        public double Top;

        [DefaultValue(0)]
        public int RunTypeIndex;

        // Manual window
        [DefaultValue(500)]
        public double ManualWindowWidth;

        [DefaultValue(600)]
        public double ManualWindowHeight;

        [DefaultValue(double.NaN)]
        public double ManualWindowTop;

        [DefaultValue(double.NaN)]
        public double ManualWindowLeft;
    }
}
