using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualVariablesViewModel : ManualGenericViewModel
    {
        public ManualVariablesViewModel() : base(FileHelper.ManualVariablesPath, false)
        {
        }
    }
}
