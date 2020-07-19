using KAGTools.Data;
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
        private string AppUpdateUrl = ConfigurationManager.AppSettings["UpdateUrl"];

        private IWindowService WindowService { get; set; }
        private IConfigService ConfigService { get; set; }
        private IModsService ModsService { get; set; }
        private IManualService ManualService { get; set; }
        private ITestService TestService { get; set; }
        private IApiService ApiService { get; set; }

        private JsonSettingsFile<UserSettings> UserSettingsFile;
        private UserSettings UserSettings { get => UserSettingsFile.Settings; }
        private FileLocations FileLocations { get; set; }
        private Task AutoUpdateTask = Task.CompletedTask;

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            UserSettingsFile.Save();
            AutoUpdateTask.Wait();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetupLogging(AppLogPath);

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: (v) => OnAppInitialInstall(v),
                onAppUpdate: (v) => OnAppUpdate(v),
                onAppUninstall: (v) => OnAppUninstall(v)
            );
            // <-- App will exit here if a Squirrel event other than onFirstRun is triggered (via command line argument)

            UserSettingsFile = new JsonSettingsFile<UserSettings>(AppUserSettingsPath);
            UserSettingsFile.Load();

            if (!EnsureValidKagDirectory())
            {
                Log.Information("No valid KAG directory chosen. Shutting down!");
                Shutdown(0);
                return;
            }

            InitializeFileLocations();
            InitializeServices();

            // Assign windows to viewmodels
            WindowService.Register<MainWindow, MainViewModel>(() => new MainViewModel(UserSettings, FileLocations, WindowService, ConfigService, ModsService, TestService));
            WindowService.Register<ModsWindow, ModsViewModel>(() => new ModsViewModel(WindowService, ConfigService, ModsService));
            WindowService.Register<ManualWindow, ManualViewModel>(() => new ManualViewModel(UserSettings, WindowService, ManualService));
            WindowService.Register<ApiWindow, ApiViewModel>(() => new ApiViewModel(ApiService));

            // Open main window
            WindowService.OpenWindow<MainViewModel>();

            // Run auto-updater in the background
            AutoUpdateTask = Task.Run(AutoUpdateAsync);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unhandled exception occured");
            AlertError("An unhandled exception occured.", true);

            e.Handled = true;
            Shutdown(1);
        }

        #region Squirrel Events
        private static void OnAppInitialInstall(Version version)
        {
            AlertInformation("App successfully installed.");
            ModifyAppShortcuts(false, false);
        }

        private static void OnAppUpdate(Version version)
        {
            AlertInformation($"App successfully updated to v{version.ToString(3)}.");
            ModifyAppShortcuts(false, true);
        }

        private static void OnAppUninstall(Version version)
        {
            AlertInformation("App successfully uninstalled.");
            ModifyAppShortcuts(true, false);
        }

        private static void ModifyAppShortcuts(bool remove, bool update)
        {
            using (UpdateManager updateManager = new UpdateManager(""))
            {
                try
                {
                    if (remove)
                    {
                        updateManager.RemoveShortcutsForExecutable(AppInfo.ExeName, ShortcutLocations);
                        updateManager.RemoveUninstallerRegistryEntry();
                    }
                    else
                    {
                        updateManager.CreateShortcutsForExecutable(AppInfo.ExeName, ShortcutLocations, update);
                        updateManager.CreateUninstallerRegistryEntry();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to modify app shortcuts");
                }
            }
        }
        #endregion

        private static void AlertInformation(string message)
        {
            MessageBox.Show(message, AppInfo.Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AlertError(string message, bool includeLogsMessage)
        {
            if (includeLogsMessage)
            {
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(message);
                messageBuilder.AppendLine();
                messageBuilder.AppendLine($"Check \"{AppLogPath}\" for more information.");
                message = messageBuilder.ToString();
            }

            MessageBox.Show(message, AppInfo.Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SetupLogging(string logPath)
        {
            string outputTemplate = "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:j}{NewLine}{Exception}";

            if(File.Exists(logPath))
            {
                try
                {
                    File.Delete(logPath);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to delete previous log file due to {0}", ex);
                }
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logPath, Serilog.Events.LogEventLevel.Information, outputTemplate)
                .CreateLogger();
        }

        private bool RequestUpdatePermission(ReleaseEntry updateRelease)
        {
            return MessageBox.Show(
                $"An update is available (v{updateRelease.Version})." + Environment.NewLine +
                "Would you like to install it?",
                AppInfo.Title,
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

                        var errorMessageBuilder = new StringBuilder();
                        errorMessageBuilder.AppendLine($"KAG.exe was not found in '{dir}'.");
                        errorMessageBuilder.AppendLine();
                        errorMessageBuilder.AppendLine("Please select your King Arthur's Gold install folder.");
                        AlertError(errorMessageBuilder.ToString(), false);
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

        private void InitializeFileLocations()
        {
            string kagDirectory = UserSettings.KagDirectory;

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

        private void InitializeServices()
        {
            WindowService = new WindowService();
            ConfigService = new ConfigService();

            ModsService = new ModsService(
                FileLocations.ModsDirectory,
                FileLocations.ModsConfigPath
            );

            ManualService = new ManualService(
                FileLocations.ManualDirectory
            );

            TestService = new TestService(
                FileLocations.KagExecutablePath,
                FileLocations.AutoConfigPath,
                FileLocations.SoloAutoStartScriptPath,
                FileLocations.ClientAutoStartScriptPath,
                FileLocations.ServerAutoStartScriptPath
            );

            ApiService = new ApiService(
                ConfigurationManager.AppSettings["ApiPlayerUrlTemplate"],
                ConfigurationManager.AppSettings["ApiPlayerAvatarUrlTemplate"],
                ConfigurationManager.AppSettings["ApiServerListUrlTemplate"],
                ConfigurationManager.AppSettings["ApiServerUrlTemplate"],
                ConfigurationManager.AppSettings["ApiServerMinimapUrlTemplate"]
            );
        }

        private async Task AutoUpdateAsync()
        {
            var autoUpdater = new AutoUpdater(
                AppUpdateUrl,
                UserSettings.UsePreReleases
            );

            bool newUpdate = await autoUpdater.AutoUpdate(RequestUpdatePermission);

            // Shutdown and restart if an update was installed
            if (newUpdate)
            {
                Dispatcher.Invoke(() => Shutdown(0));
            }
        }
    }
}
