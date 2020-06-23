using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualFunctionsViewModel : ManualGenericViewModel
    {
        public ManualFunctionsViewModel() : base(FileHelper.ManualFunctionsPath, false)
        {
        }
    }
}
