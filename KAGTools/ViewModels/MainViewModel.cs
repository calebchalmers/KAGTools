using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KAGTools.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly string[] DEFAULT_GAMEMODES =
        {
            "CTF",
            "TDM",
            "Sandbox",
            "WAR",
            "Challenge",
        };

        private string _gamemode;
        private int _screenWidth;
        private int _screenHeight;
        private bool _fullscreen;
        private ObservableCollection<string> _gamemodes;
        private bool _startupOptionsEnabled;
        private bool _gamemodeOptionEnabled;

        private UserSettings UserSettings { get; }
        private FileLocations FileLocations { get; }
        private IWindowService WindowService { get; }
        private IConfigService ConfigService { get; }
        private IModsService ModsService { get; }
        private ITestService TestService { get; }

        public MainViewModel(UserSettings userSettings, FileLocations fileLocations, IWindowService windowService, IConfigService configService, IModsService modsService, ITestService testService)
        {
            UserSettings = userSettings;
            FileLocations = fileLocations;
            WindowService = windowService;
            ConfigService = configService;
            ModsService = modsService;
            TestService = testService;

            OpenKagFolderCommand = new RelayCommand(ExecuteOpenKagFolderCommand);
            TestMultiplayerCommand = new RelayCommand(ExecuteTestMultiplayerCommand);
            TestSoloCommand = new RelayCommand(ExecuteTestSoloCommand);
            ModsCommand = new RelayCommand(ExecuteModsCommand);
            ManualCommand = new RelayCommand(ExecuteManualCommand);
            ApiCommand = new RelayCommand(ExecuteApiCommand);
            UpdateGamemodesCommand = new RelayCommand(ExecuteUpdateGamemodesCommand);

            // Get settings from config files
            // startup_config.cfg
            var screenWidthProperty = new IntConfigProperty("Window.Width", ScreenWidth);
            var screenHeightProperty = new IntConfigProperty("Window.Height", ScreenHeight);
            var fullscreenProperty = new BoolConfigProperty("Fullscreen", Fullscreen);

            StartupOptionsEnabled = ConfigService.ReadConfigProperties(FileLocations.StartupConfigPath,
                screenWidthProperty,
                screenHeightProperty,
                fullscreenProperty
            );

            _screenWidth = screenWidthProperty.Value;
            _screenHeight = screenHeightProperty.Value;
            _fullscreen = fullscreenProperty.Value;

            // autoconfig.cfg
            var gamemodeProperty = new StringConfigProperty("sv_gamemode", Gamemode);

            GamemodeOptionEnabled = ConfigService.ReadConfigProperties(FileLocations.AutoConfigPath,
                gamemodeProperty
            );

            _gamemode = gamemodeProperty.Value;
        }

        public string Gamemode
        {
            get => _gamemode;
            set
            {
                Set(ref _gamemode, value);
                SaveAutoConfigInfo();
            }
        }

        public int ScreenWidth
        {
            get => _screenWidth;
            set
            {
                Set(ref _screenWidth, value);
                SaveStartupInfo();
            }
        }

        public int ScreenHeight
        {
            get => _screenHeight;
            set
            {
                Set(ref _screenHeight, value);
                SaveStartupInfo();
            }
        }

        public bool Fullscreen
        {
            get => _fullscreen;
            set
            {
                Set(ref _fullscreen, value);
                SaveStartupInfo();
            }
        }

        public ObservableCollection<string> Gamemodes
        {
            get => _gamemodes;
            set => Set(ref _gamemodes, value);
        }

        public bool StartupOptionsEnabled
        {
            get => _startupOptionsEnabled;
            set => Set(ref _startupOptionsEnabled, value);
        }

        public bool GamemodeOptionEnabled
        {
            get => _gamemodeOptionEnabled;
            set => Set(ref _gamemodeOptionEnabled, value);
        }

        // Expose user settings to view
        public double WindowTop
        {
            get => UserSettings.MainWindowTop;
            set => Set(ref UserSettings.MainWindowTop, value);
        }

        public double WindowLeft
        {
            get => UserSettings.MainWindowLeft;
            set => Set(ref UserSettings.MainWindowLeft, value);
        }

        public int TestType
        {
            get => UserSettings.TestType;
            set => Set(ref UserSettings.TestType, value);
        }

        public ICommand OpenKagFolderCommand { get; private set; }
        public ICommand TestMultiplayerCommand { get; private set; }
        public ICommand TestSoloCommand { get; private set; }
        public ICommand ModsCommand { get; private set; }
        public ICommand ManualCommand { get; private set; }
        public ICommand ApiCommand { get; private set; }
        public ICommand UpdateGamemodesCommand { get; private set; }

        private void ExecuteOpenKagFolderCommand()
        {
            WindowService.OpenInExplorer(FileLocations.KagDirectory);
        }

        private async void ExecuteTestMultiplayerCommand()
        {
            var portProperty = new IntConfigProperty("sv_port", -1);

            if (ConfigService.ReadConfigProperties(FileLocations.AutoConfigPath, portProperty) && 
                TestService.TryFixMultiplayerAutoConfigProperties(ConfigService))
            {
                await TestService.TestMultiplayerAsync(portProperty.Value, UserSettings.SyncClientServerClosing);
            }
        }

        private void ExecuteTestSoloCommand()
        {
            TestService.TestSolo();
        }

        private void ExecuteModsCommand()
        {
            WindowService.OpenWindow<ModsViewModel>();
        }

        private void ExecuteManualCommand()
        {
            WindowService.OpenWindow<ManualViewModel>();
        }

        private void ExecuteApiCommand()
        {
            WindowService.OpenWindow<ApiViewModel>();
        }

        private void ExecuteUpdateGamemodesCommand()
        {
            var newGamemodes = new List<string>(DEFAULT_GAMEMODES.Length);
            bool hasCustomGamemodes = false;

            foreach (Mod mod in ModsService.EnumerateActiveMods())
            {
                string gamemode = ConfigService.FindGamemodeOfMod(mod);
                if (gamemode != null && !newGamemodes.Contains(gamemode))
                {
                    newGamemodes.Add(gamemode);
                    hasCustomGamemodes = true;
                }
            }

            if (hasCustomGamemodes)
            {
                newGamemodes.Add("");
            }

            newGamemodes.AddRange(DEFAULT_GAMEMODES);

            Gamemodes = new ObservableCollection<string>(newGamemodes);
            RaisePropertyChanged("Gamemode"); // Make sure that gamemode doesn't get cleared after updating the options
        }

        private void SaveStartupInfo()
        {
            StartupOptionsEnabled = ConfigService.WriteConfigProperties(FileLocations.StartupConfigPath,
                new IntConfigProperty("Window.Width", ScreenWidth),
                new IntConfigProperty("Window.Height", ScreenHeight),
                new BoolConfigProperty("Fullscreen", Fullscreen)
            );
        }

        private void SaveAutoConfigInfo()
        {
            GamemodeOptionEnabled = ConfigService.WriteConfigProperties(FileLocations.AutoConfigPath,
                new StringConfigProperty("sv_gamemode", Gamemode)
            );
        }
    }
}
