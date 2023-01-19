using System;
using System.Globalization;
using System.Windows.Data;

namespace UnityModStudio.Options;

public class EqualityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value != null && value.Equals(parameter);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => true.Equals(value) ? parameter : Binding.DoNothing;
}