# Remediation Final Coverage Thresholds

Timestamp: 2026-07-04T18:50:10.0000000-04:00
Task: P4-T6
Command: Parse `docs/features/active/2026-07-04-coverage-gaps-test-seams-236/evidence/qa-gates/remediation-final-coverage-targets.2026-07-04T17-29.md` and enforce AC8 numeric coverage thresholds.
EXIT_CODE: 1

Output Summary:
- Repository-wide line coverage is 46.15%, below the required 80.00%; AC8 remains unchecked.
- Issue #236 changed/new non-exempt production coverage is 95.74%, above the required 90.00%.
- Every issue #236 changed/new production file is at or above the required 90.00%.
- Original issue #236 target coverage passes the required target checks.
- No numeric coverage value required for this threshold check is missing.
- Threshold enforcement failed only because repository-wide line coverage remains below 80.00%.

Threshold Results:
| Check | Required | Actual | Status |
| --- | ---: | ---: | --- |
| Repository-wide line coverage | >= 80.00% | 46.15% | FAIL |
| Issue #236 changed/new non-exempt production coverage | >= 90.00% | 95.74% | PASS |
| Per-file changed/new production coverage minimum | >= 90.00% | 90.41% | PASS |
| Original issue #236 target coverage | PASS | PASS | PASS |
| Numeric coverage values present | PASS | PASS | PASS |

AC8 Status:
- AC8 is not satisfied.
- AC8 must remain unchecked.
- Remediation is blocked at `[P4-T6]` unless the repository-wide line coverage floor is changed by an approved plan revision or additional coverage work raises repository-wide line coverage to at least 80.00%.
