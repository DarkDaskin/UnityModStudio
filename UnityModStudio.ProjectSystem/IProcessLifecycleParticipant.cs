using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Debug;

namespace UnityModStudio.ProjectSystem;

public interface IProcessLifecycleParticipant
{
    Task OnBeforeLaunchAsync(ILaunchProfile profile, ProcessStartInfo processStartInfo);
    Task OnAfterLaunchAsync(ILaunchProfile profile, Process process);
    Task OnAfterExitAsync(ILaunchProfile profile, Process process);
}