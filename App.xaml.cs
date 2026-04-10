using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace UltraFocusMode
{
    public partial class App : Application
    {
        private NotifyIcon?    _trayIcon;
        private HotkeyManager? _hotkeys;
        private AppSettings    _settings = AppSettings.Load();

        private CaptureWindow? _captureWindow;
        private OverlayWindow? _overlayWindow;

        // ─── Startup ────────────────────────────────────────────────────────────

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            BuildTrayIcon();
            InitHotkey();
        }

        // ─── Tray icon ──────────────────────────────────────────────────────────

        private void BuildTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon    = MakeCrosshairIcon(),
                Visible = true,
                Text    = $"UltraFocusMode  [{_settings.GetDisplayString()}]"
            };

            var menu = new ContextMenuStrip();
            menu.Items.Add("Settings",   null, (_, _) => ShowSettings());
            menu.Items.Add("-");
            menu.Items.Add("Exit", null, (_, _) => ExitApp());

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick     += (_, _) => ShowSettings();
        }

        private static Icon MakeCrosshairIcon()
        {
            using var bmp = new Bitmap(32, 32);
            using var g   = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var pen = new Pen(Color.White, 2f);
            g.DrawLine(pen, 16, 2,  16, 12);
            g.DrawLine(pen, 16, 20, 16, 30);
            g.DrawLine(pen, 2,  16, 12, 16);
            g.DrawLine(pen, 20, 16, 30, 16);
            g.DrawEllipse(pen, 8, 8, 16, 16);

            var hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }

        // ─── Hotkey ─────────────────────────────────────────────────────────────

        private void InitHotkey()
        {
            _hotkeys = new HotkeyManager();
            _hotkeys.ActivatePressed += OnActivateHotkey;
            _hotkeys.ExitPressed     += OnExitHotkey;
            ApplyHotkey();
        }

        public void ApplyHotkey()
        {
            bool ok = _hotkeys?.Register(_settings) ?? false;
            if (_trayIcon != null)
                _trayIcon.Text = ok
                    ? $"UltraFocusMode  [{_settings.GetDisplayString()}]"
                    : "UltraFocusMode  (hotkey not registered)";
        }

        // ─── Activation / deactivation ──────────────────────────────────────────

        private void OnActivateHotkey()
        {
            // When using same combo to exit: toggle if already active
            if (_settings.UseSameHotkeyToExit)
            {
                if (_captureWindow?.IsVisible == true || _overlayWindow?.IsVisible == true)
                {
                    DismissAll();
                    return;
                }
            }

            // Don't double-activate
            if (_captureWindow?.IsVisible == true) return;
            if (_overlayWindow?.IsVisible  == true) return;

            _captureWindow = new CaptureWindow();
            _captureWindow.WindowSelected += OnWindowSelected;
            _captureWindow.Cancelled      += OnCaptureCancelled;
            _captureWindow.Show();
        }

        private void OnExitHotkey() => DismissAll();

        private void DismissAll()
        {
            _captureWindow?.Close();
            _captureWindow = null;

            _overlayWindow?.Dismiss();
            // _overlayWindow is set to null inside Dismissed callback
        }

        private void OnWindowSelected(IntPtr hwnd, NativeMethods.RECT rect)
        {
            _captureWindow?.Close();
            _captureWindow = null;

            _overlayWindow?.Dismiss();
            _overlayWindow = new OverlayWindow(hwnd, _settings);
            _overlayWindow.Dismissed += () => _overlayWindow = null;
            _overlayWindow.Show();
        }

        private void OnCaptureCancelled()
        {
            _captureWindow?.Close();
            _captureWindow = null;
        }

        // ─── Settings ───────────────────────────────────────────────────────────

        private void ShowSettings()
        {
            var win = new SettingsWindow(_settings);
            win.SettingsSaved += newSettings =>
            {
                _settings = newSettings;
                ApplyHotkey();
            };
            win.ShowDialog();
        }

        // ─── Exit ───────────────────────────────────────────────────────────────

        private void ExitApp()
        {
            _trayIcon?.Dispose();
            _hotkeys?.Dispose();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _hotkeys?.Dispose();
            base.OnExit(e);
        }
    }
}
