# Final QA Test + Coverage Evidence

Timestamp: 2026-03-20T22:24:13.2149022-04:00
Command: `vstest.console.exe <all-test-assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
Repo Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
EXIT_CODE: 0

## Output Summary

- Test run successful.
- **Total tests:** 2523
- **Passed:** 2521
- **Failed:** 0
- **Skipped:** 2
- **Duration:** 16.9090 seconds
- **Coverage artifact:** `coverage/coverage.cobertura.xml`
- **UtilitiesCS line coverage:** 47.29%
- **Overall repo line coverage:** 56.71%
