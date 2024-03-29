using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudymentaryNet8
{
    public class KeyToImgSourceConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<string, ImageSource> conversion = (Dictionary<string, ImageSource>)Application.Current.Resources["imageReferences"];
            return (ImageSource)conversion[(string)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1 : 0;
        }
    }
}
