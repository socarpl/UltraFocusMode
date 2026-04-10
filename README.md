# UltraFocusMode

A lightweight Windows utility that dims everything on your screen except the window you choose — helping you stay focused without closing or minimising anything else.

---

## How it works

1. **Press the activation hotkey** (default `Ctrl + Alt + F`)
2. Your cursor changes to a **crosshair**
3. **Click any window** — everything around it turns black
4. **Press the exit hotkey** (default: same combination, `Ctrl + Alt + F`) to restore the screen

The dark overlay tracks the focused window in real time — if you move or resize it, the overlay follows.

---

## Features

- Fullscreen overlay covering all monitors simultaneously
- Transparent hole precisely cut around the target window — click-through so you can interact with the app normally while in focus mode
- Overlay tracks window position and size live (drag, resize, snap — all followed)
- Automatically exits focus mode if the focused window is closed or minimised
- Fade in / fade out animation
- Configurable overlay opacity
- Configurable activation and exit hotkeys
- Settings saved to `settings.json` next to the executable
- Runs silently in the system tray — no taskbar entry

---

## Installation

No installer needed. Grab the latest `UltraFocusMode.zip` from the [Actions](../../actions) tab (or [Releases](../../releases) if available), extract, and run `UltraFocusMode.exe`.

> **Requirements:** Windows 10 or 11 (x64). No .NET installation required — the runtime is bundled.

---

## Usage

### Starting the app

Run `UltraFocusMode.exe`. A crosshair icon appears in your **system tray** (bottom-right corner, near the clock). The app runs entirely from there.

### Entering focus mode

| Step | Action |
|------|--------|
| 1 | Press the activation hotkey (`Ctrl + Alt + F` by default) |
| 2 | Cursor becomes a crosshair — a hint banner appears at the bottom of the screen |
| 3 | Left-click the window you want to focus on |
| 4 | Everything else goes dark |

### Exiting focus mode

Press the **exit hotkey**. By default this is the same combination as the activation hotkey (`Ctrl + Alt + F`), acting as a toggle.

You can also cancel the window-selection step (crosshair mode) by pressing `Escape`.

---

## Settings

Right-click the tray icon and choose **Settings**, or double-click the icon.

| Setting | Description | Default |
|---------|-------------|---------|
| **Activation hotkey** | Key combination that triggers the crosshair cursor | `Ctrl + Alt + F` |
| **Overlay opacity** | How dark the background dims (20 % – 100 %) | `85 %` |
| **Use same combination to exit** | When checked, the activation hotkey also exits focus mode (toggle behaviour) | Enabled |
| **Exit hotkey** | Separate combination to exit focus mode — only available when the checkbox above is unchecked | `Ctrl + Alt + F` |

### Changing a hotkey

1. Click the hotkey text box
2. Press the desired key combination (e.g. `Ctrl + Shift + F`)
3. The box updates immediately to show the recorded combination
4. Press **Save**

Press `Escape` while the box is focused to cancel recording without changing the hotkey. Click **Clear** to remove the hotkey entirely.

### Settings file

Settings are stored in `settings.json` in the same folder as the executable. You can edit it manually if needed.

```json
{
  "HotkeyKey": "F",
  "HotkeyCtrl": true,
  "HotkeyAlt": true,
  "HotkeyShift": false,
  "HotkeyWin": false,
  "OverlayOpacity": 0.85,
  "UseSameHotkeyToExit": true,
  "ExitHotkeyKey": "F",
  "ExitHotkeyCtrl": true,
  "ExitHotkeyAlt": true,
  "ExitHotkeyShift": false,
  "ExitHotkeyWin": false
}
```

---

## Tray icon menu

| Option | Action |
|--------|--------|
| **Settings** | Opens the settings dialog |
| **Exit** | Closes the application |

Double-clicking the tray icon also opens Settings.

---

## Keyboard shortcuts summary

| Shortcut | Context | Action |
|----------|---------|--------|
| `Ctrl + Alt + F` | Anywhere | Enter focus mode (show crosshair) |
| `Ctrl + Alt + F` | Focus mode active | Exit focus mode (when using same combo) |
| `Escape` | Crosshair visible | Cancel window selection |

> All shortcuts above reflect the default configuration and can be changed in Settings.

---

## Building from source

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/socarpl/UltraFocusMode.git
cd UltraFocusMode
dotnet build -c Release
```

To produce a self-contained single executable:

```bash
dotnet publish UltraFocusMode.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

## CI / CD

Every push to `master` automatically builds a self-contained `UltraFocusMode.zip` via GitHub Actions. Download the latest build from the [Actions](../../actions) tab.
