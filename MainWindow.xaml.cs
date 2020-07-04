using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using ButtonHandler.Native;
using System.Windows.Interop;
using Microsoft.Win32;

namespace PowerButtonHandler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NativeService nativeService;
        private NativeAPIs.HookCallbackProc procHook;
        private IntPtr handleKeyboardHook;
        private IntPtr handleMouseHook;

        public MainWindow()
        {
            InitializeComponent();

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                WMType msgType = (WMType)msg;
            }
            catch { }

            if (msg == (int)WMType.POWERBROADCAST && wParam.ToInt32() == NativeAPIs.PBT_POWERSETTINGCHANGE)
            {
                // Extract data from message
                NativeAPIs.POWERBROADCAST_SETTING ps =
                 (NativeAPIs.POWERBROADCAST_SETTING)Marshal.PtrToStructure(
                     lParam, typeof(NativeAPIs.POWERBROADCAST_SETTING));
                
                if(ps.PowerSetting == NativeAPIs.GUID_MONITOR_POWER_ON)
                {
                    Int32 iData = (Int32)ps.Data;
                    if(iData == 0)
                        nativeService.CurrentMonitorState = MonitorState.MonitorStateOff;
                    if (iData == 1)
                        nativeService.CurrentMonitorState = MonitorState.MonitorStateOn;
                }
                
            }

            return IntPtr.Zero;
        }


        private void RegisterForPowerNotifications(IntPtr hwnd)
        {
            IntPtr hMonitorOn = NativeAPIs.RegisterPowerSettingNotification(hwnd,
                ref NativeAPIs.GUID_MONITOR_POWER_ON,
                NativeAPIs.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            //source.AddHook(WndProc);
            //RegisterForPowerNotifications(source.Handle);

            nativeService = new NativeService();
            procHook = (nCode, wParam, lParam) =>
            {
                if (nCode >= 0 && wParam == (IntPtr)WMType.KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (
                        (System.Windows.Forms.Keys)vkCode == System.Windows.Forms.Keys.VolumeDown
                        || (System.Windows.Forms.Keys)vkCode == System.Windows.Forms.Keys.VolumeUp
                        )
                    {
                        if(this.WindowState == WindowState.Maximized)
                        {
                            this.WindowState = WindowState.Minimized;
                            this.Topmost = false;
                        }
                        else
                        {
                            this.WindowState = WindowState.Maximized;
                            this.Topmost = true;
                        }
                            
                        return new IntPtr(1);
                    }
                }

                if (this.WindowState == WindowState.Maximized)
                {
                    if (nCode >= 0
                    && (
                        wParam == (IntPtr)WMType.LBUTTONDOWN
                        || wParam == (IntPtr)WMType.LBUTTONUP
                        || wParam == (IntPtr)WMType.MOUSEHOVER
                        || wParam == (IntPtr)WMType.MOUSEMOVE
                        || wParam == (IntPtr)WMType.MOUSEACTIVATE
                        || wParam == (IntPtr)WMType.MOUSELEAVE
                    ))
                    {
                        return new IntPtr(1);
                    }
                }

                return NativeAPIs.CallNextHookEx(handleKeyboardHook, nCode, wParam, lParam);
            };
            handleKeyboardHook = nativeService.SetHook(HookType.WH_KEYBOARD_LL, procHook);
            handleMouseHook = nativeService.SetHook(HookType.WH_MOUSE_LL, procHook);

            nativeService.SuppressStandby();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            //source.RemoveHook(WndProc);

            nativeService.ClearHook(handleKeyboardHook);
            nativeService.ClearHook(handleMouseHook);
            nativeService.EnableStandby();
        }
    }
}
