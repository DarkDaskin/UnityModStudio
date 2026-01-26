using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UnityModStudio.Build;

internal static class FrameworkDependentAssemblyLoader
{
    static FrameworkDependentAssemblyLoader()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
    }

    private static Assembly? AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyFileName = new AssemblyName(args.Name).Name + ".dll";
        var basePath = Path.GetDirectoryName(typeof(FrameworkDependentAssemblyLoader).Assembly.Location)!;
        var subPath = RuntimeInformation.FrameworkDescription.Contains("Framework") ? "net461" : "netstandard2.0";
        var assemblyPath = Path.Combine(basePath, subPath, assemblyFileName);
        return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
    }

    public static void Enable() { }
}