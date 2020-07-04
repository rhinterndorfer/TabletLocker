using System;
using System.Runtime.InteropServices;

namespace ButtonHandler.Native
{
    public class NativeAPIs
    {
        public static Guid GUID_MONITOR_POWER_ON = new Guid(0x02731015, 0x4510, 0x4526, 0x99, 0xE6, 0xE5, 0xA1, 0x7E, 0xBD, 0x1A, 0xEA);

        public const int PBT_APMQUERYSUSPEND = 0x0000;
        public const int PBT_APMQUERYSTANDBY = 0x0001;
        public const int PBT_APMQUERYSUSPENDFAILED = 0x0002;
        public const int PBT_APMQUERYSTANDBYFAILED = 0x0003;
        public const int PBT_APMSUSPEND = 0x0004;
        public const int PBT_APMSTANDBY = 0x0005;
        public const int PBT_APMRESUMECRITICAL = 0x0006;
        public const int PBT_APMRESUMESUSPEND = 0x0007;
        public const int PBT_APMRESUMESTANDBY = 0x0008;
        public const int PBT_APMBATTERYLOW = 0x0009;
        public const int PBT_APMPOWERSTATUSCHANGE = 0x000A; // power status
        public const int PBT_APMOEMEVENT = 0x000B;
        public const int PBT_APMRESUMEAUTOMATIC = 0x0012;
        public const int PBT_POWERSETTINGCHANGE = 0x8013; // DPPE

        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        public const int DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001;

        public const int POWER_REQUEST_CONTEXT_VERSION = 0;
        public const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;
        public const int POWER_REQUEST_CONTEXT_DETAILED_STRING = 0x2;

        public delegate IntPtr HookCallbackProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT
        {
            public UInt32 Version;
            public UInt32 Flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string SimpleReasonString;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
            HookCallbackProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr PowerCreateRequest(ref POWER_REQUEST_CONTEXT Context);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PowerSetRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PowerClearRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        [DllImport(@"User32", SetLastError = true)]
        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);
    }
}
