using Microsoft.VisualStudio.Sdk.TestFramework;

namespace UnityModStudio.Options.Tests;

[TestClass]
public static class AssemblyFixture
{
    internal static GlobalServiceProvider MockServiceProvider { get; private set; } = null!;

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context) => MockServiceProvider = new GlobalServiceProvider();

    [AssemblyCleanup]
    public static void AssemblyCleanup() => MockServiceProvider.Dispose();
}