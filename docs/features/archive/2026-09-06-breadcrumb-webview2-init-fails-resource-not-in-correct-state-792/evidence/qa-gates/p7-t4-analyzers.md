# [P7-T4] Analyzer step (final toolchain loop)

- Issue: #792
- Timestamp: 2026-09-17T21-07
- PASS-NUMBER: 1
- Command: CMD-OUTLOOK, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (CMD-BUILD-ANALYZE; run from `coverage/plan792-helper.ps1 -Step analyze -PassNumber 1` with the item worktree as the working directory; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`; console output captured to the gitignored `coverage/p7-t4-pass1-analyze.log`)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line; no process was ended)
  - `Build succeeded.` (1 line; `Build FAILED.` 0 lines)
  - `    0 Warning(s)` (verbatim msbuild summary line; the [P0-T10] baseline count is 0, so the count is not greater than the baseline)
  - `    0 Error(s)` (verbatim msbuild summary line; exact-line match `^\s+0 Error\(s\)$` = true)
  - `Time Elapsed 00:00:17.81`; 11877 log lines; 0 lines matching `: error `; 0 lines matching `: warning `.

## Non-vacuity check of the Rebuild

From `coverage/p7-t4-pass1-analyze.log`:

- CSC-INVOCATIONS: 36 (lines naming `csc.exe`/`csc.dll`; equal to the [P0-T10] baseline count), of which 1 names `QuickFiler.csproj` or `/out:...QuickFiler.dll`
- CORECOMPILE lines: 83 counted unanchored (`CoreCompile:`) and 83 with the `^\s*(\d+>)?CoreCompile:` node-prefix-tolerant anchor (an anchored `^CoreCompile:` count is not used because `/m` prefixes secondary-node lines with `N>`)
- CORECOMPILE-SKIPPED: 0 (no `Skipping target "CoreCompile"` line)
- PROJECTS-DONE-REBUILD: 18 lines matching `Done Building Project .*\.csproj" \(Rebuild target\(s\)\)`. Reconciliation with the [P0-T10] figure of 19: the log carries 20 `Done Building Project ... (Rebuild target(s))` lines in total = 18 `.csproj` + 1 `Tags.Test.csproj.metaproj` (the solution-generated metaproject, which the baseline's looser `.csproj` pattern also matched, giving 19) + 1 `TaskMaster.sln`; 0 lines carry `(default targets)`, so every project was driven by the Rebuild target.
- ANALYZER-ARG-LINES: 34 (`/analyzer:` lines; equal to baseline); `CS0006` lines: 0
- Assemblies rewritten during the run (local time), before and after: `UtilitiesCS/bin/Debug/UtilitiesCS.dll` 20:09:52 to 21:07:06; `QuickFiler/bin/Debug/QuickFiler.dll` 20:46:49 to 21:07:10; `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` 20:46:51 to 21:07:13; `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` 20:46:54 to 21:07:16.

Three independent signals (csc invocation count, zero skipped CoreCompile targets, and every tracked assembly's LastWriteTime advancing to the run window) agree that the Rebuild compiled every project rather than returning a warm no-op.

msbuild was resolved from `PATH` (`MSBUILD-ON-PATH: True`).
