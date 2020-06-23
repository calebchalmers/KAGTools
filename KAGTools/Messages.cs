using GalaSoft.MvvmLight;

namespace KAGTools
{
    public class FocusSelectedItemMessage
    {

    }

    public class CloseWindowMessage
    {
        public CloseWindowMessage(ViewModelBase viewModel, bool? dialogResult = null)
        {
            ViewModel = viewModel;
            DialogResult = dialogResult;
        }

        public ViewModelBase ViewModel { get; set; }
        public bool? DialogResult { get; set; }
    }
}
