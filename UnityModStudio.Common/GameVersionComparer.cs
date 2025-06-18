using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityModStudio.Common;

public class GameVersionComparer : Comparer<string>
{
    private static readonly Regex SplitRegex = new(@"(?:(\w*)(\W?))+");
    private static readonly IComparer<int> IntComparer = Comparer<int>.Default;
    private static readonly IComparer<string> StringComparer = System.StringComparer.OrdinalIgnoreCase;

    public override int Compare(string? x, string? y)
    {
        if (x is null || y is null)
            return Default.Compare(x!, y!);

        var xParts = Split(x).ToArray();
        var yParts = Split(y).ToArray();
        var length = Math.Max(xParts.Length, yParts.Length);
        for (var i = 0; i < length; i++)
        {
            var xPart = xParts.ElementAtOrDefault(i);
            var yPart = yParts.ElementAtOrDefault(i);

            switch ((xPart, yPart))
            {
                case (null, null):
                    return 0;
                case (null, _):
                    return -1;
                case (_, null):
                    return 1;
            }

            var result = (xPart.IntValue != null && yPart.IntValue != null)
                ? IntComparer.Compare(xPart.IntValue.Value, yPart.IntValue.Value)
                : StringComparer.Compare(xPart.StringValue, yPart.StringValue);
            if (result != 0)
                return result;

            result = SeparatorComparer.Instance.Compare(xPart.Separator, yPart.Separator);
            if (result != 0)
                return result;
        }

        return 0;
    }

    private static IEnumerable<VersionPart> Split(string version)
    {
        var match = SplitRegex.Match(version);
        for (var i = 0; i < match.Groups[1].Captures.Count; i++)
        {
            var value = match.Groups[1].Captures[i].Value;
            var separator = match.Groups[2].Captures[i].Value;
            yield return new VersionPart(value, separator);
        }
    }


    private class VersionPart(string value, string separator)
    {
        public readonly string StringValue = value;
        public readonly int? IntValue = int.TryParse(value, out var i) ? i : null;
        public readonly string Separator = separator;
    }

    private class SeparatorComparer : Comparer<string?>
    {
        public static readonly SeparatorComparer Instance = new();

        public override int Compare(string? x, string? y)
        {
            return (x, y) switch
            {
                (_, "-") or ("-", _) => -StringComparer.Compare(x!, y!),
                _ => StringComparer.Compare(x!, y!)
            };
        }
    }
}