# Baseline Full-Suite Test-and-Coverage Run (P0-T9) — remediation cycle 1, issue #614

Timestamp: 2026-08-26T21-22

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

EXIT_CODE: 0

`ExpectedExitCode` is deliberately omitted: the run is fully green, so the default expectation of 0
matches the observed exit code. No rule-6 flake (#594 / #592 / #586 / #584) was observed.

## Output Summary — test results

Runner verdict line: `Test Run Successful.`

| Metric | Reference (post-#614-delivery review run) | This baseline run |
| --- | --- | --- |
| Total tests | 6569 | **6569** |
| Passed | 6569 | **6569** |
| Failed | 0 | **0** |
| Skipped | 0 | **0** |
| Exit code | 0 | **0** |
| Total time | 37.72 s | 32.9965 s |

- Lines matching `^\s*Failed ` in the full runner log: **0**. Lines matching `^\s*Skipped `: **0**.
- Counts are identical to the recorded 6569 / 6569 / 0 reference. **No NEW failure.**

## Output Summary — coverage

The run was green, so `Invoke-DotnetCoverageCollection` did not throw and the runner performed its
in-place `ConvertTo-KoverageCoberturaXml` rewrite. The raw pre-post-processing Cobertura was
therefore consumed in place and the unfiltered figure is **unavailable for this run**, exactly as on
the delivery-cycle Phase 0 baseline and its P9-T4 final run. That figure is informational only and
gates nothing.

`coverage\coverage.cobertura.xml` (the allowlist-filtered artifact) was copied to
`coverage\coverage.cobertura.filtered.p0-t9r.xml` — the gitignored `coverage/` tree, never under
`evidence/`. That copy is the P5-T5 baseline input.

| Figure | Reference (review run) | This baseline run |
| --- | --- | --- |
| Filtered first-party line coverage | 84.8696% (53972 / 63594) | **84.8712% (53973 / 63594)** |
| Filtered branch coverage | 78.8331% (12741 / 16162) | **78.8454% (12743 / 16162)** |
| Unfiltered line coverage | unavailable (raw consumed in place) | unavailable (raw consumed in place) |

Both denominators (63594 lines, 16162 branches) are identical to the reference run; the +1 covered
line and +2 covered branches are the known run-to-run nondeterminism of `dotnet-coverage`. The
**gating baseline for this cycle is the measured 84.8712% / 78.8454%**, which is the stricter of the
two figures.

Allowlist packages present in the filtered artifact (9, matching `Get-KoverageProjectAllowlist`):
QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags, TaskMaster, TaskTree,
VBFunctions.

The pre-existing repo-wide shortfall against the 85% floor (84.87% < 85%) is recorded as
pre-existing and is explicitly out of scope for this cycle per the remediation inputs; it is NOT
gated to 85% anywhere in this plan.

Raw runner log contains absolute host paths including the machine account name; it was written to
the session scratchpad outside the repository and is not copied under `evidence/`.
