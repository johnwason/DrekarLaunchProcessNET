using DrekarLaunchProcess;
using System;
using System.Collections.Generic;
using System.Text;
using Mono.Unix;

namespace DrekarLaunchProcess
{
    internal class WaitForExit_posix : IWaitForExitO
    {
        UnixSignal sig_term;
        UnixSignal sig_int;

        public WaitForExit_posix()
        {
            sig_term = new UnixSignal(Mono.Unix.Native.Signum.SIGTERM);
            sig_int = new UnixSignal(Mono.Unix.Native.Signum.SIGINT);
        }

        public bool CallbackWaitForExit(Action exit_callback)
        {
            var t = new System.Threading.Thread(
                delegate ()
                {
                    WaitForExit();
                    exit_callback();
                }
            );
            t.Start();
            return true;
        }

        public void Dispose()
        {
            sig_term?.Dispose();
            sig_term = null;
            sig_int?.Dispose();
            sig_int = null;
        }

        public void WaitForExit()
        {
            UnixSignal.WaitAny(new UnixSignal[] { sig_term, sig_int });
        }
    }
}
