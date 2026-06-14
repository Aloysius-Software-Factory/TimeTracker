# Graph Report - .  (2026-06-14)

## Corpus Check
- Corpus is ~4,478 words - fits in a single context window. You may not need a graph.

## Summary
- 140 nodes · 182 edges · 15 communities (13 shown, 2 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 9 edges (avg confidence: 0.95)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Tracker Core (PInvoke, Idle Detection)|Tracker Core (P/Invoke, Idle Detection)]]
- [[_COMMUNITY_Solution Architecture & Data Models|Solution Architecture & Data Models]]
- [[_COMMUNITY_TimelinePanel Custom Rendering|TimelinePanel Custom Rendering]]
- [[_COMMUNITY_MainForm UI & Chart Building|MainForm UI & Chart Building]]
- [[_COMMUNITY_CSV Data Parsing & File Discovery|CSV Data Parsing & File Discovery]]
- [[_COMMUNITY_MainForm Designer Layout|MainForm Designer Layout]]
- [[_COMMUNITY_Application Settings|Application Settings]]
- [[_COMMUNITY_TimeTracker Program Entry|TimeTracker Program Entry]]
- [[_COMMUNITY_TimeTracker Resources|TimeTracker Resources]]
- [[_COMMUNITY_Display Program Entry|Display Program Entry]]
- [[_COMMUNITY_Display Resources|Display Resources]]
- [[_COMMUNITY_Project Files|Project Files]]
- [[_COMMUNITY_WinForms Framework Rationale|WinForms Framework Rationale]]

## God Nodes (most connected - your core abstractions)
1. `Tracker` - 22 edges
2. `TimelinePanel` - 13 edges
3. `MainForm` - 9 edges
4. `TimeTrackerDisplay.MainForm` - 9 edges
5. `CsvParser` - 8 edges
6. `MainForm` - 7 edges
7. `TimeTracker.Tracker` - 5 edges
8. `TimeTrackerDisplay.CsvParser` - 5 edges
9. `DllImport` - 4 edges
10. `Settings` - 4 edges

## Surprising Connections (you probably didn't know these)
- `TimeTracker.Tracker` --implements--> `CSV Log File Format`  [INFERRED]
  Tracker.cs → AGENTS.md
- `TimeTrackerDisplay.CsvParser` --references--> `CSV Log File Format`  [INFERRED]
  TimeTrackerDisplay/CsvData.cs → AGENTS.md
- `AGENTS.md Documentation` --references--> `TimeTracker (tray logger)`  [EXTRACTED]
  AGENTS.md → TimeTracker.csproj
- `AGENTS.md Documentation` --references--> `TimeTrackerDisplay (CSV viewer)`  [EXTRACTED]
  AGENTS.md → TimeTrackerDisplay/TimeTrackerDisplay.csproj
- `TimeTracker (tray logger)` --shares_data_with--> `TimeTrackerDisplay (CSV viewer)`  [INFERRED]
  TimeTracker.csproj → TimeTrackerDisplay/TimeTrackerDisplay.csproj

## Communities (15 total, 2 thin omitted)

### Community 0 - "Tracker Core (P/Invoke, Idle Detection)"
Cohesion: 0.12
Nodes (15): ApplicationContext, bool, DllImport, double, Icon, IntPtr, NotifyIcon, POINT (+7 more)

### Community 1 - "Solution Architecture & Data Models"
Cohesion: 0.14
Nodes (20): AGENTS.md Documentation, Auto CSV Pair Discovery, CSV Log File Format, Idle Detection via Cursor + Keyboard, TimeTracker.IdleSettings, Pie Chart Visualization, TimeTrackerDisplay (CSV viewer), TimeTracker (tray logger) (+12 more)

### Community 2 - "TimelinePanel Custom Rendering"
Cohesion: 0.19
Nodes (9): Color, Dictionary, Graphics, PaintEventArgs, EventArgs, int, TimeSpan, TimelinePanel (+1 more)

### Community 3 - "MainForm UI & Chart Building"
Cohesion: 0.18
Nodes (10): Chart, DayData, Form, EventArgs, List, string, TimeSpan, FilePair (+2 more)

### Community 4 - "CSV Data Parsing & File Discovery"
Cohesion: 0.20
Nodes (8): Regex, AppLogRecord, DateTime, List, CsvParser, DayData, IdleLogRecord, TimeTrackerDisplay

### Community 5 - "MainForm Designer Layout"
Cohesion: 0.22
Nodes (7): Button, DateTimePicker, Label, TabControl, Panel, MainForm, TimeTrackerDisplay

### Community 6 - "Application Settings"
Cohesion: 0.38
Nodes (5): ApplicationSettingsBase, Settings, TimeTracker.Properties, TimeTrackerDisplay.Properties, Settings

### Community 7 - "TimeTracker Program Entry"
Cohesion: 0.40
Nodes (3): STAThread, Program, TimeTracker

### Community 8 - "TimeTracker Resources"
Cohesion: 0.40
Nodes (4): CultureInfo, Resources, ResourceManager, TimeTracker.Properties

### Community 9 - "Display Program Entry"
Cohesion: 0.40
Nodes (3): STAThread, Program, TimeTrackerDisplay

### Community 10 - "Display Resources"
Cohesion: 0.40
Nodes (4): TimeTrackerDisplay.Properties, CultureInfo, Resources, ResourceManager

## Knowledge Gaps
- **43 isolated node(s):** `TimeTracker`, `STAThread`, `TimeTracker`, `TimeTracker`, `NotifyIcon` (+38 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **2 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TimelinePanel` connect `TimelinePanel Custom Rendering` to `MainForm Designer Layout`?**
  _High betweenness centrality (0.026) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `TimeTrackerDisplay.MainForm` (e.g. with `Pie Chart Visualization` and `TimeTrackerDisplay (CSV viewer)`) actually correct?**
  _`TimeTrackerDisplay.MainForm` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `TimeTracker`, `STAThread`, `TimeTracker` to the rest of the system?**
  _48 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Tracker Core (P/Invoke, Idle Detection)` be split into smaller, more focused modules?**
  _Cohesion score 0.12169312169312169 - nodes in this community are weakly interconnected._
- **Should `Solution Architecture & Data Models` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._