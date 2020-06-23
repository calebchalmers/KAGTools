using System;
using System.Globalization;
using System.Windows.Data;

namespace KAGTools.Xaml.Converters
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Yes" : "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Yes".Equals((string)value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
