using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaFusionPlayer.Presentation.Converters
{
    public class ProgressToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values == null || values.Length < 2)
                    return 0.0;

                var currentValue = System.Convert.ToDouble(values[0]);
                var maximumValue = System.Convert.ToDouble(values[1]);

                if (maximumValue <= 0)
                    return 0.0;

                var progress = currentValue / maximumValue;
                return progress;
            }
            catch
            {
                return 0.0;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}