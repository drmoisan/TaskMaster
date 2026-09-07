# Baseline 3 of 4 — Nullable / Type-Check Gate (`/t:Rebuild`) ([P0-T10])

Timestamp: 2026-08-27T20-01

Command:
```
& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```
(run through `pwsh -NoProfile` from the workspace root; MSBuild resolved through `vswhere`)

Resolved MSBuild path:
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

Argument list as passed:
`TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

`/p:Nullable=enable` was **not** added, per `CLAUDE.md` §C#1.3: nullable participation in this
repository is per-file opt-in and CI omits the solution-wide property deliberately.

EXIT_CODE: 0

## Output Summary

- **Error count: 0** (`0 Error(s)` in the MSBuild summary).
- Distinct `: error XXnnnn` lines in the log: 0. In particular no `CS86xx` diagnostic was reported
  from any `#nullable enable` file, including
  `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, whose line 1 carries the directive.
- Warning count: 5 — the same five pre-existing `packages.config` / System.Reactive packaging
  advisories recorded in `baseline-2-analyzers-rebuild.2026-08-27T20-01.md`. They are MSBuild-level
  messages with no rule ID and are not promoted to errors by `/p:TreatWarningsAsErrors=true`, which
  applies to compiler diagnostics.
- **Non-vacuity check: `Skipping target "CoreCompile"` lines = 0.** `/t:Rebuild` recompiled every
  project, so the nullable-flow analysis actually ran. A warm `/t:Build` would have returned exit 0
  with `CoreCompile` skipped on every project and could not have failed.

This baseline is clean, so any `CS86xx` error observed at `[P2-T17]` or `[P4-T3]` is attributable to
this change.
