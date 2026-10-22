# P0-T5 MSTest Coverage Baseline

Timestamp: 2026-08-31T13:35:45Z

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\remediation-baseline.cobertura.xml`

EXIT_CODE: unavailable; the owned coverage process tree was stopped after sustained non-progress.

Output Summary: The wrapper discovered nine test assemblies and began the run under `dotnet-coverage`. It did not emit `coverage/remediation-baseline.cobertura.xml`; `dotnet-coverage` PID 84620, `vstest.console` PID 88472, and its child testhost remained active without further CPU progress. Because no Cobertura report was generated, numeric line coverage and final passing/failing totals are unavailable. This task remains unchecked pending a bounded coverage-run recovery.

Generated Cobertura path: `coverage/remediation-baseline.cobertura.xml` (not generated)
