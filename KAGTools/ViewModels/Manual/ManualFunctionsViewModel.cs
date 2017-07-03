using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
