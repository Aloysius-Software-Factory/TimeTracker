# TimeTracker

A lightweight activity tracker for legacy Windows machines (Windows XP and later). Runs as a system tray application that logs foreground window activity and idle/active state to CSV files. Includes a companion viewer app for browsing and charting the recorded data.

## Why this exists

Designed for older machines (e.g. Windows XP) where modern activity tracking tools are unavailable, too heavy, or incompatible. No installer, no dependencies, no admin rights required — just build and drop the executable.

## Projects

| Project | Description |
|---|---|
| **TimeTracker** | Tray icon background app. Logs foreground window switches and idle/active state to CSV every second. |
| **TimeTrackerDisplay** | WinForms CSV viewer. Opens recorded CSV files and visualises them with pie charts and a timeline bar. |

## Build

```powershell
msbuild TimeTracker.sln /p:Configuration=Debug
# or, with .NET Framework targeting pack installed:
dotnet build TimeTracker.sln
```

Output: `TimeTracker\bin\Debug\TimeTracker.exe` and `TimeTrackerDisplay\bin\Debug\TimeTrackerDisplay.exe`

No NuGet packages, no external dependencies — only the .NET Framework 4.0 runtime is required.

## Configuration

These are stored in `app.config` (and sync'd to `Properties\Settings.settings`). Edit before building to change defaults.

| Setting | Type | Default | Purpose |
|---|---|---|---|
| `IdleThresholdMinutes` | `int` | 3 | How often (in minutes) the idle/active state is evaluated. |
| `CursorMoveThresholdPixels` | `double` | 2000 | Total Euclidean distance the mouse cursor must move **within one evaluation window** to be considered active. |
| `KeyInputThreshold` | `int` | 10 | Total key presses detected **within one evaluation window** to be considered active. |

### How idle/active state works

Every `IdleThresholdMinutes` (default: 3 minutes), the tracker evaluates the window:

- **Active** — if either:
  - cumulative mouse cursor movement ≥ `CursorMoveThresholdPixels` (2000 px), or
  - cumulative key presses ≥ `KeyInputThreshold` (10 keystrokes)
- **Idle** — otherwise (both mouse movement **and** key presses are below their thresholds)

Cursor movement is measured as the sum of Euclidean distances between successive cursor samples (sampled once per second). Key presses are counted via `GetAsyncKeyState` for all 256 virtual key codes each second.

### Example

With defaults, if over any 3-minute window the mouse moves fewer than 2000 pixels total **and** fewer than 10 keys are pressed, the interval is logged as idle. If the user moves the mouse 2000+ pixels **or** presses 10+ keys, it is logged as active.

## Data files

Written to `%LOCALAPPDATA%\TimeTracker\` as `{yyyy-M-d}-app.csv` and `{yyyy-M-d}-idle.csv`.

### `*-app.csv` (foreground window activity)

```
LogTimestamp,ApplicationName,StartTimestamp,EndTimestamp,DurationInSeconds
```

| Column | Description |
|---|---|
| `LogTimestamp` | When the record was written |
| `ApplicationName` | Executable name of the foreground window (e.g. `notepad.exe`) |
| `StartTimestamp` | When this window became active |
| `EndTimestamp` | When the user switched away |
| `DurationInSeconds` | How long it stayed in the foreground |

### `*-idle.csv` (active/idle state)

```
Status,StartTimestamp,EndTimestamp,DurationMinutes
```

| Column | Description |
|---|---|
| `Status` | `active` or `idle` |
| `StartTimestamp` | When this state began |
| `EndTimestamp` | When the state changed |
| `DurationMinutes` | Duration of the state in minutes |

## Viewer

`TimeTrackerDisplay.exe` opens CSV files and shows per-day tabs with:

- A **timeline bar** (green = active, orange = idle)
- **Pie charts** for app usage breakdown and active/idle ratio
- Filterable by date and time range
