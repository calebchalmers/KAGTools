using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KAGTools.Properties.Settings.Default.FirstRun = false;
            KAGTools.Properties.Settings.Default.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            KAGTools.Properties.Settings.Default.Upgrade();
        }
    }
}
