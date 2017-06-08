using KAGTools.Services;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static KAGTools.Properties.Settings Settings = KAGTools.Properties.Settings.Default;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.FirstRun = false;
            Settings.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(Settings.UpgradeRequired)
            {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
                Settings.Save();
            }

            if (!Directory.Exists(Settings.KagDirectory))
            {
                // KAG Directory not specified or doesn't exist. Show dialog...
                if (!FindKagDirectory())
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            IViewService viewService = ServiceManager.RegisterService<IViewService>(new ViewService());
            viewService.RegisterView(typeof(MainWindow), typeof(MainViewModel));
            viewService.RegisterView(typeof(ModsWindow), typeof(ModsViewModel));
            viewService.RegisterView(typeof(InputWindow), typeof(InputViewModel));

            ServiceManager.GetService<IViewService>().OpenWindow(new MainViewModel());
        }

        private bool FindKagDirectory()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select your King Arthur's Gold install directory";
            dialog.IsFolderPicker = true;

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Settings.KagDirectory = dialog.FileName;
                return true;
            }
            return false;
        }
    }
}
