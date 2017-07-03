using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
