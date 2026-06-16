# TimeTracker — AGENTS.md

## Build

```powershell
msbuild TimeTracker.sln /p:Configuration=Debug
# or (requires .NET Framework targeting pack)
dotnet build TimeTracker.sln
```

Outputs go to `bin\Debug\` and `TimeTrackerDisplay\bin\Debug\`.

## Solution (.NET Framework 4.0, WinForms, VS2019)

Two projects, zero NuGet dependencies, zero tests, no CI.

### TimeTracker (tray logger, `Tracker.cs`)

- `Program.cs` → `Application.Run(new Tracker())` (STAThread entry point)
- `Tracker.cs` → `ApplicationContext` with 1-second `Timer` that polls `GetForegroundWindow` for active exe changes and checks cursor/keyboard for idle detection
- Idle thresholds in `app.config` (via `Properties.Settings`):
  - `IdleThresholdMinutes` = 3 (window before idle check triggers)
  - `CursorMoveThresholdPixels` = 2000 (total Euclidean distance)
  - `KeyInputThreshold` = 10 (total key presses)
- Writes `{yyyy-M-d}-app.csv` and `{yyyy-M-d}-idle.csv` to `%LOCALAPPDATA%\TimeTracker\`
- Tray icon with "Exit" context menu; flushes pending records on exit

### TimeTrackerDisplay (CSV viewer, `TimeTrackerDisplay\`)

- WinForms viewer: file picker → `CsvParser.DiscoverPairedFiles` (auto-discovers `-app`/`-idle` pairs in same dir) → per-day tabbed view
- Each tab: top half = `TimelinePanel` (custom `Panel`, green=active/orange=idle bar), bottom half = two pie charts (app usage + active/idle breakdown)
- Date/time filters (from/to) with `NumericUpDown` selectors for time range
- Charts use `System.Windows.Forms.DataVisualization.Charting` (built-in, no NuGet)

## CSV format

- `-app.csv` header: `LogTimestamp,ApplicationName,StartTimestamp,EndTimestamp,DurationInSeconds`
- `-idle.csv` header: `Status,StartTimestamp,EndTimestamp,DurationMinutes`
- Simple comma-separated, no quoting/escaping

## Conventions

- Namespace: `TimeTracker` / `TimeTrackerDisplay`
- No async/await, no LINQ-heavy patterns (used sparingly in Display only)
- All UI in `System.Windows.Forms`; no WPF
- No test project, no test framework