using KAGTools.Helpers;
using KAGTools.Services;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Squirrel;
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
        private const ShortcutLocation ShortcutLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;
        private static KAGTools.Properties.Settings Settings = KAGTools.Properties.Settings.Default;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.FirstRun = false;
            Settings.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SquirrelAwareApp.HandleEvents(                    
                onFirstRun: OnFirstRun,
                onInitialInstall: (v) => OnAppInitialInstall(v, null),
                onAppUpdate: (v) => OnAppUpdate(v, null),
                onAppUninstall: (v) => OnAppUninstall(v, null));

            if (!UpdateHelper.RestoreSettings())
            {
                MessageBoxHelper.Error("There was an error restoring settings.");
            }

            /*if (Settings.UpgradeRequired)
            {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
                Settings.Save();
            }*/

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
            viewService.RegisterView(typeof(ManualWindow), typeof(ManualViewModel));
            viewService.RegisterView(typeof(ApiWindow), typeof(ApiViewModel));

            ServiceManager.GetService<IViewService>().OpenWindow(new MainViewModel());
        }

        #region Squirrel Events
        private static void OnFirstRun()
        {
            System.Diagnostics.Debug.WriteLine("OnFirstRun");
            MessageBoxHelper.Info("Install successful.");
        }

        private static void OnAppInitialInstall(Version version, UpdateManager mgr)
        {
            System.Diagnostics.Debug.WriteLine("OnAppInitialInstall");
            mgr.CreateShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations, false);
            mgr.CreateUninstallerRegistryEntry();
        }

        private static void OnAppUpdate(Version version, UpdateManager mgr)
        {
            System.Diagnostics.Debug.WriteLine("OnAppUpdate");
            mgr.CreateShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations, true);
            mgr.CreateUninstallerRegistryEntry();

            MessageBoxHelper.Info(string.Format("New version (v{0}) installed.", AssemblyHelper.FileVersionInfo.ProductVersion));
        }

        private static void OnAppUninstall(Version version, UpdateManager mgr)
        {
            System.Diagnostics.Debug.WriteLine("OnAppUninstall");
            mgr.RemoveShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations);
            mgr.RemoveUninstallerRegistryEntry();

            MessageBoxHelper.Info("Uninstall successful.");
        }
        #endregion

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
