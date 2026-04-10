using System;
using System.Windows.Input;
using System.Windows.Interop;

namespace UltraFocusMode
{
    /// <summary>
    /// Registers up to two system-wide hotkeys (activate + optional separate exit)
    /// and fires distinct events for each.
    /// </summary>
    public sealed class HotkeyManager : IDisposable
    {
        private const int ActivateId = 9001;
        private const int ExitId     = 9002;

        private HwndSource? _source;
        private bool _activateRegistered;
        private bool _exitRegistered;

        public event Action? ActivatePressed;
        public event Action? ExitPressed;

        public HotkeyManager()
        {
            var p = new HwndSourceParameters("UltraFocusModeHotkey")
            {
                HwndSourceHook = WndProc,
                ParentWindow   = new IntPtr(-3),   // HWND_MESSAGE
                Width = 0, Height = 0,
                WindowStyle = 0
            };
            _source = new HwndSource(p);
        }

        public bool Register(AppSettings settings)
        {
            if (_source == null) return false;
            Unregister();

            // ── Activate hotkey ──────────────────────────────────────────────────
            if (settings.HotkeyKey != Key.None)
            {
                uint vk   = (uint)KeyInterop.VirtualKeyFromKey(settings.HotkeyKey);
                uint mods = settings.GetModifiers();
                _activateRegistered = NativeMethods.RegisterHotKey(_source.Handle, ActivateId, mods, vk);
            }

            // ── Separate exit hotkey (only when not reusing activate combo) ──────
            if (!settings.UseSameHotkeyToExit && settings.ExitHotkeyKey != Key.None)
            {
                uint vk   = (uint)KeyInterop.VirtualKeyFromKey(settings.ExitHotkeyKey);
                uint mods = settings.GetExitModifiers();
                _exitRegistered = NativeMethods.RegisterHotKey(_source.Handle, ExitId, mods, vk);
            }

            return _activateRegistered;
        }

        public void Unregister()
        {
            if (_activateRegistered && _source != null)
            {
                NativeMethods.UnregisterHotKey(_source.Handle, ActivateId);
                _activateRegistered = false;
            }
            if (_exitRegistered && _source != null)
            {
                NativeMethods.UnregisterHotKey(_source.Handle, ExitId);
                _exitRegistered = false;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == ActivateId) { ActivatePressed?.Invoke(); handled = true; }
                else if (id == ExitId) { ExitPressed?.Invoke();    handled = true; }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            Unregister();
            _source?.Dispose();
            _source = null;
        }
    }
}
