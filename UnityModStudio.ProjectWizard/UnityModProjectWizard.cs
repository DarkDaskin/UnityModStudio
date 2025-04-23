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
            replacementsDictionary["$BuildPackageVersion$"] = Utils.GetPackageVersion();
            replacementsDictionary["$GameName$"] = viewModel.GameName ?? "";
            replacementsDictionary["$GameVersion$"] = viewModel.GameVersion ?? "";

            return true;
        }
    }
}
