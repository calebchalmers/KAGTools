using KAGTools.Services;
using KAGTools.ViewModels;
using KAGTools.Windows;
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
            IViewService viewService = ServiceManager.RegisterService<IViewService>(new ViewService());
            viewService.RegisterView(typeof(MainWindow), typeof(MainViewModel));
            viewService.RegisterView(typeof(ModsWindow), typeof(ModsViewModel));

            ServiceManager.GetService<IViewService>().OpenWindow(new MainViewModel());

            KAGTools.Properties.Settings.Default.Upgrade();
        }
    }
}
