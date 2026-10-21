# P0-T15 — Full-Suite Test and Coverage Baseline

Timestamp: 2026-08-31T19-05
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
EXIT_CODE: 0
ExpectedExitCode: 0

DISCOVERED_ASSEMBLY_COUNT: 9

## Test Counts

- Total: 6894
- Passed: 6894
- Failed: 0
- Skipped: 0

Output Summary: The runner discovered 9 test assemblies and reported `Test Run Successful.` with `Total tests: 6894` and `Passed: 6894` in `Total time: 54.7773 Seconds`. The vstest summary block printed no `Failed:` and no `Skipped:` line, which vstest omits when the corresponding count is zero; both counts are therefore recorded as 0. The run then completed its coverage stage, printing `Code coverage results: ...\coverage\coverage.cobertura.xml`, `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact: ...\coverage\coverage.cobertura.xml`. The runner exited 0.

The run was started detached and polled to completion; no partial result was recorded. It was not truncated at a shell timeout.

BASELINE_COVERAGE_BELOW_FLOOR: not applicable. The runner exited 0. `Invoke-MSTestWithCoverage.ps1` line 341 calls `Assert-CoberturaLineCoverageThreshold` on the post-processed XML, which throws below 80 percent line coverage; it did not throw, so the repository is at or above the CLAUDE.md 80 line floor at branch head. Because this field is absent, the second branch of the P6-T6 expectation rule is unavailable and no non-zero coverage exit code is authorized anywhere later in this plan on coverage-floor grounds.

Corroboration note: this exit code is recorded as a corroborating observation of the one governing coverage figure derived in P0-T16, not as a second measurement. The runner's floor check reads the root `line-rate` attribute of the same `ConvertTo-KoverageCoberturaXml` output that the governing derivation reads.
