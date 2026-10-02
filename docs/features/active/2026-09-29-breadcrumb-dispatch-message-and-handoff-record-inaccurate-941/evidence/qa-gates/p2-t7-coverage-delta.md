# P2-T7 coverage delta
Timestamp: 2026-10-01T07-27
Command: comparison of FEATURE/evidence/baseline/p0-t10-mstest-coverage.md and p0-t11-dispatcher-line-hits.md against FEATURE/evidence/qa-gates/p2-t5-mstest-coverage.md and p2-t6-dispatcher-line-hits.md
EXIT_CODE: 0
Output Summary:
BASELINE-FIRST-PARTY: First-party coverage: lines 15183/62181 (24.42%), branches 3764/16224 (23.20%)
FINAL-FIRST-PARTY: First-party coverage: lines 15183/62181 (24.42%), branches 3764/16224 (23.20%)
BASELINE-QF-LINE-RATE: 0.820135 (10460/12754)
FINAL-QF-LINE-RATE: 0.820135 (10460/12754)
BASELINE-QF-BRANCH-RATE: 0.782717 (2518/3217)
FINAL-QF-BRANCH-RATE: 0.782717 (2518/3217)
CHANGED-LINES: 180 baseline 1 final 1; 181 baseline 1 final 1; 182 baseline 1 final 1; 183 baseline 1 final 1; 184 baseline 1 final 1; 185 baseline 1 final 1; 186 baseline 1 final 1; 187 baseline 1 final 1
Clause 1 (every baseline line with HITS >= 1 has final HITS >= 1): held (8 of 8).
Clause 2 (final row set contains every baseline line number): held (180 to 187 present; no baseline row existed for 188).
Re-run clause: measurement 1 of P2-T5 read FINAL-QF-LINE-RATE 0.820056 (10459/12754) and FINAL-QF-BRANCH-RATE 0.782406 (2517/3217), each below baseline by one covered unit. Per this task, P2-T5 and P2-T6 were re-run once with identical arguments (measurement 2). Both projections are quoted in p2-t5-mstest-coverage.md: measurement 1 package QuickFiler LINE missed=2295 covered=10459, BRANCH missed=700 covered=2517; measurement 2 (reported above as FINAL) LINE missed=2294 covered=10460, BRANCH missed=699 covered=2518. Measurement 2 equals baseline, so no QF-RATE-DELTA record applies. The one-unit difference between measurements 1 and 2 occurred with no source change between them and with the changed dispatcher lines at HITS 1 in both; the cause is not established in this task.
Test total: BASELINE-TOTAL 1469, FINAL-TOTAL 1470 (one new test).
Reason no test-file rate exists: test-assembly lines are outside the coverage denominator (coverage settings exclude .*\.Test\.dll$)
AC9 status: changed-line clauses held.
