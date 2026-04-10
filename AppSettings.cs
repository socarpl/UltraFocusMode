using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace UltraFocusMode
{
    public class AppSettings
    {
        public Key    HotkeyKey   { get; set; } = Key.F;
        public bool   HotkeyCtrl  { get; set; } = true;
        public bool   HotkeyAlt   { get; set; } = true;
        public bool   HotkeyShift { get; set; } = false;
        public bool   HotkeyWin   { get; set; } = false;
        public double OverlayOpacity { get; set; } = 0.85;

        // Exit behaviour
        public bool UseSameHotkeyToExit { get; set; } = true;
        public Key  ExitHotkeyKey   { get; set; } = Key.F;
        public bool ExitHotkeyCtrl  { get; set; } = true;
        public bool ExitHotkeyAlt   { get; set; } = true;
        public bool ExitHotkeyShift { get; set; } = false;
        public bool ExitHotkeyWin   { get; set; } = false;

        private static readonly string SettingsPath =
            Path.Combine(AppContext.BaseDirectory, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<AppSettings>(json, opts) ?? new AppSettings();
                }
            }
            catch { /* fall through to defaults */ }
            return new AppSettings();
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }

        public uint GetModifiers()
        {
            uint mods = NativeMethods.MOD_NOREPEAT;
            if (HotkeyCtrl)  mods |= NativeMethods.MOD_CONTROL;
            if (HotkeyAlt)   mods |= NativeMethods.MOD_ALT;
            if (HotkeyShift) mods |= NativeMethods.MOD_SHIFT;
            if (HotkeyWin)   mods |= NativeMethods.MOD_WIN;
            return mods;
        }

        public uint GetExitModifiers()
        {
            uint mods = NativeMethods.MOD_NOREPEAT;
            if (ExitHotkeyCtrl)  mods |= NativeMethods.MOD_CONTROL;
            if (ExitHotkeyAlt)   mods |= NativeMethods.MOD_ALT;
            if (ExitHotkeyShift) mods |= NativeMethods.MOD_SHIFT;
            if (ExitHotkeyWin)   mods |= NativeMethods.MOD_WIN;
            return mods;
        }

        public string GetExitDisplayString()
        {
            if (UseSameHotkeyToExit) return GetDisplayString();
            if (ExitHotkeyKey == Key.None) return "(none)";
            var parts = new List<string>();
            if (ExitHotkeyCtrl)  parts.Add("Ctrl");
            if (ExitHotkeyAlt)   parts.Add("Alt");
            if (ExitHotkeyShift) parts.Add("Shift");
            if (ExitHotkeyWin)   parts.Add("Win");
            parts.Add(ExitHotkeyKey.ToString());
            return string.Join(" + ", parts);
        }

        public string GetDisplayString()
        {
            if (HotkeyKey == Key.None) return "(none)";
            var parts = new List<string>();
            if (HotkeyCtrl)  parts.Add("Ctrl");
            if (HotkeyAlt)   parts.Add("Alt");
            if (HotkeyShift) parts.Add("Shift");
            if (HotkeyWin)   parts.Add("Win");
            parts.Add(HotkeyKey.ToString());
            return string.Join(" + ", parts);
        }
    }
}
