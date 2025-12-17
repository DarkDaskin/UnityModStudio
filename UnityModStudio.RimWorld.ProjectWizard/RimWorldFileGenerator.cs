using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

    public static void UpdatePreviewImage(string imageFilePath, string modName)
    {
        var tempFilePath = imageFilePath + ".tmp";
        using (var bitmap = Bitmap.FromFile(imageFilePath))
        {
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            LoadFontFromResources("RimWordFont.ttf");
            var font = new Font("RimWordFont", 48);
            graphics.DrawString(modName, font, Brushes.White, new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // Save to a temporary file first to avoid file access conflict.
            bitmap.Save(tempFilePath);
        }
        File.Replace(tempFilePath, imageFilePath, null);
    }

    private static void LoadFontFromResources(string resourceName)
    {
        var fontCollection = new PrivateFontCollection();
        using var resourceStream = typeof(RimWorldFileGenerator).Assembly.GetManifestResourceStream(typeof(RimWorldFileGenerator), resourceName) ??
                                   throw new ArgumentException("Resource not found.", nameof(resourceName));
        var length = (int)resourceStream.Length;
        using var memoryStream = new MemoryStream(length);
        resourceStream.CopyTo(memoryStream);
        var buffer = memoryStream.GetBuffer();
        var ptr = Marshal.AllocHGlobal(length);
        Marshal.Copy(buffer, 0, ptr, length);
        fontCollection.AddMemoryFont(ptr, length);
        Marshal.FreeHGlobal(ptr);
    }
}