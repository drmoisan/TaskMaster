# P6-T7 — Repository-wide coverage delta

Timestamp: 2026-09-04T01-59

Command: arithmetic over the two recorded Cobertura root elements. The baseline figures are read from
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t6-coverage.md`
and the post-change figures from
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t6-coverage.md`.

EXIT_CODE: 0

**This artifact records the second execution of P6-T7**, run after the toolchain-loop restart that
P6-T13 caused. The post-change figures are those of the refreshed Cobertura document, not the
pre-P6-T13 one.

## The six figures

| Figure | Value |
|---|---|
| P0-T6 baseline `line-rate` | **85.43%** (raw 0.854332) |
| P0-T6 baseline `branch-rate` | **79.53%** (raw 0.795348) |
| P6-T6 post-change `line-rate` | **85.46%** (raw 0.85459) |
| P6-T6 post-change `branch-rate` | **79.52%** (raw 0.795242) |
| Signed line-rate difference | **+0.03** percentage points |
| Signed branch-rate difference | **-0.01** percentage points |

Supporting root attributes: baseline `lines-covered` 55265 against post-change 55321, on an unchanged
`lines-valid` of 64734. The denominator is unchanged because the eleven Write Set paths add
production lines in files already measured and add test files, which the coverage configuration
excludes from the denominator.

## Does this change lower the repository-wide figure?

**No — this change does not lower the repository-wide line coverage figure.** Line coverage rose by
0.03 percentage points, from 85.43% to 85.46%, on an unchanged denominator; 56 additional lines are
covered. Branch coverage moved down by 0.01 percentage points, from 79.53% to 79.52%, which is a
rounding-scale movement arising from the new `try`/`catch` arms and the classification branch this
item adds: those arms create new branch points, and the ones an unreachable COM-touching path guards
cannot be taken under test. Both figures remain above the `>= 80%` line-coverage floor.

## Governing floor

The governing floor is **CLAUDE.md's General Unit Test Policy UT2**, which is rank 1 in
`policy-compliance-order` and therefore supersedes the 85%/75% pair in
`.claude/rules/general-unit-test.md`, per D2. UT2 sets repository-wide line coverage at `>= 80%`
against its testable denominator and `>= 90%` for new and changed code.

Per D2, **the repository-wide figure is a record-and-report obligation for this item**, because no
merge-base coverage baseline exists in this feature folder against which a blocking regression could
be adjudicated. The blocking floors this item is gated on are the **`>= 90%` new-and-changed-code
floor**, evaluated by P6-T8 for the new file and P6-T9 for the changed lines, and the
**no-regression-on-changed-lines** rule.

Output Summary: repository-wide line coverage moved from 85.43% to 85.46%, a signed difference of
+0.03 percentage points, and branch coverage from 79.53% to 79.52%, a signed difference of -0.01
percentage points. This change does not lower the repository-wide line figure. CLAUDE.md's General
Unit Test Policy UT2 governs; per D2 the repository-wide figure is record-and-report for this item
while the 90% new-and-changed-code floor is blocking.
