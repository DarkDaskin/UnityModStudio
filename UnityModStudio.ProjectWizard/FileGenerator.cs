using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.Threading;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectWizard;

public static class FileGenerator
{
    public static async Task UpdateJsonFileAsync(string path, Action<JsonObject> action)
    {
        using var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
        var root = (await JsonNode.ParseAsync(stream))?.AsObject() ?? throw new InvalidOperationException($"No root object in file '{path}'.");
        action(root);
        stream.SetLength(0); // Clear the file before writing
        await JsonSerializer.SerializeAsync(stream, root, new JsonSerializerOptions { WriteIndented = true });
    }
         
    public static void UpdateLaunchSettings(JsonObject root, IReadOnlyDictionary<string, string?> gameVersions)
    {
        var profiles = GetOrAddChild(root, "profiles");

        foreach (var version in gameVersions)
        {
            var profile = GetOrAddChild(profiles, version.Key);
            profile["commandName"] = "Executable";
            if (!string.IsNullOrEmpty(version.Value))
                profile["gameVersion"] = version.Value;
        }
    }

    private static JsonObject GetOrAddChild(JsonObject parent, string key)
    {
        if (parent.TryGetPropertyValue(key, out var value) && value is JsonObject child)
            return child;

        var newChild = new JsonObject();
        parent[key] = newChild;
        return newChild;
    }

    public static async Task UpdateXmlFileAsync(string path, Action<XDocument> action)
    {
        await TaskScheduler.Default;

        var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
        action(document);
        document.Save(path);
    }

    public static void UpdateProject(XDocument document, IReadOnlyCollection<Game> games)
    {
        if (document.Root == null)
            throw new XmlException("The XML document does not have a root element.");
        if (games.Count == 0)
            throw new InvalidOperationException("No games are defined.");
        Debug.Assert(games.Select(game => game.GameName).Distinct().Count() == 1);

        var ns = document.Root.Name.Namespace;
        var gameVersionsByTargetFramework = games.ToLookup(game => game.TargetFrameworkMoniker);
        
        var gameNameElement = document.Root.Elements(ns + "PropertyGroup").Elements(ns + "GameName").Single();
        var gameName = games.First().GameName ?? throw new InvalidOperationException("Game name is not defined.");
        gameNameElement.Value = gameName;

        var targetFrameworkElement = document.Root.Elements(ns + "PropertyGroup").Elements(ns + "TargetFramework").Single();
        var targetFrameworksElement = document.Root.Elements(ns + "PropertyGroup").Elements(ns + "TargetFrameworks").Single();
        var gameVersionElement = document.Root.Elements(ns + "PropertyGroup").Elements(ns + "GameVersion").Single();

        if (gameVersionsByTargetFramework.Count() == 1)
        {
            IncludeWhitespace(targetFrameworksElement).Remove();

            var gameVersions = GetGameVersions(games);
            switch (gameVersions.Length)
            {
                case 0:
                    Debug.Assert(games.Count == 1);

                    IncludeWhitespace(gameVersionElement).Remove();
                    break;

                case 1:
                    gameVersionElement.Value = gameVersions[0];
                    break;

                default:
                    gameVersionElement.ReplaceWith(new XElement(ns + "GameVersions", string.Join(";", gameVersions)));
                    break;
            }
        }
        else
        {
            IncludeWhitespace(targetFrameworkElement).Remove();
            
            gameVersionElement.ReplaceWith(AddWhitespace(gameVersionsByTargetFramework.Select(g =>
                new XElement(ns + "GameVersions", new XAttribute("Condition", $"'$(TargetFramework)' == '{g.Key}'"),
                    // ToArray() to evaluate eagerly, so gameVersionElement is still attached to the document
                    string.Join(";", GetGameVersions(g)))), gameVersionElement).ToArray().AsEnumerable());
        }


        static string[] GetGameVersions(IEnumerable<Game> games) => games
            .Select(game => game.Version)
            .Where(version => version != null)
            .Cast<string>()
            .OrderBy(version => version, new GameVersionComparer())
            .ToArray();
    }

    public static IEnumerable<XNode> AddWhitespace(IEnumerable<XElement> elements, XElement reference)
    {
        var indent = reference.PreviousNode is XText textNode ? textNode.Value.Split('\n').Last() : "";
        var isFirst = true;
        foreach (var element in elements)
        {
            if (!isFirst)
                yield return new XText($"\n{indent}");
            isFirst = false;
            yield return element;
        }
    }

    public static IEnumerable<XNode> IncludeWhitespace(XElement element)
    {
        yield return element;
        if (element.PreviousNode is XText textNode && string.IsNullOrWhiteSpace(textNode.Value))
            yield return textNode;
    }

    public static XElement GetElementSafe(XElement parent, XName name) => 
        parent.Element(name) ?? throw new XmlException($"Element '{name}' not found in XML document.");
}