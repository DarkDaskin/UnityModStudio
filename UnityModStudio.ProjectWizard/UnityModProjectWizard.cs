using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using UnityModStudio.Common.ModLoader;

namespace UnityModStudio.ProjectWizard
{
    public class UnityModProjectWizard : IWizard
    {
        private IComponentModel? _componentModel;
        private string? _gamePath;
        private IModLoaderManager _modLoaderManager = NullModLoaderManager.Instance;
        
        public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _componentModel = (IComponentModel) Package.GetGlobalService(typeof(SComponentModel));

            if(!TryInvokeWizard(runKind, replacementsDictionary))
                throw new WizardBackoutException();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            // Skip example ModInit files for mod loaders except the selected one.
            // TODO: store example files in corresponding packages.
            var nameParts = Path.GetFileNameWithoutExtension(filePath).Split('.');
            if (nameParts.Length == 2 && 
                nameParts[0].Equals("ModInit", StringComparison.OrdinalIgnoreCase) &&
                !nameParts[1].Equals(_modLoaderManager.Id, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        public void ProjectFinishedGenerating(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveGamePath(project.FullName);

            AddModLoaderReference(project.FullName);
        }

        void IWizard.RunFinished() { }

        void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

        void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
        {
            if (runKind != WizardRunKind.AsNewProject)
                return false;

            var window = new ProjectWizardWindow();
            var viewModel = window.ViewModel;
            _componentModel!.DefaultCompositionService.SatisfyImportsOnce(viewModel);
            if (!window.ShowDialog() ?? false)
                return false;

            _gamePath = viewModel.GamePath;
            _modLoaderManager = viewModel.SelectedModLoader;

            replacementsDictionary["$TargetFramework$"] = viewModel.TargetFrameworkMoniker;
            replacementsDictionary["$GameName$"] = viewModel.GameName;
            replacementsDictionary["$BuildPackageVersion$"] = GetBuildPackageVersion();

            // In .props there is no TargetFramework, in .targets it's too late, so it has to be specified in .csproj.
            if (!viewModel.TargetFrameworkMoniker?.StartsWith("netstandard") ?? false)
                replacementsDictionary["$DisableImplicitFrameworkReferences$"] = "true";

            return true;
        }

        // Assuming all projects have the same version, which is ensured by Directory.Build.props.
        private static string GetBuildPackageVersion() => 
            typeof(UnityModProjectWizard).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        private void SaveGamePath(string projectFilePath)
        {
            var userFilePath = Path.ChangeExtension(projectFilePath, ".csproj.user");
            var document = XDocument.Load(userFilePath);
            document.XPathSelectElement("//*[local-name()='GamePath']")!.Value = _gamePath;
            document.Save(userFilePath);
        }

        private void AddModLoaderReference(string projectFilePath)
        {
            if (_modLoaderManager.PackageName == null || _modLoaderManager.PackageVersion == null)
                return;

            var document = XDocument.Load(projectFilePath);
            var packageItemGroupElement = document.XPathSelectElement("/Project/ItemGroup[PackageReference]")!;
            packageItemGroupElement.Add(new XElement("PackageReference", 
                new XAttribute("Include", _modLoaderManager.PackageName),
                new XAttribute("Version", _modLoaderManager.PackageVersion),
                // Install as development dependency only.
                new XAttribute("IncludeAssets", "build;analyzers")));
            document.Save(projectFilePath);
        }
    }
}
