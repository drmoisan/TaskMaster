Timestamp: 2026-08-11T13-40
Command: `pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; $raw = Get-Content "docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/coverage-remeasurement-run1.raw.cobertura.xml" -Raw -Encoding UTF8; $corrected = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content "docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/coverage-remeasurement-run1.corrected.cobertura.xml" -Value $corrected -Encoding UTF8 -NoNewline'`
EXIT_CODE: 0
Output Summary: `coverage-remeasurement-run1.corrected.cobertura.xml` was written successfully. Its readable root attributes are `line-rate=0.855451`, `branch-rate=0.790449`, `lines-valid=62401`, and `lines-covered=53381`.

Determination: The raw report was converted with the current helper. Only the corrected XML will be used for coverage observations.
