# Phase 0 — baseline formatting state

Timestamp: 2026-08-27T23-21
Task: [P0-T9]
Command: `dotnet tool run csharpier check .` from the worktree root, under `pwsh -NoProfile`
EXIT_CODE: 0

## Result

Complete stdout, verbatim:

```
Checked 1543 files in 4510ms.
```

## BASELINE_UNFORMATTED

The complete list of files reported as unformatted is **empty**:

```
BASELINE_UNFORMATTED: (none)
```

Cardinality: **0**.

An empty list is an explicitly valid result under `[P0-T9]`. The command is read-only and rewrote no
file; the exit code 0 confirms csharpier found no formatting difference in any of the 1543 files it
checked.

This empty set is the comparison basis for `[P10-T3]`. Because it is empty, `[P10-T3]`'s "subset of the
baseline set" branch degenerates: at final QC the repository-wide `csharpier check .` must report
`EXIT_CODE: 0` with no unformatted file at all, since any reported file would necessarily fall outside an
empty baseline set.

Output Summary: csharpier check . exited 0 over 1543 files with zero unformatted files. BASELINE_UNFORMATTED
is the empty set.
