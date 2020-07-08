using KAGTools.Data;
using KAGTools.Helpers;
using KAGTools.Services;
using KAGTools.ViewModels;
using KAGTools.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using Squirrel;
using System;
using System.Configuration;
using System.IO;
using System.Text;
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
        private const ShortcutLocation ShortcutLocations = ShortcutLocation.StartMenu | ShortcutLocation.Desktop;

        private string AppLogPath = Path.GetFullPath(@"..\common\log.txt");
        private string AppUserSettingsPath = Path.GetFullPath(@"..\common\usersettings.json");

        private static JsonSettingsService<UserSettings> UserSettingsService;
        private UserSettings UserSettings { get => UserSettingsService.Settings; }
        private FileLocations FileLocations { get; set; }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UserSettingsService.Save();
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

            SetupLogging(AppLogPath);

            UserSettingsService = new JsonSettingsService<UserSettings>(AppUserSettingsPath);
            UserSettingsService.Load();

            if (!EnsureValidKagDirectory())
            {
                Log.Information("No valid KAG directory chosen. Shutting down!");
                Shutdown(0);
                return;
            }

            InitializeFileLocations(UserSettings.KagDirectory);

            // Initialize services
            var configService = new ConfigService();
            var modsService = new ModsService(FileLocations.ModsDirectory, FileLocations.ModsConfigPath);
            var manualService = new ManualService(FileLocations.ManualDirectory);
            var testService = new TestService(FileLocations);
            var windowService = new WindowService();

            // Assign windows to viewmodels
            windowService.Register<MainWindow, MainViewModel>(() => new MainViewModel(UserSettings, FileLocations, windowService, configService, modsService, testService));
            windowService.Register<ModsWindow, ModsViewModel>(() => new ModsViewModel(windowService, configService, modsService));
            windowService.Register<ManualWindow, ManualViewModel>(() => new ManualViewModel(UserSettings, windowService, manualService));
            windowService.Register<ApiWindow, ApiViewModel>(() => new ApiViewModel());

            // Open main window
            windowService.OpenWindow<MainViewModel>();

            // Run auto-updater in the background
            Task.Run(async () =>
            {
                var autoUpdateService = new AutoUpdateService(
                    ConfigurationManager.AppSettings["UpdateUrl"], 
                    UserSettings.UsePreReleases
                );

                bool newUpdate = await autoUpdateService.AutoUpdate(RequestUpdatePermission);

                // Shutdown and restart if an update was installed
                if (newUpdate)
                {
                    Dispatcher.Invoke(() => Shutdown(0));
                }
            });
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unhandled exception occured");
            AlertError("An unhandled exception occured.");

            e.Handled = true;
            Shutdown(1);
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

        private void AlertError(string message)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(message);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"Check \"{AppLogPath}\" for more information.");

            MessageBox.Show(
                messageBuilder.ToString(),
                AssemblyHelper.AppName + " Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void SetupLogging(string logPath)
        {
            string outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:j}{NewLine}{Exception}";

            try
            {
                File.Delete(logPath);
            }
            catch (Exception) { } // It's not a problem if we can't delete the old log

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, Serilog.Events.LogEventLevel.Information, outputTemplate)
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
                if (!string.IsNullOrEmpty(dir))
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

        private void InitializeFileLocations(string kagDirectory)
        {
            FileLocations = new FileLocations
            {
                KagDirectory = kagDirectory,
                ModsDirectory = Path.Combine(kagDirectory, "Mods"),
                ManualDirectory = Path.Combine(kagDirectory, "Manual", "interface"),
                AutoConfigPath = Path.Combine(kagDirectory, "autoconfig.cfg"),
                StartupConfigPath = Path.Combine(kagDirectory, "startup_config.cfg"),
                ModsConfigPath = Path.Combine(kagDirectory, "mods.cfg"),
                KagExecutablePath = Path.Combine(kagDirectory, "KAG.exe"),

                SoloAutoStartScriptPath = Path.GetFullPath(@"Resources\solo_autostart.as"),
                ClientAutoStartScriptPath = Path.GetFullPath(@"Resources\client_autostart.as"),
                ServerAutoStartScriptPath = Path.GetFullPath(@"Resources\server_autostart.as")
            };
        }
    }
}
