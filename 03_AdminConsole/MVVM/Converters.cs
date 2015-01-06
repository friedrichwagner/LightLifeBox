using System;
using System.Windows;
using System.Windows.Data;

namespace Lumitech.Converters
{
    public class BoolToOppositeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null) return value;

            if (targetType != typeof(bool?))
                throw new InvalidOperationException("The target must be a boolean?");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value == null) return value;

            if (targetType != typeof(bool?))
                throw new InvalidOperationException("The target must be a boolean?");

            return !(bool)value;
        }
    }

    /*public class BoolToOppositeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool hasFocus = (bool)value;
                if (hasFocus) return Visibility.Collapsed; 
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(); 
        }
    }

    public class TextInputToVisibilityConverter : IMultiValueConverter 
    { 
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
        { 
            // Always test MultiValueConverter inputs for non-null 
            // (to avoid crash bugs for views in the designer) 
            if (values[0] is bool && values[1] is bool) 
            { 
                bool hasText = !(bool)values[0]; 
                bool hasFocus = (bool)values[1]; 
                if (hasFocus || hasText) 
                    return Visibility.Collapsed; 
            } 
            return Visibility.Visible; 
        } 

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) 
        { 
            throw new NotImplementedException(); 
        } 
    } */
} 