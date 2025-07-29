namespace UnityModStudio.Common.Tests;

public static class TestUtils
{
    public static void CopyDirectory(string sourcePath, string destinationPath, Func<string, bool>? predicate = null)
    {
        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        foreach (var sourceFilePath in Directory.EnumerateFiles(sourcePath))
            if (predicate?.Invoke(sourceFilePath) ?? true)
                File.Copy(sourceFilePath, Path.Combine(destinationPath, Path.GetFileName(sourceFilePath)));

        foreach (var sourceSubDirectoryPath in Directory.EnumerateDirectories(sourcePath))
            if (predicate?.Invoke(sourceSubDirectoryPath) ?? true)
                CopyDirectory(sourceSubDirectoryPath, Path.Combine(destinationPath, Path.GetFileName(sourceSubDirectoryPath)), predicate);
    }
}