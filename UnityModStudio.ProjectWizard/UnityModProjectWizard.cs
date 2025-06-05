using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.IO;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudio.Threading;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectWizard
{
    public class UnityModProjectWizard : IWizard
    {
        private IComponentModel? _componentModel;
        private Game? _game;
        
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

            var projectPath = project.FullName;
            ThreadHelper.JoinableTaskFactory.Run(() => CleanUpProjectAsync(projectPath));
        }

        void IWizard.RunFinished() { }

        public bool ShouldAddProjectItem(string filePath) => filePath switch
        {
            "Class1.cs" => _game?.DoorstopMode != DoorstopMode.DebuggingAndModLoading,
            "ModInit.cs" => _game?.DoorstopMode == DoorstopMode.DebuggingAndModLoading,
            _ => true
        };

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

            _game = viewModel.Game;
            var selectedGameVersions = viewModel.GameVersions
                .Where(vm => vm.IsSelected)
                .Select(vm => vm.Version)
                .ToArray();
            
            replacementsDictionary["$TargetFramework$"] = viewModel.TargetFrameworkMoniker ?? "";
            replacementsDictionary["$BuildPackageVersion$"] = Utils.GetPackageVersion();
            replacementsDictionary["$GameName$"] = viewModel.GameName ?? "";
            replacementsDictionary["$GameVersion$"] = selectedGameVersions.Length <= 1 ? viewModel.GameVersion ?? "" : "";
            replacementsDictionary["$GameVersions$"] = selectedGameVersions.Length > 1 ? string.Join(";", selectedGameVersions) : "";

            return true;
        }

        private static async Task CleanUpProjectAsync(string projectPath)
        {
            await TaskScheduler.Default;

            var document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
            document.Descendants()
                .Where(element => !element.HasAttributes && !element.HasElements && string.IsNullOrEmpty(element.Value))
                .SelectMany(IncludeWhitespace)
                .Remove();
            document.Save(projectPath);
        }

        private static IEnumerable<XNode> IncludeWhitespace(XElement element)
        {
            yield return element;
            if (element.PreviousNode is XText textNode && string.IsNullOrWhiteSpace(textNode.Value))
                yield return textNode;
        }
    }
}
