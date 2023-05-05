using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace GPS_Organizer.Utils
{
    public class ColorToMediaColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VRageMath.Color vrageColor)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(vrageColor.A, vrageColor.R, vrageColor.G, vrageColor.B));
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush mediaBrush)
            {
                var mediaColor = mediaBrush.Color;
                return new VRageMath.Color(mediaColor.R, mediaColor.G, mediaColor.B, mediaColor.A);
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
