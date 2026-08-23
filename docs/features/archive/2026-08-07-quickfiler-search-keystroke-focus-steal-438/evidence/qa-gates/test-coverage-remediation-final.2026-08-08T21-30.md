## [P2-T5] Full Coverage-Enabled Test Run (Final)

- Timestamp: 2026-08-08T21-30
- Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-remediation-final.cobertura.xml' ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Total tests: 6350 (= 6348 baseline + 2 new). Passed: 6350. Failed: 0. Zero discovered assembly paths contained `\.claude\` (same fixed nine-assembly list as P0-T8). Repository-wide `line-rate = 0.858548` (`lines-covered=95474`, `lines-valid=111204`), `branch-rate = 0.792717` (`branches-covered=22139`, `branches-valid=27928`). This run completed cleanly on the first attempt (no hang, unlike the first P0-T8 attempt).

### Preliminary comparison against the fixed floor (0.858665 line / 0.792502 branch)

- `branch-rate = 0.792717 >= 0.792502` — meets the fixed floor.
- `line-rate = 0.858548 < 0.858665` — marginally below the fixed floor (delta `-0.000117`), consistent with the run-to-run coverage nondeterminism already documented and root-caused in P0-T8 (isolated to `EfcHomeController.cs`, `PropertyStore.cs`, `SegmentStopWatch.cs`; none is the R1 target file). Full investigation and disposition recorded in P2-T7's evidence artifact.
