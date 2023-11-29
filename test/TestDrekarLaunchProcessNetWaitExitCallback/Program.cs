using DrekarLaunchProcess;
using System;

namespace TestDrekarLanchProcessNETWaitExitCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var evt = new System.Threading.AutoResetEvent(false);
            Console.WriteLine("Press Ctrl-C to exit using callback");
            using (var wait_for_exit = new CWaitForExit())
            {
                wait_for_exit.CallbackWaitForExit(() => evt.Set());
                evt.WaitOne();
            }
            Console.WriteLine("Done");
        }
    }
}