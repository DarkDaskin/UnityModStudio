using System.Diagnostics;
using System.Globalization;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;

namespace UnityModStudio.Build.Tests;

[TestClass]
public static class AssemblyFixture
{
    public static TestBinaryLogger BinaryLogger { get; } = new();

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        MSBuildLocator.RegisterDefaults();

        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        // This must be a separate method, so MSBuildLocator can do its work before MSBuild assemblies are accessed.
        RegisterGlobalBinaryLogger();
    }

    private static void RegisterGlobalBinaryLogger() => ProjectCollection.GlobalProjectCollection.RegisterLogger(BinaryLogger);

    // TODO: kill VBCSCompiler
    [AssemblyCleanup]
    public static void AssemblyCleanup() => BuildManager.DefaultBuildManager.ShutdownAllNodes();
}