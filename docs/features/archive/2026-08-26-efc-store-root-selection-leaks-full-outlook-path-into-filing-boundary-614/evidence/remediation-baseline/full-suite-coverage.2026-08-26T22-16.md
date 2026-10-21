# Baseline Full-Suite Test and Coverage Gate — remediation cycle 2

Timestamp: 2026-08-26T22-16

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

EXIT_CODE: 0

Output Summary:

- Test run successful: 6587 total, 6587 passed, 0 failed.
- No rule-6 flake and no new failure were observed against the 6587/6587/0 reference.
- Raw Cobertura: `coverage\coverage.cobertura.raw.p0-t9c2.xml`.
- Filtered Cobertura: `coverage\coverage.cobertura.filtered.p0-t9c2.xml`.
- Filtered line coverage: 84.8758% (53998 / 63620).
- Filtered branch coverage: 78.8585% (12753 / 16172).
- The out-of-band raw/filtered preservation run used the runner's own
  `Invoke-DotnetCoverageCollection` and `ConvertTo-KoverageCoberturaXml` functions against the same
  nine test assemblies and also completed 6587/6587/0 with exit code 0.
- The small covered-line variation from the 84.8790% reference is within the plan's documented
  run-to-run coverage nondeterminism; test population and failure count are unchanged.
