using System.ComponentModel;

namespace KAGTools.Data
{
    public class UserSettings
    {
        // Application
        [DefaultValue("")]
        public string KagDirectory;

        [DefaultValue(false)]
        public bool UsePreReleases;

        [DefaultValue(true)]
        public bool SyncClientServerClosing;

        // Main window
        [DefaultValue(double.NaN)]
        public double MainWindowLeft;

        [DefaultValue(double.NaN)]
        public double MainWindowTop;

        [DefaultValue(0)]
        public int TestType;

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
