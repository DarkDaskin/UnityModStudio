using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace UnityModStudio.ProjectWizard;

public class SimpleMarkdownConverter : IValueConverter
{
    private static readonly string[] BoldSeparator = ["**"];

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var textBlock = new TextBlock();
        var stringValue = value?.ToString() ?? "";
        var parts = stringValue.Split(BoldSeparator, StringSplitOptions.None);
        if (parts.Length > 1)
            for (var i = 0; i < parts.Length; i++)
                textBlock.Inlines.Add(new Run(parts[i]) { FontWeight = i % 2 > 0 ? FontWeights.Bold : FontWeights.Normal });
        else
            textBlock.Text = stringValue;
        return textBlock;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}