# Remediation baseline: coverage test route on unmodified branch head (#1002, cycle 1)

Timestamp: 2026-10-22T13-20
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
EXIT_CODE: 0
Output Summary:
- Test run outcome Completed: total 7579, executed 7579, passed 7579, failed 0 (skipped 0 derived as total minus executed; error, timeout, aborted, notExecuted 0).
- First-party coverage: lines 57527/66780 (86.14%), branches 14018/17439 (80.38%).
- Run against the already-built test assemblies of the branch head; no source change preceded it.
- Raw trx and Cobertura kept out of git (Cobertura and trx remain under the gitignored coverage output).
