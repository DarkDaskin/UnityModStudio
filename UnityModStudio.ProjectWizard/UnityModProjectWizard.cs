using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
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
            var solution = project.DTE.Solution;
            // Temporarily remove the project from the solution to avoid potential change conflict.
            // Without that, the "One or more projects have changes which cannot be automatically handled by the project system" error may occur.
            solution.Remove(project);
            ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateXmlFileAsync(projectPath, 
                document => FileGenerator.UpdateProject(document, _selectedGames)));
            solution.AddFromFile(projectPath);

            var launchSettingsPath = Path.Combine(Path.GetDirectoryName(projectPath)!, @"Properties\launchSettings.json");
            if (File.Exists(launchSettingsPath))
            {
                Debug.Assert(_game != null, nameof(_game) + " != null");
                var gameVersions = _selectedGames.ToDictionary(game => game.DisplayName, game => game.Version!);
                ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateJsonFileAsync(launchSettingsPath,
                    root => FileGenerator.UpdateLaunchSettings(root, _game!.DisplayName, gameVersions)));
            }
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
            _selectedGames = viewModel.GetSelectedGames();
            
            // Target framework(s) must be set here, in ProjectFinishedGenerating it's too late.
            var targetFrameworks = _selectedGames.Select(game => game.TargetFrameworkMoniker).Distinct().ToArray();
            replacementsDictionary["$TargetFramework$"] = targetFrameworks.Length == 1 ? targetFrameworks[0] : "";
            replacementsDictionary["$TargetFrameworks$"] = targetFrameworks.Length > 1 ? string.Join(";", targetFrameworks) : "";
            replacementsDictionary["$BuildPackageVersion$"] = Utils.GetPackageVersion();

            return true;
        }
    }
}
