using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using UnityModStudio.Common;
using UnityModStudio.Common.Options;

namespace UnityModStudio.ProjectWizard;

public class UnityModProjectWizard : IWizard
{
    private IComponentModel? _componentModel;
    private bool _isBasicTemplate;
    private bool _dontAddProjectToSolution;
    private Game[] _selectedGames = [];
    private bool _useModLoading;

    public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _componentModel = (IComponentModel) Package.GetGlobalService(typeof(SComponentModel));
        _isBasicTemplate = replacementsDictionary.GetBoolean("$IsBasicTemplate$") ?? false;
        _dontAddProjectToSolution = replacementsDictionary.GetBoolean("$DontAddProjectToSolution$") ?? false;

        if (!TryInvokeWizard(runKind, replacementsDictionary))
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
        if (!_dontAddProjectToSolution)
            solution.AddFromFile(projectPath);

        var launchSettingsPath = Path.Combine(Path.GetDirectoryName(projectPath)!, @"Properties\launchSettings.json");
        if (File.Exists(launchSettingsPath))
        {
            var gameVersions = _selectedGames.ToDictionary(game => game.DisplayName, game => game.Version!);
            ThreadHelper.JoinableTaskFactory.Run(() => FileGenerator.UpdateJsonFileAsync(launchSettingsPath,
                root => FileGenerator.UpdateLaunchSettings(root, gameVersions)));
        }
    }

    void IWizard.RunFinished() { }

    public bool ShouldAddProjectItem(string filePath) => filePath switch
    {
        _ when !_isBasicTemplate => true,
        "Class1.cs" => !_useModLoading,
        "ModInit.cs" => _useModLoading,
        _ => true
    };

    void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

    void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

    private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
    {
        if (runKind != WizardRunKind.AsNewProject)
            return false;

        var gameRegistry = _componentModel!.GetService<IGameRegistry>();
        if (replacementsDictionary.TryGetValue("$SelectedGameIds$", out var selectedGameIdsString) && selectedGameIdsString is not null)
        {
            var selectedGameIds = selectedGameIdsString.Split(';').Select(Guid.Parse);
            _selectedGames = selectedGameIds.Select(id => gameRegistry.FindGameById(id)).OfType<Game>().ToArray();
        }
        else
        {
            var window = new ProjectWizardWindow();
            var viewModel = window.ViewModel;
            viewModel.IsBasicTemplate = _isBasicTemplate;
            _componentModel!.DefaultCompositionService.SatisfyImportsOnce(viewModel);

            if (replacementsDictionary.TryGetValue("$ModLoaderId$", out var modLoaderId))
                viewModel.ModLoaderId = modLoaderId;

            if (!window.ShowModal() ?? false)
                return false;

            _selectedGames = viewModel.GetSelectedGames();
            _useModLoading = _selectedGames.Any(game => game.DoorstopMode == DoorstopMode.DebuggingAndModLoading);
        }

        foreach (var game in _selectedGames)
            gameRegistry.EnsureAllGameProperties(game);

        // Target framework(s) must be set here, in ProjectFinishedGenerating it's too late.
        var targetFrameworks = _selectedGames.Select(game => game.TargetFrameworkMoniker).Distinct().ToArray();
        replacementsDictionary["$TargetFramework$"] = targetFrameworks.Length == 1 ? targetFrameworks[0] : "";
        replacementsDictionary["$TargetFrameworks$"] = targetFrameworks.Length > 1 ? string.Join(";", targetFrameworks) : "";
        replacementsDictionary["$BuildPackageVersion$"] = Utils.GetPackageVersion();

        return true;
    }
}