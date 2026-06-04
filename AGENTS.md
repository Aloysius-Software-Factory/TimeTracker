# TimeTracker — AGENTS.md

## Project

Single-project .NET Framework 4.0 Windows Forms app (WinExe), built with Visual Studio 2019.

## Build

```powershell
msbuild TimeTracker.sln /p:Configuration=Debug
# or
dotnet build TimeTracker.sln   # requires .NET Framework targeting pack
```

## Known issues

- `.csproj` references `Form1.cs` / `Form1.Designer.cs` but those files do not exist on disk — will fail to compile.
- `Program.cs` calls `Application.Run(new Tracker())` but `Tracker` does not inherit from `Form` or `ApplicationContext` — will fail to compile.
- Target framework is `v4.0`; requires Visual Studio 2019 (or compatible MSBuild + .NET Framework 4.0 SDK) to build.
- No tests, no CI, no package dependencies.

## Architecture

| File | Role |
|---|---|
| `Program.cs` | STAEntryPoint — calls `Application.Run(new Tracker())` |
| `Tracker.cs` | Stub class — intended as the main form but not wired up yet |
| `Properties/` | Assembly metadata, settings, resources |

## Conventions

- Namespace: `TimeTracker`
- Assembly: `TimeTracker` (single EXE output to `bin\Debug\|Release\`)
- UI framework: WinForms (`System.Windows.Forms`)
