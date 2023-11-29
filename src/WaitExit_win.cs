using System;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Collections.Generic;
using Windows.Win32.System.Console;

namespace DrekarLaunchProcess
{
    class WaitForExitImpl : IDisposable
    {
        static Dictionary<int, WaitForExitImpl> _instances = new Dictionary<int, WaitForExitImpl>();
        static int _instances_count = 0;
        int _this_intstance;

        internal WaitForExitImpl()
        {
            // Add to _instances
            lock (_instances)
            {
                _instances.Add(_instances_count, this);
                _this_intstance = _instances_count;
                _instances_count += 1;
            }
        }

        ~WaitForExitImpl()
        {
            lock (_instances)
            {
                _instances.Remove(_this_intstance);
            }
        }

        internal unsafe bool Create(string class_name, string window_name)
        {
            var module = GetModuleHandle((string)null);
            var module_hinstance = new HINSTANCE(module.DangerousGetHandle());
            fixed (char* class_name_ptr = class_name)
            {
                WNDCLASSEXW wcex = new WNDCLASSEXW();
                wcex.cbSize = (uint)Marshal.SizeOf(wcex);
                wcex.lpfnWndProc = wndProcDelegate;
                wcex.hInstance = module_hinstance;
                wcex.lpszClassName = new PCWSTR(class_name_ptr);
                if (RegisterClassEx(wcex) == 0)
                    return false;
                m_hwnd = CreateWindowEx(0, class_name, window_name, 0, 0, 0, 0, 0, HWND.HWND_MESSAGE, null, module, (void*)(_this_intstance));
                return m_hwnd != null;
            }
        }
        static HWND m_hwnd;

        // Assuming WndProc is defined somewhere in this class
        private static WNDPROC wndProcDelegate = WndProc;

        unsafe static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            WaitForExitImpl this_ = null;

            if (msg == WM_NCCREATE)
            {
                CREATESTRUCTW lpcs = Marshal.PtrToStructure<CREATESTRUCTW>(lParam);
                int int_this = (int)lpcs.lpCreateParams;
                SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_USERDATA, int_this);
                lock (_instances)
                {
                    //_instances[int_this].m_hwnd = hWnd;
                    m_hwnd = hWnd;
                }
            }
            else
            {
                int int_this = GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_USERDATA);
                lock (_instances)
                {

                    _instances.TryGetValue(int_this, out this_);
                }
            }

            if (this_ != null)
            {
                return this_.HandleMessage(msg, wParam, lParam);
            }
            else
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }

        LRESULT HandleMessage(uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case WM_DESTROY:
                    OnDestroy();
                    break;
                case WM_CLOSE:
                    DestroyWindow(m_hwnd); 
                    break;
                default:
                    return DefWindowProc(m_hwnd, msg, wParam, lParam);
            }

            return new LRESULT(0);
        }

        void OnDestroy()
        {
            PostQuitMessage(0);
        }

        internal void RunMessageLoop()
        {
            MSG msg;

            while (GetMessage(out msg, new HWND(), 0, 0))
            {
                TranslateMessage(msg);
                DispatchMessage(msg);
            }
        }

        public void Dispose()
        {
            if (m_hwnd != IntPtr.Zero)
            {
                DestroyWindow(m_hwnd);
                m_hwnd = new HWND(IntPtr.Zero);
            }
        }


        static BOOL ConsoleHandlerRoutineImpl(uint CtrlType)
        {
            if (CtrlType == CTRL_C_EVENT || CtrlType == CTRL_BREAK_EVENT || CtrlType == CTRL_CLOSE_EVENT)
            {
                PostMessage(m_hwnd, WM_CLOSE, 0, 0);
                return true;
            }
            return false;
        }

        static PHANDLER_ROUTINE ConsoleHandlerRoutine = ConsoleHandlerRoutineImpl;
        internal bool SetConsoleHandlerRoutine()
        {
            return SetConsoleCtrlHandler(ConsoleHandlerRoutine, true);            
        }

    }
    class CWaitForExitWin : IWaitForExitO
    {
        WaitForExitImpl impl;
        public CWaitForExitWin()
        {
            impl = new WaitForExitImpl();
        }

        public void Dispose()
        {
            impl?.Dispose();
            impl = null;
        }

        ~CWaitForExitWin()
        {
            impl?.Dispose();
            impl = null;            
        }

        public void WaitForExit()
        {
            if (!impl.Create("drekar_message_window", "drekar_hidden_window"))
            {
                Console.Error.WriteLine("Failed to create message window");
                return;
            }

            if (!impl.SetConsoleHandlerRoutine())
            {
                Console.Error.WriteLine("Failed to set console control handler");
                return;
            }

            impl.RunMessageLoop();
        }

        public bool CallbackWaitForExit(Action exit_callback)
        {
            var t = new System.Threading.Thread(
                delegate()
                {
                    WaitForExit();
                    exit_callback();
                }
            );
            t.Start();
            return true;
        }
    }
}
