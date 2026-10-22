# AC17 — coverage delta recomputed over equal modes (Issue 638)

Timestamp: 2026-08-29T13-02

Command: derivation only — the fields below are copied from
`evidence/remediation-baseline/ac17-commensurable-baseline.md` and
`evidence/qa-gates/p7-t1-coverage-postchange.md` and differenced; no new command was run.

EXIT_CODE: 0

Output Summary: with both figures measured in `koverage-processed` mode, the repository-wide
line coverage moves from 85.26 percent at the merge base to 85.33 percent after the change, a
delta of +0.07 points. Branch coverage moves from 79.24 to 79.31, also +0.07 points. The
change does not lower either figure, which is what AC17 requires of the repository-wide
measurement. The blocking change-scoped clause was already satisfied at 93.10 percent.

## Numeric fields

```
BASELINE_REPO_LINE_COVERAGE_PERCENT:     85.26   (remediation baseline, merge base ecdb1c84)
POSTCHANGE_REPO_LINE_COVERAGE_PERCENT:   85.33   (from [P7-T1])
DELTA_REPO_LINE_COVERAGE_POINTS:         +0.07
CHANGED_LINE_COVERAGE_PERCENT:           93.10   (from [P7-T2])

BASELINE_REPO_BRANCH_COVERAGE_PERCENT:   79.24
POSTCHANGE_REPO_BRANCH_COVERAGE_PERCENT: 79.31
DELTA_REPO_BRANCH_COVERAGE_POINTS:       +0.07

BASELINE_COVERAGE_XML_MODE:              koverage-processed
POSTCHANGE_COVERAGE_XML_MODE:            koverage-processed
```

The two recorded modes are equal, which was the clause `[P7-T3]` could not satisfy.

## Underlying counts

```
                 lines-covered   lines-valid   packages
baseline                 54735         64195          9
post-change              54802         64221          9
difference                 +67           +26          0
```

The denominator grew by 26 executable lines, which are the executable lines the change adds to
`QuickFiler/Controllers/EfcDataModel.cs`. The numerator grew by 67, so 41 lines that were
executable and uncovered at the merge base are covered after the change: the eleven new tests
in `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` reach guard and early-return
paths in `EfcDataModel` that no prior test exercised. Package count is identical at 9, so the
allowlist filter selected the same assemblies in both runs and the denominators are
constructed the same way.

Line-covered counts drift slightly between runs of an unchanged tree on a suite this size, so
the +0.07 point movement is small relative to the tolerance the plan set. It is reported as
"not lowered" rather than as a measured improvement, which is the claim AC17 actually requires
and the claim the evidence supports.

## Threshold conformance

- Change-scoped line coverage 93.10 percent is at or above the 90.0 percent floor AC17 sets
  for changed lines, measured entirely inside the single `[P7-T1]` post-processed artifact, so
  the mode question never affected it.
- Repository-wide line coverage 85.33 percent clears the 80 percent floor in `CLAUDE.md` § UT2
  and the 85 percent floor in `.claude/rules/general-unit-test.md`.
- Repository-wide branch coverage 79.31 percent clears the 75 percent floor in
  `.claude/rules/quality-tiers.md`.

## Disposition

`[P7-T3]`'s `REMEDIATION-REQUIRED` finding is resolved. Its artifact is left unedited as the
record of the state at plan-execution time; this artifact and
`evidence/remediation-baseline/ac17-commensurable-baseline.md` carry the resolution. AC17 is
checked off in `spec.md` on this evidence.
