using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Services
{
    public interface IViewService : IService
    {
        void RegisterView(Type windowType, Type viewModelType);

        void OpenWindow(ViewModelBase viewModel);
        bool? OpenDialog(ViewModelBase viewModel);
    }
}
