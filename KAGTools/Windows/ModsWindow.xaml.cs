using System.ComponentModel;
using System.Windows;

namespace KAGTools.Windows
{
    /// <summary>
    /// Interaction logic for ModsWindow.xaml
    /// </summary>
    public partial class ModsWindow : Window
    {
        public ModsWindow()
        {
            InitializeComponent();

            listbox.Items.SortDescriptions.Add(new SortDescription("IsActive", ListSortDirection.Descending));
            listbox.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }
    }
}
