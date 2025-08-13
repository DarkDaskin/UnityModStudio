using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks;

public class GetFileVersion : Task
{
	[Required]
	public string? Path { get; set; }

	[Output]
	public string? FileVersion { get; set; }

	public override bool Execute()
	{
		if (!File.Exists(Path))
		{
			Log.LogError("Specified file does not exist.");
			return false;
		}

		FileVersion = FileVersionInfo.GetVersionInfo(Path!).FileVersion;
		return true;
	}
}