# PowerShell Coverage Comparison (R1, issue #985)

Timestamp: 2026-10-09T15-29
Command: derived from `remediation-baseline/ps-coverage.2026-10-09T15-19.md` (P0-T8) and `qa-gates/ps-coverage-r1.2026-10-09T15-29.md` (P4-T4, ITERATION 1, the passing iteration)
EXIT_CODE: 0
Output Summary:
- Aggregate line coverage (scripts/dependencies, scripts/hygiene, scripts/vscode): baseline 94.98 (1891/1991), final 94.99 (1898/1998), delta +0.01. Gate 80.00: PASS. POLICY-85-OBSERVATION: MET.
- `BindingRedirectSync.psm1` (new code): baseline 100.00 (114/114), final 100.00 (120/120). Gate 90.00: PASS.
- `Repair-PackageManifestConsistency.ps1` (changed lines, no regression): baseline 94.14 (225/239), final 94.17 (226/240). Gate not below 94.14: PASS.
- PowerShell branch coverage: not measured by Pester; no branch gate applies.
