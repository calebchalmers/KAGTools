using GalaSoft.MvvmLight;
using System;
using System.Runtime.CompilerServices;

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
