Timestamp: 2026-08-11T13-15
Command: `pwsh -NoProfile -Command 'Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = "tests"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/powershell-baseline.jacoco.xml"; $r = Invoke-Pester -Configuration $c; "PASSED=$($r.PassedCount) FAILED=$($r.FailedCount) SKIPPED=$($r.SkippedCount) TOTAL=$($r.TotalCount) LINEPCT=$($r.CodeCoverage.CoveragePercent)"; exit [int]($r.FailedCount -ne 0)'`
EXIT_CODE: 0

Passed: 64
Failed: 0
Skipped: 0
Total: 64
LineCommandCoveragePercent: 69.4047619047619
CoverageScope: 840 analyzed commands in 11 files
BranchCoverage: NOT MEASURABLE — Pester 5.x emits no branch counter

Output Summary: Pester completed with 64 passed, 0 failed, and 0 skipped. Direct numeric PowerShell line/command coverage was 69.4047619047619%. Branch coverage is an observation unavailable from Pester 5.x and is not fabricated.
