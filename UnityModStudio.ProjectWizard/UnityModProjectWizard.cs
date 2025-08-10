using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectWizard
{
    public class UnityModProjectWizard : IWizard
    {
        private IComponentModel? _componentModel;
        private Game? _game;
        private Game[] _selectedGames = [];

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
            ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateXmlFileAsync(projectPath, 
                document => FileGenerator.UpdateProject(document, _selectedGames)));
        }

        void IWizard.RunFinished() { }

        public bool ShouldAddProjectItem(string filePath) => filePath switch
        {
            "Class1.cs" => _game?.DoorstopMode != DoorstopMode.DebuggingAndModLoading,
            "ModInit.cs" => _game?.DoorstopMode == DoorstopMode.DebuggingAndModLoading,
            _ => true
        };

        void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.Name == "launchSettings.json")
            {
                var path = projectItem.Document.Path;
                Debug.Assert(_game != null, nameof(_game) + " != null");
                var gameVersions = _selectedGames.ToDictionary(game => game.DisplayName, game => game.Version!);
                ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateJsonFileAsync(path,
                    root => FileGenerator.UpdateLaunchSettings(root, _game!.DisplayName, gameVersions)));
            }
        }

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
            _selectedGames = viewModel.GameVersions
                .Where(vm => vm.IsSelected)
                .Select(vm => vm.Game)
                .ToArray();
            
            replacementsDictionary["$BuildPackageVersion$"] = Utils.GetPackageVersion();

            return true;
        }
    }
}
