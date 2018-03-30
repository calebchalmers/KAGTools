using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KAGTools.Helpers
{
    public static class MessageBoxHelper
    {
        public static bool? Show(string message, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            MessageBoxResult result = MessageBox.Show(message, AssemblyHelper.AppName, button, icon);

            switch(result)
            {
                case MessageBoxResult.No:
                    return false;
                case MessageBoxResult.OK:
                case MessageBoxResult.Yes:
                    return true;
                default:
                    return null;
            }
        }

        public static bool? Info(string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            return Show(message, button, MessageBoxImage.Information);
        }

        public static bool? Ask(string message, MessageBoxButton button = MessageBoxButton.YesNo)
        {
            return Show(message, button, MessageBoxImage.Question);
        }

        public static bool? Warn(string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            return Show(message, button, MessageBoxImage.Warning);
        }

        public static bool? Error(string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            return Show(message, button, MessageBoxImage.Error);
        }
    }
}
