using Microsoft.Build.Utilities;

namespace UnityModStudio.Build;

internal static class TaskLoggingHelperExtensions
{
    public static void LogErrorWithCode(this TaskLoggingHelper log, string? code, string message, params object[] messageArgs) => 
        log.LogError(null, code, null, null, null, 0, 0, 0, 0, message, messageArgs);

    public static void LogWarningWithCode(this TaskLoggingHelper log, string? code, string message, params object[] messageArgs) => 
        log.LogWarning(null, code, null, null, null, 0, 0, 0, 0, message, messageArgs);
}