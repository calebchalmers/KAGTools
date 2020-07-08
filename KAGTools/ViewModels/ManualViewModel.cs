using GalaSoft.MvvmLight;
using KAGTools.Data;
using KAGTools.Services;
using System.Linq;

namespace KAGTools.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        private UserSettings UserSettings { get; }

        public ManualViewModel(UserSettings userSettings, WindowService windowService, ManualService manualService)
        {
            UserSettings = userSettings;

            ManualDocumentViewModels = manualService.ManualDocuments.Select(
                document => new ManualDocumentViewModel(document, windowService, manualService)
            ).ToArray();
        }

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
