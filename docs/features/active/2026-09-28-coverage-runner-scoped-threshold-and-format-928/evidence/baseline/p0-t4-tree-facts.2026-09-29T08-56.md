# P0-T4 Tree Facts

Timestamp: 2026-09-29T08-56
Task: P0-T4
Command: Read, Grep and Glob tools only, against <repo-root> at the base anchor (no script-tree changes present per P0-T3)
EXIT_CODE: 0

(a) scripts/vscode/Invoke-MSTestWithCoverage.ps1
- Read renders 440 numbered lines, line 440 empty. Expected 440 / last empty. Observed: equal.
- Line 272: `$ErrorActionPreference = 'Stop'`, no leading whitespace. Observed: equal.
- Line 274: `function Invoke-MSTestWithCoverageMain {`. Observed: equal.
- Lines 276 to 277: `.SYNOPSIS` and its one-line text; line 278 `#>` closes the help block. Observed: equal.
- Line 295: `. (Join-Path $ScriptRoot 'Invoke-MSTest.TrxSummary.ps1')`. Observed: equal.
- Line 306: `$resolvedSearchRoot = Join-Path $repoRoot $SearchRoot`. Observed: equal.
- Line 386: `    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent`. Observed: equal.
- Line 387: `    Assert-CoberturaBranchCoverageThreshold -CoberturaXml $processedXmlContent`. Observed: equal.
- Grep count `Invoke-MSTestWithCoverage\.Scope\.ps1`: 0. Expected 0.
- Grep count `\.DESCRIPTION`: 5. Expected 5.
- Grep count `\.PARAMETER SearchRoot`: 0. Expected 0.

(b) scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1
- Read renders 127 numbered lines, line 127 empty. Observed: equal.
- `-lt 80`: 1 (line 52). `-lt 75`: 1 (line 122). `required 80% threshold`: 1 (line 54). `required 75% threshold`: 1 (line 124). Expected 1 each. Observed: equal.

(c) scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
- Read renders 472 numbered lines, line 472 empty. Observed: equal.
- Lines 2 to 6 dot-source ClosureFilter, PackageRate, Threshold, FirstParty, Projection part files (five). Observed: equal.

(d) tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
- Read renders 499 numbered lines, line 499 empty. Observed: equal.

(e) Grep `TestDrive` over tests/scripts/vscode: 0 matches. Expected 0. Observed: equal.

(f) Glob tests/scripts/vscode: 19 files; no Invoke-MSTestWithCoverage.Scope.Tests.ps1. Glob tests/scripts/dependencies: 7 files. Expected 19 / absent / 7. Observed: equal.

(g) Glob scripts/vscode: 15 entries (14 .ps1 plus TaskMaster.cli.runsettings); no Invoke-MSTestWithCoverage.Scope.ps1. Expected absent. Observed: equal.

(h) scripts/vscode/Invoke-MSTest.ps1
- Read renders 263 numbered lines, line 263 empty. Observed: equal.
- `Write-Host` statements at lines 210 and 211 (Grep also matches the retention comment text at lines 250 and 251). Observed: equal.

Output Summary:
- All eight fact groups (a) through (h) observed equal to the plan's expected values.
- No stop condition fired (citation-drift not triggered).
