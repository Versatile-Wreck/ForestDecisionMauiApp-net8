// Converters/EnumToStringConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ForestDecisionMauiApp.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return value.ToString(); // 直接返回枚举成员的名称
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 通常在Picker中不需要ConvertBack，除非你想从字符串转回枚举
            if (value is string strValue && targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, strValue, true);
                }
                catch
                {
                    return Activator.CreateInstance(targetType); // 返回默认值
                }
            }
            return value;
        }
    }
}