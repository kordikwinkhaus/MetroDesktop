using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetroDesktop
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool original = (bool)value;
            bool negate = GetParamVal(parameter);
            if (negate)
            {
                original = !original;
            }

            return (original) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool GetParamVal(object parameter)
        {
            if (parameter == null) return false;
            return System.Convert.ToBoolean(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
