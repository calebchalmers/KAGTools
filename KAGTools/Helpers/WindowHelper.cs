using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Helpers
{
    public static class WindowHelper
    {
        private static Dictionary<Type, Type> viewMap;
        private static List<Window> openWindows;

        static WindowHelper()
        {
            viewMap = new Dictionary<Type, Type>();
            openWindows = new List<Window>();
        }

        public static void Register(Type viewModelType, Type windowType)
        {
            if(viewMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel already registered.");
            }

            viewMap.Add(viewModelType, windowType);
        }

        public static void OpenWindow(ViewModelBase viewModel)
        {
            Window window = CreateWindow(viewModel);
            window.Show();
        }

        public static bool? OpenDialog(ViewModelBase viewModel)
        {
            Window window = CreateWindow(viewModel);
            return window.ShowDialog();
        }

        private static Window CreateWindow(ViewModelBase viewModel)
        {
            Type viewModelType = viewModel.GetType();

            if (!viewMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel not registered.");
            }

            Type windowType = viewMap[viewModelType];
            Window window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;
            window.Owner = openWindows.ElementAtOrDefault(0); // Owner is the first opened window
            window.Closed += (s, e) => openWindows.Remove((Window)s);
            openWindows.Add(window);

            return window;
        }

        public static void OnCloseWindowMessage(CloseWindowMessage msg)
        {
            Window targetWindow = openWindows.FirstOrDefault(w => w.DataContext == msg.ViewModel);

            if(targetWindow != null)
            {
                targetWindow.DialogResult = msg.DialogResult;
                targetWindow.Close();
            }
        }
    }
}
