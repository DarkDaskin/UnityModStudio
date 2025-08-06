using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace UnityModStudio.Build.Tests;

public class TestBinaryLogger : ILogger, IEventSource
{
    private IEventSource? _eventSource;
    private BinaryLogger? _innerLogger;
    private string? _testName;
    private int _initCount = 0;

    public void InitializeTest(TestContext context)
    {
        _testName = $"{context.ManagedType?.Split('.').Last()}.{context.TestName}";
        ReplaceLogger(null);
    }

    public void SetSuffix(string suffix) => ReplaceLogger(suffix);

    private void ReplaceLogger(string? suffix)
    {
#if DEBUG
        _innerLogger?.Shutdown();

        _innerLogger = new BinaryLogger
        {
            CollectProjectImports = BinaryLogger.ProjectImportsCollectionMode.Embed,
            Parameters = $"{_testName}{suffix}.binlog",
        };
        _innerLogger.Initialize(this);
#endif
    }

    public void Initialize(IEventSource eventSource)
    {
#if DEBUG
        _eventSource = eventSource;

        _eventSource.MessageRaised += OnMessageRaised;
        _eventSource.ErrorRaised += OnErrorRaised;
        _eventSource.WarningRaised += OnWarningRaised;
        _eventSource.BuildStarted += OnBuildStarted;
        _eventSource.BuildFinished += OnBuildFinished;
        _eventSource.ProjectStarted += OnProjectStarted;
        _eventSource.ProjectFinished += OnProjectFinished;
        _eventSource.TargetStarted += OnTargetStarted;
        _eventSource.TargetFinished += OnTargetFinished;
        _eventSource.TaskStarted += OnTaskStarted;
        _eventSource.TaskFinished += OnTaskFinished;
        _eventSource.CustomEventRaised += OnCustomEventRaised;
        _eventSource.StatusEventRaised += OnStatusEventRaised;
        _eventSource.AnyEventRaised += OnAnyEventRaised;

        _initCount++;
#endif
    }

    public void Shutdown()
    {
#if DEBUG
        if (_initCount == 0)
            return;

        if (--_initCount == 0)
            _innerLogger?.Shutdown();
#endif
    }

    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Diagnostic;
    public string? Parameters { get; set; }

    public event BuildMessageEventHandler? MessageRaised;
    public event BuildErrorEventHandler? ErrorRaised;
    public event BuildWarningEventHandler? WarningRaised;
    public event BuildStartedEventHandler? BuildStarted;
    public event BuildFinishedEventHandler? BuildFinished;
    public event ProjectStartedEventHandler? ProjectStarted;
    public event ProjectFinishedEventHandler? ProjectFinished;
    public event TargetStartedEventHandler? TargetStarted;
    public event TargetFinishedEventHandler? TargetFinished;
    public event TaskStartedEventHandler? TaskStarted;
    public event TaskFinishedEventHandler? TaskFinished;
    public event CustomBuildEventHandler? CustomEventRaised;
    public event BuildStatusEventHandler? StatusEventRaised;
    public event AnyEventHandler? AnyEventRaised;

    private void OnMessageRaised(object _, BuildMessageEventArgs e) => MessageRaised?.Invoke(this, e);
    
    private void OnErrorRaised(object _, BuildErrorEventArgs e) => ErrorRaised?.Invoke(this, e);
    
    private void OnWarningRaised(object _, BuildWarningEventArgs e) => WarningRaised?.Invoke(this, e);
    
    private void OnBuildStarted(object _, BuildStartedEventArgs e) => BuildStarted?.Invoke(this, e);
    
    private void OnBuildFinished(object _, BuildFinishedEventArgs e) => BuildFinished?.Invoke(this, e);
    
    private void OnProjectStarted(object _, ProjectStartedEventArgs e) => ProjectStarted?.Invoke(this, e);
    
    private void OnProjectFinished(object _, ProjectFinishedEventArgs e) => ProjectFinished?.Invoke(this, e);
    
    private void OnTargetStarted(object _, TargetStartedEventArgs e) => TargetStarted?.Invoke(this, e);
    
    private void OnTargetFinished(object _, TargetFinishedEventArgs e) => TargetFinished?.Invoke(this, e);
    
    private void OnTaskStarted(object _, TaskStartedEventArgs e) => TaskStarted?.Invoke(this, e);
    
    private void OnTaskFinished(object _, TaskFinishedEventArgs e) => TaskFinished?.Invoke(this, e);
    
    private void OnCustomEventRaised(object _, CustomBuildEventArgs e) => CustomEventRaised?.Invoke(this, e);
    
    private void OnStatusEventRaised(object _, BuildStatusEventArgs e) => StatusEventRaised?.Invoke(this, e);
    
    private void OnAnyEventRaised(object _, BuildEventArgs e) => AnyEventRaised?.Invoke(this, e);
}