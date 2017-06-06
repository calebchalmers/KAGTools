using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KAGTools.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static Properties.Settings Settings = Properties.Settings.Default;

        private string _gamemode;
        private int _screenWidth;
        private int _screenHeight;
        private bool _fullscreen;

        public MainViewModel()
        {
            if (string.IsNullOrEmpty(Settings.KagDirectory))
            {
                return;
            }

            OpenKAGFolderCommand = new RelayCommand(ExecuteOpenKAGFolderCommand);
            OpenScreenshotsCommand = new RelayCommand(ExecuteOpenScreenshotsCommand);
            RunLocalhostCommand = new RelayCommand(ExecuteRunLocalhostCommand);
            ModsCommand = new RelayCommand(ExecuteModsCommand);

            //FileHelper.GetStartupInfo(ref _screenWidth, ref _screenHeight, ref _fullscreen);

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

        private void SaveStartupInfo()
        {
            FileHelper.SetConfigInfo(
                FileHelper.StartupConfigPath,
                new ConfigPropertyDouble ("Window.Width", ScreenWidth),
                new ConfigPropertyDouble ("Window.Height", ScreenHeight),
                new ConfigPropertyBoolean ("Fullscreen", Fullscreen)
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

        public string Gamemode
        {
            get { return _gamemode; }
            set
            {
                if (_gamemode != value)
                {
                    _gamemode = value;
                    RaisePropertyChanged();
                    SaveAutoConfigInfo();
                }
            }
        }

        public int ScreenWidth
        {
            get { return _screenWidth; }
            set
            {
                if(_screenWidth != value)
                {
                    _screenWidth = value;
                    RaisePropertyChanged();
                    SaveStartupInfo();
                }
            }
        }

        public int ScreenHeight
        {
            get { return _screenHeight; }
            set
            {
                if (_screenHeight != value)
                {
                    _screenHeight = value;
                    RaisePropertyChanged();
                    SaveStartupInfo();
                }
            }
        }

        public bool Fullscreen
        {
            get { return _fullscreen; }
            set
            {
                if (_fullscreen != value)
                {
                    _fullscreen = value;
                    RaisePropertyChanged();
                    SaveStartupInfo();
                }
            }
        }

        public ICommand OpenKAGFolderCommand { get; private set; }
        public ICommand OpenScreenshotsCommand { get; private set; }
        public ICommand RunLocalhostCommand { get; private set; }
        public ICommand ModsCommand { get; private set; }

        private void ExecuteOpenKAGFolderCommand()
        {
            Process.Start(FileHelper.KagDir);
        }

        private void ExecuteOpenScreenshotsCommand()
        {
            Process.Start(FileHelper.ScreenshotsDir);
        }

        private void ExecuteRunLocalhostCommand()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = FileHelper.RunLocalhostPath;
            startInfo.WorkingDirectory = FileHelper.KagDir;
            Process.Start(startInfo);
        }

        private void ExecuteModsCommand()
        {
            MessengerInstance.Send("EditMods");
        }
    }
}
