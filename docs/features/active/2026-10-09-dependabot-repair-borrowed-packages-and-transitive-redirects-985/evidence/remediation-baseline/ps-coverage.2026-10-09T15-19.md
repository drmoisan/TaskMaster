# PowerShell Baseline Direct Coverage (R1, issue #985)

Timestamp: 2026-10-09T15-19
Command: pwsh -NoProfile -File <SCRATCH>\985r1-cmd\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985r1-pester-baseline.xml
EXIT_CODE: 0
Output Summary:
- TEST-PATH-COUNT: 3; COVERAGE-PATH-COUNT: 3; Pester 5.6.1
- PESTER Passed=438 Failed=0 Skipped=0 NotRun=0 Total=438
- COVERAGE LinePercent=94.98 Covered=1891 Total=1991
- FILE-COVERAGE scripts/dependencies/dependencies/BindingRedirectSync.psm1 LinePercent=100.00 Covered=114 Total=114
- FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=94.14 Covered=225 Total=239
- R1-BASELINE-PS-AGGREGATE: 94.98
- R1-BASELINE-MODULE-LINE: 100.00
- R1-BASELINE-REPAIR-SCRIPT-LINE: 94.14
- R1-BASELINE-REPAIR-TESTS-TOTAL: 31
- Raw JaCoCo document kept under the ignored `coverage/` directory (not copied into FEATURE).

FILE-TESTS lines:
```
FILE-TESTS AnalyzerItemRepair.Tests.ps1 Passed=13 Failed=0 Total=13
FILE-TESTS BindingRedirectSync.Tests.ps1 Passed=18 Failed=0 Total=18
FILE-TESTS BindingRedirectVerification.Tests.ps1 Passed=16 Failed=0 Total=16
FILE-TESTS ConsistencyVerifier.Tests.ps1 Passed=14 Failed=0 Total=14
FILE-TESTS DependabotConfig.Tests.ps1 Passed=17 Failed=0 Total=17
FILE-TESTS Install-RepoDotNetSdk.Tests.ps1 Passed=6 Failed=0 Total=6
FILE-TESTS Invoke-MSTest.AssemblyDiscovery.Tests.ps1 Passed=5 Failed=0 Total=5
FILE-TESTS Invoke-MSTest.Main.Tests.ps1 Passed=12 Failed=0 Total=12
FILE-TESTS Invoke-MSTest.ResultsDirectory.Tests.ps1 Passed=3 Failed=0 Total=3
FILE-TESTS Invoke-MSTest.RunSettings.Tests.ps1 Passed=28 Failed=0 Total=28
FILE-TESTS Invoke-MSTest.TrxSummary.Tests.ps1 Passed=7 Failed=0 Total=7
FILE-TESTS Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 Passed=5 Failed=0 Total=5
FILE-TESTS Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1 Passed=12 Failed=0 Total=12
FILE-TESTS Invoke-MSTestWithCoverage.FirstParty.Tests.ps1 Passed=7 Failed=0 Total=7
FILE-TESTS Invoke-MSTestWithCoverage.Helpers.Tests.ps1 Passed=20 Failed=0 Total=20
FILE-TESTS Invoke-MSTestWithCoverage.Merge.Tests.ps1 Passed=6 Failed=0 Total=6
FILE-TESTS Invoke-MSTestWithCoverage.PackageRate.Tests.ps1 Passed=2 Failed=0 Total=2
FILE-TESTS Invoke-MSTestWithCoverage.Projection.Tests.ps1 Passed=11 Failed=0 Total=11
FILE-TESTS Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1 Passed=9 Failed=0 Total=9
FILE-TESTS Invoke-MSTestWithCoverage.Scope.Tests.ps1 Passed=22 Failed=0 Total=22
FILE-TESTS Invoke-MSTestWithCoverage.Threshold.Tests.ps1 Passed=13 Failed=0 Total=13
FILE-TESTS Invoke-Restore.Tests.ps1 Passed=7 Failed=0 Total=7
FILE-TESTS Invoke-VSBuild.Tests.ps1 Passed=16 Failed=0 Total=16
FILE-TESTS PackageCompatibility.Tests.ps1 Passed=8 Failed=0 Total=8
FILE-TESTS PackageGraph.Tests.ps1 Passed=32 Failed=0 Total=32
FILE-TESTS ProjectConsistency.Tests.ps1 Passed=18 Failed=0 Total=18
FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 Passed=6 Failed=0 Total=6
FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 Passed=31 Failed=0 Total=31
FILE-TESTS RepositoryTreeConsistency.Tests.ps1 Passed=5 Failed=0 Total=5
FILE-TESTS Sync-PackageReferences.Tests.ps1 Passed=15 Failed=0 Total=15
FILE-TESTS Test-RepositoryHygiene.Git.Tests.ps1 Passed=5 Failed=0 Total=5
FILE-TESTS Test-RepositoryHygiene.Rules.Tests.ps1 Passed=29 Failed=0 Total=29
FILE-TESTS Test-RepositoryHygiene.Tests.ps1 Passed=15 Failed=0 Total=15
FILE-TESTS TestProcessCleanup.Tests.ps1 Passed=5 Failed=0 Total=5
```
