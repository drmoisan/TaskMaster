# Baseline — NuGet Restore (Issue #418)

Task: `[P0-T5]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T14-55

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 0

Output Summary: Restore succeeded. MSBuild reported `Build succeeded.` with
`1 Warning(s)` and `0 Error(s)`, elapsed `00:00:40.14`. NuGet reported
`Installed: 91 package(s) to packages.config projects` into
`C:\Users\DanMoisan\source\repos\drmoisan\TaskMaster\packages`. The single warning is a
pre-existing vulnerability advisory unrelated to `SVGControl` or `SVGControl.Test`:

```text
UtilitiesCS\UtilitiesCS.csproj : warning NU1902: Package 'AngleSharp' 1.4.0 has a known
moderate severity vulnerability, https://github.com/advisories/GHSA-pgww-w46g-26qg
```

No package-resolution error was reported. Baseline restore state is clean apart from that
advisory.

## Toolchain Detail

```text
Using MSBuild: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
MSBuild version 18.4.0+6e61e96ac for .NET Framework
Build started 8/4/2026 2:55:52 PM.
Building solution configuration "Debug|Any CPU".
```

NuGet config files used:

- `C:\Users\DanMoisan\AppData\Roaming\NuGet\NuGet.Config`
- `C:\Program Files (x86)\NuGet\Config\Microsoft.VisualStudio.FallbackLocation.config`
- `C:\Program Files (x86)\NuGet\Config\Microsoft.VisualStudio.Offline.config`

Feeds used:

- `C:\Users\DanMoisan\.nuget\packages\`
- `https://api.nuget.org/v3/index.json`
- `C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\`

## Scope Note (baseline fact, not a defect introduced here)

This restore is solution-scoped. `SVGControl.Test` is not a member of `TaskMaster.sln`
(recorded in `svgcontrol-test-buildability.2026-08-04T14-36.md` under task `[P0-T10]`), so
`SVGControl.Test/packages.config` was not part of the restore graph and its seven pinned
packages were not restored by this command. That is the expected baseline consequence of the
project's absence from the solution and is the state task `[P1-T3]` is planned to change. No
action is taken here.

Restore log line count: 539.
