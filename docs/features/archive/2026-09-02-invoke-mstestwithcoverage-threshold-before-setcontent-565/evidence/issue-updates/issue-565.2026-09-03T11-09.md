Timestamp: 2026-09-03T11-09

PostedAs: comment

Exact text posted:
---
Fixed on branch `bug/invoke-mstestwithcoverage-threshold-before-setcontent-565`.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1`'s `Invoke-MSTestWithCoverageMain` now persists the
post-processed Cobertura document (`Set-Content`) immediately after `$processedXmlContent` is
computed, before `Assert-CoberturaLineCoverageThreshold` is called. On a sub-threshold run the
artifact left on disk at `-CoverageOutput` is now the same post-processed document the threshold
assertion judged, not the raw `dotnet-coverage` output.

A new Pester test in `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
(`Describe 'Invoke-MSTestWithCoverageMain'`) proves the ordering deterministically: it mocks
`ConvertTo-KoverageCoberturaXml` to return a sub-threshold fixture, asserts the call throws, and
asserts `Set-Content` was invoked exactly once before the throw.

No change to the 80% threshold value or its message text
(`Assert-CoberturaLineCoverageThreshold`, now in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`
following issue #733's extraction). Full PowerShell toolchain (PoshQC format, PSScriptAnalyzer,
Pester) passes cleanly with no regression; coverage of the changed file is unchanged at 90.09%.
---

GitHub comment URL: https://github.com/drmoisan/TaskMaster/issues/565#issuecomment-5524940952
