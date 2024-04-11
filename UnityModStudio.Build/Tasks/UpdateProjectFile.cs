using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace UnityModStudio.Build.Tasks
{
    public class UpdateProjectFile : Task
    {
        private static readonly XNamespace MsbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        [Required]
        public string? ProjectFile { get; set; }

        public ITaskItem[] Properties { get; set; } = [];

        public ITaskItem[] Items { get; set; } = [];

        // Names have to be specified separately to correctly clean the file if Items is empty.
        public ITaskItem[] ItemNames { get; set; } = [];

        // NOTE: This will reformat the document, SaveOptions.DisableFormatting only preserves whitespace between elements.
        // TODO: Preserve as much formatting as possible.
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(ProjectFile))
            {
                Log.LogError("Project file is not specified.");
                return false;
            }

            if (Properties.Length == 0 && ItemNames.Length == 0)
            {
                Log.LogWarning("No properties or items to set.");
                return true;
            }

            var document = File.Exists(ProjectFile)
                ? XDocument.Load(ProjectFile!)
                : new XDocument(new XElement(MsbuildNamespace + "Project"));

            // Default namespace may be implicit or explicit, determine that from the root element.
            var ns = document.Root?.Name.Namespace ?? XNamespace.None;
            if (document.Root?.Name.LocalName != "Project" || (ns != XNamespace.None && ns != MsbuildNamespace))
            {
                Log.LogError("Invalid root element.");
                return false;
            }
            
            SetProperties(document, ns);
            SetItems(document, ns);

            document.Save(ProjectFile!);

            return true;
        }

        private void SetProperties(XDocument document, XNamespace ns)
        {
            var propertyGroupXName = ns + "PropertyGroup";
            var propertyGroupElements = document.Root!.Elements(propertyGroupXName).Where(HasNoCondition).ToList();
            var propertyNames = new HashSet<string>(Properties.Select(item => item.ItemSpec));
            // New properties are appended to either the first property group which contains one of the existing properties, 
            // or the first existing property group.
            var newPropertyGroupElement =
                propertyGroupElements.FirstOrDefault(
                    pgElement => pgElement.Elements().Where(HasNoCondition).Any(element => propertyNames.Contains(element.Name.LocalName))) ??
                propertyGroupElements.FirstOrDefault();
            
            foreach (var propertyItem in Properties)
            {
                var name = propertyItem.ItemSpec;
                var value = propertyItem.GetMetadata("Value");
                var comment = propertyItem.GetMetadata("Comment");

                var propertyXName = ns + name;
                var propertyElement = propertyGroupElements.Elements(propertyXName).LastOrDefault(HasNoCondition);

                if (propertyElement == null)
                {
                    propertyElement = new XElement(propertyXName);
                    newPropertyGroupElement ??= new XElement(propertyGroupXName);
                    newPropertyGroupElement.Add(propertyElement);
                }

                propertyElement.Value = value;

                if (!string.IsNullOrEmpty(comment))
                {
                    var commentNode = propertyElement.NodesBeforeSelf()
                        .SkipWhile(node => node.NodeType == XmlNodeType.Whitespace).Take(1)
                        .OfType<XComment>().FirstOrDefault();
                    if (commentNode == null)
                    {
                        commentNode = new XComment(comment);
                        propertyElement.AddBeforeSelf(commentNode);
                    }
                    else
                        commentNode.Value = comment;
                }
            }

            if (newPropertyGroupElement is { Document: null })
                document.Root.Add(newPropertyGroupElement);
        }

        private void SetItems(XDocument document, XNamespace ns)
        {
            var itemsByName = Items.ToLookup(item => item.GetMetadata("ItemName"));

            var itemGroupXName = ns + "ItemGroup";
            var itemGroupElements = document.Root!.Elements(itemGroupXName).Where(HasNoCondition).ToList();

            var hasLoggedInvalidItemAction = false;

            foreach (var itemNameItem in ItemNames)
            {
                var itemName = itemNameItem.ItemSpec;

                // Items in current group are appended to either the first item group which contains one of the existing items, 
                // or the first existing empty item group.
                var currentItemGroupElement =
                    itemGroupElements.FirstOrDefault(
                        igElement => igElement.Elements().Where(HasNoCondition).Any(element => element.Name.LocalName == itemName)) ??
                    itemGroupElements.FirstOrDefault(igElement => igElement.IsEmpty);

                // Remove existing items.
                itemGroupElements.Elements().Where(HasNoCondition).Where(element => element.Name.LocalName == itemName).Remove();

                // Remove empty item groups except the one which will be filled.
                itemGroupElements.Where(igElement => igElement.IsEmpty).Skip(1).Remove();

                if (!itemsByName.Contains(itemName))
                    continue;

                var items = itemsByName[itemName];
                var itemXName = ns + itemName;

                foreach (var item in items)
                {
                    var itemActionString = item.GetMetadata("ItemAction");
                    var itemAction = ItemAction.Include;
                    if (!string.IsNullOrEmpty(itemActionString) && !Enum.TryParse(itemActionString, true, out itemAction))
                    {
                        if (!hasLoggedInvalidItemAction)
                        {
                            Log.LogWarning("One or more items have invalid ItemAction. Skipping.");
                            hasLoggedInvalidItemAction = true;
                        }
                        continue;
                    }

                    var itemElement = new XElement(itemXName, new XAttribute(itemAction.ToString(), item.ItemSpec));
                    currentItemGroupElement ??= new XElement(itemGroupXName);
                    currentItemGroupElement.Add(itemElement);

                    var includeMetadataString = item.GetMetadata("IncludeMetadata");
                    if (!string.IsNullOrEmpty(includeMetadataString))
                    {
                        var includedMetadataNames = new HashSet<string>(includeMetadataString.Split(';'), StringComparer.OrdinalIgnoreCase);
                        foreach (string metadataName in item.MetadataNames)
                        {
                            if (!includedMetadataNames.Contains(metadataName))
                                continue;

                            var metadataValue = item.GetMetadata(metadataName);
                            itemElement.Add(new XAttribute(metadataName, metadataValue));
                        }
                    }
                }

                if (currentItemGroupElement is { Document: null })
                    document.Root.Add(currentItemGroupElement);
            }
        }

        // Conditional propertes and items are not considered as setting them might have unexpected results.
        private static bool HasNoCondition(XElement element) => element.Attribute("Condition") == null;


        private enum ItemAction
        {
            Include,
            Update,
            Remove,
        }
    }
}