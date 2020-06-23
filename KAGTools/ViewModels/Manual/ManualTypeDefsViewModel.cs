using KAGTools.Helpers;

namespace KAGTools.ViewModels.Manual
{
    public class ManualTypeDefsViewModel : ManualGenericViewModel
    {
        public ManualTypeDefsViewModel() : base(FileHelper.ManualTypeDefsPath, false)
        {
        }
    }
}
