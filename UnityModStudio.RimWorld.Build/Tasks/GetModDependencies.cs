using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Build;

namespace UnityModStudio.RimWorld.Build.Tasks;

public class GetModDependencies : Task
{
    private static readonly Regex SteamIdRegex = 
        new(@"^(?:steam://url/CommunityFilePage/(?<id>\d+)|https?://steamcommunity\.com/(?:workshop|sharedfiles)/filedetails/\?id=(?<id>\d+))$", RegexOptions.IgnoreCase);

    [Required]
    public string? AboutFilePath { get; set; }

    [Output]
    public ITaskItem[] ModDependencies { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            var document = XDocument.Load(AboutFilePath!);
            if (document.Root?.Name != "ModMetaData")
                throw new XmlException("Missing or incorrect root element.");

            var unversionedElements = document.Root
                .Elements("modDependencies")
                .Elements("li")
                .Select(element => (element, version: default(string)));
            var versionedElements = document.Root
                .Elements("modDependenciesByVersion")
                .Elements()
                .SelectMany(versionElement => versionElement
                    .Elements("li")
                    .Select(element => (element, version: (string?)versionElement.Name.LocalName)));
            ModDependencies = unversionedElements
                .Concat(versionedElements)
                .Select(p => (
                    packageId: (string?)p.element.Element("packageId"),
                    url: (string?)p.element.Element("steamWorkshopUrl"),
                    p.version))
                .Where(x => !string.IsNullOrWhiteSpace(x.packageId))
                .Select(x =>
                {
                    var metadata = new Dictionary<string, string>();
                    var steamId = ExtractSteamId(x.url);
                    if (steamId != null)
                        metadata.Add("SteamId", steamId);
                    if (x.version != null)
                        metadata.Add("GameVersion", x.version);
                    return new TaskItem(x.packageId, metadata);
                })
                .ToArray<ITaskItem>();

        }
        catch (Exception exception)
        {
            Log.LogErrorWithCode("UMSRW0002", "Error while parsing About.xml: {0}", exception.Message);
            return false;
        }

        return true;
    }

    private static string? ExtractSteamId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var match = SteamIdRegex.Match(url);
        return !match.Success ? null : match.Groups["id"].Value;
    }
}