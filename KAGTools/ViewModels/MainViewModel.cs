using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using KAGTools.Data;
using KAGTools.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
        ObservableCollection<string> _gamemodes;
        private bool _startupOptionsEnabled;
        private bool _gamemodeOptionEnabled;

        public MainViewModel(UserSettings userSettings, ManualDocument[] manualDocuments)
        {
            UserSettings = userSettings;
            ManualDocuments = manualDocuments;

            OpenKagFolderCommand = new RelayCommand(ExecuteOpenKagFolderCommand);
            TestMultiplayerCommand = new RelayCommand(ExecuteTestMultiplayerCommand);
            TestSoloCommand = new RelayCommand(ExecuteTestSoloCommand);
            ModsCommand = new RelayCommand(ExecuteModsCommand);
            ManualCommand = new RelayCommand(ExecuteManualCommand);
            ApiCommand = new RelayCommand(ExecuteApiCommand);

            InitializeGamemodes(FileHelper.GetMods(true));

            // Get settings from config files
            // startup_config.cfg
            var screenWidthProperty = new IntConfigProperty("Window.Width", ScreenWidth);
            var screenHeightProperty = new IntConfigProperty("Window.Height", ScreenHeight);
            var fullscreenProperty = new BoolConfigProperty("Fullscreen", Fullscreen);

            StartupOptionsEnabled = FileHelper.ReadConfigProperties(FileHelper.StartupConfigPath,
                screenWidthProperty,
                screenHeightProperty,
                fullscreenProperty
            );

            _screenWidth = screenWidthProperty.Value;
            _screenHeight = screenHeightProperty.Value;
            _fullscreen = fullscreenProperty.Value;

            // autoconfig.cfg
            var gamemodeProperty = new StringConfigProperty("sv_gamemode", Gamemode);

            GamemodeOptionEnabled = FileHelper.ReadConfigProperties(FileHelper.AutoConfigPath,
                gamemodeProperty
            );

            _gamemode = gamemodeProperty.Value;
        }

        private UserSettings UserSettings { get; }
        private ManualDocument[] ManualDocuments { get; }

        public string Gamemode
        {
            get => _gamemode;
            set => this.SetProperty(ref _gamemode, value, SaveAutoConfigInfo);
        }

        public int ScreenWidth
        {
            get => _screenWidth;
            set => this.SetProperty(ref _screenWidth, value, SaveStartupInfo);
        }

        public int ScreenHeight
        {
            get => _screenHeight;
            set => this.SetProperty(ref _screenHeight, value, SaveStartupInfo);
        }

        public bool Fullscreen
        {
            get => _fullscreen;
            set => this.SetProperty(ref _fullscreen, value, SaveStartupInfo);
        }

        public ObservableCollection<string> Gamemodes
        {
            get => _gamemodes;
            set => this.SetProperty(ref _gamemodes, value);
        }

        public bool StartupOptionsEnabled
        {
            get => _startupOptionsEnabled;
            set => this.SetProperty(ref _startupOptionsEnabled, value);
        }

        public bool GamemodeOptionEnabled
        {
            get => _gamemodeOptionEnabled;
            set => this.SetProperty(ref _gamemodeOptionEnabled, value);
        }

        // Expose user settings to view
        public double WindowTop
        {
            get => UserSettings.Top;
            set => this.SetProperty(ref UserSettings.Top, value);
        }

        public double WindowLeft
        {
            get => UserSettings.Left;
            set => this.SetProperty(ref UserSettings.Left, value);
        }

        public int RunTypeIndex
        {
            get => UserSettings.RunTypeIndex;
            set => this.SetProperty(ref UserSettings.RunTypeIndex, value);
        }

        public ICommand OpenKagFolderCommand { get; private set; }
        public ICommand TestMultiplayerCommand { get; private set; }
        public ICommand TestSoloCommand { get; private set; }
        public ICommand ModsCommand { get; private set; }
        public ICommand ManualCommand { get; private set; }
        public ICommand ApiCommand { get; private set; }

        private void ExecuteOpenKagFolderCommand()
        {
            Process.Start(FileHelper.KagDir);
        }

        private async void ExecuteTestMultiplayerCommand()
        {
            await TestHelper.TestMultiplayer();
        }

        private void ExecuteTestSoloCommand()
        {
            TestHelper.TestSolo();
        }

        private void ExecuteModsCommand()
        {
            ModsViewModel viewModel = new ModsViewModel();
            WindowHelper.OpenDialog(viewModel);

            InitializeGamemodes(viewModel.Items.Where(mod => mod.IsActive == true));
        }

        private void ExecuteManualCommand()
        {
            ManualViewModel viewModel = new ManualViewModel(UserSettings, ManualDocuments);
            WindowHelper.OpenWindow(viewModel);
        }

        private void ExecuteApiCommand()
        {
            ApiViewModel viewModel = new ApiViewModel();
            WindowHelper.OpenDialog(viewModel);
        }

        private void SaveStartupInfo()
        {
            StartupOptionsEnabled = FileHelper.WriteConfigProperties(FileHelper.StartupConfigPath,
                new IntConfigProperty("Window.Width", ScreenWidth),
                new IntConfigProperty("Window.Height", ScreenHeight),
                new BoolConfigProperty("Fullscreen", Fullscreen)
            );
        }

        private void SaveAutoConfigInfo()
        {
            GamemodeOptionEnabled = FileHelper.WriteConfigProperties(FileHelper.AutoConfigPath,
                new StringConfigProperty("sv_gamemode", Gamemode)
            );
        }

        private void InitializeGamemodes(IEnumerable<Mod> activeMods)
        {
            var newGamemodes = new List<string>(DEFAULT_GAMEMODES.Length);

            if (activeMods != null)
            {
                bool hasCustomGamemodes = false;

                foreach (Mod mod in activeMods)
                {
                    string gamemode = FileHelper.FindGamemodeOfMod(mod.Directory);
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
            }

            newGamemodes.AddRange(DEFAULT_GAMEMODES);

            Gamemodes = new ObservableCollection<string>(newGamemodes);
            RaisePropertyChanged("Gamemode"); // Make sure that gamemode doesn't get cleared after updating the options
        }
    }
}
