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
            InitializeComponent();

            Title += string.Format(" v{0}", AssemblyHelper.Version);
        }
    }
}
