Timestamp: 2026-08-22T14-17

Command: pwsh -NoProfile -Command 'Select-Xml -Path "docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/qa-gates/coverage-postchange-remediation.cobertura.xml" -XPath "/coverage" | ForEach-Object { $_.Node.GetAttribute("lines-covered"); $_.Node.GetAttribute("lines-valid"); $_.Node.GetAttribute("line-rate") }'

EXIT_CODE: 0

Output Summary:
- lines-covered: 53392
- lines-valid: 62401
- line-rate: 0.855627
- Post-change line-coverage percentage (line-rate * 100, four decimal places): 85.5627%
