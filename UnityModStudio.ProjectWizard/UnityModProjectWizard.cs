using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using UnityModStudio.Common;
using UnityModStudio.Common.ModLoader;

namespace UnityModStudio.ProjectWizard
{
    [SuppressMessage("vs-threading", "VSTHRD010", Justification = "All code runs on main thread.")]
    public class UnityModProjectWizard : IWizard
    {
        private static readonly IXmlNamespaceResolver XmlNsResolver;

        private IComponentModel? _componentModel;
        private IModLoaderManager _modLoaderManager = NullModLoaderManager.Instance;

        static UnityModProjectWizard()
        {
            var nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("t", "http://schemas.microsoft.com/developer/vstemplate/2005");
            nsManager.AddNamespace("p", "http://schemas.microsoft.com/developer/msbuild/2003");
            XmlNsResolver = nsManager;
        }
        
        public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _componentModel = (IComponentModel) Package.GetGlobalService(typeof(SComponentModel));

            if(!TryInvokeWizard(runKind, replacementsDictionary))
                throw new WizardBackoutException();
        }

        public void ProjectFinishedGenerating(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            AddModLoaderReference(project);

            AddModLoaderExampleItem(project);
        }

        void IWizard.RunFinished() { }

        bool IWizard.ShouldAddProjectItem(string filePath) => true;

        void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

        void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
        {
            if (runKind != WizardRunKind.AsNewProject)
                return false;

            var window = new ProjectWizardWindow();
            var viewModel = window.ViewModel;
            _componentModel!.DefaultCompositionService.SatisfyImportsOnce(viewModel);
            if (!window.ShowModal() ?? false)
                return false;

            _modLoaderManager = viewModel.SelectedModLoader;

            replacementsDictionary["$TargetFramework$"] = viewModel.TargetFrameworkMoniker;
            replacementsDictionary["$BuildPackageVersion$"] = GetBuildPackageVersion();
            replacementsDictionary["$GamePath$"] = viewModel.GamePath;
            replacementsDictionary["$GameDisplayName$"] = viewModel.Game?.DisplayName;
            // Just for information.
            replacementsDictionary["$GameName$"] = viewModel.GameName;
            // Must be known without running any targets to be picked up by launchSettings.json
            replacementsDictionary["$GameExecutableFileName$"] = viewModel.GameExecutableFileName;

            // In .props there is no TargetFramework, in .targets it's too late, so it has to be specified in .csproj.
            if (!viewModel.TargetFrameworkMoniker?.StartsWith("netstandard") ?? false)
                replacementsDictionary["$DisableImplicitFrameworkReferences$"] = "true";

            return true;
        }

        // Assuming all projects have the same version, which is ensured by version.json.
        private static string GetBuildPackageVersion() => Utils.GetPackageVersion(typeof(UnityModProjectWizard).Assembly)!;
        
        private void AddModLoaderReference(Project project)
        {
            if (_modLoaderManager.PackageName == null || _modLoaderManager.PackageVersion == null)
                return;

            EditProject(project, document =>
            {
                var packageItemGroupElement = document.XPathSelectElement("/Project/ItemGroup[PackageReference]")!;
                packageItemGroupElement.Add(new XElement("PackageReference", 
                    new XAttribute("Include", _modLoaderManager.PackageName),
                    new XAttribute("Version", _modLoaderManager.PackageVersion),
                    // Install as development dependency only.
                    new XAttribute("IncludeAssets", "build;analyzers")));
            });
        }

        private void AddModLoaderExampleItem(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var exampleTemplatePath = _modLoaderManager.GetExampleTemplatePath(GetLanguage(project.FullName));
            if (exampleTemplatePath == null)
                return;

            var fileName = GetDefaultFileName(exampleTemplatePath);
            project.ProjectItems.AddFromTemplate(exampleTemplatePath, fileName);

            // Delete our dummy class since mod loader template provides a better one.
            project.ProjectItems.Cast<ProjectItem>()
                .First(item => Path.GetFileNameWithoutExtension(item.Name).Equals("Class1", StringComparison.OrdinalIgnoreCase))
                .Delete();
        }

        private static void EditProject(Project project, Action<XDocument> action)
        {
            if (!project.Saved)
                project.Save();

            var document = XDocument.Load(project.FullName);
            action(document);
            document.Save(project.FullName);
        }

        private static string GetLanguage(string projectFilePath) => Path.GetExtension(projectFilePath).ToLowerInvariant() switch
        {
            ".csproj" => "CSharp",
            ".vbproj" => "VisualBasic",
            ".vcxproj" => "VC",
            _ => ""
        };

        private static string GetDefaultFileName(string itemTemplatePath)
        {
            var document = XDocument.Load(itemTemplatePath);
            return document.XPathSelectElement("/t:VSTemplate/t:TemplateData/t:DefaultName", XmlNsResolver)?.Value ?? 
                   document.XPathSelectElement("/t:VSTemplate/t:TemplateContent/t:ProjectItem", XmlNsResolver)?.Value ?? 
                   throw new InvalidOperationException($"Item template at '{itemTemplatePath}' does not define a file name.");
        }
    }
}
