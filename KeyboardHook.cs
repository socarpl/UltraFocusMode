using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UltraFocusMode
{
    /// <summary>
    /// Low-level keyboard hook that intercepts the Escape key globally.
    /// Active only while installed — call Install() / Uninstall() / Dispose() to manage lifetime.
    /// </summary>
    public sealed class KeyboardHook : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        // Keep a reference so the delegate isn't collected while the hook is active.
        private readonly NativeMethods.LowLevelKeyboardProc _proc;

        public event Action? EscapePressed;

        public KeyboardHook()
        {
            _proc = HookCallback;
        }

        public void Install()
        {
            if (_hookId != IntPtr.Zero) return;

            using var proc   = Process.GetCurrentProcess();
            using var module = proc.MainModule!;
            _hookId = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _proc,
                NativeMethods.GetModuleHandle(module.ModuleName),
                0);
        }

        public void Uninstall()
        {
            if (_hookId == IntPtr.Zero) return;
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                (wParam == (IntPtr)NativeMethods.WM_KEYDOWN ||
                 wParam == (IntPtr)NativeMethods.WM_SYSKEYDOWN))
            {
                var ks = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                if (ks.vkCode == NativeMethods.VK_ESCAPE)
                {
                    EscapePressed?.Invoke();
                    return new IntPtr(1); // swallow the key
                }
            }
            return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose() => Uninstall();
    }
}
