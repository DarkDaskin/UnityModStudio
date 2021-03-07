using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace UnityModStudio.Options
{
    [InheritedExport]
    public interface IFolderBrowserService
    {
        Task<string?> BrowseForFolderAsync(string windowTitle, string? initialPath = null);
    }

    public class FolderBrowserService : IFolderBrowserService
    {
        public async Task<string?> BrowseForFolderAsync(string windowTitle, string? initialPath = null)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var uiShell = await ServiceProvider.GetGlobalServiceAsync<SVsUIShell, IVsUIShell>();

            return BrowseForFolder(uiShell, windowTitle, initialPath);
        }

        private static string? BrowseForFolder(IVsUIShell uiShell, string windowTitle, string? initialPath)
        {
            const int maxLength = 1000;
            // ReSharper disable InconsistentNaming, IdentifierTypo
            // See https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-browseinfow
            const uint BIF_RETURNONLYFSDIRS = 0x00000001;
            // ReSharper restore InconsistentNaming, IdentifierTypo

            var hr = uiShell.GetDialogOwnerHwnd(out var ownerHwnd);
            ErrorHandler.ThrowOnFailure(hr);

            var dirNamePtr = Marshal.AllocCoTaskMem((maxLength + 1) * sizeof(char));
            try
            {
                var browseInfoBox = new[]
                {
                    new VSBROWSEINFOW
                    {
                        lStructSize = (uint) Marshal.SizeOf<VSBROWSEINFOW>(),
                        nMaxDirName = maxLength,
                        pwzDirName = dirNamePtr,
                        pwzInitialDir = initialPath ?? "",
                        pwzDlgTitle = windowTitle,
                        hwndOwner = ownerHwnd,
                        dwFlags = BIF_RETURNONLYFSDIRS, // Any other flags fail the call
                    }
                };

                hr = uiShell.GetDirectoryViaBrowseDlg(browseInfoBox);
                return hr switch
                {
                    VSConstants.S_OK => Marshal.PtrToStringAuto(dirNamePtr),
                    VSConstants.OLE_E_PROMPTSAVECANCELLED => null,
                    _ => throw Marshal.GetExceptionForHR(hr),
                };
            }
            finally
            {
                Marshal.FreeCoTaskMem(dirNamePtr);
            }
        }
    }
}