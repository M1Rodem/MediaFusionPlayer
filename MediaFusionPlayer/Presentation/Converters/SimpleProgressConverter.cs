using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaFusionPlayer.Presentation.Converters
{
    public class SimpleProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d && parameter is double width && width > 0)
                return d * width;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}