# Final test-and-coverage gate — remediation cycle 2

Timestamp: 2026-08-26T22-27

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

EXIT_CODE: 0

## Output Summary

- Authoritative runner result: 6586 total, 6586 passed, 0 failed.
- The total equals the P0-T9 baseline total of 6587 minus 1, as required by D-F.
- No failures occurred in `EfcSelectionGuardTests`, `EmailFilerConfig_Tests`, or the rule-8 must-stay-green set.
- No new failure occurred relative to the P0-T9 baseline.
- Filtered Cobertura: `coverage\coverage.cobertura.filtered.p5-t4c2.xml`.
- Filtered line coverage: 84.8841% (53988 / 63602).
- Filtered branch coverage: 78.8692% (12750 / 16166).

The successful canonical runner rewrote its Cobertura output in place. That successful filtered output was preserved as the required filtered artifact.

## Raw-preservation support run

The separate same-population collection used only to preserve the pre-conversion raw Cobertura produced `coverage\coverage.cobertura.raw.p5-t4c2.xml` but encountered the pre-existing issue #592 pump-host timeout cascade under machine load.

ExpectedExitCode: 1

Observed preservation-run exit code: 1. Nine `QfcItemController` pump-host tests expired at their existing 60,000 ms timeout. These failures match #592, are outside the cycle-2 touched classes and rule-8 set, and did not occur in the authoritative runner. The gitignored diagnostic stream is retained at `coverage\p5-t4c2-preservation-attempt2.log`; it is not copied into evidence because it contains absolute host paths.
