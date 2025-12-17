using System;
using System.ComponentModel.Composition;
using System.IO;
using UnityModStudio.Common.Options;

namespace UnityModStudio.RimWorld.Common.Options;

[InheritedExport]
public interface IRimWorldSettingsManager : IStore
{
    RimWorldSettings Settings { get; }
}

public class RimWorldSettingsManager(string storePath) : StoreBase<RimWorldSettings>(storePath), IRimWorldSettingsManager
{
    public RimWorldSettingsManager() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"UnityModStudio\RimWorldSettings.json"))
    { }

    public RimWorldSettings Settings { get; private set; } = new();

    protected override void Reset() => Settings = new();

    protected override void Import(RimWorldSettings data) => Settings = data;

    protected override RimWorldSettings Export() => Settings;

    public override string StoreType => "RimWorld settings";
}