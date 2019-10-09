//BoolToVisConverter.cs

/* Developed for Intel(R) Perceptual Computing Challenge 2013
 * by Prabu Arumugam (cincoutprabu@gmail.com)
 * http://www.codeding.com/
 * 2013-Aug-18
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PerceptualPegSolitaire
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool invert = parameter != null && parameter.ToString().ToLower().Equals("invert");
                if ((bool)value) return invert ? Visibility.Hidden : Visibility.Visible;
                else return invert ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return value.Equals(Visibility.Visible) ? true : false;
            }
            return false;
        }
    }
}
