# P0-T8 — Pre-existing CSharpier formatting state of the whole tree

Timestamp: 2026-09-13T04-56
Command: dotnet tool run csharpier check .
EXIT_CODE: 0

## CMD-CHECK output, verbatim

```
Checked 1626 files in 5132ms.
CHECK-EXIT: 0
```

The command inspected 1626 files and named none. CSharpier 1.2.6 prints one line per file that would
be reformatted; a run that names no file and exits 0 reports no drift anywhere in the tree.

PreExistingDriftFiles: NONE

DriftInsideWriteSet: NONE

## Consequence for P5-T1, recorded as this task directs

Both figures read `NONE`. Under the rule P0-T8 states, that is the branch in which the repository-wide
format command in P5-T1 rewrites nothing outside the Write Set, so the P5-T1 scope lock is satisfiable
exactly as written. The second branch of the rule does not apply: there is no pre-existing drift for
P5-T1 to repair, so no path is admitted to the Scope-lock rule on the strength of this record, and
acceptance criterion AC22 is not placed at risk by a repair this task would otherwise have had to
sanction.

The same conclusion carries forward to P6-T6 and P7-T22, which consume this figure through P5-T1: a
repository-wide format performed at the end of this run may only rewrite files this item edited,
because no other file in the tree was drifting at the anchor. A file outside the Write Set appearing
in a post-format porcelain status would therefore be a genuine scope-lock failure rather than a
pre-existing condition, and must be reported as one.

Output Summary: Repo-wide CSharpier check exited 0 after inspecting 1626 files and reported no
drifting file. PreExistingDriftFiles: NONE and DriftInsideWriteSet: NONE. The command was run while
this item held the shared build lock, which was released immediately after it returned. Acceptance
met.
