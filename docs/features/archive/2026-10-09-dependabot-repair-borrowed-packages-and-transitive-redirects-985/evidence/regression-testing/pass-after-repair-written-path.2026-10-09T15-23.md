# Pass-after: Repair entry-point tests (R1, CR-1 and CR-2, issue #985)

Timestamp: 2026-10-09T15-23
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1,tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
EXIT_CODE: 0
Output Summary:
- TEST-PATH-COUNT: 2; Pester 5.6.1
- PESTER Passed=40 Failed=0 Skipped=0 NotRun=0 Total=40
- FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 Passed=9 Failed=0 Total=9
- FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 Passed=31 Failed=0 Total=31 (equals R1-BASELINE-REPAIR-TESTS-TOTAL 31)
- N7 and N9, which failed in `fail-before-repair-written-path.2026-10-09T15-22.md`, now pass; N8 still passes.
