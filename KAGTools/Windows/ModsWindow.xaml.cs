using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

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

            Messenger.Default.Register<FocusSelectedItemMessage>(this, e =>
            {
                listbox.Focus();
                listbox.ScrollIntoView(listbox.SelectedItem);
            });
        }

        private void listbox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key != Key.Delete)
            {
                searchBox.Focus();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
