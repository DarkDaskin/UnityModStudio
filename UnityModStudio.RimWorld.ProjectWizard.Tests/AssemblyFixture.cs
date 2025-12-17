namespace UnityModStudio.RimWorld.ProjectWizard.Tests;

[TestClass]
public static class AssemblyFixture
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context) => UnityModStudio.Options.Tests.AssemblyFixture.AssemblyInit(context);

    [AssemblyCleanup]
    public static void AssemblyCleanup() => UnityModStudio.Options.Tests.AssemblyFixture.AssemblyCleanup();
}