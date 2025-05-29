// Converters/InverseBoolConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ForestDecisionMauiApp.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false; // 或者根据你的逻辑返回其他值
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            // 或者抛出异常
            return false;
        }
    }
}