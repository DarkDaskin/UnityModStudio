using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;

namespace UnityModStudio.Build.Tests;

[TestClass]
public static class AssemblyFixture
{
    public static TestBinaryLogger BinaryLogger { get; } = new();

    [ModuleInitializer]
    public static void ModuleInitializer() => MSBuildLocator.RegisterDefaults();

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        ProjectCollection.GlobalProjectCollection.RegisterLogger(BinaryLogger);
    }

    // TODO: kill VBCSCompiler
    [AssemblyCleanup]
    public static void AssemblyCleanup() => BuildManager.DefaultBuildManager.ShutdownAllNodes();
}