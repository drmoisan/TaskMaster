# PowerShell Coverage Comparison (P4-T7)

Timestamp: 2026-10-09T14-26
Command: comparison of FEATURE/evidence/baseline/ps-coverage.2026-10-09T14-04.md (P0-T8) and FEATURE/evidence/qa-gates/ps-coverage.2026-10-09T14-26.md (P4-T4, iteration 2)
EXIT_CODE: 0
Output Summary:
- Baseline aggregate line coverage (scripts/dependencies, scripts/hygiene, scripts/vscode): 94.64 (1765/1865)
- Final aggregate line coverage: 94.98 (1891/1991)
- Delta: +0.34 percentage points (gate: final at or above 80.00; PASS)
- BindingRedirectSync.psm1 (new code) final line coverage: 100.00 (114/114) (gate: at or above 90.00; PASS)
- Repair-PackageManifestConsistency.ps1 (changed file) baseline 93.83 (213/227), final 94.14 (225/239) (gate: final not lower than baseline; PASS)
- POLICY-85-OBSERVATION: MET (94.98)
- PowerShell branch coverage: not measured by Pester; no branch gate applies (.claude/rules/powershell.md).
- Result: PASS.
