using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualObjectsViewModel : ManualGenericViewModel
    {
        public ManualObjectsViewModel() : base(FileHelper.ManualObjectsPath, true)
        {
        }
    }
}
