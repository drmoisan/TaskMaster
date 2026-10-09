# PowerShell QA Direct Coverage (R1, issue #985)

ITERATION: 1
Timestamp: 2026-10-09T15-29
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985r1-pester-final.xml
EXIT_CODE: 0
Output Summary:
- TEST-PATH-COUNT: 3; COVERAGE-PATH-COUNT: 3; Pester 5.6.1
- PESTER Passed=447 Failed=0 Skipped=0 NotRun=0 Total=447 (baseline 438; +9 new tests)
- COVERAGE LinePercent=94.99 Covered=1898 Total=1998 (gate 80.00: PASS)
- FILE-COVERAGE scripts/dependencies/dependencies/BindingRedirectSync.psm1 LinePercent=100.00 Covered=120 Total=120 (gate 90.00: PASS)
- FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=94.17 Covered=226 Total=240 (gate: not below R1-BASELINE-REPAIR-SCRIPT-LINE 94.14: PASS)
- POLICY-85-OBSERVATION: MET (aggregate 94.99 is at or above 85)
- FILE-TESTS BindingRedirectSync.Tests.ps1 Passed=24 Failed=0 Total=24
- FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 Passed=9 Failed=0 Total=9
- FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 Passed=31 Failed=0 Total=31
- All other FILE-TESTS lines identical to the baseline (`remediation-baseline/ps-coverage.2026-10-09T15-19.md`), each Failed=0.
- Raw JaCoCo document kept under the ignored `coverage/` directory (not copied into FEATURE).
