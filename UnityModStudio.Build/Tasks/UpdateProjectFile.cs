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
        public ITaskItem? ProjectFile { get; set; }

        [Required]
        public ITaskItem[] Properties { get; set; } = Array.Empty<ITaskItem>();

        // NOTE: This will reformat the document, SaveOptions.DisableFormatting only preserves whitespace between elements.
        // TODO: Preserve as much formatting as possible.
        public override bool Execute()
        {
            if (string.IsNullOrWhiteSpace(ProjectFile?.ItemSpec))
            {
                Log.LogError("Project file is not specified.");
                return false;
            }

            if (Properties.Length == 0)
            {
                Log.LogWarning("No properties to set.");
                return true;
            }

            var document = File.Exists(ProjectFile!.ItemSpec)
                ? XDocument.Load(ProjectFile.ItemSpec)
                : new XDocument(new XElement(MsbuildNamespace + "Project"));

            // Default namespace may be implicit or explicit, determine that from the root element.
            var ns = document.Root?.Name.Namespace ?? XNamespace.None;
            if (document.Root?.Name.LocalName != "Project" || (ns != XNamespace.None && ns != MsbuildNamespace))
            {
                Log.LogError("Invalid root element.");
                return false;
            }
            
            var propertyGroupXName = ns + "PropertyGroup";
            var propertyGroupElements = document.Root.Elements(propertyGroupXName).Where(HasNoCondition).ToList();
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
            
            document.Save(ProjectFile.ItemSpec);

            return true;


            // Conditional propertes are not considered as setting them might have unexpected results.
            static bool HasNoCondition(XElement element) => element.Attribute("Condition") == null;
        }
    }
}