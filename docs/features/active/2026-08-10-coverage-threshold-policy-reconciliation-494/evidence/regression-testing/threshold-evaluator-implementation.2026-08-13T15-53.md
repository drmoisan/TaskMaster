Timestamp: 2026-08-13T15-53
Command: `. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1; Assert-CoberturaLineCoverageThreshold` with absent, non-numeric, 79.9999%, 80%, and 80.0001% in-memory Cobertura XML inputs
EXIT_CODE: 0
Output Summary:

- Added pure `Assert-CoberturaLineCoverageThreshold` with an explicit Cobertura XML string parameter.
- It deterministically returned the expected missing-summary, non-numeric, and below-threshold errors.
- It accepted exact 80% and above-80% input.
- The function has no filesystem or process dependency.
