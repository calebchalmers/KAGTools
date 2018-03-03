using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void AutoSelectTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((Xceed.Wpf.Toolkit.AutoSelectTextBox)sender).SelectAll();
        }

        private void AutoSelectTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((Xceed.Wpf.Toolkit.AutoSelectTextBox)sender).SelectionLength = 0;
        }
    }
}
