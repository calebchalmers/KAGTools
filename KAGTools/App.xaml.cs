﻿using GalaSoft.MvvmLight.Messaging;
using KAGTools.Data;
using KAGTools.Helpers;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using Squirrel;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace KAGTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Data.UserSettings UserSettings;

        private const ShortcutLocation ShortcutLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UserSettings.Save(FileHelper.SettingsPath);
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
            UserSettings = Data.UserSettings.Load(FileHelper.SettingsPath);

            if (!EnsureValidKagDirectory())
            {
                Log.Information("No valid KAG directory chosen. Shutting down!");
                Shutdown(0);
                return;
            }

            FileHelper.KagDir = UserSettings.KagDirectory;

            // Assign windows to viewmodels
            WindowHelper.Register(typeof(MainViewModel), typeof(MainWindow));
            WindowHelper.Register(typeof(ModsViewModel), typeof(ModsWindow));
            WindowHelper.Register(typeof(InputViewModel), typeof(InputWindow));
            WindowHelper.Register(typeof(ManualViewModel), typeof(ManualWindow));
            WindowHelper.Register(typeof(ApiViewModel), typeof(ApiWindow));

            // Listen for close window messages
            Messenger.Default.Register<CloseWindowMessage>(this, WindowHelper.OnCloseWindowMessage);

            //TEMPORARY
            var manualDocuments = new ManualDocument[]
            {
                new ManualDocument("Objects", true, FileHelper.GetManualFunctions(FileHelper.ManualObjectsPath, true), () => Process.Start(FileHelper.ManualObjectsPath)),
                new ManualDocument("Functions", false, FileHelper.GetManualFunctions(FileHelper.ManualFunctionsPath), () => Process.Start(FileHelper.ManualFunctionsPath)),
                new ManualDocument("Hooks", false, FileHelper.GetManualFunctions(FileHelper.ManualHooksPath), () => Process.Start(FileHelper.ManualHooksPath)),
                new ManualDocument("Enums", true, FileHelper.GetManualFunctions(FileHelper.ManualEnumsPath, true), () => Process.Start(FileHelper.ManualEnumsPath)),
                new ManualDocument("Variables", false, FileHelper.GetManualFunctions(FileHelper.ManualVariablesPath), () => Process.Start(FileHelper.ManualVariablesPath)),
                new ManualDocument("TypeDefs", false, FileHelper.GetManualFunctions(FileHelper.ManualTypeDefsPath), () => Process.Start(FileHelper.ManualTypeDefsPath))
            };

            // Open main window
            WindowHelper.OpenWindow(new MainViewModel(UserSettings, manualDocuments));

            // Run auto-updater in the background
            Task.Run(() =>
                UpdateHelper.AutoUpdate(
                    ConfigurationManager.AppSettings["UpdateUrl"],
                    RequestUpdatePermission,
                    UserSettings.UsePreReleases
                )
            );
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unhandled exception occured");
            MessageBox.Show(string.Format("An unhandled exception occured in {0}.{1}Check \"{2}\" for more information.", AssemblyHelper.AppName, Environment.NewLine, Path.GetFullPath(FileHelper.LogPath)), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true;
            Environment.Exit(1);
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
            string outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:j}{NewLine}{Exception}";

            try
            {
                File.Delete(FileHelper.LogPath);
            }
            catch (Exception) { } // It's not a problem if we can't delete the old log

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(FileHelper.LogPath, Serilog.Events.LogEventLevel.Information, outputTemplate)
                .CreateLogger();
        }

        private bool RequestUpdatePermission(ReleaseEntry updateRelease)
        {
            return MessageBox.Show(
                $"An update is available (v{updateRelease.Version})." + Environment.NewLine +
                "Would you like to install it?",
                AssemblyHelper.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes
            ) == MessageBoxResult.Yes;
        }

        private bool EnsureValidKagDirectory()
        {
            string dir = UserSettings.KagDirectory;

            // Keep asking until they give a valid directory or cancel
            while (true)
            {
                if(!string.IsNullOrEmpty(dir))
                {
                    // Check if folder contains KAG executable
                    if (File.Exists(Path.Combine(dir, "KAG.exe")))
                    {
                        Log.Information("KAG install folder is valid: {KagDirectory}", dir);
                        UserSettings.KagDirectory = dir;
                        return true;
                    }
                    else
                    {
                        Log.Information("Could not find KAG.exe in specified install folder: {KagDirectory}", dir);

                        MessageBox.Show(
                            $"KAG.exe was not found in '{dir}'." + Environment.NewLine +
                            "Please select your King Arthur's Gold install folder.",
                            "Invalid Folder",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                else
                {
                    Log.Information("KAG install folder is unspecified");
                }

                Log.Information("Showing KAG install folder dialog");

                var dialog = new CommonOpenFileDialog
                {
                    Title = "Select your King Arthur's Gold install folder",
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
                    dir = dialog.FileName;
                }
                else
                {
                    // User cancelled
                    return false;
                }
            }
        }
    }
}
