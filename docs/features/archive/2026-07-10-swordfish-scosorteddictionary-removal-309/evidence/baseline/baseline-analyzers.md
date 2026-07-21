# Phase 0 — C# Analyzer Baseline (P0-T3)

- Timestamp: 2026-07-10T23:25
- Command: `"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` (repo root; dash-switch form used because the Bash tool runs git-bash/MSYS, which mangles slash-prefixed MSBuild switches into paths — see project memory `project_build_test_env`)
- EXIT_CODE: 0
- Output Summary: `Build succeeded.` — 76 Warning(s), 0 Error(s), Time Elapsed 00:00:20.01. All 20 projects in the solution (first-party + vendored `SVGControl`/`UtilitiesSwordfish`) built cleanly under `-t:Rebuild`. Warnings are pre-existing and span: `CS0618` (obsolete `IAsyncEnumerable` LINQ extension usages in `UtilitiesCS`, `ToDoModel`, `TaskVisualization`, `QuickFiler`, `TaskMaster`), `CS8632` (nullable annotation outside `#nullable` context, mostly in `.Test` projects), `CS0169`/`CS0067` (unused fields/events in test doubles), `CS0108` (member hiding in `QuickFiler.IItemViewer`), `CS4014` (un-awaited call in `TaskVisualization`), and `MSTEST0032` (always-true assertion in `QuickFiler.Test`). None reference `ScoSortedDictionary` or the SCO directory.

Full build log preserved at (session scratchpad, not committed): `p0t3_analyzer_baseline_rebuild.log`.
