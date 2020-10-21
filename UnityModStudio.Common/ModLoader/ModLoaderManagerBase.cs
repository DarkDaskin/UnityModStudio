using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace UnityModStudio.Common.ModLoader
{
    public abstract class ModLoaderManagerBase : IModLoaderManager
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public virtual int Priority => 0;
        public virtual string? PackageName => null;
        public virtual string? PackageVersion => null;
        public virtual string? ExampleTemplatePath => null;

        public abstract bool IsInstalled(string gamePath);

        public virtual string? GetExampleTemplatePath(string language) => null;

        protected string GetConventionalPackageName() => GetType().Assembly.GetName().Name;

        protected string? GetConventionalPackageVersion() =>
            GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        protected string? GetConventionalExampleTemplatePath(string language, string templateName, string? category = null)
        {
            const int fallbackLcid = 1033;
            return GetConventionalExampleTemplatePath(language, CultureInfo.CurrentUICulture.LCID, templateName, category) ??
                   GetConventionalExampleTemplatePath(language, fallbackLcid, templateName, category);
        }

        private string? GetConventionalExampleTemplatePath(string language, int lcid, string templateName, string? category)
        {
            var templateFileName = templateName + ".vstemplate";
            var path = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "ItemTemplates", language, category ?? "", lcid.ToString(), templateName, templateFileName);
            return File.Exists(path) ? path : null;
        }
    }
}