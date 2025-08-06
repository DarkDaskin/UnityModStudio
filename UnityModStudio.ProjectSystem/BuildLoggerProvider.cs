using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell;

namespace UnityModStudio.ProjectSystem;

[Export(typeof(IBuildLoggerProviderAsync))]
[AppliesTo(ProjectCapability.UnityModStudio)]
[method: ImportingConstructor]
public class BuildLoggerProvider(ConfiguredProject configuredProject) : IBuildLoggerProviderAsync
{
    public Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<string> targets, IImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
    {
        var isDesignTime = properties.TryGetValue("DesignTimeBuild", out var s) && bool.TryParse(s, out var v) && v;
        return Task.FromResult<IImmutableSet<ILogger>>(isDesignTime ? [new DetectingLogger(this)] : []);
    }

    private void OnBuildFinished(bool shouldTriggerEvaluation)
    {
        if (shouldTriggerEvaluation)
            RunUnderWriteLockOnce(projectCollection =>
            {
                projectCollection.SetGlobalProperty("_ReEvaluate", Guid.NewGuid().ToString("N"));
                configuredProject.NotifyProjectChange();
            });
        else
            RunUnderWriteLockOnce(projectCollection => projectCollection.RemoveGlobalProperty("_ReEvaluate"));
    }

    private void RunUnderWriteLockOnce(Action<ProjectCollection> action)
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            if (configuredProject.Services.ProjectLockService.IsWriteLockHeld)
                return;

            await configuredProject.Services.ProjectLockService.WriteLockAsync(async releaser =>
            {
                action(releaser.ProjectCollection);
                await releaser.ReleaseAsync();
            });
        });
    }


    private class DetectingLogger(BuildLoggerProvider parent) : ILogger
    {
        private string? _currentTarget;
        private string? _currentTask;
        private bool _shouldTriggerEvaluation;

        public void Initialize(IEventSource eventSource) => eventSource.AnyEventRaised += OnAnyEventRaised;
        
        public void Shutdown() { }

        public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Diagnostic;
        public string? Parameters { get; set; }

        private void OnAnyEventRaised(object sender, BuildEventArgs args)
        {
            switch (args)
            {
                case TargetStartedEventArgs args2:
                    _currentTarget = args2.TargetName;
                    break;

                case TaskStartedEventArgs args2:
                    _currentTask = args2.TaskName;
                    break;

                case TaskParameterEventArgs {
                    Kind: TaskParameterMessageKind.TaskOutput, 
                    ItemType: "_HasWrittenResolvedReferencesProjectFile",
                    Items: [ITaskItem taskItem]
                } when _currentTarget == "ResolveGameAssemblyReferences" && _currentTask == "UpdateProjectFile" && bool.TryParse(taskItem.ItemSpec, out var v) && v:
                    _shouldTriggerEvaluation = true;
                    break;

                case BuildFinishedEventArgs { Succeeded: true }:
                    parent.OnBuildFinished(_shouldTriggerEvaluation);
                    _shouldTriggerEvaluation = false;
                    break;
            }
        }
    }
}