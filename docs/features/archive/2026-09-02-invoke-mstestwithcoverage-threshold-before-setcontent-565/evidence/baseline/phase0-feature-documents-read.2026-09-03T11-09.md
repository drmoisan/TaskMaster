Timestamp: 2026-09-03T11-09

Work Mode: full-bug
AC Source: docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/spec.md (sole source)

Files Read:
- docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/issue.md
- docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/spec.md
- docs/features/active/2026-09-02-invoke-mstestwithcoverage-threshold-before-setcontent-565/research/invoke-mstestwithcoverage-ordering-fix.2026-09-02T09-00.md
- scripts/vscode/Invoke-MSTestWithCoverage.ps1
- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
- tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1

Post-#733 drift note (recorded here per the delegating orchestrator's directive, re-verified
directly against the reconciled tree rather than trusted from the delegation prompt):

- scripts/vscode/Invoke-MSTestWithCoverage.ps1 now reads, at lines 339-345 (a uniform +1 shift
  from the plan's assumed 338-345 anchor, because a new `Get-Content` statement now precedes
  `ConvertTo-KoverageCoberturaXml`):
  339: Write-Output 'Post-processing coverage XML for Koverage compatibility...'
  340: $xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
  341: $processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
  342: Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
  343: (blank)
  344: Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
  345: Write-Output "Done. Coverage artifact: $resolvedOutputPath"
  The ordering defect (Assert before Set-Content) is confirmed still present: swap target is
  actual lines 342 (Assert) and 344 (Set-Content), not the plan's original 341/343.

- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 no longer contains
  `Assert-CoberturaLineCoverageThreshold`. Issue #733 extracted it into a new file
  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1, dot-sourced from
  Invoke-MSTestWithCoverage.Helpers.ps1 line 4 (`. (Join-Path $PSScriptRoot
  'Invoke-MSTestWithCoverage.Threshold.ps1')`), which Invoke-MSTestWithCoverage.ps1 itself
  dot-sources transitively at line 261 via Helpers.ps1. No call-site change is required. The
  threshold value and message text are byte-for-byte unchanged: confirmed at
  scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 line 52
  (`if ($percentage -lt 80) {`) and line 54
  (`throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."`).

- tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1's `Describe
  'Invoke-MSTestWithCoverageMain'` block (opens line 346) now contains, in order: `It 'fails
  when the search root cannot be found'` (lines 409-414), a new #733 test `It 'excludes
  assemblies discovered under a .claude worktree segment'` (lines 416-442), then the Describe
  block's closing `}` (line 443). This is confirmed directly against the reconciled tree.

BASELINE_SHA for this execution: dc5e8c0fa39b27b3d5523d6e82daafe8c844ae12 (recorded formally in
P0-T3; re-derived here, not carried forward from the plan's original 5ebaaf10-era assumption).
