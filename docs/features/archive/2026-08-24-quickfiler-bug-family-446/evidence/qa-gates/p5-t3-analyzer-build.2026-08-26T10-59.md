# [P5-T3] Analyzer Gate — ACCEPTED PASS

Timestamp: 2026-08-26T10-59

Task: [P5-T3]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Executed through `pwsh -NoProfile`. `$msbuild` was resolved at run time with the plan's vswhere
prelude to the Visual Studio 18 Community full-framework MSBuild under
`<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
(`MSBuild version 18.8.2+ce25c0108 for .NET Framework`).

EXIT_CODE: 0

## Command-text assertions required by this task

- The recorded command text contains `/t:Rebuild` and **does not contain** `/t:Build`.
- The recorded command text **does not contain** `Nullable=enable`. Per D-Plan-6, no project in
  this repository carries a `<Nullable>` element and CI omits the property deliberately.

## MSBuild Summary Counts

- Error count: `0`
- Warning count: `5`
- `Build succeeded.`, Time Elapsed `00:00:23.86`

## Non-vacuity of the gate

`/t:Rebuild` is load-bearing: MSBuild's incremental up-to-date check compares timestamps and does
not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit `0` with
`CoreCompile` skipped on every project and runs no analyzer at all. Counts taken over the full
build log:

- Occurrences of `Skipping target "CoreCompile"`: **0**
- Lines mentioning `CoreCompile`: 73
- Lines mentioning `csc.exe`: 36

Zero skips together with executed `CoreCompile` target banners establishes that every project was
compiled and that the analyzers ran. The gate could have failed.

## Warning detail

All five warnings are the same pre-existing diagnostic emitted by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`: "The
project contains a packages.config file, which is not supported by System.Reactive v7.0 or later."
It is emitted once per affected project (`ToDoModel.csproj`, `QuickFiler.csproj`,
`TaskMaster.csproj`, `UtilitiesCS.Test.csproj` among them). This is the same warning population as
the `[P0-T10]` baseline, which also recorded `5 Warning(s)` / `0 Error(s)`. The change set
introduced no new warning.

No analyzer rule diagnostic and no `CS` diagnostic was reported. In particular there are **0**
occurrences of `IDE0005` (unnecessary using directive), so the `using System.Diagnostics;`
directive in `QuickFiler/Controllers/QfcFormController.Actions.cs` — whose only consumer,
`Stopwatch`, was removed by a Phase 3 change — is **not** flagged by this gate. No task in this
plan authorizes removing that directive and it was not removed.

## Build-output lock check

Occurrences of `being used by another process`, `MSB3021`, `MSB3023`, `MSB3026` and `MSB3027` in
the build log: **0**. No `obj/` or `bin/` output was locked and no build failure was caused by a
lock.

## Output Summary

Analyzer gate passed: `EXIT_CODE: 0`, `0 Error(s)`, `5 Warning(s)` all pre-existing and equal in
number to the `[P0-T10]` baseline. Non-vacuity proven by zero `Skipping target "CoreCompile"`
occurrences across the whole log. No locked build output.
