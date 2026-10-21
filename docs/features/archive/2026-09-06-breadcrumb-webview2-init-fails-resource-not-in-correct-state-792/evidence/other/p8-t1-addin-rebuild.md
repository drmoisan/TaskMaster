# [P8-T1] Human checkpoint and add-in rebuild

- Issue: #792
- Timestamp: 2026-09-17T21-26
- Command: CMD-OUTLOOK, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` (plain Rebuild, no analyzer or nullable switch, as the task text specifies; run from the gitignored `coverage/p8-t1-rebuild.ps1` under `pwsh -NoProfile -WorkingDirectory <item worktree> -File`; the script's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; `CWD-LEAF: item-792`; HEAD `986ce5aafb5cae63fb9a01ce1d904491ea2b3b95`; console output captured to the gitignored `coverage/p8-t1-rebuild.log`; the exit code was read from `$LASTEXITCODE` at script scope immediately after the native call, not from captured output)
- EXIT_CODE: 0
- Output Summary:
  - `OUTLOOK-CLOSED: true` (printed first; no `HALT:` line; no process named `OUTLOOK` was found; no process was ended and `Stop-Process` was not called)
  - `Build succeeded.` (1 line; `Build FAILED.` 0 lines)
  - `    0 Warning(s)` (verbatim msbuild summary line)
  - `    0 Error(s)` (verbatim msbuild summary line; exact-line match `^\s+0 Error\(s\)$` = 1)
  - `Time Elapsed 00:00:16.11`; 11698 log lines; 0 lines matching `: error `; 0 lines matching `: warning `; 0 lines matching `MSB3021`.
  - Build timestamp: started 2026-09-17T21:26:52, ended 2026-09-17T21:27:08 (local time).

## CMD-OUTLOOK gate

- Result: `OUTLOOK-CLOSED: true`.
- Positive control: the same `Get-Process -Name <name> -ErrorAction SilentlyContinue` form applied to `pwsh` returned 10 processes in the same invocation, so the null result for `OUTLOOK` is a true absence.
- Resident processes at gate time: `MSBuild` 0, `vstest.console` 2 (two-day-old orphans from a dead session, per the delegation), `testhost` 0. Issue #906 describes the resident-worker contention condition; no contention symptom (lock error, `MSB3021`, nondeterministic failure) was observed in this run.

## Non-vacuity check of the Rebuild

From `coverage/p8-t1-rebuild.log`:

- CSC-INVOCATIONS: 36 (lines naming `csc.exe`/`csc.dll`; equal to the [P7-T4] and [P0-T10] counts), of which 1 names `/out:...TaskMaster.dll`
- CORECOMPILE lines: 80 counted unanchored (`CoreCompile:`); an anchored `^CoreCompile:` count is not used because `/m` prefixes secondary-node lines with `N>`
- CORECOMPILE-SKIPPED: 0 (no `Skipping target "CoreCompile"` line)
- PROJECTS-DONE-REBUILD: 18 lines matching `Done Building Project .*\.csproj" \(Rebuild target\(s\)\)`; 0 lines carry `(default targets)`, so every project was driven by the Rebuild target
- `MSBUILD-ON-PATH: True`

## Add-in output assembly (proof the add-in was rewritten)

LastWriteTime (local time) before and after the run:

| Assembly | Before | After |
|---|---|---|
| `TaskMaster/bin/Debug/TaskMaster.dll` (the add-in) | 2026-09-17 21:08:08 | 2026-09-17 21:27:02 |
| `TaskMaster/bin/Debug/QuickFiler.dll` (copied dependency, source of the fix) | not sampled before the run | 2026-09-17 21:26:59 |
| `QuickFiler/bin/Debug/QuickFiler.dll` | 2026-09-17 21:08:05 | 2026-09-17 21:26:59 |
| `UtilitiesCS/bin/Debug/UtilitiesCS.dll` | 2026-09-17 21:08:02 | 2026-09-17 21:26:56 |

The add-in assembly's LastWriteTime advanced into the build window (21:26:52 to 21:27:08), the csc invocation count matches the earlier full rebuilds, and no `CoreCompile` target was skipped; the three signals agree that `TaskMaster/bin/Debug` now holds the add-in compiled from HEAD `986ce5aaf`.

Observation as recorded at 21:27 (SUPERSEDED — see "Correction: the manifest gap was real" below): no `TaskMaster.vsto` or `TaskMaster.dll.manifest` exists in `TaskMaster/bin/Debug` after the run and the log never invokes a manifest-generation target. `TaskMaster/TaskMaster.csproj` (line 550) imports `Microsoft.VisualStudio.Tools.Office.targets` only when `BuildingInsideVisualStudio` is `true`, and its comment (lines 546-549) states that command-line builds "only need the compiled add-in assembly". Every earlier build in this plan produced the same output shape. The 21:27 record described this as "not a defect" and "by design"; that framing was wrong for the purpose of this task, because a registered `|vstolocal` add-in cannot load without the `.vsto` deployment manifest.

## Correction: the manifest gap was real

The 21:27 check found `TaskMaster.vsto` and `TaskMaster.dll.manifest` BOTH ABSENT. That was a real gap, not a moot caveat: with the registered manifest pointing at `TaskMaster/bin/Debug/TaskMaster.vsto`, Outlook had nothing to load. A manifest-generating build (one with `BuildingInsideVisualStudio` true, that is a build driven from Visual Studio) closed the gap between the command-line rebuild and the verification session. Observed on 2026-09-18 (`Get-Item` over the item worktree's `TaskMaster/bin/Debug`):

| File | Length | LastWriteTime (local) |
|---|---|---|
| `TaskMaster/bin/Debug/TaskMaster.vsto` | 6615 | 2026-09-17 21:43:18 |
| `TaskMaster/bin/Debug/TaskMaster.dll.manifest` | 86173 | 2026-09-17 21:43:18 |
| `TaskMaster/bin/Debug/TaskMaster.dll` | 294400 | 2026-09-17 21:27:02 (unchanged from the table above) |

The two manifests were written sixteen minutes after the command-line rebuild ended (21:27:08) and four seconds before the add-in's `ThisAddIn_Startup()` fired (21:43:22). `TaskMaster.dll` kept its 21:27:02 timestamp, so the manifests were generated over the assembly this task rebuilt, not over a newer one; the add-in that Outlook loaded is the one compiled from HEAD `986ce5aaf`.

Instruction for the next reader: after a command-line `msbuild` rebuild of this solution, the manifest-generation step is a separate, required action before Outlook can load the `|vstolocal` add-in. Do not skip it as a non-issue.

## Maintainer confirmation (recorded 2026-09-18T06-28)

The task's second half, "the person reopens Outlook and confirms the add-in loaded", is recorded here from the maintainer's direct verification, with the registry and log facts re-derived by the executor on 2026-09-18.

- Registered add-in: `HKCU\Software\Microsoft\Office\Outlook\Addins\TaskMaster` has `LoadBehavior` = 3 and `Manifest` resolving to the item worktree's `TaskMaster/bin/Debug/TaskMaster.vsto` with the `|vstolocal` suffix (re-derived: `Get-ItemProperty` on that key; the manifest string contains `/item-792/TaskMaster/bin/Debug/TaskMaster.vsto` and ends `|vstolocal`; the absolute prefix is deliberately not transcribed).
- Outlook reopened at: process start 2026-09-17 21:43:18 (`Get-Process -Name OUTLOOK`, `StartTime`, still running on 2026-09-18); add-in session start 2026-09-17 21:43:22 (`ThisAddIn_Startup() fired`, first line of the worktree's own `TaskMaster/bin/Debug/logs/debug_2026-09-17.log`).
- Add-in loaded: YES. Observed by the maintainer through the add-in's ribbon and QuickFiler/Efc views being usable in that session ([P8-T2] records the runbook observations), and instrument-verified by the session log in this worktree's own `TaskMaster/bin/Debug/logs/` directory, which the add-in writes only when it is running from this output directory. The log's first line is the add-in startup event at 21:43:22,837 and its `QuickFiler.*` loggers wrote lines during the session.
- Recorded by / at: atomic-executor on behalf of the maintainer's stated observation, 2026-09-18T06-28 (local).

No process was ended at any point; Outlook was still running when this confirmation was recorded and was not closed or killed by the executor.

With this confirmation the task's acceptance is fully satisfied: `OUTLOOK-CLOSED: true` before the build, `EXIT_CODE: 0` with the exact `0 Error(s)` line, the build timestamp, and the person's confirmation that Outlook was reopened with the add-in loaded, with no `Stop-Process` anywhere. Nothing in this artifact is evidence about AC-U5 itself; that is [P8-T2].
