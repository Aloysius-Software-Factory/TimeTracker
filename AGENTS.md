# TimeTracker — AGENTS.md

## Project

Two-project .NET Framework 4.0 solution: **TimeTracker** (tray logger) and **TimeTrackerDisplay** (CSV viewer), built with Visual Studio 2019.

## Build

```powershell
msbuild TimeTracker.sln /p:Configuration=Debug
# or
dotnet build TimeTracker.sln   # requires .NET Framework targeting pack
```

## Known issues

- Target framework is `v4.0`; requires Visual Studio 2019 (or compatible MSBuild + .NET Framework 4.0 SDK) to build.
- No tests, no CI, no package dependencies.

## Projects

### TimeTracker (tray logger)

Tray icon background app that logs foreground window changes and idle/active state to CSV files.

| File | Role |
|---|---|
| `Program.cs` | STAEntryPoint — calls `Application.Run(new Tracker())` |
| `Tracker.cs` | `ApplicationContext` with timer that polls foreground window, cursor/keyboard for idle detection |
| `Properties/` | Assembly metadata, settings (idle thresholds), resources |

Outputs `{yyyy-M-d}-app.csv` and `{yyyy-M-d}-idle.csv` to `%LOCALAPPDATA%\TimeTracker\`.

### TimeTrackerDisplay (CSV viewer)

WinForms app that reads the CSV files and visualizes them with pie charts and activity timelines.

| File | Role |
|---|---|
| `Program.cs` | STAEntryPoint |
| `MainForm.cs` | Main form — file selection, date/time filters, tabbed per-day display |
| `MainForm.Designer.cs` | Control layout |
| `CsvData.cs` | Data models (`AppLogRecord`, `IdleLogRecord`, `DayData`) and CSV parser with auto-pair discovery |
| `TimelinePanel.cs` | Custom `Panel` that draws a colour-coded active/idle timeline bar |
| `Properties/` | Assembly metadata |

Features:
- **Open CSV Files** button (multi-select) — auto-discovers paired `-app`/`-idle` files in same directory
- **Date range** filter (from/to)
- **Time range** filter (from/to, with up/down selectors)
- Per-day tabbed view: **pie chart** (app usage grouped by application name) + **timeline bar** (green=active, orange=idle)
- Uses built-in `System.Windows.Forms.DataVisualization` charting (no NuGet dependencies)

## Conventions

- Namespace: `TimeTracker` / `TimeTrackerDisplay`
- UI framework: WinForms (`System.Windows.Forms`)
- Charts: `System.Windows.Forms.DataVisualization.Charting`
