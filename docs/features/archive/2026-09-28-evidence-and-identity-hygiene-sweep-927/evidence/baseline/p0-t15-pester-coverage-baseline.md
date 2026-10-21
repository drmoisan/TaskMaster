# P0-T15 PowerShell Pester-with-coverage baseline

Timestamp: 2026-09-29T09-07
Command: git status --porcelain -- "*.csproj" (before); PESTER-CI-SHAPE with the two hygiene entries omitted: pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies", "tests/scripts/vscode"); $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies", "scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; ... (the counts, COVERAGE, PACKAGE| and FILE| lines of the Gate command reference) ...; if ($r.FailedCount -gt 0 -or $r.PassedCount -eq 0) { exit 1 } else { exit 0 }'; mcp__drm-copilot__run_poshqc_test with scan_folders ["scripts/dependencies", "scripts/vscode", "tests/scripts/dependencies", "tests/scripts/vscode"]; git status --porcelain -- "*.csproj" (after)
EXIT_CODE: 0
Output Summary:
- Porcelain over project files before: empty. After (taken after both the direct runs and the MCP run): empty. The two spans are identical; no project file was rewritten.
- PESTER Passed=320 Failed=0 Skipped=0 Total=320
- COVERAGE LinePercent=94.49 Covered=1613 Total=1707
- MCP channel: POSHQC MCP AVAILABLE ok=true (summary: "Ran bundled PoshQC test ... with 4 selected scan folder(s)."; the tool returns no counts).
- Run note: the direct payload was run twice. The first run's stdout was piped to a second pwsh process that filtered the result lines, which hid the payload's own exit code; it printed the same counts and figures as below. The payload was then re-run unpiped with its output redirected to the ignored log coverage/logs/927-pester-baseline.log, and that run's observed exit code (0) is the EXIT_CODE row above.

BASELINE-PESTER-PASSED: 320
BASELINE-PESTER-FAILED: 0
BASELINE-PS-LINE-PERCENT: 94.49

PACKAGE and FILE lines (verbatim):

```text
PACKAGE| scripts/dependencies | covered=777 missed=16
FILE| scripts/dependencies/dependencies/AnalyzerItemRepair.psm1 | covered=106 missed=0
FILE| scripts/dependencies/dependencies/ConsistencyVerifier.psm1 | covered=158 missed=2
FILE| scripts/dependencies/dependencies/PackageCompatibility.psm1 | covered=33 missed=0
FILE| scripts/dependencies/dependencies/PackageGraph.psm1 | covered=164 missed=0
FILE| scripts/dependencies/dependencies/ProjectConsistency.psm1 | covered=103 missed=0
FILE| scripts/dependencies/dependencies/Repair-PackageManifestConsistency.ps1 | covered=213 missed=14
PACKAGE| scripts/vscode | covered=836 missed=78
FILE| scripts/vscode/vscode/Install-RepoDotNetSdk.ps1 | covered=13 missed=20
FILE| scripts/vscode/vscode/Invoke-MSTest.ps1 | covered=49 missed=7
FILE| scripts/vscode/vscode/Invoke-MSTest.TrxSummary.ps1 | covered=40 missed=2
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1 | covered=93 missed=0
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | covered=32 missed=1
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | covered=204 missed=8
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.PackageRate.ps1 | covered=18 missed=0
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.Projection.ps1 | covered=39 missed=1
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.ps1 | covered=113 missed=13
FILE| scripts/vscode/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | covered=33 missed=0
FILE| scripts/vscode/vscode/Invoke-Restore.ps1 | covered=22 missed=1
FILE| scripts/vscode/vscode/Invoke-VSBuild.ps1 | covered=46 missed=3
FILE| scripts/vscode/vscode/Sync-PackageReferences.ps1 | covered=105 missed=22
FILE| scripts/vscode/vscode/TestProcessCleanup.ps1 | covered=29 missed=0
```
