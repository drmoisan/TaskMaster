# Final QC stage 2 — analyzer / lint gate

Timestamp: 2026-08-26T13-42
Task: [P7-T3]

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild, run from the worktree root; the CLAUDE.md
analyzer command verbatim with `/t:Rebuild` per decision D1):

```
MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

EXIT_CODE: 0

## Result

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:22.27
```

| Metric | Value |
|---|---|
| **Errors** | **0** |
| Warnings | 5 |

`/t:Rebuild` is used rather than `/t:Build` because MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and the analyzer gate cannot fail. The 22-second elapsed time confirms every project was
recompiled.

## Warning breakdown

All 5 warnings are the pre-existing `System.Reactive.PackagesConfigCheck.targets(31,5)` notice — "The
project contains a packages.config file, which is not supported by System.Reactive v7.0 or later" —
emitted once per project that carries a `packages.config` referencing System.Reactive 7.0.0. None
originates in a file this feature touched, and none is an analyzer diagnostic.

## Comparison against the `[P0-T10]` baseline

| | Errors | Warnings | Exit code |
|---|---|---|---|
| `[P0-T10]` baseline | 0 | 5 | 0 |
| `[P7-T3]` post-change | 0 | 5 | 0 |

Unchanged. This feature introduced no analyzer diagnostic.

Output Summary: EXIT_CODE 0, 0 errors, 5 warnings — identical to the `[P0-T10]` baseline. The lint
stage passes and the toolchain loop does not restart.
