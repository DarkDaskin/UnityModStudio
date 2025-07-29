using Microsoft.Build.Framework;

namespace UnityModStudio.Build.Tests;

public class TestLogger : ILogger
{
    public List<BuildErrorEventArgs> BuildErrors { get; set; } = [];
    public List<BuildWarningEventArgs> BuildWarnings { get; set; } = [];

    public void Initialize(IEventSource eventSource)
    {
        eventSource.ErrorRaised += (_, args) => BuildErrors.Add(args);
        eventSource.WarningRaised += (_, args) => BuildWarnings.Add(args);
    }

    public void Shutdown() { }

    public LoggerVerbosity Verbosity { get; set; }
    public string? Parameters { get; set; }
}