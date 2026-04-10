using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace UltraFocusMode
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;

        // ── Activation hotkey pending state ──────────────────────────────────────
        private Key  _actKey;
        private bool _actCtrl, _actAlt, _actShift, _actWin;
        private bool _recordingAct;

        // ── Exit hotkey pending state ─────────────────────────────────────────────
        private Key  _exitKey;
        private bool _exitCtrl, _exitAlt, _exitShift, _exitWin;
        private bool _recordingExit;

        public event Action<AppSettings>? SettingsSaved;

        public SettingsWindow(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;

            // Seed activation hotkey
            _actKey   = settings.HotkeyKey;
            _actCtrl  = settings.HotkeyCtrl;
            _actAlt   = settings.HotkeyAlt;
            _actShift = settings.HotkeyShift;
            _actWin   = settings.HotkeyWin;

            // Seed exit hotkey
            _exitKey   = settings.ExitHotkeyKey;
            _exitCtrl  = settings.ExitHotkeyCtrl;
            _exitAlt   = settings.ExitHotkeyAlt;
            _exitShift = settings.ExitHotkeyShift;
            _exitWin   = settings.ExitHotkeyWin;

            HotkeyBox.Text     = settings.GetDisplayString();
            ExitHotkeyBox.Text = BuildExitDisplay();
            OpacitySlider.Value = settings.OverlayOpacity;
            RefreshOpacityLabel();

            SameComboCheck.IsChecked = settings.UseSameHotkeyToExit;
            ApplyExitRowState();
        }

        // ─── Activation hotkey recording ─────────────────────────────────────────

        private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _recordingAct = true;
            HotkeyBox.Text = "Press combination…";
        }

        private void HotkeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _recordingAct = false;
            if (HotkeyBox.Text == "Press combination…")
                HotkeyBox.Text = BuildActDisplay();
        }

        private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recordingAct) return;
            e.Handled = true;
            RecordKey(e, ref _actKey, ref _actCtrl, ref _actAlt, ref _actShift, ref _actWin,
                      out bool cancel);
            if (cancel) { _recordingAct = false; HotkeyBox.Text = BuildActDisplay(); return; }
            HotkeyBox.Text = BuildActDisplay();
            _recordingAct = false;
            Keyboard.ClearFocus();
        }

        private void ClearActivationHotkey_Click(object sender, RoutedEventArgs e)
        {
            _actKey = Key.None;
            _actCtrl = _actAlt = _actShift = _actWin = false;
            HotkeyBox.Text = "(none)";
        }

        // ─── Exit hotkey recording ────────────────────────────────────────────────

        private void ExitHotkeyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _recordingExit = true;
            ExitHotkeyBox.Text = "Press combination…";
        }

        private void ExitHotkeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _recordingExit = false;
            if (ExitHotkeyBox.Text == "Press combination…")
                ExitHotkeyBox.Text = BuildExitDisplay();
        }

        private void ExitHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_recordingExit) return;
            e.Handled = true;
            RecordKey(e, ref _exitKey, ref _exitCtrl, ref _exitAlt, ref _exitShift, ref _exitWin,
                      out bool cancel);
            if (cancel) { _recordingExit = false; ExitHotkeyBox.Text = BuildExitDisplay(); return; }
            ExitHotkeyBox.Text = BuildExitDisplay();
            _recordingExit = false;
            Keyboard.ClearFocus();
        }

        private void ClearExitHotkey_Click(object sender, RoutedEventArgs e)
        {
            _exitKey = Key.None;
            _exitCtrl = _exitAlt = _exitShift = _exitWin = false;
            ExitHotkeyBox.Text = "(none)";
        }

        // ─── Shared key-recording helper ──────────────────────────────────────────

        private static void RecordKey(KeyEventArgs e,
            ref Key key, ref bool ctrl, ref bool alt, ref bool shift, ref bool win,
            out bool cancel)
        {
            cancel = false;
            var k = e.Key == Key.System ? e.SystemKey : e.Key;

            if (k is Key.LeftCtrl or Key.RightCtrl or
                     Key.LeftAlt  or Key.RightAlt  or
                     Key.LeftShift or Key.RightShift or
                     Key.LWin or Key.RWin)
                return;   // wait for non-modifier

            if (k == Key.Escape) { cancel = true; return; }

            key   = k;
            ctrl  = Keyboard.IsKeyDown(Key.LeftCtrl)  || Keyboard.IsKeyDown(Key.RightCtrl);
            alt   = Keyboard.IsKeyDown(Key.LeftAlt)   || Keyboard.IsKeyDown(Key.RightAlt);
            shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            win   = Keyboard.IsKeyDown(Key.LWin)      || Keyboard.IsKeyDown(Key.RWin);
        }

        // ─── Checkbox ─────────────────────────────────────────────────────────────

        private void SameComboCheck_Changed(object sender, RoutedEventArgs e)
            => ApplyExitRowState();

        private void ApplyExitRowState()
        {
            bool useSame = SameComboCheck.IsChecked == true;
            ExitHotkeyRow.IsEnabled = !useSame;
            ExitHotkeyRow.Opacity   = useSame ? 0.35 : 1.0;
        }

        // ─── Opacity slider ───────────────────────────────────────────────────────

        private void OpacitySlider_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<double> e) => RefreshOpacityLabel();

        private void RefreshOpacityLabel()
        {
            if (OpacityLabel != null)
                OpacityLabel.Text = $"{(int)Math.Round(OpacitySlider.Value * 100)}%";
        }

        // ─── Display helpers ──────────────────────────────────────────────────────

        private string BuildActDisplay()
        {
            if (_actKey == Key.None) return "(none)";
            var p = new List<string>();
            if (_actCtrl)  p.Add("Ctrl");
            if (_actAlt)   p.Add("Alt");
            if (_actShift) p.Add("Shift");
            if (_actWin)   p.Add("Win");
            p.Add(_actKey.ToString());
            return string.Join(" + ", p);
        }

        private string BuildExitDisplay()
        {
            if (_exitKey == Key.None) return "(none)";
            var p = new List<string>();
            if (_exitCtrl)  p.Add("Ctrl");
            if (_exitAlt)   p.Add("Alt");
            if (_exitShift) p.Add("Shift");
            if (_exitWin)   p.Add("Win");
            p.Add(_exitKey.ToString());
            return string.Join(" + ", p);
        }

        // ─── Window-level ESC / Enter ─────────────────────────────────────────────

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_recordingAct || _recordingExit) return;
            if (e.Key == Key.Escape) { e.Handled = true; Close(); }
            if (e.Key == Key.Enter)  { e.Handled = true; DoSave(); }
        }

        // ─── Buttons ──────────────────────────────────────────────────────────────

        private void Save_Click(object sender, RoutedEventArgs e)   => DoSave();
        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void DoSave()
        {
            _settings.HotkeyKey   = _actKey;
            _settings.HotkeyCtrl  = _actCtrl;
            _settings.HotkeyAlt   = _actAlt;
            _settings.HotkeyShift = _actShift;
            _settings.HotkeyWin   = _actWin;

            _settings.UseSameHotkeyToExit = SameComboCheck.IsChecked == true;

            _settings.ExitHotkeyKey   = _exitKey;
            _settings.ExitHotkeyCtrl  = _exitCtrl;
            _settings.ExitHotkeyAlt   = _exitAlt;
            _settings.ExitHotkeyShift = _exitShift;
            _settings.ExitHotkeyWin   = _exitWin;

            _settings.OverlayOpacity = Math.Round(OpacitySlider.Value, 2);

            _settings.Save();
            SettingsSaved?.Invoke(_settings);
            Close();
        }
    }
}
