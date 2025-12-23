using System;
using System.Runtime.InteropServices;

namespace UnityModStudio.Common;

internal class VersionInfo
{
    public static string? GetStringFileInfo(string path, string name)
    {
        var fviSize = GetFileVersionInfoSize(path, IntPtr.Zero);
        if (fviSize == 0)
            return null;

        var fviBytes = new byte[fviSize];
        if (!GetFileVersionInfo(path, 0, fviSize, fviBytes))
            return null;

        if (!VerQueryValue(fviBytes, $@"\StringFileInfo\040904b0\{name}", out var valuePtr, out var valueLength))
            return null;

        return Marshal.PtrToStringUni(valuePtr);
    }
    
    [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetFileVersionInfoSize(string lptstrFilename, IntPtr lpdwHandle);

    [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetFileVersionInfo(string lptstrFilename, int dwHandle, int dwLen, byte[] lpData);

    [DllImport("version.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool VerQueryValue(byte[] pBlock, string lpSubBlock, out IntPtr lplpBuffer, out int puLen);

}