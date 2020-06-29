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

        public UserSettings UserSettings { get; }
        public ManualDocumentViewModel[] ManualDocumentViewModels { get; }
    }
}
