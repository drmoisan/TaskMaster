## [P2-T5] Full Coverage-Enabled Test Run (Final) — Adopted Result

- Timestamp: 2026-08-08T22-30
- Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput 'docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-remediation-final.cobertura.xml' ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Total tests: 6350. Passed: 6350. Failed: 0. Zero discovered assembly paths contained `\.claude\`. Repository-wide `line-rate = 0.85862`, `branch-rate = 0.79286`. This is the artifact adopted as the final, current-on-disk `coverage-remediation-final.cobertura.xml` for P2-T6/P2-T7/P2-T8.

### Full attempt history for P2-T5 (six coverage-suite runs total, in order)

| # | Purpose | Result | Total | Line-rate | Branch-rate |
|---|---|---|---|---|---|
| — | P0-T8 baseline (before test edit) | Success (after 1 killed hang) | 6348/6348 | 0.858512 | 0.792359 |
| 1 | P2-T5 pass 1 | Success | 6350/6350 | 0.858548 | 0.792717 |
| 2 | P2-T5 pass 2 | Success | 6350/6350 | 0.858485 | 0.792574 |
| 3 | P2-T5 pass 3 | **Hung** (killed after ~15 min at zero CPU delta, same suite position as the P0-T8 hang) | n/a | n/a | n/a |
| 4 | P2-T5 pass 4 | **Genuine flaky failure**, unrelated to R1: `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` expected `InvalidOperationException`, observed `TaskCanceledException` (timing-sensitive Dispatcher-yield test racing under system load; file untouched by this remediation) | 6349/6350 | n/a (run treated as failed by the script; not used for gate evaluation) | n/a |
| 5 | P2-T5 pass 5 | Success | 6350/6350 | 0.858629 | 0.792789 |
| 6 (**adopted**) | P2-T5 pass 6 | Success | 6350/6350 | **0.85862** | **0.79286** |

None of the failed/hung attempts (3, 4) is used as gate evidence; only the five clean, all-tests-passing runs (baseline + passes 1, 2, 5, 6) are used for the coverage-gate analysis in P2-T7.
