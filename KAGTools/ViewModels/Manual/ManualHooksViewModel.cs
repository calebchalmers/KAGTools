using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualHooksViewModel : ManualGenericViewModel
    {
        public ManualHooksViewModel() : base(FileHelper.ManualHooksPath, false)
        {
        }
    }
}
