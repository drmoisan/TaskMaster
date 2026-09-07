# [P3-T8] Coverage delta, baseline against post-change

Timestamp: 2026-09-07T08-08

Command: no command of its own; this task compares the five counters [P0-T12] recorded against the five counters
[P3-T5] recorded, both produced by the one pinned D13 aggregation block applied identically to both documents.

EXIT_CODE: 0

ExpectedExitCode: 0

## Comparability precondition (recorded first, as D13 requires)

BASELINE-LINES-VALID: 133485
FINAL-LINES-VALID: 133765
LINES-VALID-DELTA: +280
DENOMINATORS-EQUAL: false

The denominators are NOT equal, so the two covered-line counts are not directly comparable and the raw
`LINES_COVERED` delta is not the measure. The growth is expected and explained: this change adds production code —
two new helper types, the AC7 gate and absence report in the provider, the AC6 score projection and the AC7
suppression in the router, and a relocated partial — and every new executable production line enters the
denominator.

COMPARISON-USED: the two derived percentages.

Per this task's own instruction, when the denominators differ the derived percentages are compared instead and
that fact is stated. It is stated here.

The same precondition applies to the branch counters:

BASELINE-BRANCHES-VALID: 33624
FINAL-BRANCHES-VALID: 33736
BRANCHES-VALID-DELTA: +112

## The five counters, side by side

| Counter | [P0-T12] baseline | [P3-T5] final | Delta |
|---|---|---|---|
| LINES_COVERED | 112855 | 113143 | +288 |
| LINES_VALID | 133485 | 133765 | +280 |
| BRANCHES_COVERED | 26642 | 26746 | +104 |
| BRANCHES_VALID | 33624 | 33736 | +112 |
| PACKAGES_MATCHED | 9 | 9 | 0 |

`PACKAGES_MATCHED` is 9 on both sides and is greater than zero on both sides, so no package-name mismatch occurred
and neither side is a silent zero-counter run.

## The comparison that is used

| Percentage | Baseline | Post-change | Delta |
|---|---|---|---|
| First-party line percentage | 84.55 | 84.58 | +0.03 |
| First-party branch percentage | 79.24 | 79.28 | +0.04 |

DID-THE-REPOSITORY-WIDE-FIRST-PARTY-LINE-PERCENTAGE-DECREASE: no. It rose from 84.55 to 84.58.

DID-THE-REPOSITORY-WIDE-FIRST-PARTY-BRANCH-PERCENTAGE-DECREASE: no. It rose from 79.24 to 79.28.

The covered count grew by 288 against a denominator growth of 280, so the added production code is covered at a
higher rate than the pre-existing first-party average, which is why the percentage moved up rather than down.

## Changed-line determination carried forward from [P3-T7]

CHANGED-LINES-EXAMINED: 315
CHANGED-LINES-NON-EXECUTABLE: 180
CHANGED-LINES-EXECUTABLE: 135
CHANGED-LINES-EXECUTABLE-WITH-ZERO-HITS: 4
CHANGED-LINES-WITH-LOWER-POST-HITS-THAN-BASELINE: 0

The repository policy that code changes must not reduce coverage for the lines that were changed is satisfied: the
regression count is 0. The four executable changed lines carrying zero hits are the null-`FolderPath` guard arm in
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` at lines 202-203, which did not exist at the base commit, and
two arguments inside `ConfigureBreadcrumbControl` in `QuickFiler/Controllers/EfcFormController.cs` at lines
1054-1055, a WebView2-bound method whose whole body was already at zero hits in the baseline document. Neither
represents coverage lost by this change. [P3-T7] records the surrounding-line evidence for both.

## Interpretation limit (D13)

These percentages are a comparability index, not the repository line-coverage rate the policy floor is defined
over. The pinned aggregation counts every `line` element under a matched package, which selects the class-level
and method-level elements alike and therefore over-counts the denominator relative to a de-duplicated per-line
count. That is sound for the comparison this task makes, because the identical method is applied to both
documents, but it is not a policy measurement. [P0-T12] recorded `BASELINE_FLOOR: MET` with the same
qualification, and no task in this plan gates on it. This task gates on direction of movement and on the
changed-line regression count, both of which are measured on the identical index.

Output Summary: The `lines-valid` comparability precondition fails — the denominator grew by 280 lines because the
change adds production code — so the comparison is made on the derived percentages, as D13 directs. The
first-party line percentage rose from 84.55 to 84.58 and the branch percentage from 79.24 to 79.28; neither
decreased. `PACKAGES_MATCHED` is 9 on both sides. The changed-line determination from [P3-T7] records 0 lines
whose post-change hits are lower than their baseline hits.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
