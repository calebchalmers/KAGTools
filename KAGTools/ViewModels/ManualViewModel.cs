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
            set => Set(ref UserSettings.ManualWindowTop, value);
        }

        public double WindowLeft
        {
            get => UserSettings.ManualWindowLeft;
            set => Set(ref UserSettings.ManualWindowLeft, value);
        }

        public double WindowWidth
        {
            get => UserSettings.ManualWindowWidth;
            set => Set(ref UserSettings.ManualWindowWidth, value);
        }

        public double WindowHeight
        {
            get => UserSettings.ManualWindowHeight;
            set => Set(ref UserSettings.ManualWindowHeight, value);
        }
    }
}
