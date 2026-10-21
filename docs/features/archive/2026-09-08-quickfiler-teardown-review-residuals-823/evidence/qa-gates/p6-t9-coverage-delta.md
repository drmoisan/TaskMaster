# Phase 6 — Repository coverage delta

Timestamp: 2026-09-09T14-49

Task: [P6-T9]

Comparing the [P0-T12] baseline counters against the [P6-T6] post-change counters. Both sides used
the same instrument, the same settings file, the same nine explicitly named assemblies, the same
four-clause filter and the same all-descendant `line` aggregation over the same nine first-party
packages, so the comparison is like for like.

## Line coverage

BASELINE-LINE-PERCENT: 84.67
POST-LINE-PERCENT: 84.68
LINE-PERCENT-DELTA: +0.01

Underlying counters: baseline 113771 of 134367; post-change 113787 of 134377. The denominator rose
by 10 and the numerator by 16.

## Branch coverage

BASELINE-BRANCH-PERCENT: 79.45
POST-BRANCH-PERCENT: 79.46
BRANCH-PERCENT-DELTA: +0.01

Underlying counters: baseline 26992 of 33972; post-change 26994 of 33972. The denominator is
unchanged and the numerator rose by 2.

## No-regression disposition

NO-REGRESSION: PASS

`LINE-PERCENT-DELTA` is +0.01, which is at least -0.10, and `BRANCH-PERCENT-DELTA` is +0.01, which
is at least -0.10. Both moved upward. The R3 change converts one covered silent-return branch into
two covered throwing branches, and the R1 change adds one collection-membership branch covered by
both new tests, which is consistent with the small upward movement.

## Floor dispositions

The two policy sources state different floors. Both are reported with the measured value beside
each, and neither is silently preferred. No threshold was lowered, weakened or deleted to make
anything pass, and no production file was added to any coverage exclusion list.

FLOOR-85-LINE: NOT MET — `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`
state a uniform line floor of 85 percent across T1 through T4. The measured post-change figure is
84.68 percent, which is 0.32 points below it. The baseline figure of 84.67 percent was also below
it, so this is a pre-existing repository-wide condition rather than a regression introduced by this
change, and the delta moved toward the floor rather than away from it.

FLOOR-75-BRANCH: MET — the same two rule files state a uniform branch floor of 75 percent for
languages whose coverage tooling measures branch coverage. The measured post-change figure is
79.46 percent, which is 4.46 points above it.

FLOOR-80-LINE: MET — `CLAUDE.md` section UT2 states a repository-wide line floor of 80 percent
against a testable denominator, after the ratified COM/VSTO/WinForms exemption. The measured
post-change figure is 84.68 percent against a denominator that has had no such exemption subtracted
from it, so the figure measured against the narrower testable denominator can only be higher. The
floor is met with 4.68 points of margin on the unexempted denominator.

FLOOR-90-NEW-CODE: see [P6-T10]. `CLAUDE.md` section UT2 requires any new module, class or method to
reach 90 percent coverage. This plan adds no new module, no new class and no new production method:
its production edits are one field replacement, one conjunct replacement, one statement replacement,
two added guard clauses and five prose rewrites. The nearest applicable measurement is the
changed-line figure, which [P6-T10] computes and dispositions.

## Recorded conflict

The 85 percent line floor stated by the two rule files and the 80 percent line floor stated by
`CLAUDE.md` section UT2 disagree. Both are recorded above as separate dispositions with the same
measured value beside each. Neither is edited, and no attempt is made here to resolve which
governs; that is a governance question outside this feature's scope, and `.claude/` is off limits
under D21 in any case. The [P0-T1] policy-read artifact records the same conflict.

Output Summary: Line coverage moved from 84.67 to 84.68 percent and branch coverage from 79.45 to
79.46 percent, so `NO-REGRESSION: PASS` on both. The 75 percent branch floor and the 80 percent
`CLAUDE.md` line floor are met; the 85 percent rule-file line floor is not met, at 84.68 percent,
a pre-existing repository-wide condition that this change moved toward rather than away from.
