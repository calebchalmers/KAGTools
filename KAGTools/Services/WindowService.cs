using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using KAGTools.ViewModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Services
{
    public class WindowService
    {
        Dictionary<Type, WindowCreationInfo> infoMap;
        private static List<Window> openWindows;

        private class WindowCreationInfo
        {
            public Type WindowType { get; set; }
            public Func<ViewModelBase> ViewModelCreator { get; set; }
        }

        public WindowService()
        {
            infoMap = new Dictionary<Type, WindowCreationInfo>();
            openWindows = new List<Window>();
        }

        /// <summary>
        /// Register a window type to a view model type.
        /// </summary>
        /// <typeparam name="TWindow">The type of widow.</typeparam>
        /// <typeparam name="TViewModel">The type of view model.</typeparam>
        /// <param name="viewModelCreator">Called when a new view model of this type is required.</param>
        public void Register<TWindow, TViewModel>(Func<TViewModel> viewModelCreator)
            where TWindow : Window
            where TViewModel : ViewModelBase
        {
            Type windowType = typeof(TWindow);
            Type viewModelType = typeof(TViewModel);

            if (infoMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel already registered.");
            }

            infoMap.Add(viewModelType, new WindowCreationInfo
            {
                WindowType = windowType,
                ViewModelCreator = viewModelCreator
            });
        }

        /// <summary>
        /// Opens a new window associated with view model type TViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">The view model type registered with the desired window type.</typeparam>
        /// <param name="modal">Wait for the window to close before returning.</param>
        /// <param name="forceNew">Force a new window to open even if there is one already open of the same type.</param>
        /// <returns>The newly created view model.</returns>
        public TViewModel OpenWindow<TViewModel>(bool modal = false, bool forceNew = false)
            where TViewModel : ViewModelBase
        {
            Type viewModelType = typeof(TViewModel);

            if (!infoMap.ContainsKey(viewModelType))
            {
                throw new ArgumentException("ViewModel not registered.");
            }

            WindowCreationInfo info = infoMap[viewModelType];

            TViewModel viewModel = (TViewModel)info.ViewModelCreator();

            Window window = CreateOrFocusWindow(info.WindowType, forceNew);
            window.DataContext = viewModel;

            if (modal) window.ShowDialog();
            else window.Show();

            return viewModel;
        }

        private Window CreateOrFocusWindow(Type windowType, bool forceNew = false)
        {
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

            // Create window and add view model to it
            Window window = (Window)Activator.CreateInstance(windowType);
            window.Owner = openWindows.ElementAtOrDefault(0); // Owner is the first opened window
            window.Closed += (s, e) => openWindows.Remove((Window)s);
            openWindows.Add(window);

            return window;
        }
    }
}
