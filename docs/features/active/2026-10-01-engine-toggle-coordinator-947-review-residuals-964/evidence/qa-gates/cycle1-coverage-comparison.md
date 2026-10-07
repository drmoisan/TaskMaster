# Coverage comparison (P2-T6)

Timestamp: 2026-10-03T09-30
Command: Read of evidence/remediation-baseline/coverage-baseline.md and evidence/qa-gates/cycle1-coverage.md
EXIT_CODE: 0
Output Summary: coordinator lines unchanged at 203/203; coordinator branches 43/44 to 44/44; Messages class-node branch-rate 0.5 to 1; both floors met; every clause MET.

BASELINE-FIRST-PARTY: First-party coverage: lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)
FINAL-FIRST-PARTY: First-party coverage: lines 56639/65881 (85.97%), branches 13687/17082 (80.13%)
(Observations only; the gates are the floors recorded in cycle1-coverage.md.)
BASELINE-COORD-LINES: 203/203
FINAL-COORD-LINES: 203/203
BASELINE-COORD-BRANCHES: 43/44 (97.73)
FINAL-COORD-BRANCHES: 44/44 (100)
BASELINE-MESSAGES-BRANCH-RATE: 0.5
FINAL-MESSAGES-BRANCH-RATE: 1
NEW-CODE-COVERAGE: not applicable, no production line or branch was added

Clauses:
- Final coordinator line count equals the baseline (no line removed from coverage): MET (203 and 203)
- Final covered branches equal final valid branches and exceed the baseline covered branches by exactly 1: MET (44 equals 44; 44 minus 43 is 1)
- Final Messages class-node branch-rate is 1: MET (1)
- Both final floors met: MET (LINE-FLOOR: MET, BRANCH-FLOOR: MET)
