using System;
using System.Windows;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace UnityModStudio.ProjectWizard
{
    public class WindowsFormsWindowAdapter : IWin32Window
    {
        private readonly WindowInteropHelper _interopHelper;

        public IntPtr Handle => _interopHelper.Handle;

        public WindowsFormsWindowAdapter(Window window)
        {
            _interopHelper = new WindowInteropHelper(window);
        }
    }
}