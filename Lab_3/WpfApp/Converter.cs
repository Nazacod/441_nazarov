using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace WpfApp
{
    [ValueConversion(typeof(IEnumerable<(string, float)>), typeof(String))]
    public class Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string converted = "";

            List<Emotion> list = new List<Emotion>((List<Emotion>)value);
            //List<(string, float)> list = new List<(string, float)>((List<(string, float)>)value)

            foreach (var item in list)
                converted += $"{item.name}: {item.value}\n";
            return converted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
