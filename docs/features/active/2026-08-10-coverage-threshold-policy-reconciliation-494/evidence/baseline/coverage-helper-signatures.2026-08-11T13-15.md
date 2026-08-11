Timestamp: 2026-08-11T13-15
Command: `git merge-base --is-ancestor fb257cd6 HEAD`; `git merge-base --is-ancestor 8d0d1fec HEAD`; `rg -n "function (Get-CoberturaCoverageSummary|Merge-CoberturaClassesByFilename|Get-CoberturaLineConditionCoverageParts|ConvertTo-KoverageCoberturaXml|Get-KoverageProjectAllowlist)" scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
EXIT_CODE: 0

Ancestry:
- `git merge-base --is-ancestor fb257cd6 HEAD`: EXIT_CODE 0
- `git merge-base --is-ancestor 8d0d1fec HEAD`: EXIT_CODE 0

Current Helper Signatures:
- `Get-KoverageProjectAllowlist`: line 4
- `Get-CoberturaCoverageSummary`: line 99
- `Get-CoberturaLineConditionCoverageParts`: line 141
- `Merge-CoberturaClassesByFilename`: line 262
- `ConvertTo-KoverageCoberturaXml`: line 393

Output Summary: Both required predecessor commits are ancestors of HEAD. All required helper symbols are present, including the two fail-closed corrected-arithmetic symbols.
