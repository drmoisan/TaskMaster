Timestamp: 2026-08-22T13-13
Command: pwsh -NoProfile -Command 'Select-Xml -Path "docs/features/active/2026-08-07-quickfiler-test-form1-live-form-491/evidence/baseline/coverage-baseline.cobertura.xml" -XPath "/coverage" | ForEach-Object { $_.Node.GetAttribute("lines-covered"); $_.Node.GetAttribute("lines-valid"); $_.Node.GetAttribute("line-rate") }'
EXIT_CODE: 0
Output Summary: Baseline numeric line-coverage headline: 85.5788% (line-rate 0.855788 multiplied by 100, rendered to four decimal places). lines-covered = 53402. lines-valid = 62401. These are the harness's Koverage-postprocessed, first-party-only figures produced by `Invoke-MSTestWithCoverage.ps1`, per the P0-T19 artifact.
