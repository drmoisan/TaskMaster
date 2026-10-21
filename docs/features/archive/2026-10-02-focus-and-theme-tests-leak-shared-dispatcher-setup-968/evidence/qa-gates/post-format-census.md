# Post-format census (issue #968, task P6-T2)

Timestamp: 2026-10-03T03-19
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES = CS13 (the first of twenty-nine separate payloads re-running every P1-T3, P2-T6, P3-T9, P4-T9, P5-T5 and P5-T12 command after the P6-T1 scoped format: CMD-LINECOUNT on CS13; CMD-TOKEN-COUNT on FIX, FAT, TS (extended list), FT, PC, PROJ (P1-T3 list), PROJ (P0-T13 list), LIV, TD, ZB, DMT, QDM, QQP, SBW and AFTP; CMD-SPAN-TOKEN-COUNT on ENSURE, SCOPE, R4SPAN, R4HEAD, R4TAIL, T1-LIVE, HELD, T-SIB and GATE-LAMBDA; CMD-HUNKS on TestSupport, the fixture tests, QfcDatamodel.cs and QfcDatamodel.QueueProcessing.cs; CMD-PIN-NESTING on T3SPAN; each the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted), followed by three git calls
Canonical command: as listed; git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test; git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs; git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test
EXIT_CODE: 0
Output Summary: WORKTREE-LEAF agent-a291a7fbabf9d0229 in every payload and every payload exited 0; every token, span, hunk and numstat value holds as last recorded for its file; LINES and printed SPAN ranges differ only for the three files P6-T1 named in REWRITTEN; GATE-LAMBDA 1, 0; T3SPAN NEST tail finally, Dispose(), finally, ShutdownDispatcher(; porcelain lists exactly the fourteen Write Set code paths (eleven ` M`, three `??`). Details below.

This is the first P6-T2 pass (no D-13 restart), and no commit of the Write Set exists yet, so every git command is anchored to HEAD as the task states.

## CMD-LINECOUNT on CS13 (every value at most 500; QfcDatamodel.cs at most 400)

| File | LINES | Last recorded | Note |
|---|---|---|---|
| FIX | 375 | 375 (P2-T6) | unchanged |
| FAT | 482 | 482 (P3-T9) | unchanged |
| TS | 442 | 442 (P3-T9) | unchanged |
| FT | 472 | 472 (P3-T9) | unchanged |
| PC | 248 | 248 (P1-T3) | unchanged |
| LIV | 352 | 348 (P5-T5) | differs; named in REWRITTEN |
| TD | 229 | 229 (P4-T9) | unchanged |
| ZB | 226 | 224 (P4-T9) | differs; named in REWRITTEN |
| DMT | 394 | 392 (P5-T5) | differs; named in REWRITTEN |
| QDM | 367 | 367 (P4-T9) | unchanged; at most 400 |
| QQP | 413 | 413 (P5-T12) | unchanged |
| SBW | 27 | 27 (P4-T9) | unchanged |
| AFTP | 49 | 49 (P5-T5) | unchanged |

## CMD-TOKEN-COUNT (each list and order as in its recording task)

- FIX (P2-T6): 4, 4, 5, 2, 1, 0, 0, 2, 1, 0 (`_pinCount` 4, `_fixtureInstalledParked` 4, `lock (FieldLock)` 5, `CompareExchange(` 2, `return new EnsureScope(` 1, `leaks exactly` 0, `A scope that installed nothing` 0, `pins for the process lifetime` 2, `install-ownership flag` 1, `installed nothing carries` 0)
- FAT (P3-T9): 0, 0, 8, 8, 1, 1, 1, 17 (`EnsureUiThreadDispatcher` 0, `private static Mock<IItemViewer> BuildExecutingViewer` 0, `QfcItemControllerTestSupport.BuildExecutingViewer()` 8, `BuildExecutingViewer` 8, `absorbs the delegate without running it` 1, `shared UiThread static is irrelevant` 1, `absorbs the queued application` 1, `[TestMethod]` 17)
- TS (P3-T9 extended list): 0, 0, 0, 0, 1, 1, 1, 1, 1 (`Becomes moot` 0, `leaks exactly` 0, `still delegate to a callee` 0, `not reachable from another test file` 0, `remaining legitimate` 1, `QfcItemController_UiThreadDispatcherPinCountTests` 1, `internal static void EnsureSynchronizationContext()` 1, `UiThreadDispatcherFixture.EnsureDispatcher();` 1, `Issue #480 shared arrange helper` 1)
- FT (P3-T9): 0, 1, 1, 8, 1, 3, 1, 1, 8 (`no other class may dispose` 0, `removed that pin: the fixture now counts pins` 1, `(W5) must not latch` 1, `[Timeout(GateTimeoutMs)]` 8, `private const int GateTimeoutMs = 60000;` 1, `EnsureUiThreadDispatcher()` 3, `issue #230 lost update` 1, `the waiter cannot observe the pre-restore value` 1, `[TestMethod]` 8)
- PC (P1-T3): 10, 1, 3, 1, 1, 4, 4, 1, 3, 1, 4, 1, 2, 2, 0, 0, 0, 1, 1 (`EnsureUiThreadDispatcher()` 10, `Regression test: fails before the fix` 1, `Specification test: passes before and after the fix` 3, `never read the shared static` 1, `[TestClass]` 1, `[TestMethod]` 4, `[Timeout(GateTimeoutMs)]` 4, `private const int GateTimeoutMs = 60000;` 1, `transaction.Install(null);` 3, `transaction.Install(live);` 1, `transaction.Dispose();` 4, `QfcItemControllerTestSupport.ShutdownDispatcher(live);` 1, `a holder that did not take the last pin must not lose the dispatcher` 2, `the last release reverts the fixture` 2, `using Moq;` 0, `Thread.Sleep` 0, `Task.Delay` 0, `public class QfcItemController_UiThreadDispatcherPinCountTests` 1, `foreignTransaction.Install(parked);` 1)
- PROJ (P1-T3 list): `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` 1, `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` 1
- PROJ (P0-T13 list, as last recorded at P5-T5): `<Compile Include=` 190 (PROJ-COMPILE-ITEMS-BASE 187 plus 3), `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` 1, `TestSupport\SynchronousBackgroundWorker.cs` 1, `TestSupport\ArmingFakeTimeProvider.cs` 1, `TestSupport\DedicatedWorkerThread.cs` 1, `TestSupport\WinFormsPumpHostTests.cs` 1
- LIV (P5-T5): 0, 2, 2, 4, 4, 4, 0, 0, 2, 1, 3, 0, 1, 4 (`class SynchronousBackgroundWorker` 0, `StartSynchronously` 2, `SynchronousBackgroundWorker.StartSynchronously` 2, `new SynchronousBackgroundWorker()` 4, `using (var worker = new SynchronousBackgroundWorker())` 4, `StartHeldOpenLoader(` 4, `Task.Yield` 0, `fake.Advance` 0, `FakeTimeProvider` 2, `using QuickFiler.Test.TestSupport;` 1, `NoSynchronizationContext` 3, `Duplicated per file` 0, `new ArmingFakeTimeProvider()` 1, `[TestMethod]` 4)
- TD (P4-T9): 0, 1, 1, 1, 0, 1, 5
- ZB (P4-T9): 0, 3, 3, 3, 3, 0, 0, 0, 0, 0, 1, 3 (`class SynchronousBackgroundWorker` 0, `StartSynchronously` 3, `SynchronousBackgroundWorker.StartSynchronously` 3, `new SynchronousBackgroundWorker()` 3, `using (var worker = new SynchronousBackgroundWorker())` 3, `InitEmailQueue(0, new` 0, `InitEmailQueue(2, new` 0, `Duplicated per file` 0, `through the nested` 0, `starting a real` 0, `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 3)
- DMT (P5-T5): 2, 2, 1, 1, 1, 1, 1, 0, 2, 6, 1, 9 (`new BackgroundWorker()` 2, `using (var worker = new BackgroundWorker())` 2, `new ArmingFakeTimeProvider()` 1, `clock.ReArm();` 1, `await Task.WhenAny(clock.Armed, pending)` 1, `must keep polling while the worker can still add candidates` 1, `the gate re-armed instead of returning` 1, `await Task.Yield();` 0, `fake.Advance` 2, `FakeTimeProvider` 6, `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 9)
- QDM (P4-T9): 0, 0, 1, 0, 2, 0, 0, 1, 0, 0, 6, 6, 1, 0, 0, 0, 0, 1, 2, 2, 3 (`Worker_RunWorkerCompleted` 0, `nameof(LoadRemainingEmailsToQueue)` 0, `nameof(LoadRemainingEmailsToQueueAsync)} Error.` 1, `nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled` 0, `LoadRemainingEmailsToQueueAsync(` 2, `LoadRemainingEmailsToQueue(BackgroundWorker bw` 0, `log4net.ILog log =` 0, `log4net.ILog logger =` 1, `Linked List Locking` 0, `#pragma` 0, `#region` 6, `#endregion` 6, `[ExcludeFromCodeCoverage]` 1, `//e.Result =` 0, `//_blockingQueue = null;` 0, `//worker.RunWorkerCompleted` 0, `ForEachAwaitWithCancellationAsync` 0, `: IQfcDatamodel` 1, `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` 2, `WorkerStarter(worker);` 2, `_remainingLoadActive = ` 3)
- QQP (P5-T12): 0, 0, 1, 0, 3, 1, 1, 0, 1, 0, 1 (`RunWorkerAsync` 0, `written on the worker thread and read` 0, `written on the worker thread` 1, `(:31-66)` 0, `TryUnhookOrReplace` 3, `WorkerStarter` 1, `share no other fence` 1, `honest producer-liveness signal` 0, `() => _remainingLoadActive,` 1, `() => false,` 0, `private volatile bool _remainingLoadActive;` 1)
- SBW (P4-T9): `class SynchronousBackgroundWorker` 1, `internal sealed class SynchronousBackgroundWorker : BackgroundWorker` 1, `internal static void StartSynchronously(BackgroundWorker worker)` 1, `Dispose` 1, `namespace QuickFiler.Test.TestSupport` 1
- AFTP (P5-T5): `internal sealed class ArmingFakeTimeProvider : FakeTimeProvider` 1, `internal Task Armed` 1, `internal void ReArm()` 1, `public override ITimer CreateTimer(` 1, `base.CreateTimer(` 1, `RunContinuationsAsynchronously` 1, `_armed.TrySetResult(true);` 1, `namespace QuickFiler.Test.TestSupport` 1

## CMD-SPAN-TOKEN-COUNT

- ENSURE (FIX): SPAN: 143-167; `_pinCount++` 1, `_fixtureInstalledParked = true;` 1, `lock (FieldLock)` 1, `return new EnsureScope(` 1
- SCOPE (FIX): SPAN: 273-316; `CompareExchange(` 0, `lock (FieldLock)` 1, `_pinCount--` 1, `_fixtureInstalledParked = false;` 1, `DispatcherField.SetValue(null, null);` 1
- R4SPAN (FT): SPAN: 212-286; `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3, `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1
- R4HEAD (FT): SPAN: 212-222; `EnsureUiThreadDispatcher()` 0, `using (` 0, `try` 2
- R4TAIL (FT): SPAN: 265-275; `}` 4, `finally` 2, `transactionA.Dispose();` 1
- T1-LIVE (LIV): SPAN: 102-173 (was 102-169 at P5-T5; LIV is named in REWRITTEN); `await` 3, `using (NoSynchronizationContext())` 2, `Task.Yield` 0, `fake.Advance` 0, `for (int i` 0, `clock.ReArm();` 1, `(await pending)` 1
- HELD (LIV): SPAN: 211-245 (was 207-241; LIV is named in REWRITTEN); `new SynchronousBackgroundWorker()` 0, `SynchronousBackgroundWorker worker,` 1, `SynchronousBackgroundWorker.StartSynchronously` 1
- T-SIB (DMT): SPAN: 103-154 (was 103-152; DMT is named in REWRITTEN); `await Task.Yield();` 0, `clock.ReArm();` 1, `await Task.WhenAny(clock.Armed, pending)` 1, `using (var worker = new BackgroundWorker())` 1, `IList<MailItem> result = await pending;` 1
- GATE-LAMBDA (QQP): SPAN: 299-310; `() => _remainingLoadActive,` 1, `() => false,` 0 (the sensitivity edit is not present)

## CMD-HUNKS (anchored to BASE)

- TestSupport: GIT_DIFF_EXIT_CODE: 0; `@@ -214,25 +214,27 @@`; `@@ -282,9 +284,9 @@`; HUNK_COUNT: 2 (every old-range start at or above 200)
- Fixture tests: GIT_DIFF_EXIT_CODE: 0; `@@ -194,17 +194,17 @@`; `@@ -218,9 +218,7 @@`; `@@ -268,6 +266,10 @@`; HUNK_COUNT: 3 (every hunk inside R4's doc and body: old ranges 194-210, 218-226, 268-273)
- QfcDatamodel.cs: GIT_DIFF_EXIT_CODE: 0; HUNK_COUNT: 7 (recorded, not gated; unchanged from P4-T9)
- QfcDatamodel.QueueProcessing.cs: GIT_DIFF_EXIT_CODE: 0; `@@ -13,13 +13,13 @@`; `@@ -282,7 +282,7 @@`; HUNK_COUNT: 2

## CMD-PIN-NESTING on T3SPAN (PC)

SPAN: 134-183

- NEST 141 [BeginTransactionAsync()] .BeginTransactionAsync()
- NEST 145 [.Install(] transaction.Install(live);
- NEST 146 [EnsureUiThreadDispatcher()] IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 147 [EnsureUiThreadDispatcher()] IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
- NEST 150 [Dispose()] pinA.Dispose();
- NEST 151 [Dispose()] pinB.Dispose();
- NEST 163 [finally] finally
- NEST 165 [Dispose()] transaction.Dispose();
- NEST 168 [finally] finally
- NEST 170 [ShutdownDispatcher(] QfcItemControllerTestSupport.ShutdownDispatcher(live);

The NEST output ends with four lines whose tokens are, in order, `finally`, `Dispose()` (the `transaction.Dispose();` line), `finally`, `ShutdownDispatcher(`: the AC3 reading that the live dispatcher is shut down in a finally block.

## numstat (git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test, exit 0)

- `141	101	QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
- `2	17	QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
- `70	47	QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
- `43	49	QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
- `20	35	QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
- `20	18	QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
- `48	15	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (equal to the P2-T6 row)
- `16	14	QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `3	0	QuickFiler.Test/QuickFiler.Test.csproj` (the project-file row in place of the P1-T3 value)
- `8	8	QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
- `1	129	QuickFiler/Controllers/QfcDatamodel.cs` (equal to the P4-T9 row)

## FIELDLOCK-ENCLOSURE (restated against the formatted diff)

The fixture's formatted diff against HEAD (`git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, exit 0) is identical to the diff transcribed in FEATURE/evidence/qa-gates/fixture-change-census.md (the file's hash and numstat row are unchanged by P6-T1). Both new fields are private statics of `UiThreadDispatcherFixture`; the three non-declaration occurrences of each lie inside the two `lock (FieldLock)` blocks, the one in `EnsureDispatcher` (`_pinCount++;`, `_fixtureInstalledParked = true;`) and the one in `EnsureScope.Dispose` (`_pinCount--;`, `_pinCount == 0`, `&& _fixtureInstalledParked`, `_fixtureInstalledParked = false;`); the scope class contains no `CompareExchange` call and writes null inline in the same critical section as the decrement.

## Porcelain (git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test, exit 0): exactly the fourteen Write Set code paths, eleven ` M` and three `??`

- ` M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
- ` M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
- ` M QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
- ` M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
- ` M QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
- ` M QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
- ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
- ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- ` M QuickFiler.Test/QuickFiler.Test.csproj`
- ` M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
- ` M QuickFiler/Controllers/QfcDatamodel.cs`
- `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`
- `?? QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`
- `?? QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`

This artifact is the evidence for AC6, AC7, AC9, AC11, AC12, AC13, AC15, AC16, AC17, AC25, AC26, AC30 and the census half of AC3, AC10, AC14, AC21, AC27, AC29 and AC31.
