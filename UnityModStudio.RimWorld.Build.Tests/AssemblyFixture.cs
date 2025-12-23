using UnityModStudio.Build.Tests;

namespace UnityModStudio.RimWorld.Build.Tests;

[TestClass]
public static class AssemblyFixture
{
    public static TestBinaryLogger BinaryLogger => 
        UnityModStudio.Build.Tests.AssemblyFixture.BinaryLogger;
    
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context) => 
        UnityModStudio.Build.Tests.AssemblyFixture.AssemblyInitialize(context);

    [AssemblyCleanup]
    public static void AssemblyCleanup() => 
        UnityModStudio.Build.Tests.AssemblyFixture.AssemblyCleanup();
}