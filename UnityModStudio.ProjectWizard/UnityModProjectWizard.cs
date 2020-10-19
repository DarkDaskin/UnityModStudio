using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;

namespace UnityModStudio.ProjectWizard
{
    public class UnityModProjectWizard : IWizard
    {
        private string? _gamePath;
        
        public void RunStarted(object automationObject, Dictionary<string, string?> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if(!TryInvokeWizard(runKind, replacementsDictionary))
                throw new WizardBackoutException();
        }

        public void ProjectFinishedGenerating(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveGamePath(project.FullName);
        }

        bool IWizard.ShouldAddProjectItem(string filePath) => true;

        void IWizard.RunFinished() { }

        void IWizard.BeforeOpeningFile(ProjectItem projectItem) { }

        void IWizard.ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        private bool TryInvokeWizard(WizardRunKind runKind, Dictionary<string, string?> replacementsDictionary)
        {
            if (runKind != WizardRunKind.AsNewProject)
                return false;

            var window = new ProjectWizardWindow();
            if (!window.ShowDialog() ?? false)
                return false;

            _gamePath = window.ViewModel.GamePath;

            replacementsDictionary["$TargetFramework$"] = window.ViewModel.TargetFrameworkMoniker;
            replacementsDictionary["$BuildPackageVersion$"] = GetBuildPackageVersion();

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
    }
}
