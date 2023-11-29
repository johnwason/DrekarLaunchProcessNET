using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DrekarLaunchProcess
{
    interface IWaitForExitO : IDisposable
    {
        void WaitForExit();
        bool CallbackWaitForExit(Action exit_callback);
    }
    public class CWaitForExit : IDisposable
    {
        IWaitForExitO impl;
        public CWaitForExit()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                impl = new CWaitForExitWin();
            }
            else
            {
                impl = new WaitForExit_posix();
            }
        }

        public void Dispose()
        {
            impl?.Dispose();
            impl = null;
        }

        public void WaitForExit()
        {
            impl.WaitForExit();
        }

        public bool CallbackWaitForExit(Action exit_callback)
        {
            return impl.CallbackWaitForExit(exit_callback);
        }
    }
}
