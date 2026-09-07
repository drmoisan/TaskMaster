# Phase 0 — NuGet Restore ([P0-T6])

Timestamp: 2026-08-28T05-11

Command: `pwsh -NoProfile -File ./scripts/vscode/Invoke-Restore.ps1`, run from the worktree root.
EXIT_CODE: 0

## Why this script rather than a bare `nuget restore`

`Invoke-Restore.ps1` is the `packages.config`-aware equivalent of `nuget restore TaskMaster.sln` for
this repository. The projects are legacy non-SDK VSTO / .NET Framework projects that import
`..\packages\<id>.<version>\build\*.props` conditionally. Without a completed restore those imports
silently do not fire, the analyzer, MSTest adapter, and coverage props never load, and the build
produces a weaker diagnostic set while still exiting 0. The restore is therefore a precondition of
every gate in this plan, not a convenience.

## Acceptance checks

| Check | Required | Observed | Result |
| --- | --- | --- | --- |
| Exit code | 0 | 0 | pass |
| `packages\MSTest.Analyzers.4.3.3` exists | yes | yes | pass |

## Restore output tail

```
1>Done Building Project "<worktree-root>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:00.53
```

The restore reported `Build succeeded` with zero warnings and zero errors. The sub-second elapsed time
reflects that the package graph was already materialised in `packages/` during worktree bootstrap
before plan execution began; this run re-verified it rather than downloading it. The absolute solution
path is elided above to keep the artifact free of a host account name.

## Resulting package state

`packages/` holds **174** package directories after this run.

Output Summary: Restore exited 0 with `Build succeeded`, zero warnings and zero errors.
`packages\MSTest.Analyzers.4.3.3` is present, satisfying this task's stated acceptance. 174 package
directories are materialised under `packages/`. The two analyzer packages whose versions are skewed
from the `Analyzer Include` items are handled separately by `[P0-T7]`.
