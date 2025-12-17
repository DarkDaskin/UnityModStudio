using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;

namespace UnityModStudio.RimWorld.ProjectWizard;

public class RimWorldModProjectPreWizard : IWizard
{
    private IComponentModel? _componentModel;

    public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));

        if (!TryInvokeWizard(runKind, replacementsDictionary))
            throw new WizardBackoutException();
    }

    void IWizard.ProjectFinishedGenerating(Project project) { }

    void IWizard.RunFinished() { }

    public bool ShouldAddProjectItem(string filePath) => true;

    void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

    void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

    private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
    {
        if (runKind != WizardRunKind.AsNewProject)
            return false;

        var window = new ProjectWizardWindow();
        var viewModel = window.ViewModel;
        viewModel.ProjectName = replacementsDictionary["$projectname$"];
        _componentModel!.DefaultCompositionService.SatisfyImportsOnce(viewModel);

        if (!window.ShowModal() ?? false)
            return false;

        var selectedGames = viewModel.GetSelectedGames();
        replacementsDictionary["$SelectedGameIds$"] = string.Join(";", selectedGames.Select(game => game.Id));
        replacementsDictionary["$SelectedGameVersions$"] = string.Join(";", selectedGames.Select(game => game.Version));
        replacementsDictionary["$ModPackageId$"] = viewModel.ModPackageId?.Trim();
        replacementsDictionary["$ModAuthor$"] = viewModel.ModAuthor?.Trim();
        replacementsDictionary["$ModName$"] = viewModel.ModName?.Trim();
        replacementsDictionary["$ModDescription$"] = viewModel.ModDescription?.Trim();
        replacementsDictionary["$UseHarmony$"] = viewModel.UseHarmony.ToString();
        replacementsDictionary["$ProjectLayout$"] = viewModel.ProjectLayout.ToString();

        return true;
    }
}