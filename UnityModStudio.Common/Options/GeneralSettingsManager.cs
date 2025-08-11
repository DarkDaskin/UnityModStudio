using System;
using System.ComponentModel.Composition;
using System.IO;

namespace UnityModStudio.Common.Options;

[InheritedExport]
public interface IGeneralSettingsManager : IStore
{
    GeneralSettings Settings { get; }
}

public class GeneralSettingsManager(string storePath) : StoreBase<GeneralSettings>(storePath), IGeneralSettingsManager
{
    public GeneralSettingsManager() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"UnityModStudio\GeneralSettings.json")) { }

    public GeneralSettings Settings { get; private set; } = new();

    protected override void Reset() => Settings = new();

    protected override void Import(GeneralSettings data) => Settings = data;

    protected override GeneralSettings Export() => Settings;

    public override string StoreType => "general settings";
}