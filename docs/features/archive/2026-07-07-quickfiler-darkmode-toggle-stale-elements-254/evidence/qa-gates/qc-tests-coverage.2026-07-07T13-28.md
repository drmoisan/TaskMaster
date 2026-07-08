# QC — Tests + Coverage (Issue #254)

Timestamp: 2026-07-07T13-28

Command (gate, pass/fail): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Settings:<lowworkers.runsettings>`

Command (numeric coverage): `dotnet-coverage collect --output-format cobertura -- vstest.console.exe <same two DLLs> /Settings:<lowworkers.runsettings>`

EXIT_CODE: 0 (gate run)

## Flakiness handling (no assertion weakened, no retries/sleeps)

The suite carries pre-existing, environmental flakiness: a small set of timing-sensitive UtilitiesCS.Test cases (OneDrive download, file-stream writer, dictionary async) time out under high parallel-worker contention and coverage instrumentation (baseline `baseline-tests-coverage.2026-07-07T13-10.md`; each such test passes in isolation at ~65ms). This is unrelated to Theme / QuickFiler dark-mode code and to issue #254. Two adjustments were used to obtain a deterministic result without touching any test logic:
1. `lowworkers.runsettings` sets MSTest `<Parallelize><Workers>4</Workers>` (overriding the assembly default of processor-count = 24) to remove the parallel-starvation timeouts. Scope, framework, assemblies, and every assertion are unchanged.
2. Numeric coverage is obtained via `dotnet-coverage collect` (Cobertura) because the built-in collector's `.coverage` binary is not offline-convertible in this environment.

Under the built-in Code Coverage collector at 4 workers the full suite is deterministically clean.

## Output Summary — gate run

Test Run Successful. Total tests: 4661, Passed: 4661, Failed: 0. (4661 = prior 4658 + 3 new Theme mail-label tests.)

- #251 no-regression (AC4) CONFIRMED: `QfcCollectionControllerDarkModeTests` suite all pass, including the unsubscribe regression tests `CleanupAsync_ThenDarkModePropertyChanged_DoesNotThrow` and `Cleanup_ThenDarkModePropertyChanged_DoesNotThrow`, plus `DarkMode_CheckedChanged_ShouldUpdateTheme`, `ToggleDark_*`, and `IsSystemDarkMode_*`.
- New tests all pass: `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread`, `Theme_MailLabelTheming_WhenProbeReturnsFalse_AppliesUnreadColors`, `Theme_MailLabelTheming_WhenProbeReturnsTrue_AppliesReadColors`.

## Numeric Post-Change Coverage (Cobertura)

- Overall line-rate: 64.28% (lines-covered 110182 / lines-valid 171399); branch-rate 33.12%. (Baseline overall 64.28% — no regression.)
- UtilitiesCS module: 87.93% (70324 / 79981). Baseline 87.89% — slight increase, no regression.
- Theme class `Theme.cs`: 66.10% (312 / 472). Baseline 62.71% — increase (new tests exercise SetMailRead/SetMailUnread).
- Theme class `Theme.Rendering.cs` (changed file): 54.05% (80 / 148). Baseline 44.78% (60 / 134) — increase.
- Changed-block line coverage (lines 44-59, the new try/catch + if/else): every executable line has hits >= 1 -> 100% changed-line coverage. Both try-success branches (read / unread) and the catch-default branch are covered.

Meets the >= 90% new/changed-code floor (100% actual) with no coverage regression on changed lines. See `coverage-comparison.<TS>.md` for the delta table.
