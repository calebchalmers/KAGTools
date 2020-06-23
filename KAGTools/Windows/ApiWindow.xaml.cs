using System.ComponentModel;
using System.Windows;

namespace KAGTools.Windows
{
    /// <summary>
    /// Interaction logic for ApiWindow.xaml
    /// </summary>
    public partial class ApiWindow : Window
    {
        public ApiWindow()
        {
            InitializeComponent();

            ServerList.Items.SortDescriptions.Add(new SortDescription("PlayerCount", ListSortDirection.Descending));
        }
    }
}
