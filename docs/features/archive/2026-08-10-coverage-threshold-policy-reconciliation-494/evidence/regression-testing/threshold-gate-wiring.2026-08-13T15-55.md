Timestamp: 2026-08-13T15-55
Command: `rg -n -C 2 "ConvertTo-KoverageCoberturaXml|Assert-CoberturaLineCoverageThreshold|Done. Coverage artifact" scripts/vscode/Invoke-MSTestWithCoverage.ps1`
EXIT_CODE: 0
Output Summary:

- `Invoke-MSTestWithCoverageMain` now invokes `Assert-CoberturaLineCoverageThreshold -CoberturaXml $processedXmlContent` directly after `ConvertTo-KoverageCoberturaXml`.
- The evaluator executes before `Set-Content` and before the success completion message.
- A below-80 processed document throws from the invoked evaluator; a valid 80%-or-higher document reaches the existing write and completion path.
