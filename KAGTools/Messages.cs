using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
