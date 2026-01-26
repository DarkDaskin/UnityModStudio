using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Build;

namespace UnityModStudio.RimWorld.Build.Tasks;

public class GetLoadFolders : Task
{
    [Required]
    public string? LoadFoldersFilePath { get; set; }

    [Output]
    public ITaskItem[] LoadFolders { get; set; } = [];

    public override bool Execute()
    {
        if (!File.Exists(LoadFoldersFilePath))
            return true;

        try
        {
            var document = XDocument.Load(LoadFoldersFilePath!);
            if (document.Root?.Name != "loadFolders")
                throw new XmlException("Missing or incorrect root element.");

            LoadFolders = document.Root.Elements()
                .SelectMany(versionElement => versionElement.Elements("li")
                    .Select(pathElement => NormalizePath(pathElement.Value)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(path => new TaskItem(path))
                .ToArray<ITaskItem>();
        }
        catch (Exception exception)
        {
            Log.LogErrorWithCode("UMSRW0001", "Error while parsing LoadFolders.xml: {0}", exception.Message);
            return false;
        }

        return true;
    }

    internal static string NormalizePath(string path) => path is "" or "/" or "\\" ? "." : path;
}