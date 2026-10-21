# PowerShell Direct Coverage Gate (P4-T4)

Timestamp: 2026-10-09T14-26
Command: pwsh -NoProfile -File CMDDIR\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985-pester-final.xml
EXIT_CODE: 0
ITERATION: 2
Output Summary:
- TEST-PATH-COUNT: 3; COVERAGE-PATH-COUNT: 3
- PESTER Passed=438 Failed=0 Skipped=0 NotRun=0 Total=438 (baseline 413; +25 new tests)
- COVERAGE LinePercent=94.98 Covered=1891 Total=1991 (gate: at or above 80.00; PASS)
- FILE-COVERAGE scripts/dependencies/dependencies/BindingRedirectSync.psm1 LinePercent=100.00 Covered=114 Total=114 (gate: at or above 90.00; PASS)
- FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=94.14 Covered=225 Total=239 (gate: at or above BASELINE-REPAIR-SCRIPT-LINE 93.83; PASS)
- POLICY-85-OBSERVATION: MET (aggregate 94.98 is at or above the 85 figure in .claude/rules/quality-tiers.md; recorded as an observation per plan D6)
- Raw JaCoCo document kept at coverage/985-pester-final.xml (ignored directory); not copied into the feature folder.
- Result: PASS.

## FILE-TESTS (new and changed files)

```
FILE-TESTS BindingRedirectSync.Tests.ps1 Passed=18 Failed=0 Total=18
FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 Passed=6 Failed=0 Total=6
FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 Passed=31 Failed=0 Total=31
FILE-TESTS RepositoryTreeConsistency.Tests.ps1 Passed=5 Failed=0 Total=5
```

All other 28 FILE-TESTS lines report Failed=0 with totals equal to the P0-T8 baseline.

## FILE-COVERAGE scripts/dependencies

```
FILE-COVERAGE scripts/dependencies/dependencies/AnalyzerItemRepair.psm1 LinePercent=100.00 Covered=106 Total=106
FILE-COVERAGE scripts/dependencies/dependencies/BindingRedirectSync.psm1 LinePercent=100.00 Covered=114 Total=114
FILE-COVERAGE scripts/dependencies/dependencies/BindingRedirectVerification.psm1 LinePercent=100.00 Covered=41 Total=41
FILE-COVERAGE scripts/dependencies/dependencies/ConsistencyVerifier.psm1 LinePercent=98.75 Covered=158 Total=160
FILE-COVERAGE scripts/dependencies/dependencies/PackageCompatibility.psm1 LinePercent=100.00 Covered=33 Total=33
FILE-COVERAGE scripts/dependencies/dependencies/PackageGraph.psm1 LinePercent=100.00 Covered=164 Total=164
FILE-COVERAGE scripts/dependencies/dependencies/ProjectConsistency.psm1 LinePercent=100.00 Covered=103 Total=103
FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=94.14 Covered=225 Total=239
```
