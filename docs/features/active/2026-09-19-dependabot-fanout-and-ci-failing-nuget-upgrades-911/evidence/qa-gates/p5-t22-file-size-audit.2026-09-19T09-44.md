# P5-T22 — Batch C file-size audit

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; for each of the six Batch C files, ([System.IO.File]::ReadAllLines(<absolute path>)).Count'
```

and

```
git -C "<execution-worktree-root>" status --porcelain --untracked-files=all -- scripts/dependencies tests/scripts/dependencies
```

EXIT_CODE: 0

## Output Summary

```
LISTED=6
scripts/dependencies/AnalyzerItemRepair.psm1 = 399
scripts/dependencies/ProjectConsistency.psm1 = 322
scripts/dependencies/ConsistencyVerifier.psm1 = 493
tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1 = 311
tests/scripts/dependencies/ProjectConsistency.Tests.ps1 = 375
tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1 = 272
PACKAGEGRAPH_IN_LIST=False
PACKAGEGRAPH_MODIFIED=False
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Files listed | exactly 6 | 6 |
| An integer line count for each | yes | 6 integers, above |
| Every count at most 500 | <= 500 | max 493 |

| File | Lines | Margin to the 500-line ceiling |
|---|---|---|
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 399 | 101 |
| `scripts/dependencies/ProjectConsistency.psm1` | 322 | 178 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | 7 |
| `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 311 | 189 |
| `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 375 | 125 |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 272 | 228 |

### Re-measured after the P6-T2 analyzer fixes

Twelve owned PSScriptAnalyzer findings were fixed at P6-T2, after this task first ran. A
line count is presentational and does not survive an intervening transformation, so the
figures above are stale for four of the six files and the audit was re-taken. The
acceptance is unchanged: six files, an integer each, every count at most 500.

| File | Lines | Change | Margin |
|---|---|---|---|
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 402 | +3 | 98 |
| `scripts/dependencies/ProjectConsistency.psm1` | 331 | +9 | 169 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | 0 | 7 |
| `tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1` | 311 | 0 | 189 |
| `tests/scripts/dependencies/ProjectConsistency.Tests.ps1` | 379 | +4 | 121 |
| `tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1` | 275 | +3 | 225 |

### Re-measured again after the P6-T3 coverage cases

P6-T3 required four further cases in
`tests/scripts/dependencies/ProjectConsistency.Tests.ps1` to lift
`scripts/dependencies/ProjectConsistency.psm1` from 86.36 to 100.00 percent line coverage.
That file is now **453** lines, 47 inside the ceiling. The other five are unchanged from
the figures immediately above. Every count remains at most 500, and the file list is still
exactly the same six.

The additions are the local bindings and their explanatory comments that the
`PSReviewUnusedParameter` fixes required, and the `$null` discards in the two fixture
delegates. The at-risk file `ConsistencyVerifier.psm1` is unchanged at 493, because its
only fix replaced two em dashes with parentheses on existing lines.

`PACKAGEGRAPH_MODIFIED` remains `False` after the fixes: no analyzer finding sat in that
file and none of the fixes touched it.

The exactly-6 clause guards against an enumerator that listed nothing. The list is the
literal six-member set the task names and is not derived from a search.

## The at-risk file

`ConsistencyVerifier.psm1` is the file this task was written to watch, and it did overrun
during P5-T8: the first implementation measured 644 lines and the first compaction pass
598. It now stands at 493 with 7 lines of margin. The overrun was resolved by relocating
the shared restore-path vocabulary into `AnalyzerItemRepair.psm1`, the module whose subject
is restore paths, and by shortening comment-based help; P5-T8's artifact records both
changes and the measurements at each step. No behaviour and no test was dropped, and no
work moved to Batch D.

The 7-line margin is the narrowest in the batch. A later phase adding to this module must
re-measure rather than assume headroom.

## PackageGraph.psm1 is deliberately absent from the list

`scripts/dependencies/PackageGraph.psm1` is not one of the six. No Batch C task wrote to
it, and a recorded modification of it would mean the Batch C production cap of three was
breached. Two independent measurements confirm it was not touched:

- `PACKAGEGRAPH_MODIFIED=False` — `git status --porcelain` scoped to that path is empty.
- The porcelain capture over `scripts/dependencies` and `tests/scripts/dependencies` lists
  exactly six untracked paths and no modified path at all:

```
?? scripts/dependencies/AnalyzerItemRepair.psm1
?? scripts/dependencies/ConsistencyVerifier.psm1
?? scripts/dependencies/ProjectConsistency.psm1
?? tests/scripts/dependencies/AnalyzerItemRepair.Tests.ps1
?? tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1
?? tests/scripts/dependencies/ProjectConsistency.Tests.ps1
```

All six are untracked rather than modified, because all six are created by this batch.
`PackageGraph.psm1` and `PackageCompatibility.psm1` are tracked and unmodified, so they do
not appear. This is the second of the three independent checks on the Phase 5 prohibition;
the other two are P6-T6's commit listing and P6-T7's exact-3-and-3 counts.
