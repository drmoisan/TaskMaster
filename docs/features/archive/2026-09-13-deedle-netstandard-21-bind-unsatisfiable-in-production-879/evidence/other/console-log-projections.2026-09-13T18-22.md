# Raw Console Log Projections

Recorded by `[P5-T11]`, with the removal span recorded by `[P5-T12]` appended below. These two
tasks run after the four-step loop has completed cleanly and are not part of it.

Every gate that reads a raw console log — `[P0-T6]`, `[P0-T7]`, `[P2-T10]`, `[P4-T1]`, `[P5-T5]`
and `[P5-T6]` — has already run and has already recorded its figure in its own `.md` artifact,
so projecting and then removing the raw logs invalidates no acceptance condition in this plan.

The `[P4-T16]` Revision R7 rebuild wrote its console log to
`TestResults/r7-build/r7-build-console.txt`, which is under the git-ignored `TestResults/`
scratch tree. It is deliberately not one of the six logs below and is neither projected nor
removed here.

Timestamp: 2026-09-14T12-56

Command: the single `[P5-T11]` span, run once. The repository root is derived at run time with
`(Resolve-Path .).Path` rather than written as a literal.

EXIT_CODE: 0

Output Summary:

```
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/analyzer-baseline-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=5030
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/baseline/nullable-baseline-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=11842
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-build-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=11858
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/pass-after-build-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=12397
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/analyzer-final-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=11722
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
PROJECTION_WRITTEN=docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/qa-gates/nullable-final-console.2026-09-13T18-22.projection.md
PROJECTION_ABSOLUTE_PATH_HITS=0
PROJECTION_SOURCE_LINES=11641
PROJECTION_SUMMARY_LINES_KEPT=4
PROJECTION_TOTAL_LINES=20
```

Acceptance Condition: MET. Exactly six `PROJECTION_WRITTEN=` lines are recorded. Every
`PROJECTION_ABSOLUTE_PATH_HITS=` value is `0`, which is the no-absolute-host-path invariant
measured on the written projection rather than assumed from the substitution. Every
`PROJECTION_TOTAL_LINES=` value is 20, which is at most 500. Every `PROJECTION_SOURCE_LINES=`
value is greater than 0, so no projection read an empty or absent file. Every
`PROJECTION_SUMMARY_LINES_KEPT=` value is 4, greater than 0: this is the positive control on the
line-matching mechanism, and it proves the four retained-line patterns matched real content, so
a zero `SKIPPING_CORECOMPILE_COUNT` in the same projection is an observation rather than an
artefact of a projection that matched nothing.

The two `qa-gates` projections each record `SKIPPING_CORECOMPILE_COUNT=0`, which is the figure
AC18 requires and the figure `[P5-T5]` and `[P5-T6]` already gated on the raw logs. Neither the
analyzer gate nor the nullable gate was vacuous.

The two `qa-gates` projections were regenerated from the Revision R7 re-execution of `[P5-T5]`
and `[P5-T6]`, so they project the logs of the final clean loop rather than of the superseded
one. Both record `DIAGNOSTIC_LINE_COUNT=0` and the retained summary lines `Build succeeded.`,
`0 Warning(s)` and `0 Error(s)`.

## Raw Console Log Removal:

Recorded by `[P5-T12]`. The six raw build console logs are removed from the feature folder's
`evidence/` tree now that `[P5-T11]` has projected each of them and every gate that reads one
has run. The reason is the standing directive that evidence artifacts carry projections rather
than raw dumps and carry no absolute host path: the four largest of the six were between 11,641
and 12,397 lines each and each retained thousands of lines carrying the absolute worktree path
including the host user name, as the `HOST_PATH_LINE_COUNT=` figure in each projection records.

Timestamp: 2026-09-14T12-57

Command: the single `[P5-T12]` span, run once.

EXIT_CODE: 0

```
RESIDUAL_TXT_COUNT=0
PROJECTION_COUNT=6
```

Acceptance Condition: MET. `RESIDUAL_TXT_COUNT=0` establishes that no `.txt` file remains
anywhere under the feature folder's `evidence/` tree, and `PROJECTION_COUNT=6` establishes that
all six projections survive the removal.

The deletions are staged by `[P6-T1]`, whose `git add` pathspec already covers the whole feature
folder, so no `git rm` was needed and none was run.

No task in this plan adds a raw `.trx` or a raw `.cobertura.xml` to git, and this task adds
none: `TestResults/` is git-ignored by the `[Tt]est[Rr]esult*/` pattern at `.gitignore` line 39
and `coverage/` by `coverage/*` at line 144, and no task copies a file out of either directory
into the feature folder.
