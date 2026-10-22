Timestamp: 2026-09-03T11-09

Check 1 (scripts/vscode/Invoke-MSTestWithCoverage.ps1, plan-assumed lines 338-345):
Does NOT match verbatim at the plan's assumed line numbers. This is the expected outcome per
the delegating orchestrator's pre-derived delta (issue #733 / PR #748 inserted a `Get-Content`
statement ahead of `ConvertTo-KoverageCoberturaXml`, producing a uniform +1 shift). Fallback
branch taken: located the block by its unique surrounding comment
`# Post-process the Cobertura XML for Koverage compatibility:` inside
`Invoke-MSTestWithCoverageMain`.

Drifted-anchor verbatim excerpt actually found (lines 339-345, read directly from
scripts/vscode/Invoke-MSTestWithCoverage.ps1 in this pass):
```
339	    Write-Output 'Post-processing coverage XML for Koverage compatibility...'
340	    $xmlContent = Get-Content $resolvedOutputPath -Raw -Encoding UTF8
341	    $processedXmlContent = ConvertTo-KoverageCoberturaXml -XmlContent $xmlContent -RepoRoot $repoRoot
342	    Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent
343	
344	    Set-Content -Path $resolvedOutputPath -Value $processedXmlContent -Encoding UTF8 -NoNewline
345	    Write-Output "Done. Coverage artifact: $resolvedOutputPath"
```
Drifted locators for [P2-T1]: swap actual line 342 (`Assert-CoberturaLineCoverageThreshold`)
with actual line 344 (`Set-Content`), not the plan's original 341/343.

Check 2 (scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1, plan-assumed line 487):
Does NOT match — a file relocation, not merely a line shift. `Assert-CoberturaLineCoverageThreshold`
no longer resides in Invoke-MSTestWithCoverage.Helpers.ps1 at all. Fallback branch taken: located
the function by the unique string `is below the required 80% threshold.`, found in a NEW file,
scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1, extracted by issue #733. Confirmed
Invoke-MSTestWithCoverage.Helpers.ps1 line 4 dot-sources it
(`. (Join-Path $PSScriptRoot 'Invoke-MSTestWithCoverage.Threshold.ps1')`), and
Invoke-MSTestWithCoverage.ps1 line 261 still dot-sources Helpers.ps1, so the function resolves
transitively with no call-site change required.

Drifted-anchor verbatim excerpt actually found (Invoke-MSTestWithCoverage.Threshold.ps1, lines
around 52-54, read directly in this pass):
```
52	    if ($percentage -lt 80) {
...
54	        throw "Cobertura line coverage $formattedPercentage% is below the required 80% threshold."
```
The threshold value (80) and message text are byte-for-byte unchanged from the plan's original
assumption; only the containing file differs.

Branch taken: drifted-anchor (fallback) for both Check 1 and Check 2.
