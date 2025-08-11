using System;
using System.Buffers;
using System.IO;
using System.Numerics;
using System.Text;

namespace UnityModStudio.Common;

public static class Utils
{
    private const string ErrorCodeDataKey = "ErrorCode";
    private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

    /// <summary>
    /// Get NuGet package version of Unity Mod Studio.
    /// </summary>
    /// <returns>Version string.</returns>
    /// <remarks>This assumes all projects have the same version, which is ensured by version.json.</remarks>
    public static string GetPackageVersion() =>
        ThisAssembly.IsPublicRelease
            ? ThisAssembly.AssemblyInformationalVersion.Split('+')[0]
            : ThisAssembly.AssemblyInformationalVersion.Replace("+", "-g");

    /// <summary>
    /// Appends trailing slash to a path if missing.
    /// </summary>
    /// <param name="path">Input path.</param>
    /// <returns>Path ending with trailing slash.</returns>
    public static string AppendTrailingSlash(string path) =>
        path.EndsWith(DirectorySeparator) ? path : path + DirectorySeparator;

    public static string? SanitizeGameVersion(string? version)
    {
        if (version is null)
            return null;

        var sb = new StringBuilder(version);
        for (var i = 0; i < sb.Length; i++)
            if (!char.IsLetterOrDigit(sb[i]))
                sb[i] = '_';
        return sb.ToString();
    }

    // Inspired by https://dev.to/emrahsungu/how-to-compare-two-files-using-net-really-really-fast-2pd9
    public static bool AreStreamsEqual(Stream streamA, Stream streamB)
    {
        if (streamA.Length != streamB.Length)
            return false;

        const int bufferLength = 4096 * 32;
        var bufferA = ArrayPool<byte>.Shared.Rent(bufferLength);
        var bufferB = ArrayPool<byte>.Shared.Rent(bufferLength);
        try
        {
            while (true)
            {
                var bytesReadA = ReadIntoBuffer(streamA, bufferA);
                var bytesReadB = ReadIntoBuffer(streamB, bufferB);

                if (bytesReadA != bytesReadB)
                    return false;

                if (bytesReadA == 0)
                    return true;

                var totalProcessed = 0;
                while (totalProcessed < bufferA.Length)
                {
                    if (!Vector.EqualsAll(new Vector<byte>(bufferA, totalProcessed), new Vector<byte>(bufferB, totalProcessed)))
                        return false;

                    totalProcessed += Vector<byte>.Count;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bufferA);
            ArrayPool<byte>.Shared.Return(bufferB);
        }

        static int ReadIntoBuffer(Stream stream, byte[] buffer)
        {
            var totalBytesRead = 0;
            while (totalBytesRead < buffer.Length)
            {
                var bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
                if (bytesRead == 0)
                    return totalBytesRead;

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }
    }

    public static TException WithErrorCode<TException>(this TException exception, string code) where TException : Exception
    {
        exception.Data[ErrorCodeDataKey] = code;
        return exception;
    }

    public static string? GetErrorCode(this Exception exception)
    {
        if (exception.Data.Contains(ErrorCodeDataKey))
            return exception.Data[ErrorCodeDataKey] as string;
        return null;
    }
}