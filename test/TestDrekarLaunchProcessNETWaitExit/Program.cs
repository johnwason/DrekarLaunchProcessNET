using DrekarLaunchProcess;
using System;

namespace TestDrekarLanchProcessNETWaitExit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to exit");
            using (var wait_for_exit = new CWaitForExit())
            {
                wait_for_exit.WaitForExit();
            }
            Console.WriteLine("Done");
        }
    }
}