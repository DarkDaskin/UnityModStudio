﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

			var propertiesByCondition = Properties.Select(item => new PropertyTaskItemInfo(item)).ToLookup(property => property.ConditionExpression);
			foreach (var propertyGroup in propertiesByCondition)
			{
				var propertyGroupElements = document.Root!.Elements(propertyGroupXName).Where(element => HasCondition(element, propertyGroup.Key)).ToList();
				var propertyNames = new HashSet<string>(propertyGroup.Select(property => property.Name));
				// New properties are appended to either the first property group which contains one of the existing properties, 
				// or the first existing property group.
				var newPropertyGroupElement =
					propertyGroupElements.FirstOrDefault(pgElement =>
						pgElement.Elements().Where(element => HasCondition(element, propertyGroup.Key)).Any(element => propertyNames.Contains(element.Name.LocalName))) ??
					propertyGroupElements.FirstOrDefault();

				foreach (var property in propertyGroup)
				{
					var propertyXName = ns + property.Name;
					var propertyElement = propertyGroupElements.Elements(propertyXName).LastOrDefault(HasNoCondition);

					if (propertyElement == null)
					{
						propertyElement = new XElement(propertyXName);
						newPropertyGroupElement ??= new XElement(propertyGroupXName, 
							string.IsNullOrEmpty(propertyGroup.Key) ? null : new XAttribute("Condition", propertyGroup.Key));
						newPropertyGroupElement.Add(propertyElement);
					}

					propertyElement.Value = property.Value;

					SetComment(propertyElement, property.Comment);
				}

				if (newPropertyGroupElement is { Document: null })
					document.Root.Add(newPropertyGroupElement);
			}
		}

		private void SetItems(XDocument document, XNamespace ns)
		{
			var itemGroupXName = ns + "ItemGroup";

			var itemsByCondition = Items.Select(item => new ItemTaskItemInfo(item)).ToLookup(item => item.ConditionExpression);
			foreach (var itemGroup in itemsByCondition)
			{
				var itemsByName = itemGroup.ToLookup(item => item.ItemName);

				var itemGroupElements = document.Root!.Elements(itemGroupXName).Where(element => HasCondition(element, itemGroup.Key)).ToList();

				var hasLoggedInvalidItemAction = false;

				foreach (var itemNameItem in ItemNames)
				{
					var itemName = itemNameItem.ItemSpec;

					// Items in current group are appended to either the first item group which contains one of the existing items, 
					// or the first existing empty item group.
					var currentItemGroupElement =
						itemGroupElements.FirstOrDefault(igElement =>
							igElement.Elements().Where(element => HasCondition(element, itemGroup.Key)).Any(element => element.Name.LocalName == itemName)) ??
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
						if (item.Action is null)
						{
							if (!hasLoggedInvalidItemAction)
							{
								Log.LogWarning("One or more items have invalid ItemAction. Skipping.");
								hasLoggedInvalidItemAction = true;
							}

							continue;
						}

						var itemElement = new XElement(itemXName, new XAttribute(item.Action.ToString(), item.ItemSpec));
						currentItemGroupElement ??= new XElement(itemGroupXName,
							string.IsNullOrEmpty(itemGroup.Key) ? null : new XAttribute("Condition", itemGroup.Key));
						currentItemGroupElement.Add(itemElement);

						foreach (var metadata in item.ExtraMetadata)
							itemElement.Add(new XAttribute(metadata.Key, metadata.Value));

						SetComment(itemElement, item.Comment);
					}

					if (currentItemGroupElement is { Document: null })
						document.Root.Add(currentItemGroupElement);
				}
			}
		}

		// Conditional propertes and items are not considered as setting them might have unexpected results.
		private static bool HasNoCondition(XElement element) => element.Attribute("Condition") == null;

		private static bool HasCondition(XElement element, string conditionExpression)
		{
			var attributeValue = (string?)element.Attribute("Condition");
			return string.IsNullOrEmpty(conditionExpression)
				? string.IsNullOrEmpty(attributeValue)
				: string.Equals(conditionExpression, attributeValue, StringComparison.InvariantCultureIgnoreCase);
		}

		private static void SetComment(XElement element, string comment)
		{
			if (string.IsNullOrEmpty(comment))
				return;

			var commentNode = element.NodesBeforeSelf()
				.SkipWhile(node => node.NodeType == XmlNodeType.Whitespace).Take(1)
				.OfType<XComment>().FirstOrDefault();
			if (commentNode == null)
			{
				commentNode = new XComment(comment);
				element.AddBeforeSelf(commentNode);
			}
			else
				commentNode.Value = comment;
		}


		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private enum ItemAction
		{
			Include,
			Update,
			Remove,
		}

		private abstract class TaskItemInfoBase(ITaskItem item)
		{
			public readonly string Comment = item.GetMetadata(nameof(Comment));
			public readonly string ConditionExpression = item.GetMetadata(nameof(ConditionExpression));
		}

		private class PropertyTaskItemInfo(ITaskItem item) : TaskItemInfoBase(item)
		{
			public readonly string Name = item.ItemSpec;
			public readonly string Value = item.GetMetadata(nameof(Value));
		}

		private class ItemTaskItemInfo(ITaskItem item) : TaskItemInfoBase(item)
		{
			public readonly string ItemSpec = item.ItemSpec;
			public readonly string ItemName = item.GetMetadata(nameof(ItemName));
			public readonly ItemAction? Action = ParseItemAction(item);
			public readonly IReadOnlyDictionary<string, string> ExtraMetadata = ParseExtraMetadata(item);

			private static ItemAction? ParseItemAction(ITaskItem item)
			{
				var itemActionString = item.GetMetadata("ItemAction");
				if (string.IsNullOrEmpty(itemActionString))
					return ItemAction.Include;
				return Enum.TryParse(itemActionString, true, out ItemAction itemAction) ? itemAction : null;
			}

			private static IReadOnlyDictionary<string, string> ParseExtraMetadata(ITaskItem item)
			{
				var extraMetadata = new Dictionary<string, string>();
				var includeMetadataString = item.GetMetadata("IncludeMetadata");
				if (!string.IsNullOrEmpty(includeMetadataString))
				{
					var includedMetadataNames = new HashSet<string>(includeMetadataString.Split(';'), StringComparer.InvariantCultureIgnoreCase);
					foreach (string metadataName in item.MetadataNames)
					{
						if (!includedMetadataNames.Contains(metadataName))
							continue;

						var metadataValue = item.GetMetadata(metadataName);
						extraMetadata.Add(metadataName, metadataValue);
					}
				}
				return extraMetadata;
			}
		}
	}
}