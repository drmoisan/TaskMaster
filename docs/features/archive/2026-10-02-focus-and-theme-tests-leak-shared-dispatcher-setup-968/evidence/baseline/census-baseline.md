# Pre-change census of the four #968 code files (issue #968, task P0-T12)

Timestamp: 2026-10-03T02-47
Command: pwsh -NoProfile -Command '<CMD-CENSUS payload>' (plan Command Reference CMD-CENSUS, executed verbatim with PREFIX expanded and WORKTREE substituted)
Canonical command: CMD-CENSUS (primary pattern `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)`, cross pattern `EnsureUiThreadDispatcher|EnsureDispatcher`, control pattern `BeginTransactionAsync\(`, over every *.cs outside packages, .claude, obj and bin)
EXIT_CODE: 0
Output Summary: WORKTREE-LEAF agent-a291a7fbabf9d0229 in every payload; LINES 342, 497, 440, 470; PRIMARY_LINES 9, CROSS_LINES 20, CONTROL_LINES 23; every token and span value equals the plan's expected pre-change value (no CENSUS MISMATCH). Details below.

Every payload below was a separate Bash call `pwsh -NoProfile -Command '<payload>'` whose payload is the named Command Reference macro with PREFIX expanded, WORKTREE substituted and the stated FILE, FILES, START, END and TOKENS values substituted; each exited 0 and printed `WORKTREE-LEAF: agent-a291a7fbabf9d0229`.

## CMD-LINECOUNT on CS4

- LINES QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 342
- LINES QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs = 497
- LINES QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs = 440
- LINES QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 470

## CMD-HASH on CS4

- BASE-HASH: QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6
- BASE-HASH: QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs = A3C35259F1C5E5D2ED8D8A3E5BA923A964E2B164ABE9D9AC7B6B32EC30644E4B
- BASE-HASH: QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs = 6DDACD2EC8DED8C83320F3F65E0A61C0BE16283A7BB2D277963C46BDA9B13779
- BASE-HASH: QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 05638767D69C12FE98B28DCE78B6827AD2C1EC64221F1E445862087668BF5DCA

## CMD-CENSUS

- CS_FILES: 1706
- PRIMARY_LINES: 9
- PRIMARY-FILE \QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs = 2
- PRIMARY-FILE \QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs = 2
- PRIMARY-FILE \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 1
- PRIMARY-FILE \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 4
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs:452 :: QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs:468 :: QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs:238 :: internal static IDisposable EnsureUiThreadDispatcher() =>
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs:239 :: UiThreadDispatcherFixture.EnsureDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs:122 :: internal static IDisposable EnsureDispatcher()
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs:60 :: QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs:119 :: IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs:166 :: IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- PRIMARY \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs:222 :: IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()
- CROSS_LINES: 20
- CROSS-FILE \QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs = 2
- CROSS-FILE \QuickFiler.Test\Controllers\QfcItemController.InitializationTests.Part2.cs = 1
- CROSS-FILE \QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs = 2
- CROSS-FILE \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 5
- CROSS-FILE \QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 10
- CROSS lines: FocusAndThemeTests 452, 468; InitializationTests.Part2 124 (comment); TestSupport 238, 239; UiThreadDispatcherFixture 26, 27, 122, 195, 244; UiThreadDispatcherFixtureTests 44, 60, 70, 107, 119, 128, 157, 166, 198, 222 (each line's text matches fact 5 and research section 2.1)
- CONTROL_LINES: 23

## CMD-TOKEN-COUNT

FIX (`_pinCount`, `_fixtureInstalledParked`, `lock (FieldLock)`, `CompareExchange(`, `return new EnsureScope(`, `leaks exactly`, `A scope that installed nothing`, `pins for the process lifetime`, `install-ownership flag`, `installed nothing carries`): 0, 0, 4, 3, 2, 1, 1, 0, 0, 0. The last value is vacuous at baseline: the phrase wraps across lines 245 and 246, so a per-line count reads 0 before the change.

FAT (`EnsureUiThreadDispatcher`, `private static Mock<IItemViewer> BuildExecutingViewer`, `QfcItemControllerTestSupport.BuildExecutingViewer()`, `BuildExecutingViewer`, `absorbs the delegate without running it`, `shared UiThread static is irrelevant`, `absorbs the queued application`, `[TestMethod]`): 2, 1, 0, 9, 0, 0, 0, 17.

TS (`Becomes moot`, `leaks exactly`, `still delegate to a callee`, `not reachable from another test file`, `remaining legitimate`, `QfcItemController_UiThreadDispatcherPinCountTests`, `internal static void EnsureSynchronizationContext()`, `UiThreadDispatcherFixture.EnsureDispatcher();`): 1, 1, 1, 1, 0, 0, 1, 1.

FT (`no other class may dispose`, `removed that pin: the fixture now counts pins`, `(W5) must not latch`, `[Timeout(GateTimeoutMs)]`, `private const int GateTimeoutMs = 60000;`, `EnsureUiThreadDispatcher()`, `issue #230 lost update`, `the waiter cannot observe the pre-restore value`, `[TestMethod]`): 1, 0, 1, 8, 1, 4, 1, 1, 8.

## CMD-SPAN-TOKEN-COUNT

- R4SPAN (FT; START `public async Task Transaction_SecondCallerCannotInstallUntilTheFirstRestores()`, END `public async Task Transaction_DisposedTwice_DoesNotOverReleaseTheGate()`): SPAN: 212-284; `EnsureUiThreadDispatcher()` 1, `using (` 2, `transactionA.Dispose();` 1, `finally` 2, `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1.
- R4HEAD (FT; START the R4SPAN START, END `Dispatcher original = UiThreadDispatcherFixture.Current;`): SPAN: 212-224; `EnsureUiThreadDispatcher()` 1, `using (` 1, `try` 1.
- R4TAIL (FT; START `issue #230 lost update`, END `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);`): SPAN: 267-273; `}` 3, `finally` 1, `transactionA.Dispose();` 0.
- ENSURE (FIX; START `internal static IDisposable EnsureDispatcher()`, END `internal const int TransactionGateAcquireTimeoutMs = 120000;`): SPAN: 122-145; `_pinCount++` 0, `_fixtureInstalledParked = true;` 0, `lock (FieldLock)` 1, `return new EnsureScope(` 2.
- SCOPE (FIX; START `private sealed class EnsureScope : IDisposable`, END `internal sealed class UiThreadDispatcherTransaction : IDisposable`): SPAN: 249-283; `CompareExchange(` 1, `lock (FieldLock)` 0, `_pinCount--` 0, `_fixtureInstalledParked = false;` 0, `DispatcherField.SetValue(null, null);` 0.

The non-zero counts (the two focus-and-theme ensure calls, the private helper, the four stale doc tokens, the R4 pin) are the positive controls for the zero gates of P6-T2 and P7-T1.
