# Drekar Launch Process

This library contains C\# client utility functions for processes launched using `drekar-launch`, although they may also be used without `drekar-launch`. Currently this package provides a reliable way for processes to receive shutdown signals from a process manager or the user using `ctrl-c`.

See `drekar-launch`: https://github.com/johnwason/drekar-launch

## Installation

Install the nuget package `DrekarLaunchProcessNET`.

## Usage

The `DrekarLaunchProcessNET` library contains one class, `DrekarLanchProcess.CWaitForExit`. This class contains two
functions `WaitForExit()` and `CallbackWaitForExit(Action cb)`. `WaitForExit` will block until the shutdown signal
is received. `CallbackWaitForExit` will call the callback `cb` when the exit signal is received. `CWaitForExit` 
implements `IDisposable` and should be used with a `using` statement.

Example:

```csharp
using DrekarLaunchProcess;
using System;

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
```

## Shutdown Signal Explanation

Reliably sending a shutdown/quit command to a process in a cross-platform manner is surprisingly difficult. On POSIX based systems like Linux and Mac OS X, signals such as `SIGINT` or `SIGTERM` are typically sent. (Ctrl-C sends `SIGINT`). These signals can be caught by the process, and used to gracefully shut down. `SIGKILL` is used to immediately terminate the process.

On Windows, it is significantly more difficult. Windows typically uses "Window Message Queues" to communicate with processes. Even processes like services that do not have a visible window often have a hidden window so messages can be received from the operating system. There is some functionality for sending console signals, but these are not reliable for all cases. Windows also has the concept of `Job Objects` that can be used to group different processes together. The combination of job objects and windows messages provide a reliable way to send and receive graceful shutdown signals.

The `WaitForExit*` commands either way for Signals on POSIX, or windows close messages and console events on Windows.
