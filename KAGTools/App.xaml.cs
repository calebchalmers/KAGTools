﻿using KAGTools.Helpers;
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
            Settings.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            if(!File.Exists(Path.Combine(Path.GetDirectoryName(AssemblyHelper.AppFilePath), "../Update.exe")))
            {
                MessageBox.Show(
                    string.Format(
                    "The installer for KAG Tools has been replaced.{0}" +
                    "To continue using the app and receiving future updates, please manually install the latest version online and uninstall this one.{0}{0}" +
                    "Sorry for the inconvenience!{0}" +
                    "You will be redirected to the download page.", 
                    Environment.NewLine), 
                    "KAG Tools",
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);

                // Open download page and close the app so people have to update
                System.Diagnostics.Process.Start("https://forum.thd.vg/resources/open-source-kag-tools.478/");
                
                Shutdown(0);
                return;
            }

            using (UpdateManager mgr = new UpdateManager(""))
            {
                SquirrelAwareApp.HandleEvents(
                    onFirstRun: OnFirstRun,
                    onInitialInstall: (v) => OnAppInitialInstall(v, mgr),
                    onAppUpdate: (v) => OnAppUpdate(v, mgr),
                    onAppUninstall: (v) => OnAppUninstall(v, mgr));
            }
#endif

            if (!UpdateHelper.RestoreSettings())
            {
                MessageBox.Show("There was an error restoring settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!Directory.Exists(Settings.KagDirectory))
            {
                // KAG Directory not specified or doesn't exist. Show dialog...
                if (!FindKagDirectory())
                {
                    Shutdown(0);
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

        private bool FindKagDirectory()
        {
            while(true) // keep asking until they give a valid directory or cancel
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
                    if (File.Exists(Path.Combine(dir, FileHelper.KAGExecutablePath))) // check if folder contains KAG executable
                    {
                        Settings.KagDirectory = dialog.FileName;
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
