# [P7-T8] Repository Coverage Delta

Timestamp: 2026-09-08T10-25

Comparing the [P0-T12] baseline counters against the [P7-T5] post-change counters. Both sides come from one collector, one configuration, one assembly selection and one filter, per D11.

BASELINE-LINE-PERCENT: 84.63
POST-LINE-PERCENT: 84.63
LINE-PERCENT-DELTA: 0.00

BASELINE-BRANCH-PERCENT: 79.39
POST-BRANCH-PERCENT: 79.39
BRANCH-PERCENT-DELTA: 0.00

NO-REGRESSION: PASS

## Underlying counters

| Counter | Baseline ([P0-T12]) | Post ([P7-T5]) | Movement |
| --- | --- | --- | --- |
| Lines covered | 113543 | 113595 | +52 |
| Lines valid | 134159 | 134219 | +60 |
| Branches covered | 26926 | 26940 | +14 |
| Branches valid | 33916 | 33932 | +16 |

At four decimal places the line rate moved from 84.6332 to 84.6341, a delta of +0.0009, and the branch rate from 79.3903 to 79.3941, a delta of +0.0038. Both round to 0.00 at the two decimal places reported above, and both are positive rather than negative.

`NO-REGRESSION: PASS` because `LINE-PERCENT-DELTA` of 0.00 is at least -0.10 and `BRANCH-PERCENT-DELTA` of 0.00 is at least -0.10. Neither rate fell.

## Floor dispositions

The two rule sources disagree on the line floor. Both are recorded and neither is silently preferred.

FLOOR-85: NOT MET — measured line coverage 84.63 percent against the 85 percent floor in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`. The shortfall is 0.37 percentage points.

FLOOR-80: MET — measured line coverage 84.63 percent against the 80 percent repository-wide floor in CLAUDE.md UT2, exceeded by 4.63 percentage points.

FLOOR-75-BRANCH: MET — measured branch coverage 79.39 percent against the 75 percent branch floor in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`, exceeded by 4.39 percentage points.

## The 85 percent shortfall is a pre-existing repository condition

The [P0-T12] baseline, captured before any task of this plan edited a source file, measured 84.63 percent line coverage. The 85 percent floor was therefore already unmet on this tree at the point this work began. It is not a condition this work caused and not one this work is scoped to fix.

What this plan is gated on is `NO-REGRESSION`, which passes: the line rate did not fall and in fact rose very slightly, as did the branch rate. Raising repository coverage from 84.63 to 85 percent would require roughly 500 additional covered lines across assemblies this issue does not touch, which is a separate piece of work with its own scope.

The disposition is recorded as measured rather than softened. A reader comparing the two rule sources should note that CLAUDE.md UT2 states 80 percent against a testable denominator with a ratified COM/VSTO/WinForms exemption, while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state 85 percent uniformly across tiers; the measurement here uses the nine first-party packages as its denominator with test assemblies excluded, and is reported against both figures without adjudicating which governs.
