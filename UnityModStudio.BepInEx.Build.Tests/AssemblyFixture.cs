using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using UnityModStudio.Build.Tests;

namespace UnityModStudio.BepInEx.Build.Tests;

[TestClass]
public static class AssemblyFixture
{
    public static TestBinaryLogger BinaryLogger => 
        UnityModStudio.Build.Tests.AssemblyFixture.BinaryLogger;

    //[ModuleInitializer]
    //public static void ModuleInitializer() => MSBuildLocator.RegisterDefaults();

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context) => 
        UnityModStudio.Build.Tests.AssemblyFixture.AssemblyInitialize(context);

    [AssemblyCleanup]
    public static void AssemblyCleanup() => 
        UnityModStudio.Build.Tests.AssemblyFixture.AssemblyCleanup();
}