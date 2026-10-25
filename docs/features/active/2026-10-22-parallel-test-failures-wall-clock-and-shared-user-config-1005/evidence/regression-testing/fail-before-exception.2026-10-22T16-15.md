# Fail-Before Exception Dossier - DfDeedle AddQfcColumnsAsync (Issue #1005)

Timestamp: 2026-10-22T16-15
Command: git grep -n -F 'AddQfcColumnsAsync(' -- '*.Test/*.cs' '*.Test/**/*.cs'
EXIT_CODE: 0
Output Summary: The before-fix AC4 Q4 search finds two `AddQfcColumnsAsync(` calls in `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` that pass neither `columnAdder` nor `timeProvider` (`AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing`, `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully`). A deterministic failing run is not possible without real timers, so this dossier substitutes for a failing run.

WhyFailingRunImpossible: The reported failure needs the three real 3000 ms deadlines armed by `TimeoutTask.TimeoutAfter` on `TimeProvider.System` to elapse before the `Task.Run` column-add work completes, which happens only when the testhost is under heavy parallel load. A deterministic local run either completes the work before the first deadline (the baseline full-suite run passed this test) or would need real timers and induced load, which the item constraints prohibit (no `Thread.Sleep`, `Task.Delay`, or real waits).

## Maintainer's captured exception (quoted from `issue.md`)

> Observe `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` fail after 9.5 s with `TimeoutException: The column add step timed out after 9000 ms for folder '(unknown folder)'` thrown from `DfDeedle.AddQfcColumnsAsync`.

Exception text: `TimeoutException: The column add step timed out after 9000 ms for folder '(unknown folder)'`

## Alternative Proof

The repeatable demonstration is the before-fix AC4 search recorded in `evidence/regression-testing/ac4-search-before.2026-10-22T16-12.md` (Q4): two `AddQfcColumnsAsync(` calls without `timeProvider:` exist in `DfDeedle_COM_Tests.cs` before the fix. Without a `timeProvider`, `TimeoutAfter` arms `TimeProvider.System.CreateTimer`, so each of the three deadlines is wall-clock bound and the test outcome depends on scheduler load. The after-fix search (`ac4-search-after.<timestamp>.md`) is expected to find zero such calls in that file and two calls in `DfDeedle_COM_Tests.QfcColumnsAsync.cs`, each passing `timeProvider: new FakeTimeProvider()`, a clock that never advances, so no deadline can fire.

## Search scope (negative-claim record)

- SearchScope: `<FEATURE>/evidence/regression-testing/`
- SearchPatterns: a failing-run artifact for `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing`
- SearchResult: none; this dossier is the fail-before evidence for the DfDeedle path.
