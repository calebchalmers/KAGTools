using KAGTools.Helpers;
using System.Windows;

namespace KAGTools.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (double.IsNaN(App.Settings.Left) || double.IsNaN(App.Settings.Top))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            InitializeComponent();

            Title += string.Format(" v{0}", AssemblyHelper.Version);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateHelper.UpdateApp();
        }
    }
}
