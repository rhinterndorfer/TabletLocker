using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ButtonHandler.Native;

namespace ButtonHandler.Native
{
    public class NativeService
    {
        private IntPtr currentPowerRequest;


        public MonitorState CurrentMonitorState { get; set; }


        public void ClearHook(IntPtr hookHandle)
        {
            NativeAPIs.UnhookWindowsHookEx(hookHandle);
        }

        public IntPtr SetHook(HookType hookType, NativeAPIs.HookCallbackProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeAPIs.SetWindowsHookEx((int)hookType, proc,
                    NativeAPIs.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void SetMonitorState(Window ownerWindow, MonitorState monitorState)
        {
            IntPtr hwnd = new WindowInteropHelper(ownerWindow).Handle;
            CurrentMonitorState = monitorState;
            NativeAPIs.SendMessage(hwnd, (uint)WMType.SYSCOMMAND, (int)SystemCommandType.SC_MONITORPOWER, (int)monitorState);
        }


        public void SuppressStandby()
        {
            // Clear current power request if there is any.
            if (currentPowerRequest != IntPtr.Zero)
            {
                NativeAPIs.PowerClearRequest(currentPowerRequest, PowerRequestType.PowerRequestAwayModeRequired);
                currentPowerRequest = IntPtr.Zero;
            }

            // Create new power request.
            NativeAPIs.POWER_REQUEST_CONTEXT pContext;
            pContext.Flags = NativeAPIs.POWER_REQUEST_CONTEXT_SIMPLE_STRING;
            pContext.Version = NativeAPIs.POWER_REQUEST_CONTEXT_VERSION;
            // This is the reason for standby suppression. It is shown when the command "powercfg -requests" is executed.
            pContext.SimpleReasonString = "Standby suppressed by ButtonHandler.exe";

            currentPowerRequest = NativeAPIs.PowerCreateRequest(ref pContext);

            if (currentPowerRequest == IntPtr.Zero)
            {
                // Failed to create power request.
                var error = Marshal.GetLastWin32Error();

                if (error != 0)
                    throw new Win32Exception(error);
            }

            bool success = NativeAPIs.PowerSetRequest(currentPowerRequest, PowerRequestType.PowerRequestAwayModeRequired);

            if (!success)
            {
                // Failed to set power request.
                currentPowerRequest = IntPtr.Zero;
                var error = Marshal.GetLastWin32Error();

                if (error != 0)
                    throw new Win32Exception(error);
            }
        }

        public void EnableStandby()
        {
            // Only try to clear power request if any power request is set.
            if (currentPowerRequest != IntPtr.Zero)
            {
                var success = NativeAPIs.PowerClearRequest(currentPowerRequest, PowerRequestType.PowerRequestAwayModeRequired);

                if (!success)
                {
                    // Failed to clear power request.
                    currentPowerRequest = IntPtr.Zero;
                    var error = Marshal.GetLastWin32Error();

                    if (error != 0)
                        throw new Win32Exception(error);
                }
                else
                {
                    currentPowerRequest = IntPtr.Zero;
                }
            }
        }

    }
}

