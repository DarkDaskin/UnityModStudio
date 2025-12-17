using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudio.Threading;
using UnityModStudio.Common;
using UnityModStudio.ProjectWizard;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.ProjectWizard;

public class RimWorldModProjectPostWizard : IWizard
{
    private string? _modPackageId;
    private string? _modAuthor;
    private string? _modName;
    private string? _modDescription;
    private string[] _selectedGameVersions = [];
    private bool _useHarmony;
    private ProjectLayout _projectLayout;

    public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
    {
        if (!TryInvokeWizard(runKind, replacementsDictionary))
            throw new WizardBackoutException();
    }

    public void ProjectFinishedGenerating(Project project)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        
        var projectFilePath = project.FullName;
        var solution = project.DTE.Solution;
        var solutionFilePath = solution.FullName;

        ProjectLayoutManager.ApplyLayout(_projectLayout, projectFilePath, solutionFilePath, 
            out var newProjectFilePath, out var startupCsPath, out var aboutXmlPath);

        if (File.Exists(aboutXmlPath))
            ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateXmlFileAsync(aboutXmlPath,
                document => RimWorldFileGenerator.UpdateMetadata(document, _modPackageId, _modAuthor, _modName, _modDescription,
                    _selectedGameVersions, _useHarmony)));
        
        ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateXmlFileAsync(newProjectFilePath,
            document => RimWorldFileGenerator.UpdateProject(document, _useHarmony)));

        solution.AddFromFile(newProjectFilePath);

        if (_projectLayout == ProjectLayout.AssetsAtTopLevel)
        {
            // Since the project file has been moved, we need to re-open the files to reflect the new paths.
            if (File.Exists(startupCsPath))
                project.DTE.ItemOperations.OpenFile(startupCsPath);
            if (File.Exists(aboutXmlPath))
                project.DTE.ItemOperations.OpenFile(aboutXmlPath);
        }
    }
    void IWizard.RunFinished() { }

    public bool ShouldAddProjectItem(string filePath) => filePath switch
    {
        "Startup.cs" => !_useHarmony,
        "Startup.Harmony.cs" => _useHarmony,
        _ => true
    };

    void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

    void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

    private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
    {
        if (runKind != WizardRunKind.AsNewProject)
            return false;

        _modPackageId = replacementsDictionary.GetString("$ModPackageId$");
        _modAuthor = replacementsDictionary.GetString("$ModAuthor$");
        _modName = replacementsDictionary.GetString("$ModName$");
        _modDescription = replacementsDictionary.GetString("$ModDescription$");
        _selectedGameVersions = replacementsDictionary.GetString("$SelectedGameVersions$")?.Split(';') ?? [];
        _useHarmony = replacementsDictionary.GetBoolean("$UseHarmony$") ?? true;
        _projectLayout = replacementsDictionary.GetEnum<ProjectLayout>("$ProjectLayout$") ?? ProjectLayout.AssetsAtTopLevel;

        return true;
    }
}