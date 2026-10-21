# P0-T16 — Repository-Wide Baseline Coverage Figures

Timestamp: 2026-08-31T19-08
Command: read `coverage\coverage.cobertura.xml` and apply the governing derivation fixed in the plan's execution rules
EXIT_CODE: 0

DERIVATION_BRANCH: the on-disk document already contained a `<sources>` element, so it is the post-processed output that a successful runner wrote and its root `coverage` attributes were read directly. `ConvertTo-KoverageCoberturaXml` was not re-applied, because applying it to already-processed content would be a second transform rather than the governing one.

## Recorded Figures

BASELINE_LINE_RATE: 0.853296
BASELINE_LINES_COVERED: 54820
BASELINE_LINES_VALID: 64245
BASELINE_BRANCH_RATE: 0.793089
BASELINE_BRANCHES_COVERED: 13059
BASELINE_BRANCHES_VALID: 16466

Output Summary: All six root `coverage` attributes were read from the single Koverage project-allowlist denominator that `ConvertTo-KoverageCoberturaXml` produces at `Invoke-MSTestWithCoverage.Helpers.ps1` lines 441 through 447. Every coverage number recorded anywhere in this change comes from this one derivation on this one denominator; none is taken from any runner's console output.

Corroboration: `BASELINE_LINE_RATE:` 0.853296 is at or above the CLAUDE.md repository-wide line floor of 0.80, which is consistent with the P0-T15 runner exiting 0 without `Assert-CoberturaLineCoverageThreshold` throwing. That agreement is expected, because the runner's floor check reads this same root `line-rate` attribute on this same denominator; it is recorded as corroboration of one figure, not as a second measurement.
