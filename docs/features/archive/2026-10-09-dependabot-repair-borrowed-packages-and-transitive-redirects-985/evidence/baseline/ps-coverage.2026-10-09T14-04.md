# PowerShell Baseline Direct Coverage (P0-T8)

Timestamp: 2026-10-09T14-04
Command: pwsh -NoProfile -File CMDDIR\985-pester.ps1 -WorkspaceRoot WORKSPACE-ROOT -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985-pester-baseline.xml
EXIT_CODE: 0
Output Summary:
- TEST-PATH-COUNT: 3; COVERAGE-PATH-COUNT: 3; PESTER-VERSION: 5.6.1
- PESTER Passed=413 Failed=0 Skipped=0 NotRun=0 Total=413
- COVERAGE LinePercent=94.64 Covered=1765 Total=1865
- FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=93.83 Covered=213 Total=227
- BASELINE-PS-AGGREGATE: 94.64
- BASELINE-REPAIR-SCRIPT-LINE: 93.83
- Raw JaCoCo document kept at coverage/985-pester-baseline.xml (ignored directory); not copied into the feature folder.
- Result: baseline green; no BASELINE-TEST-RED.

## FILE-TESTS lines

```
FILE-TESTS AnalyzerItemRepair.Tests.ps1 Passed=13 Failed=0 Total=13
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
FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 Passed=31 Failed=0 Total=31
FILE-TESTS RepositoryTreeConsistency.Tests.ps1 Passed=4 Failed=0 Total=4
FILE-TESTS Sync-PackageReferences.Tests.ps1 Passed=15 Failed=0 Total=15
FILE-TESTS Test-RepositoryHygiene.Git.Tests.ps1 Passed=5 Failed=0 Total=5
FILE-TESTS Test-RepositoryHygiene.Rules.Tests.ps1 Passed=29 Failed=0 Total=29
FILE-TESTS Test-RepositoryHygiene.Tests.ps1 Passed=15 Failed=0 Total=15
FILE-TESTS TestProcessCleanup.Tests.ps1 Passed=5 Failed=0 Total=5
```

## FILE-COVERAGE (Repair script)

```
FILE-COVERAGE scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 LinePercent=93.83 Covered=213 Total=227
```
