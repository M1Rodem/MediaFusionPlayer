using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MediaFusionPlayer.Presentation.Converters
{
    public class PlayPauseIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlaying = (value is bool b) && b;
            return isPlaying ? "Pause" : "Play";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}