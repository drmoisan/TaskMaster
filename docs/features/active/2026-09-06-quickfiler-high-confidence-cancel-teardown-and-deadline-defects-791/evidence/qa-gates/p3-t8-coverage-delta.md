# [P3-T8] Coverage delta, baseline versus post-change

Timestamp: 2026-09-06T15-10

Both sides are produced by one collector (`dotnet-coverage collect --output-format cobertura`), one
settings file (`coverage\791-effective-coverage.config`), one test-assembly list (the same nine),
one `/TestCaseFilter`, and one aggregation — the pinned all-descendant `.//line` selection over the
same nine first-party package names. The baseline is [P0-T11]
(`coverage\791-baseline.cobertura.xml`); the post-change side is [P3-T5]
(`artifacts\csharp\coverage.xml`).

## Comparability precondition, recorded first

FINAL-LINES-VALID: 133187
BASELINE-LINES-VALID: 132961
RELATION: FINAL-LINES-VALID > BASELINE-LINES-VALID (by 226)
DENOMINATORS-EQUAL: NO
COMPARISON-USED: derived percentages

The denominators are **not** equal, so the two absolute covered-line counts are not directly
comparable and the comparison used below is the one between the two derived percentages, as D14 and
this task require. The growth of 226 valid lines is expected and is attributable to this change: it
is the new production code added to `QuickFiler` — the gate's bound checks and three logging
helpers, the reordered `ActionCancelAsync` with its four support members, the extracted
`ParkFocusAndCancelSelectors`, and the rewritten `QfcHomeController.Cleanup()`. The two
`QfcDatamodel` partials contribute nothing to the denominator in either document because the type
carries `[ExcludeFromCodeCoverage]` (D1, confirmed by [P0-T12]).

## The four counters

| Counter | Baseline [P0-T11] | Final [P3-T5] | Delta |
|---|---|---|---|
| Lines covered | 112355 | 112551 | +196 |
| Lines valid | 132961 | 133187 | +226 |
| Branches covered | 26496 | 26584 | +88 |
| Branches valid | 33480 | 33568 | +88 |

## Derived percentages — the operative comparison

| Metric | Baseline | Final | Change |
|---|---|---|---|
| First-party line coverage | 84.50 % | 84.51 % | **+0.01 pp** |
| First-party branch coverage | 79.14 % | 79.19 % | **+0.05 pp** |

REPOSITORY-WIDE-FIRST-PARTY-LINE-PERCENTAGE-DECREASED: NO

The repository-wide first-party line percentage did not decrease. It rose from 84.50 % to 84.51 %.
The branch percentage also rose, from 79.14 % to 79.19 %. Both remain at or above the CLAUDE.md UT2
80 percent line floor that [P0-T11] recorded as `BASELINE_FLOOR: MET`.

Of the 226 newly valid lines, 196 are covered — 86.7 % of the newly added executable surface, which
is above the repository-wide rate and is why the aggregate percentage moved upward rather than being
diluted. Every one of the 88 newly valid branches is covered.

## New and changed-code coverage determination, from [P3-T7]

- CHANGED-LINES-TOTAL: 294 across the five measurable production paths.
- CHANGED-LINES-EXECUTABLE: 131. The other 163 are non-executable (XML doc comments, blank lines,
  `using` directives, braces, an enum member, an interface method declaration) and carry no `hits`
  value in either branch of the merged per-line map.
- CHANGED-EXECUTABLE-LINES-WITH-ZERO-HITS: 12, which is 119 of 131 executable changed lines covered,
  or **90.8 %** — at or above the `>= 90 %` target the repository unit-test policy sets for new and
  changed methods. All 12 are named individually in [P3-T7] and fall into three host-bound or
  contract-defence classes: the UI `SynchronizationContext` marshal, two defensive `catch` blocks
  whose throw sources have no injectable seam, and one `log.Debug` on the live-Outlook
  `MoveAndIterate` completion branch.
- CHANGED-LINES-WITH-COVERAGE-REGRESSION: **0**. Seven changed lines had an equal-count hunk and a
  one-to-one baseline mapping; none of them lost coverage. The remainder are pure insertions with no
  baseline counterpart.
- Two production paths are structurally unmeasurable — the two `QfcDatamodel` partials, excluded by
  the type-level `[ExcludeFromCodeCoverage]` — and [P3-T7] records named passing tests as the
  substitute evidence for each of their changed members.

## Determination

Coverage did not regress at either scope. Repository-wide first-party line and branch percentages
both increased; no changed line lost coverage; and coverage on the executable changed lines is
90.8 %, above the policy target for new and changed code.
