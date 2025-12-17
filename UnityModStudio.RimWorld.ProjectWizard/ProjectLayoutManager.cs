using System.IO;
using System.Linq;
using UnityModStudio.RimWorld.Common.Options;

namespace UnityModStudio.RimWorld.ProjectWizard;

public static class ProjectLayoutManager
{
    private static readonly string[] KnownProjectFileNames = ["Startup.cs"];
    private static readonly string[] KnownProjectDirectoryNames = ["Properties"];

    public static void ApplyLayout(ProjectLayout layout, string projectFilePath, string solutionFilePath,
        out string newProjectFilePath, out string newStartupCsPath, out string newAboutXmlPath)
    {
        var projectFile = new FileInfo(projectFilePath);
        var projectDirectory = projectFile.Directory!;
        var solutionFile = string.IsNullOrEmpty(solutionFilePath) ? null : new FileInfo(solutionFilePath);
        var solutionDirectory = solutionFile?.Directory ?? projectDirectory;

        if (layout == ProjectLayout.ProjectAtTopLevel)
        {
            newProjectFilePath = projectFilePath;
            newStartupCsPath = Path.Combine(projectDirectory.FullName, "Startup.cs");
            newAboutXmlPath = Path.Combine(projectDirectory.FullName, @"Assets\About\About.xml");
            return;
        }

        var sourcesDirectory = Directory.CreateDirectory(Path.Combine(solutionDirectory.FullName, "Sources"));
        newAboutXmlPath = Path.Combine(solutionDirectory.FullName, @"About\About.xml");

        if (projectDirectory.FullName == solutionDirectory.FullName)
        {
            newProjectFilePath = Path.Combine(sourcesDirectory.FullName, projectFile.Name);
            newStartupCsPath = Path.Combine(sourcesDirectory.FullName, "Startup.cs");

            File.Move(projectFilePath, newProjectFilePath);
            foreach (var subDirectory in solutionDirectory.GetDirectories())
                if (KnownProjectDirectoryNames.Contains(subDirectory.Name))
                    subDirectory.MoveTo(Path.Combine(sourcesDirectory.FullName, subDirectory.Name));
            foreach (var file in solutionDirectory.GetFiles())
                if (KnownProjectFileNames.Contains(file.Name))
                    file.MoveTo(Path.Combine(sourcesDirectory.FullName, file.Name));
        }
        else
        {
            newProjectFilePath = Path.Combine(sourcesDirectory.FullName, projectDirectory.Name, projectFile.Name);
            newStartupCsPath = Path.Combine(sourcesDirectory.FullName, projectDirectory.Name, "Startup.cs");

            projectDirectory.MoveTo(Path.Combine(sourcesDirectory.FullName, projectDirectory.Name));
        }

        var assetsDirectory = projectDirectory.EnumerateDirectories("Assets").Single();
        foreach (var subDirectory in assetsDirectory.GetDirectories())
            subDirectory.MoveTo(Path.Combine(solutionDirectory.FullName, subDirectory.Name));
        foreach (var file in assetsDirectory.GetFiles())
            file.MoveTo(Path.Combine(solutionDirectory.FullName, file.Name));
        assetsDirectory.Delete();
    }
}