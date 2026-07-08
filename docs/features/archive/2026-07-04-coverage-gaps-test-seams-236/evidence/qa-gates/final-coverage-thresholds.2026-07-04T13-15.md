Timestamp: 2026-07-04T13-15
Task: P6-T7
Command: Threshold evaluation using docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\final-coverage-targets.2026-07-04T13-15.md
EXIT_CODE: 0

Output Summary:
- Evaluated final coverage thresholds required by issue #236.
- Repository-wide line coverage threshold: `>= 80%`.
- Issue #236 changed/new non-exempt code coverage threshold: `>= 90%`.
- Changed-line regression threshold: final changed/new-code coverage must not regress against baseline comparable changed-line coverage.
- Target verdicts use machine-checkable class coverage and compare final target coverage against baseline target coverage.

Threshold Results:
| Gate | Required | Actual | Verdict |
| --- | ---: | ---: | --- |
| Repository-wide line coverage | >= 80.00% | 45.12% | FAIL |
| Issue #236 changed/new non-exempt code coverage | >= 90.00% | 71.19% | FAIL |
| Changed-line coverage regression | Final >= baseline comparable changed-line coverage | Final 71.19%; baseline comparable 4.03% | PASS |

Target Verdicts:
| Target | Baseline | Final | Verdict |
| --- | ---: | ---: | --- |
| EfcViewerQueue | 0.00% | 92.31% | PASS |
| ItemViewerQueue | 0.00% | 94.87% | PASS |
| QfcThemeHelper | 0.00% | 88.48% | PASS |
| EfcHomeController | 15.87% | 49.81% | PASS |
| TlpCellStates | 62.20% | 92.09% | PASS |

Overall Coverage Verdict:
- FAIL.
- Blocking reason: repository-wide line coverage is 45.12%, below the required 80.00%; issue #236 changed/new non-exempt code coverage is 71.19%, below the required 90.00%.
- This artifact does not authorize reporting issue #236 execution as COMPLETE.
