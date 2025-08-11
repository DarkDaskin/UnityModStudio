using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace UnityModStudio.Common.Options;

public interface IStore : IDisposable
{
    bool WatchForChanges { get; set; }
    string StoreType { get; }

    Task LoadAsync();
    Task SaveAsync();
}

public abstract class StoreBase<T>: IStore
{
    private readonly string _storePath;
    private readonly FileSystemWatcher _fsWatcher;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public bool WatchForChanges
    {
        get => _fsWatcher.EnableRaisingEvents;
        set => _fsWatcher.EnableRaisingEvents = value;
    }

    public virtual string StoreType => GetType().Name;

    protected StoreBase(string storePath)
    {
        _storePath = storePath;
        var directoryName = Path.GetDirectoryName(_storePath);
        if (string.IsNullOrEmpty(directoryName))
            directoryName = ".";
        Directory.CreateDirectory(directoryName);
        _fsWatcher = new FileSystemWatcher(directoryName, Path.GetFileName(_storePath))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = false,
            IncludeSubdirectories = false
        };
        _fsWatcher.Changed += OnStoreChanged;
    }

    private async void OnStoreChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            await LoadAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Failed to load {StoreType}: {exception.Message}");
        }
    }

    public async Task LoadAsync()
    {
        await _loadLock.WaitAsync();

        Reset();

        try
        {
            using var stream = File.OpenRead(_storePath);

            if (stream.Length == 0)
                return;

            var data = await JsonSerializer.DeserializeAsync<T>(stream);
            if (data != null)
                Import(data);
        }
        catch (FileNotFoundException)
        {
            // Remain empty.
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public async Task SaveAsync()
    {
        var oldWatchForChanges = WatchForChanges;
        WatchForChanges = false;

        try
        {
            using var stream = File.Open(_storePath, FileMode.Create);
            await JsonSerializer.SerializeAsync(stream, Export());
        }
        finally
        {
            WatchForChanges = oldWatchForChanges;
        }
    }

    public virtual void Dispose()
    {
        _fsWatcher.Dispose();
        _loadLock.Dispose();
    }

    protected abstract void Reset();

    protected abstract void Import(T data);

    protected abstract T Export();
}

public static class StoreExtensions
{
    public static void Load(this IStore store) => store.LoadAsync().GetAwaiter().GetResult();

    public static void Save(this IStore store) => store.SaveAsync().GetAwaiter().GetResult();
}