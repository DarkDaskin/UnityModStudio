namespace UnityModStudio.ProjectWizard.Tests;

[TestClass]
public static class AssemblyFixture
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext context) => Options.Tests.AssemblyFixture.AssemblyInit(context);

    [AssemblyCleanup]
    public static void AssemblyCleanup() => Options.Tests.AssemblyFixture.AssemblyCleanup();
}