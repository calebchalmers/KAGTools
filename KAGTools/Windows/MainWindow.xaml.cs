using GalaSoft.MvvmLight.Messaging;
using KAGTools.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (double.IsNaN(Properties.Settings.Default.Left) || double.IsNaN(Properties.Settings.Default.Top))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title += string.Format(" v{0}", version.ToString());
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateHelper.UpdateApp();
        }
    }
}
