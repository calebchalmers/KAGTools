using GalaSoft.MvvmLight.Messaging;
using KAGTools.Helpers;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
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

        #region Settings
        public static Data.Settings Settings;
        private static string ConfigPath = Path.Combine("..", "config");
        private static string SettingsFileName = "settings.json";
        private static string SettingsFilePath = Path.Combine(ConfigPath, SettingsFileName);
        private static JsonSerializerSettings SettingsSerializerSettings = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Populate,
            Formatting = Formatting.Indented,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double
        };

        public static void LoadSettings()
        {
            string json = "{}";
            if(File.Exists(SettingsFilePath))
            {
                json = File.ReadAllText(SettingsFilePath);
            }
            Settings = JsonConvert.DeserializeObject<Data.Settings>(json, SettingsSerializerSettings);
        }

        public static void SaveSettings()
        {
            Directory.CreateDirectory(ConfigPath);
            string json = JsonConvert.SerializeObject(Settings, SettingsSerializerSettings);
            File.WriteAllText(SettingsFilePath, json);
        }
        #endregion

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SaveSettings();
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

            LoadSettings();

            if (!Directory.Exists(Settings.KagDirectory))
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
