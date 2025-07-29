using System;
using System.Threading.Tasks;
using System.Windows;
using UnityModStudio.Common.Options;

namespace UnityModStudio.Options;

public static class GameRegistryExtensions
{
    public static async Task LoadSafeAsync(this IGameRegistry gameRegistry)
    {
        try
        {
            await gameRegistry.LoadAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Failed to load game registry: {exception.Message}", "Failed to load game registry", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static async Task SaveSafeAsync(this IGameRegistry gameRegistry)
    {
        try
        {
            await gameRegistry.SaveAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show($"Failed to save game registry: {exception.Message}", "Failed to save game registry", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}