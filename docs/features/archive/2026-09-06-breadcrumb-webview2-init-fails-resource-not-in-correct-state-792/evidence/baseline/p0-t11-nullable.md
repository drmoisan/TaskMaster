# [P0-T11] Nullable baseline

- Issue: #792
- Timestamp: 2026-09-17T18-42
- Command: CMD-OUTLOOK, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; console output captured to the gitignored `coverage/p0-t11-nullable.log`)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line)
  - `Build succeeded.`
  - `    0 Warning(s)` (verbatim msbuild summary line)
  - `    0 Error(s)` (verbatim msbuild summary line; exact-line match `^\s+0 Error\(s\)$` = true)
  - Elapsed 14 seconds; 11638 log lines; 0 lines matching `: error `; 0 lines matching `: warning `.

## Non-vacuity check of the Rebuild

From `coverage/p0-t11-nullable.log`:

- CSC-INVOCATIONS: 36
- CORECOMPILE-SKIPPED: 0
- PROJECTS-DONE-REBUILD: 19
- ANALYZER-ARG-LINES: 34; `Meziantou.Analyzer.3.0.203` on 31 lines, no `CS0006`.
- Assemblies rewritten during the run (local time): `QuickFiler/bin/Debug/QuickFiler.dll` 18:42:44, `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` 18:42:46, `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` 18:42:49, `UtilitiesCS/bin/Debug/UtilitiesCS.dll` 18:42:41.

msbuild was resolved from `PATH` (`MSBUILD-ON-PATH: true`).
