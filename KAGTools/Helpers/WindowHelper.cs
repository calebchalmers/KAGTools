using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (viewMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel already registered.");
            }

            viewMap.Add(viewModelType, windowType);
        }

        public static void OpenWindow(ViewModelBase viewModel, bool forceNew = false)
        {
            Window window = CreateWindow(viewModel, forceNew);
            window.Show();
        }

        public static bool? OpenDialog(ViewModelBase viewModel)
        {
            Window window = CreateWindow(viewModel, true);
            return window.ShowDialog();
        }

        private static Window CreateWindow(ViewModelBase viewModel, bool forceNew)
        {
            Type viewModelType = viewModel.GetType();

            if (!viewMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel not registered.");
            }

            Type windowType = viewMap[viewModelType];

            if (!forceNew)
            {
                Window focusWindow = openWindows.LastOrDefault(w => w.GetType() == windowType);

                // Activate window and show if minimized
                if (focusWindow != null)
                {
                    if (focusWindow.WindowState == WindowState.Minimized)
                    {
                        focusWindow.WindowState = WindowState.Normal;
                    }

                    focusWindow.Activate();
                    return focusWindow;
                }
            }

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

            if (targetWindow != null)
            {
                targetWindow.DialogResult = msg.DialogResult;
                targetWindow.Close();
            }
        }
    }
}
