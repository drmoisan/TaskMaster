# [P7-T5] Nullable step (final toolchain loop)

- Issue: #792
- Timestamp: 2026-09-17T21-07
- PASS-NUMBER: 1
- Command: CMD-OUTLOOK, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` (CMD-BUILD-NULLABLE; no `/p:Nullable=enable`; run from `coverage/plan792-helper.ps1 -Step nullable -PassNumber 1` with the item worktree as the working directory; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`; console output captured to the gitignored `coverage/p7-t5-pass1-nullable.log`)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line; no process was ended)
  - `Build succeeded.` (1 line; `Build FAILED.` 0 lines)
  - `    0 Warning(s)` (verbatim msbuild summary line)
  - `    0 Error(s)` (verbatim msbuild summary line; exact-line match `^\s+0 Error\(s\)$` = true)
  - `Time Elapsed 00:00:15.80`; 11904 log lines; 0 lines matching `: error `; 0 lines matching `: warning `.

## Non-vacuity check of the Rebuild

From `coverage/p7-t5-pass1-nullable.log`:

- CSC-INVOCATIONS: 36 (equal to the [P0-T11] baseline count), of which 1 names `QuickFiler.csproj` or `/out:...QuickFiler.dll`
- CORECOMPILE lines: 72 counted unanchored and 72 with the `^\s*(\d+>)?CoreCompile:` node-prefix-tolerant anchor
- CORECOMPILE-SKIPPED: 0
- PROJECTS-DONE-REBUILD: 18 `.csproj` lines (plus the `Tags.Test.csproj.metaproj` and `TaskMaster.sln` lines, 20 in total; the same reconciliation as [P7-T4] applies to the baseline's 19)
- ANALYZER-ARG-LINES: 34; `CS0006` lines: 0
- Assemblies rewritten during the run (local time), before and after: `UtilitiesCS/bin/Debug/UtilitiesCS.dll` 21:07:06 to 21:08:02; `QuickFiler/bin/Debug/QuickFiler.dll` 21:07:10 to 21:08:05; `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` 21:07:13 to 21:08:08; `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` 21:07:16 to 21:08:11.

The before timestamps are the [P7-T4] outputs of this pass and every after timestamp is later, so this Rebuild produced the assemblies that [P7-T6] and [P7-T7] run.

msbuild was resolved from `PATH` (`MSBUILD-ON-PATH: True`).
