using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools
{
    public static class Extensions
    {
        public static void SetProperty<T>(this ViewModelBase viewModel, ref T field, T value, Action setAction = null, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(field, value)) return;
            field = value;
            viewModel.RaisePropertyChanged(propertyName);
            setAction?.Invoke();
        }
    }
}
