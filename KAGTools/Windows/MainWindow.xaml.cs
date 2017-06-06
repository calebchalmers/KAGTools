using GalaSoft.MvvmLight.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KAGTools.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (!Directory.Exists(Properties.Settings.Default.KagDirectory))
            {
                // KAG Directory not specified. Show dialog...
                if (!FindKagDirectory())
                {
                    Close();
                }
            }

            Messenger.Default.Register<string>(this, (msg) => ReceiveMessage(msg));

            if (Properties.Settings.Default.FirstRun)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;

            Title += string.Format(" v{0}", version.ToString());
        }

        private void ReceiveMessage(string msg)
        {
            if (msg == "EditMods")
            {
                OpenModsWindow();
            }
        }

        private bool FindKagDirectory()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select your King Arthur's Gold install directory";
            dialog.IsFolderPicker = true;

            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.KagDirectory = dialog.FileName;
                return true;
            }
            return false;
        }

        private void OpenModsWindow()
        {
            ModsWindow modsWindow = new ModsWindow();
            modsWindow.Owner = this;
            modsWindow.ShowDialog();
        }
    }
}
