using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using KAGTools.Data;
using KAGTools.Helpers;
using System.Net.Sockets;

namespace KAGTools.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static Properties.Settings Settings = Properties.Settings.Default;

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

        public MainViewModel()
        {
            if (string.IsNullOrEmpty(Settings.KagDirectory))
            {
                return;
            }

            OpenKAGFolderCommand = new RelayCommand(ExecuteOpenKAGFolderCommand);
            RunServerClientCommand = new RelayCommand(ExecuteRunServerClientCommand);
            RunLocalhostCommand = new RelayCommand(ExecuteRunLocalhostCommand);
            ModsCommand = new RelayCommand(ExecuteModsCommand);
            ManualCommand = new RelayCommand(ExecuteManualCommand);
            ApiCommand = new RelayCommand(ExecuteApiCommand);

            //FileHelper.GetStartupInfo(ref _screenWidth, ref _screenHeight, ref _fullscreen);

            InitializeGamemodes(FileHelper.GetMods(true));

            // Get settings from config files
            // startup_config.cfg
            var screenWidthProperty = new ConfigPropertyDouble("Window.Width", ScreenWidth);
            var screenHeightProperty = new ConfigPropertyDouble("Window.Height", ScreenHeight);
            var fullscreenProperty = new ConfigPropertyBoolean("Fullscreen", Fullscreen);

            FileHelper.GetConfigInfo(
                FileHelper.StartupConfigPath,
                screenWidthProperty,
                screenHeightProperty,
                fullscreenProperty
                );

            _screenWidth = (int)screenWidthProperty.Value;
            _screenHeight = (int)screenHeightProperty.Value;
            _fullscreen = fullscreenProperty.Value;

            // autoconfig.cfg
            var gamemodeProperty = new ConfigPropertyString("sv_gamemode", Gamemode);

            FileHelper.GetConfigInfo(
                FileHelper.AutoConfigPath,
                gamemodeProperty
                );

            _gamemode = gamemodeProperty.Value;
        }

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

        public ICommand OpenKAGFolderCommand { get; private set; }
        public ICommand RunServerClientCommand { get; private set; }
        public ICommand RunLocalhostCommand { get; private set; }
        public ICommand ModsCommand { get; private set; }
        public ICommand ManualCommand { get; private set; }
        public ICommand ApiCommand { get; private set; }

        private void ExecuteOpenKAGFolderCommand()
        {
            Process.Start(FileHelper.KagDir);
        }

        private void ExecuteRunServerClientCommand()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = FileHelper.KAGExecutablePath,
                Arguments = string.Format("noautoupdate nolauncher autostart \"{0}\" autoconfig autoconfig.cfg", FileHelper.ServerAutoStartScriptPath),
                WorkingDirectory = FileHelper.KagDir
            });

            Process.Start(new ProcessStartInfo()
            {
                FileName = FileHelper.KAGExecutablePath,
                Arguments = string.Format("noautoupdate nolauncher autostart \"{0}\"", FileHelper.ClientAutoStartScriptPath),
                WorkingDirectory = FileHelper.KagDir
            });
        }

        private void ExecuteRunLocalhostCommand()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = FileHelper.RunLocalhostPath,
                WorkingDirectory = FileHelper.KagDir
            });
        }

        private void ExecuteModsCommand()
        {
            ModsViewModel viewModel = new ModsViewModel();
            WindowHelper.OpenDialog(viewModel);

            InitializeGamemodes(viewModel.Items.Where(mod => mod.IsActive));
        }

        private void ExecuteManualCommand()
        {
            ManualViewModel viewModel = new ManualViewModel();
            WindowHelper.OpenWindow(viewModel);
        }

        private void ExecuteApiCommand()
        {
            ApiViewModel viewModel = new ApiViewModel();
            WindowHelper.OpenDialog(viewModel);
        }

        private void SaveStartupInfo()
        {
            FileHelper.SetConfigInfo(
                FileHelper.StartupConfigPath,
                new ConfigPropertyDouble("Window.Width", ScreenWidth),
                new ConfigPropertyDouble("Window.Height", ScreenHeight),
                new ConfigPropertyBoolean("Fullscreen", Fullscreen)
                );

            //FileHelper.SetStartupInfo(ScreenWidth, ScreenHeight, Fullscreen);
        }

        private void SaveAutoConfigInfo()
        {
            FileHelper.SetConfigInfo(
                FileHelper.AutoConfigPath,
                new ConfigPropertyString("sv_gamemode", Gamemode)
                );
        }

        private bool IsComment(string line)
        {
            return line.StartsWith("#");
        }

        private void InitializeGamemodes(IEnumerable<Mod> activeMods)
        {
            var newGamemodes = new List<string>(DEFAULT_GAMEMODES.Length);

            bool hasCustomGamemodes = false;

            foreach (Mod mod in activeMods)
            {
                if (mod.Gamemode != "N/A" && !newGamemodes.Contains(mod.Gamemode))
                {
                    newGamemodes.Add(mod.Gamemode);
                    hasCustomGamemodes = true;
                }
            }

            if (hasCustomGamemodes)
            {
                newGamemodes.Add("");
            }

            newGamemodes.AddRange(DEFAULT_GAMEMODES);

            Gamemodes = new ObservableCollection<string>(newGamemodes);
        }
    }
}
