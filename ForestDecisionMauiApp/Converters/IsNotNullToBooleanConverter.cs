// Converters/IsNotNullToBooleanConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ForestDecisionMauiApp.Converters
{
    public class IsNotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 通常不需要 ConvertBack
            throw new NotImplementedException();
        }
    }
}