using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace KAGTools.UserControls
{
    /// <summary>
    /// Interaction logic for ManualGenericUserControl.xaml
    /// </summary>
    public partial class ManualDocumentUserControl : UserControl
    {
        public ManualDocumentUserControl()
        {
            InitializeComponent();

            TypeControl.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SearchControl.Focus();
        }
    }
}
