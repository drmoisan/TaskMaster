# Format-pass scope gate (Issue #824, task P5-T3)

Timestamp: 2026-09-09T16-00

Command: comparison of the `---NAMES---` listing captured by P5-T1 against the D6 Owned Write Set
and against the pre-existing-drift list recorded by P0-T8. No new command was issued; this task
consumes two recorded observations.

EXIT_CODE: 0

## Inputs

| Input | Source artifact | Content |
|---|---|---|
| P5-T1 `---NAMES---` listing | `evidence/qa-gates/csharpier-format.2026-09-09T15-58.md` | 37 paths |
| P0-T8 pre-existing-drift list | `evidence/baseline/csharpier-check-baseline.2026-09-09T15-06.md` | `none` |

The P0-T8 drift list is **empty**: the baseline `dotnet tool run csharpier check .` exited 0 and
reported no file as needing formatting. That is a material fact for this gate, and it removes the
blocked branch entirely. Had the drift list been non-empty, P5-T2's repository-wide `check .` could
have reached exit 0 only by way of a repair this feature does not own, and the correct outcome would
have been a blocked report rather than a silent waiver.

## Disposition of every path in the P5-T1 `---NAMES---` listing

Each path takes one of the two dispositions this task defines. All 37 take the first.

| Disposition | Count |
|---|---|
| Satisfies a D6 class | 37 |
| Absent from D6 but present in the P0-T8 drift list | 0 |

### Satisfies D6 class 1 — the two modified source files (2 paths)

- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs`

These are the only two files the P5-T1 format pass rewrote, and both are named in the plan's Scope
section as modified by this feature.

### Satisfies D6 class 2 — under the #824 feature folder (35 paths)

The plan file `plan.2026-09-08T23-51.md`, plus 34 evidence artifacts under `evidence/baseline/`
(15), `evidence/other/` (3), `evidence/qa-gates/` (8) and `evidence/regression-testing/` (8). The
qa-gates count is one higher than at the P4-T8 scope gate because
`csharpier-format.2026-09-09T15-58.md` was written between the two.

CSharpier did not rewrite any of these: `.csharpierignore:4-8` excludes `**/evidence/**` from
formatting, and markdown is not a format CSharpier processes. They appear in the listing because
they are this run's own evidence writes, not because the formatter touched them.

### Satisfies D6 class 3 — under `.claude/agent-memory/` (0 paths)

None at this point in the run.

## Result

No path in the listing lies outside the three D6 classes, so neither remediation branch of this task
is taken:

- The blocked branch, for a path outside D6 that appears in the P0-T8 drift list, does not arise:
  that list is empty.
- The restore-and-restart branch, for a path outside D6 that was clean at baseline and that this
  plan does not edit, does not arise: no such path is present, so no
  `git checkout HEAD -- <path>` restoration was performed and the loop was not restarted for this
  reason.

The format pass rewrote nothing outside this feature's scope. The gate passes.

Note on the loop restart that did occur: P5-T1 pass 1 rewrote two tracked files and restarted the
loop, and pass 2 reported `FORMAT_CHANGED_TREE=False`. That restart was caused by the formatter
reflowing this feature's own new code, which is D6 class 1, not by any out-of-scope path. It is
recorded in the P5-T1 artifact rather than here, because this gate concerns scope rather than
convergence.
