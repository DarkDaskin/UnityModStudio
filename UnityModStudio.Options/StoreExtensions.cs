using System;
using System.Threading.Tasks;
using System.Windows;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

public static class StoreExtensions
{
    public static async Task LoadSafeAsync(this IStore store)
    {
        try
        {
            await store.LoadAsync();
        }
        catch (Exception exception)
        {
            var storeType = store.StoreType;
            MessageBox.Show($"Failed to load {storeType}: {exception.Message}", $"Failed to load {storeType}", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static async Task SaveSafeAsync(this IStore store)
    {
        try
        {
            await store.SaveAsync();
        }
        catch (Exception exception)
        {
            var storeType = store.StoreType;
            MessageBox.Show($"Failed to save {storeType}: {exception.Message}", $"Failed to save {storeType}", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}