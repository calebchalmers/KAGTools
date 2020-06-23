using System.ComponentModel;

namespace KAGTools.Data
{
    public class Settings
    {
        [DefaultValue("")]
        public string KagDirectory { get; set; }

        [DefaultValue(false)]
        public bool UsePreReleases { get; set; }

        // Main window
        [DefaultValue(double.NaN)]
        public double Left { get; set; }

        [DefaultValue(double.NaN)]
        public double Top { get; set; }

        [DefaultValue(0)]
        public int RunTypeIndex { get; set; }

        // Manual window
        [DefaultValue(500)]
        public double ManualWindowWidth { get; set; }

        [DefaultValue(600)]
        public double ManualWindowHeight { get; set; }

        [DefaultValue(double.NaN)]
        public double ManualWindowTop { get; set; }

        [DefaultValue(double.NaN)]
        public double ManualWindowLeft { get; set; }
    }
}
