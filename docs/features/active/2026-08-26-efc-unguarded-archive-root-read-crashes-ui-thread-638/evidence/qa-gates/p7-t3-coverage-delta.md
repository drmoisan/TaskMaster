# [P7-T3] Coverage delta report (Issue 638)

Timestamp: 2026-08-29T12-44

Command: derivation only — the four numeric fields below are copied from the [P0-T12],
[P7-T1] and [P7-T2] artifacts and differenced; no new command was run.

EXIT_CODE: 0

Output Summary:

## Numeric fields

```
BASELINE_REPO_LINE_COVERAGE_PERCENT:    70.70   (from [P0-T12])
POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:  85.33   (from [P7-T1])
CHANGED_LINE_COVERAGE_PERCENT:          93.10   (from [P7-T2])
DELTA_REPO_LINE_COVERAGE_POINTS:       +14.63   (85.33 - 70.70)
```

All four are present and none reads `UNVERIFIED`.

## Modes

```
BASELINE_COVERAGE_XML_MODE:    raw                  (from [P0-T12])
POSTCHANGE_COVERAGE_XML_MODE:  koverage-processed   (from [P7-T1])
```

The two recorded modes are **not equal**.

## Outcome — REMEDIATION-REQUIRED

`DELTA_REPO_LINE_COVERAGE_POINTS: +14.63` is at or above the `-0.50` tolerance, so the
delta clause alone would pass. The mode clause does not:

REMEDIATION-REQUIRED: [P7-T3] the baseline and post-change coverage figures were computed in
different modes (`raw` versus `koverage-processed`), so their difference measures the
denominator rather than this change. The task's acceptance requires the two modes to be
equal and records a mismatch as REMEDIATION-REQUIRED rather than as a pass.

Cause and scope. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:341` asserts the coverage
threshold on the post-processed content and `:343` writes that content back only afterwards,
so a run that terminates earlier leaves the raw `dotnet-coverage` root attributes on disk.
The [P0-T12] baseline run terminated at `:236` on a single pre-existing failing test,
`QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`,
a five-second wall-clock timing assertion in a file this change does not touch and one that
passed in the same task's direct-harness run. It therefore never reached post-processing.
The [P7-T1] post-change run passed all 6870 tests and did reach post-processing. The two
denominators differ accordingly: the raw file carried 14 `<package>` elements including
every `.Test` assembly and `lines-valid=82363`, while the post-processed file carries 9 and
`lines-valid=64221`, because
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:417-421` removes every
non-allowlisted package and `:442-445` recomputes the root attributes over what remains.

Consequence. Per the task text this is recorded as REMEDIATION-REQUIRED rather than as a
pass, and [P8-T19] therefore leaves AC17 unchecked.

What is not in doubt. The repo-wide figure is recorded and reported per AC17 but is not
itself a blocking threshold. The blocking clause is the change-scoped one in [P7-T2], and
it passes: `CHANGED_LINE_COVERAGE_PERCENT: 93.10` is at or above the required 90.0, measured
entirely within the single `koverage-processed` artifact, so the mode mismatch does not
affect it. The post-change repo-wide figure of 85.33 also clears both the 80 percent floor
in `CLAUDE.md` § UT2 and the 85 percent floor in `.claude/rules/general-unit-test.md`, and
the post-change branch figure of 79.31 clears the 75 percent branch floor.

Remediation route. Re-running
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` on the merge base
`ecdb1c84ba8541ab67042985919cfed4df768c01` until the flaky liveness test passes would yield
a `koverage-processed` baseline commensurable with [P7-T1], after which the delta could be
recomputed and AC17 evaluated. That is outside this plan's task set.
