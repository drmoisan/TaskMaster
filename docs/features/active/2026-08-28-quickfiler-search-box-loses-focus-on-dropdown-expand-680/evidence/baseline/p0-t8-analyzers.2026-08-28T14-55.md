# P0-T8 — Baseline Analyzer Build (Issue #680)

Timestamp: 2026-08-28T15-02

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(run with `/v:m` for a readable log; `/t:Rebuild` is mandatory — a warm `/t:Build` skips
`CoreCompile` and runs no analyzers — and `/p:Nullable=enable` is deliberately not passed,
per CLAUDE.md.)

EXIT_CODE: 0

Output Summary:

- `MSBuild version 18.9.1+a81b43525 for .NET Framework`. All projects rebuilt and produced their
  output assemblies (SVGControl, VBFunctions, UtilitiesCS, Tags, TaskTree, ToDoModel,
  TaskVisualization, QuickFiler, TaskMaster and each of their `.Test` companions).
- Warning count: 5 warning lines in the minimal-verbosity log, 0 error lines.
- All 5 warnings are the same pre-existing, non-code diagnostic emitted once per project that
  carries a `packages.config`:
  `packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.`
  emitted for `UtilitiesCS.csproj`, `ToDoModel.csproj`, `QuickFiler.csproj`, `TaskMaster.csproj`,
  and `UtilitiesCS.Test.csproj`.
- Zero analyzer rule diagnostics (no `warning CSxxxx`, `MAxxxx`, `RCSxxxx`, `Sxxxx`, or `AsyncFixer`
  codes) were emitted. The distinct-warning-code tabulation over the log is empty because the only
  warnings carry no rule identifier.

Acceptance: satisfied — `EXIT_CODE: 0`.
