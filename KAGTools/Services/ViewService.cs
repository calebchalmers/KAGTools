using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Services
{
    public class ViewService : IViewService
    {
        readonly Dictionary<Type, Type> _viewMap;
        readonly List<Window> _openedWindows;

        public ViewService()
        {
            _viewMap = new Dictionary<Type, Type>();
            _openedWindows = new List<Window>();
        }

        public void RegisterView(Type windowType, Type viewModelType)
        {
            lock (_viewMap)
            {
                if (_viewMap.ContainsKey(viewModelType))
                    throw new ArgumentException("ViewModel already registered");
                _viewMap[viewModelType] = windowType;
            }
        }

        [DebuggerStepThrough]
        public void OpenWindow(ViewModelBase viewModel)
        {
            // Create window for that view tabModel.
            Window window = CreateWindow(viewModel);

            // Open the window.
            window.Show();
        }

        [DebuggerStepThrough]
        public bool? OpenDialog(ViewModelBase viewModel)
        {
            // Create window for that view tabModel.
            Window window = CreateWindow(viewModel);

            // Open the window and return the result.
            return window.ShowDialog();
        }

        Window CreateWindow(ViewModelBase viewModel)
        {
            Type windowType;
            lock (_viewMap)
            {
                if (!_viewMap.ContainsKey(viewModel.GetType()))
                    throw new ArgumentException("viewModel not registered");
                windowType = _viewMap[viewModel.GetType()];
            }

            var window = (Window)Activator.CreateInstance(windowType);
            window.DataContext = viewModel;
            window.Closed += OnClosed;

            lock (_openedWindows)
            {
                // Last window opened is considered the 'owner' of the window.
                // May not be 100% correct in some situations but it is more
                // then good enough for handling dialog windows
                if (_openedWindows.Count > 0)
                {
                    Window lastOpened = _openedWindows[_openedWindows.Count - 1];

                    if (window != lastOpened)
                        window.Owner = lastOpened;
                }

                _openedWindows.Add(window);
            }

            // Listen for the close event
            Messenger.Default.Register<CloseWindowMessage>(window, viewModel, OnRequestClose);

            return window;
        }

        void OnRequestClose(CloseWindowMessage message)
        {
            var window = _openedWindows.SingleOrDefault(w => w.DataContext == message.ViewModel);
            if (window != null)
            {
                Messenger.Default.Unregister<CloseWindowMessage>(window, message.ViewModel, OnRequestClose);
                window.DialogResult = message.DialogResult;
                window.Close();
            }
        }

        void OnClosed(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Closed -= OnClosed;

            lock (_openedWindows)
            {
                _openedWindows.Remove(window);
            }
        }
    }
}
