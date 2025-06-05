using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityModStudio.Common;

public class GameVersionComparer : Comparer<string>
{
	private static readonly Regex SplitRegex = new(@"(?:(\w*)(\W?))+");
	private static readonly IComparer<int> IntComparer = Comparer<int>.Default;
	private static readonly IComparer<string> StringComparer = new VersionPartComparer();

	public override int Compare(string x, string y)
	{
		var xParts = Split(x).ToArray();
		var yParts = Split(y).ToArray();
		var length = Math.Min(xParts.Length, yParts.Length);
		for (var i = 0; i < length; i++)
		{
			var xPart = xParts[i];
			var yPart = yParts[i];
			
			var result = (xPart.IntValue != null && yPart.IntValue != null)
				? IntComparer.Compare(xPart.IntValue.Value, yPart.IntValue.Value)
				: StringComparer.Compare(xPart.StringValue, yPart.StringValue);
			if (result != 0)
				return result;

			result = StringComparer.Compare(xPart.Separator, yPart.Separator);
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

	/// <summary>
	/// Same as <see cref="System.StringComparer.OrdinalIgnoreCase"/>, but empty string compares greater.
	/// </summary>
	private class VersionPartComparer : Comparer<string?>
	{
		public override int Compare(string? x, string? y)
		{
			var isXEmpty = string.IsNullOrEmpty(x);
			var isYEmpty = string.IsNullOrEmpty(y);
			if (isXEmpty && isYEmpty)
				return 0;
			if (isXEmpty)
				return 1;
			if (isYEmpty) 
				return -1;
			return System.StringComparer.OrdinalIgnoreCase.Compare(x, y);
		}
	}
}