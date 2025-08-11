using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Build.Tasks;

public abstract class StoreTaskBase<TStore> : Task where TStore : IStore
{
    [Required]
    public string StorePath { get; set; } = "";

    protected TStore? Store { get; private set; }

    [MemberNotNullWhen(true, nameof(Store))]
    public override bool Execute()
    {
        try
        {
            Store = GetStore();
            return true;
        }
        catch (Exception exception)
        {
            Log.LogError($"Unable to initialize {StoreName}: {exception.Message}");
            return false;
        }
    }

    protected abstract string StoreName { get; }

    protected abstract TStore CreateStore(string storePath);

    // TODO: retrieve from VS?
    private TStore GetStore()
    {
        var holder = (StoreHolder?)BuildEngine4.GetRegisteredTaskObject(typeof(StoreHolder), RegisteredTaskObjectLifetime.AppDomain);
        if (holder != null)
        {
            if (holder.StorePath == StorePath)
                return holder.Store;

            BuildEngine4.UnregisterTaskObject(typeof(StoreHolder), RegisteredTaskObjectLifetime.AppDomain);
            holder.Dispose();
        }

        var store = CreateStore(StorePath);
        store.Load();
        store.WatchForChanges = true;
        holder = new StoreHolder(store, StorePath);
        BuildEngine4.RegisterTaskObject(typeof(StoreHolder), holder, RegisteredTaskObjectLifetime.AppDomain, true);
        return store;
    }


    private class StoreHolder(TStore store, string? storePath) : IDisposable
    {
        public readonly TStore Store = store;
        public readonly string? StorePath = storePath;

        public void Dispose() => Store.Dispose();
    }
}