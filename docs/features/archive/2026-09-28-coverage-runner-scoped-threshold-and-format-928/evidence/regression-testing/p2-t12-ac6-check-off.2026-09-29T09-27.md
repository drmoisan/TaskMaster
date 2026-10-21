# P2-T12 AC6 Decision

Timestamp: 2026-09-29T09-27
Task: P2-T12
Command: none (decision over the final-iteration P2-T2 and P2-T3 artifacts); Grep count of `^- \[x\] AC` over issue.md
EXIT_CODE: 0

Evidence read:

- evidence/qa-gates/p2-t2-analyze.iter1.2026-09-29T09-18.md: every analyzer condition met. N_SCRIPTS_FINAL 13 is not above N_SCRIPTS_BASELINE 13. The entry point, the new part file, the new test file and the test folder are each ok true. Invoke-MSTest.ps1 reports 2, equal to its baseline of 2, and it is absent from `git diff --numstat 177b6d78e -- scripts/vscode` (D10 check-off clause met).
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md: the JUnit conditions were all met (334 tests, 0 failures, 0 errors, 0 skipped). COVERAGE-ROUTE: C.
- evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md: COVERAGE-ROUTE: C, BASELINE_POPULATION_LINE_PERCENT 94.49. Both figures come from the same route and the same command.

Condition-by-condition (outcome (i) requires all):

| Condition | Observed | Met |
|---|---|---|
| FINAL at or above 80.00 | 94.46 | yes |
| FINAL at or above BASELINE (same route) | 94.46 against 94.49 | no |
| New part file at or above 90 | 100.00 (5 of 5) | yes |
| CHANGED-LINES-UNCOVERED: none, entry point | 408 | no |
| CHANGED-LINES-UNCOVERED: none, new part file | none | yes |
| CHANGED-LINES-UNCOVERED, Invoke-MSTest.ps1 | not listed by the numstat; not required | n/a |
| P2-T2 met every condition | yes | yes |
| D10: Invoke-MSTest.ps1 absent from the numstat, or run (F) ok true | absent | yes |

Outcome: (ii). The AC6 line is left unchanged and the Grep count of `^- \[x\] AC` stays at 5.

AC6-STATUS: PENDING
Reason: below-baseline. FINAL_POPULATION_LINE_PERCENT is 94.46 and BASELINE_POPULATION_LINE_PERCENT is 94.49, both by Route C. The entry point also has one uncovered changed line (CHANGED-LINES-UNCOVERED: 408), which AC6's "every changed production line covered" clause does not allow.

Both failures have one measured cause, recorded in the P2-T3 iteration 1 artifact. Line 408, the scoped-arm `Write-Warning`, is executed by It 10 of the new test file, but the run does not credit it. Pester 5.6.1 breakpoint coverage binds each entry-point line to the parsed copy of the function in the first test file that executes it. Invoke-MSTest.RunSettings.Tests.ps1 sorts before the new file and never takes the scoped arm. That one missed line accounts for the population movement: 1620 covered and 95 missed against 1613 and 94 at baseline.

Plan outcome for AC6: remediation-required, not PASS. The executor did not edit the AC6 text.

Output Summary: AC6 left unchecked (AC6-STATUS: PENDING, below-baseline). Analyzer and Pester-suite conditions are met. The population is 94.46 against a 94.49 baseline, and entry-point line 408 is uncovered in the combined run because of the breakpoint-binding effect documented in the P2-T3 artifact. Remediation-required.
