using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualEnumsViewModel : ManualGenericViewModel
    {
        public ManualEnumsViewModel() : base(FileHelper.ManualEnumsPath, true)
        {
        }
    }
}
