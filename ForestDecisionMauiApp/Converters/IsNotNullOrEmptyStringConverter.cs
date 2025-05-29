// Converters/IsNotNullOrEmptyStringConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ForestDecisionMauiApp.Converters
{
    public class IsNotNullOrEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 检查传入的 value 是否为 string 类型，并且不为 null 或空字符串
            if (value is string strValue)
            {
                return !string.IsNullOrEmpty(strValue);
            }
            return false; // 如果不是字符串或为 null，则认为它“空”，返回 false
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 这个转换器通常是单向的，所以 ConvertBack 不需要实现
            throw new NotImplementedException("IsNotNullOrEmptyStringConverter is a one-way converter.");
        }
    }
}