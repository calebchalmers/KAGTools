using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
