using System;
using System.Globalization;
using System.Windows.Data;

namespace BioInfo_Terminal.Methods.Messaging
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var formatParameter = (string) parameter;
            if (formatParameter != null && formatParameter.Contains("{")) return string.Format(formatParameter, value);
            return DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}