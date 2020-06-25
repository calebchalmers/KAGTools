using GalaSoft.MvvmLight.Messaging;
using KAGTools.Helpers;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Serilog;
using Squirrel;
using System;
using System.IO;
using System.Windows;

namespace KAGTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Data.Settings Settings;

        private const ShortcutLocation ShortcutLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SettingsHelper.Save(Settings);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            using (UpdateManager mgr = new UpdateManager(""))
            {
                SquirrelAwareApp.HandleEvents(
                    onFirstRun: OnFirstRun,
                    onInitialInstall: (v) => OnAppInitialInstall(v, mgr),
                    onAppUpdate: (v) => OnAppUpdate(v, mgr),
                    onAppUninstall: (v) => OnAppUninstall(v, mgr));
            }
            // <-- App will exit here if a Squirrel event other than onFirstRun is triggered (via command line argument)

            SetupLogging();
            Settings = SettingsHelper.Load();

            if (string.IsNullOrEmpty(FileHelper.KagDir) || !Directory.Exists(FileHelper.KagDir))
            {
                // KAG Directory not specified or doesn't exist. Show dialog...
                if (!FindKagDirectory())
                {
                    Shutdown(0);
                    return;
                }
            }

            // Assign windows to viewmodels
            WindowHelper.Register(typeof(MainViewModel), typeof(MainWindow));
            WindowHelper.Register(typeof(ModsViewModel), typeof(ModsWindow));
            WindowHelper.Register(typeof(InputViewModel), typeof(InputWindow));
            WindowHelper.Register(typeof(ManualViewModel), typeof(ManualWindow));
            WindowHelper.Register(typeof(ApiViewModel), typeof(ApiWindow));

            // Listen for close window messages
            Messenger.Default.Register<CloseWindowMessage>(this, WindowHelper.OnCloseWindowMessage);

            // Open main window
            WindowHelper.OpenWindow(new MainViewModel());
        }

        #region Squirrel Events
        // Runs after install
        private static void OnFirstRun()
        {
            MessageBox.Show(AssemblyHelper.AppName + " was successfully installed.", "Install", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Runs before app launch
        private static void OnAppInitialInstall(Version version, UpdateManager mgr)
        {
            mgr.CreateShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations, false);
            mgr.CreateUninstallerRegistryEntry();
        }

        // Runs before app launch
        private static void OnAppUpdate(Version version, UpdateManager mgr)
        {
            mgr.CreateShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations, true);
            mgr.CreateUninstallerRegistryEntry();

            MessageBox.Show(string.Format("{0} version (v{1}) was successfully installed.", AssemblyHelper.AppName, AssemblyHelper.Version), "Update", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void OnAppUninstall(Version version, UpdateManager mgr)
        {
            mgr.RemoveShortcutsForExecutable(AssemblyHelper.AppFileName, ShortcutLocations);
            mgr.RemoveUninstallerRegistryEntry();

            MessageBox.Show(AssemblyHelper.AppName + " was successfully uninstalled.", "Uninstall", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        private void SetupLogging()
        {
            string outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

            try
            {
                File.Delete(FileHelper.LogPath);
            }
            catch (Exception) { } // It's not a problem if we can't delete the old log

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(FileHelper.LogPath, Serilog.Events.LogEventLevel.Information, outputTemplate)
                .CreateLogger();
        }

        private bool FindKagDirectory()
        {
            while (true) // keep asking until they give a valid directory or cancel
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    Title = "Select your King Arthur's Gold install directory",
                    IsFolderPicker = true,

                    AddToMostRecentlyUsedList = false,
                    AllowNonFileSystemItems = false,
                    EnsureFileExists = true,
                    EnsurePathExists = true,
                    EnsureReadOnly = false,
                    EnsureValidNames = true,
                    Multiselect = false,
                    ShowPlacesList = true
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string dir = dialog.FileName;
                    if (File.Exists(Path.Combine(dir, "KAG.exe"))) // check if folder contains KAG executable
                    {
                        Settings.KagDirectory = dir;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(
                            string.Format("KAG.exe was not found in '{0}'.{1}Please select your King Arthur's Gold install directory.", dir, Environment.NewLine),
                            "Invalid Directory",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        continue;
                    }
                }
                return false;
            }
        }
    }
}
