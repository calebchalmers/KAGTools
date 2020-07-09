using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace KAGTools.Services
{
    public class WindowService : IWindowService
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

            if (!forceNew)
            {
                var existingWindow = FindWindowByViewModel<TViewModel>();

                // If there is an existing window and forceNew is false, focus the window and return it's view model
                if (existingWindow != null)
                {
                    if (existingWindow.WindowState == WindowState.Minimized)
                    {
                        existingWindow.WindowState = WindowState.Normal;
                    }

                    existingWindow.Activate();
                    return (TViewModel)existingWindow.DataContext;
                }
            }

            // If no window was found or forceNew is true, create a new window and view model
            TViewModel viewModel = (TViewModel)info.ViewModelCreator();

            Window window = CreateWindow(info.WindowType, forceNew);
            window.DataContext = viewModel;

            if (modal) window.ShowDialog();
            else window.Show();

            return viewModel;
        }

        private Window FindWindowByViewModel<TViewModel>()
            where TViewModel : ViewModelBase
        {
            return openWindows.LastOrDefault(w => w.DataContext is TViewModel);
        }

        private Window CreateWindow(Type windowType, bool forceNew = false)
        {
            // Create window and add view model to it
            Window window = (Window)Activator.CreateInstance(windowType);
            window.Owner = openWindows.FirstOrDefault(); // Owner is the first opened window
            window.Closed += (s, e) => openWindows.Remove((Window)s);
            openWindows.Add(window);

            return window;
        }

        /// <summary>
        /// Opens a file or folder in File Explorer.
        /// </summary>
        /// <param name="path">The file or folder to open.</param>
        public void OpenInExplorer(string path)
        {
            Process.Start("explorer.exe", $"\"{path}\"");
        }

        /// <summary>
        /// Show an alert to the user.
        /// </summary>
        /// <param name="message">The message shown.</param>
        /// <param name="title">An optional title.</param>
        /// <param name="error">Whether or not the alert represents an error notification.</param>
        public void Alert(string message, string title = null, bool error = false)
        {
            MessageBox.Show(
                message,
                title ?? AppInfo.Title,
                MessageBoxButton.OK,
                error ? MessageBoxImage.Error : MessageBoxImage.Information,
                MessageBoxResult.OK,
                MessageBoxOptions.None
            );
        }
    }
}
