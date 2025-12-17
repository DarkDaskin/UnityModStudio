using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using static UnityModStudio.ProjectWizard.FileGenerator;

namespace UnityModStudio.RimWorld.ProjectWizard;

public static class RimWorldFileGenerator
{
    public static void UpdateProject(XDocument document, bool useHarmony)
    {
        if (document.Root == null)
            throw new XmlException("The XML document does not have a root element.");

        var ns = document.Root.Name.Namespace;

        if (!useHarmony)
        {
            var packageReferenceElements = document.Root
                .Elements(ns + "ItemGroup")
                .Elements(ns + "PackageReference")
                .ToArray();
            foreach (var packageReferenceElement in packageReferenceElements)
            {
                var includeAttribute = packageReferenceElement.Attribute("Include");
                if (includeAttribute?.Value is "Lib.Harmony.Ref" or "HarmonyTools.Analyzers")
                    IncludeWhitespace(packageReferenceElement).Remove();
            }            
        }
    }

    public static void UpdateMetadata(XDocument document, string? packageId, string? author, string? name, string? description,
        IReadOnlyCollection<string> gameVersions, bool useHarmony)
    {                                                  ;
        if (document.Root == null)
            throw new XmlException("The XML document does not have a root element.");

        GetElementSafe(document.Root,"packageId").Value = packageId ?? "";
        GetElementSafe(document.Root, "name").Value = name ?? "";
        GetElementSafe(document.Root, "author").Value = author ?? "";
        GetElementSafe(document.Root, "description").Value = description ?? "";

        var supportedVersionsElement = GetElementSafe(document.Root, "supportedVersions");
        var supportedVersionDummyElement = GetElementSafe(supportedVersionsElement, "li");
        supportedVersionDummyElement.AddAfterSelf(AddWhitespace(gameVersions.Select(version => new XElement("li", version)), supportedVersionDummyElement));
        supportedVersionDummyElement.Remove();

        if (!useHarmony && document.Root.Element("modDependencies") is {} depencenciesElement)
            IncludeWhitespace(depencenciesElement).Remove();
    }
}