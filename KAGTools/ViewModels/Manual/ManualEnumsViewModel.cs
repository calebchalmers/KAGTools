using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
