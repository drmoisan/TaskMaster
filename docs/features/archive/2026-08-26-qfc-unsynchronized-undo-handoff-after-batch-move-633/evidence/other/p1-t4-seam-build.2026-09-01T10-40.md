# Seam build verification (P1-T4)

Timestamp: 2026-09-01T10-40
Task: [P1-T4]
Working directory: WORKTREE

## Prerequisite step

Command: `New-Item -ItemType Directory -Force -Path FEATURE/evidence/other`
EXIT_CODE: 0

This step is a hard prerequisite, not decoration. MSBuild's file logger does not create intermediate
directories: a `/flp:logfile=` target whose directory part does not exist terminates the build with
MSB1029 before any project is built. `FEATURE/evidence/other` did not exist before this task.

## Build step

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

File log: `FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt` (12124 lines).

Verbatim summary lines:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

Count of occurrences of the literal `Skipping target "CoreCompile"` in
`FEATURE/evidence/other/p1-t4-seam-build.msbuild.txt`: **0**.

Count of `CS`, `CA`, `IDE`, `SA`, `MA`, `RCS`, or `S`-prefixed analyzer or compiler diagnostic lines: 0.

Output Summary: The Phase 1 seam compiles cleanly. The full-solution rebuild exited 0 with 0 errors and
the same 5 pre-existing System.Reactive `packages.config` warnings the P0-T8 baseline recorded — the
warning count did not move, so the seam introduced no new diagnostic. Zero `Skipping target
"CoreCompile"` occurrences confirm every project was genuinely recompiled, so the analyzers actually
observed the new `ItemProcessor` property and the rewritten worker call site.

Two facts the compile confirms that a search cannot. First, the lambda
`item => item.Filer.SortAsync(item.Helpers)` is accepted as a `Func<FilerQueueItem, Task>`: the method
returns `Task<bool>`, which carries a reference conversion to `Task`, so the seam type needs no generic
parameter. Second, the property initializer references no instance member, so no CS0236 arises and no
workaround was required.

Phase 1 leaves observable production behaviour unchanged: the `guard` field, the `Consumer` semantics,
and both `Enqueue` overloads are untouched, and the per-item `try`, `catch (Exception e)`,
`item.Helpers.First()` diagnostic, and `logger.Error` call are byte-identical around the seam call.
That is what lets the Phase 2 fail-before tests compile and still observe the defect.
