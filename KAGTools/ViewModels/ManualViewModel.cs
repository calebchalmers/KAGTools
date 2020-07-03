using GalaSoft.MvvmLight;
using KAGTools.Data;
using System.Linq;

namespace KAGTools.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        public ManualViewModel(UserSettings userSettings, ManualDocument[] manualDocuments)
        {
            UserSettings = userSettings;
            ManualDocumentViewModels = manualDocuments.Select(doc => new ManualDocumentViewModel(doc)).ToArray();
        }

        private UserSettings UserSettings { get; }
        public ManualDocumentViewModel[] ManualDocumentViewModels { get; }

        public double WindowTop
        {
            get => UserSettings.ManualWindowTop;
            set => this.SetProperty(ref UserSettings.ManualWindowTop, value);
        }

        public double WindowLeft
        {
            get => UserSettings.ManualWindowLeft;
            set => this.SetProperty(ref UserSettings.ManualWindowLeft, value);
        }

        public double WindowWidth
        {
            get => UserSettings.ManualWindowWidth;
            set => this.SetProperty(ref UserSettings.ManualWindowWidth, value);
        }

        public double WindowHeight
        {
            get => UserSettings.ManualWindowHeight;
            set => this.SetProperty(ref UserSettings.ManualWindowHeight, value);
        }
    }
}
