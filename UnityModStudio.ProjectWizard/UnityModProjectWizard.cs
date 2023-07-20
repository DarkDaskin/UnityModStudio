using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        
        public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _componentModel = (IComponentModel) Package.GetGlobalService(typeof(SComponentModel));

            if(!TryInvokeWizard(runKind, replacementsDictionary))
                throw new WizardBackoutException();
        }

        void IWizard.ProjectFinishedGenerating(Project project) { }

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
            
            replacementsDictionary["$TargetFramework$"] = viewModel.TargetFrameworkMoniker ?? "";
            replacementsDictionary["$BuildPackageVersion$"] = GetBuildPackageVersion();
            replacementsDictionary["$GameInstanceId$"] = viewModel.Game?.Id.ToString();
            replacementsDictionary["$GamePath$"] = Utils.AppendTrailingSlash(viewModel.GamePath!);
            replacementsDictionary["$GameName$"] = viewModel.GameName ?? "";
            replacementsDictionary["$GameVersion$"] = viewModel.GameVersion ?? "";
            // Must be known without running any targets to be picked up by launchSettings.json
            replacementsDictionary["$GameExecutableFileName$"] = viewModel.GameExecutableFileName ?? "";
            replacementsDictionary["$DoorstopMode$"] = viewModel.Game?.DoorstopMode.ToString();

            // In .props there is no TargetFramework, in .targets it's too late, so it has to be specified in .csproj.
            if (!viewModel.TargetFrameworkMoniker?.StartsWith("netstandard") ?? false)
                replacementsDictionary["$DisableImplicitFrameworkReferences$"] = "true";

            return true;
        }

        // Assuming all projects have the same version, which is ensured by version.json.
        private static string GetBuildPackageVersion() => Utils.GetPackageVersion(typeof(UnityModProjectWizard).Assembly)!;
    }
}
