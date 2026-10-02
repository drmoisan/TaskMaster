# R1 P0-T3 Tree Facts

Timestamp: 2026-09-29T10-43
Task: P0-T3 (remediation-plan.2026-09-29T10-00.md)
Command: Read tool and Grep tool only, over the files named below (all under <repo-root>)
EXIT_CODE: 0

Every item is recorded as observed value; each equals the plan's expected value.

## (a) scripts/vscode/Invoke-MSTestWithCoverage.ps1

- Read renders 467 numbered lines; line 467 is empty: observed.
- Line 272 is `$ErrorActionPreference = 'Stop'`, no leading whitespace: observed. Grep `^\$ErrorActionPreference = 'Stop'` count 1; Grep `\$ErrorActionPreference = 'Stop'` (any indentation) count 1, so it is the only occurrence.
- Line 312 ends `beyond its three production files.`: observed.
- Line 313 is `    . (Join-Path $ScriptRoot 'Invoke-MSTestWithCoverage.Scope.ps1')`: observed.
- Line 403 empty: observed.
- Line 404 begins `    # Skipped on a scoped run`: observed.
- Line 407 is `    if (Test-CoverageRunIsScoped -RepoRoot $repoRoot -ResolvedSearchRoot $resolvedSearchRoot) {`: observed.
- Line 408 begins `        Write-Warning ("Coverage threshold assertions skipped`: observed.
- Line 411 `    else {`; line 412 `        Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent`; line 413 `        Assert-CoberturaBranchCoverageThreshold -CoberturaXml $processedXmlContent`; line 414 `    }`: observed.
- Line 415 begins `    Write-Output (Get-CoberturaFirstPartyCoverageReport`: observed.
- Grep counts: `Write-Warning` 2; `Assert-CoberturaCoverageThresholdForRun` 0; `\.PARAMETER SearchRoot` 1.

## (b) scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1

- Read renders 50 numbered lines; line 50 empty: observed.
- Line 1 `Set-StrictMode -Version Latest`; line 3 begins `# Added for issue #928.`; line 5 contains `three-file`; line 8 `function Test-CoverageRunIsScoped {`; lines 34 and 37 each `        [Parameter(Mandatory = $true)]`; line 41 begins `    $separators`; line 45 begins `    return -not [string]::Equals(`; line 49 `}`: observed.
- Grep counts: `ValidateNotNullOrEmpty` 0; `IsPathRooted` 0; `^function ` 1.

## (c) tests/scripts/vscode/Invoke-MSTestWithCoverage.Scope.Tests.ps1

- Read renders 233 numbered lines; line 233 empty: observed.
- Grep counts: `^\s*It '` 14; `^Describe '` 3; `^\s*Context '` 2; `Mock Write-Warning` 1; `Assert-CoberturaCoverageThresholdForRun` 0; `must be an absolute path` 0 (the combined alternation over tests/scripts/vscode returned 0, so each token is 0 in this file and in the folder).
- Line 130 `    }`; line 131 `}`; line 133 begins `Describe 'Invoke-MSTestWithCoverageMain threshold gating by search root'`: observed.

## (d) scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1

- Read renders 127 numbered lines; line 127 empty: observed.
- Line 52 `    if ($percentage -lt 80) {`; line 53 begins `        $formattedPercentage`; line 54 begins `        throw "Cobertura line coverage`; line 122 `    if ($percentage -lt 75) {`; line 123 begins `        $formattedPercentage`; line 124 begins `        throw "Cobertura branch coverage`: observed.
- Grep (content mode over the alternation) returns exactly lines 52, 54, 122 and 124, one each for `-lt 80`, `required 80% threshold`, `-lt 75`, `required 75% threshold`: count 1 each.

## (e) tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1

- Read renders 176 numbered lines; line 176 empty: observed.
- Grep counts: `^\s*It '` 5; `line-rate="0\.[0-7]` 0; `Mock Invoke-DotnetCoverageCollection \{\}` 1; `Mock Assert-Cobertura` 4.
- Line 18 is `    . $coverageAst.GetScriptBlock()`: observed.

## (f) Mock Resolve-Path over tests/scripts/vscode

- Grep content mode with -A 1: 11 matching lines across 8 files. Every one of the 11 `Path` values (10 on the matching line; 1 on the context line, for the mock whose script block opens on its own line) begins with a drive letter, a colon and a backslash. Values not transcribed.

## (g) .github/workflows/_pester.yml

- Line 38 `          Import-Module Pester -RequiredVersion 5.6.1`; line 45 contains `CodeCoverage.Path = @('scripts/dependencies', 'scripts/vscode')`; line 71 `          if ($linePercent -lt 80) { exit 1 }`: observed.
- Grep count `UseBreakpoints`: 0.

## (h) .gitignore

- Line 144 is `coverage/*`: observed.

## (i) Feature-folder evidence and original plan

- evidence/baseline/p0-t7-test-baseline.2026-09-29T09-05.md line 71 `- POPULATION_LINE: covered=1613 missed=94`; line 75 begins `- BASELINE_POPULATION_LINE_PERCENT: 94.49`: observed.
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md line 35 `- POPULATION_LINE: covered=1620 missed=95`; line 62 `- CHANGED-LINES-UNCOVERED (Invoke-MSTestWithCoverage.ps1): 408`: observed.
- evidence/qa-gates/p2-t4-loop-closure.2026-09-29T09-25.md line 21 `LOOP-CLOSED: no`: observed.
- plan.2026-09-28T19-45.md Grep `^- \[ \] \[P` count 2; the matching lines are 280 `- [ ] [P2-T3]` and 281 `- [ ] [P2-T4]`: observed.

Output Summary:
- Every citation (a) through (i) re-derived on the worktree at bde728cd4 equals the plan's expected value.
- No citation drift; no BLOCKED condition.
