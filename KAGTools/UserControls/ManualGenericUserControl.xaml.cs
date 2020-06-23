using System.Windows;
using System.Windows.Controls;

namespace KAGTools.UserControls
{
    /// <summary>
    /// Interaction logic for ManualGenericUserControl.xaml
    /// </summary>
    public partial class ManualGenericUserControl : UserControl
    {
        public ManualGenericUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtbox_Search.Focus();
        }
    }
}
