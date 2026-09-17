# [P0-T10] Analyzer baseline

- Issue: #792
- Timestamp: 2026-09-17T18-40
- Command: CMD-OUTLOOK, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; console output captured to the gitignored `coverage/p0-t10-analyze.log`)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line)
  - `Build succeeded.`
  - `    0 Warning(s)` (verbatim msbuild summary line)
  - `    0 Error(s)` (verbatim msbuild summary line; exact-line match `^\s+0 Error\(s\)$` = true)
  - Elapsed 16 seconds; 4994 log lines; 0 lines matching `: error `; 0 lines matching `: warning `.

## Non-vacuity check of the Rebuild

The 16-second duration was short enough to warrant confirming that the Rebuild compiled rather than skipped. From `coverage/p0-t10-analyze.log`:

- CSC-INVOCATIONS: 36 (lines naming `csc.exe`/`csc.dll`)
- CORECOMPILE-SKIPPED: 0 (no `Skipping target "CoreCompile"` line)
- PROJECTS-DONE-REBUILD: 19 (`Done Building Project "<x>.csproj" (Rebuild target(s))` lines)
- ANALYZER-ARG-LINES: 34; the provisioned `Meziantou.Analyzer.3.0.203` path appears on 31 lines and produced no `CS0006`.
- Assemblies rewritten during the run (local time): `QuickFiler/bin/Debug/QuickFiler.dll` 18:40:38, `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` 18:40:40, `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` 18:40:43, `UtilitiesCS/bin/Debug/UtilitiesCS.dll` 18:40:34.

msbuild was resolved from `PATH` (`MSBUILD-ON-PATH: true`).
