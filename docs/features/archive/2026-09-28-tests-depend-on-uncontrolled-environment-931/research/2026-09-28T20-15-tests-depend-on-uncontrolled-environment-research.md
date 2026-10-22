# Research: tests depend on uncontrolled environment (issue #931)

- Issue: #931 (consolidates #905 and #906)
- Work mode: full-bug; run `bugs-2026-09-28`
- Branch: `bug/tests-depend-on-uncontrolled-environment-931`
- Inputs read: `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/issue.md`, `spec.md`, `plan.2026-09-28T20-01.md`; `docs/features/potential/promoted/2026-09-28-tests-depend-on-uncontrolled-environment.md`; the #900 feature folder evidence under `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/`.
- Evidence basis: every file and line cited below was read from the current tree of this worktree with `Read`/`Grep`. No command was executed; no test was run. Line numbers are those of the current tree, which differ from the issue text in places (noted where relevant).

## 0. Summary of findings

1. The #900 pattern is present at `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:228-238` (in-thread precondition) and `:385-403` (`RunOnDedicatedWorkerThread`).
2. `QuickFiler.Test` contains exactly 22 `Task.Run` call sites (derived twice, see the numeric section). Exactly **2 are AFFECTED**: `BreadcrumbPopupBoundaryCoverageTests.cs:58` and `ItemViewerBreadcrumbThreadAffinityTests.cs:332`. The other 6 cited candidates are UNAFFECTED because the guard they exercise decides by ambient `SynchronizationContext` reference identity or by an executing-callback marker, never by thread identity; 14 further sites are TCS completions or thread-agnostic.
3. `FileInfoWrapper` already has a seam: `internal FileInfoWrapper(IFileInfo)` at `UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs:21-24`, reachable through `InternalsVisibleTo("UtilitiesCS.Test")` at `UtilitiesCS/Properties/AssemblyInfo.cs:19`. No production change is required.
4. A `MemoryStream` cannot replace the `OpenRead()` fixture because `IFileInfo.OpenRead()` returns the concrete `FileStream` type (`UtilitiesCS/Interfaces/IHelperClasses/IFileInfo.cs:26`). The test-owned stream must be a `FileStream`; the repository's established handle-free-of-contention shape is a read-only, `FileShare.ReadWrite` open of the test's own loaded assembly (already used in the same file at lines 189-224). The three property/cast/`ToString` tests need no handle at all.
5. Parallel regime: `TaskMaster.runsettings:4-7` and `scripts/vscode/TaskMaster.cli.runsettings:4-7` both set `Workers=0`, `Scope=ClassLevel`. None of the seven candidate classes nor `FileInfoWrapper_Tests` carries `[DoNotParallelize]`.

---

## A. Distinct-thread defect (issue #905 portion)

### A1. The canonical #900 / PR #904 shape (verified in the current tree)

Helper, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:385-403`:

```csharp
private static Exception RunOnDedicatedWorkerThread(Action action)
{
    Exception captured = null;
    var thread = new Thread(() =>
    {
        try
        {
            action();
        }
        catch (Exception error)
        {
            captured = error;
        }
    });
    thread.IsBackground = true;
    thread.Start();
    thread.Join();
    return captured;
}
```

In-thread precondition placed before the guarded call, `:228-238` (the second instance is identical at `:277-291`):

```csharp
Exception captured = RunOnDedicatedWorkerThread(() =>
{
    bool isOwnerThread = scope.Viewer.UiDispatcher.CheckAccess();
    isOwnerThread
        .Should()
        .BeFalse(
            "the dedicated worker thread must not be the thread that constructed "
                + "the viewer, or the boundary assertion would pass vacuously"
        );
    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
});
```

Shape: `new Thread(...)`, `IsBackground = true`, `Start()`, untimed `Join()`, exception marshalled back by field, precondition asserted inside the delegate, no apartment state set. The XML remarks at `:203-217` and `:377-384` record the rationale (a `Task.Run` work item queued from a pool thread lands on that thread's local queue and a blocking wait can run it inline; a constructed `Thread` is distinct from every live thread by construction; the `Join()` parks no pool slot waiting on another pool slot).

Other dedicated-thread helpers exist in the project but serve different purposes and are not the pattern: `Controllers/QfcItemController.TestSupport.cs:251-271` (`StartRunningDispatcher`, STA + `Dispatcher.Run()`), `Controllers/QfcItemController.UiThreadDispatcherFixture.cs:175-203` (parked STA dispatcher), `Controllers/BayesianPerformanceController.TestSupport.cs:16-53` (STA viewer host), `TestSupport/WinFormsPumpHost.cs:53` (WinForms pump), `Helper Classes/EmailMoveMonitorTests.cs:281-287` (marshal-target thread inside a test delegate).

### A2. Complete enumeration of `Task.Run` call sites in `QuickFiler.Test` (22 sites)

Guard semantics that decide the classification (all verified in `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`):

- `Dispatch(Action)` (`:71-151`) calls `IsCurrentBoundary()` (`:255-278`). With a captured context (`_context != null`) the boundary is `ReferenceEquals(SynchronizationContext.Current, _context)` (`:269-272`); **thread identity is never consulted**. Only when `_context == null` (owner-only test dispatcher from `CreateForCurrentThreadTests()` `:62-65`, or the private 3-argument constructor with a null context) does the guard compare `Environment.CurrentManagedThreadId` with `_ownerThreadId` (`:276-277`).
- `DispatchValue<T>` (`:157-235`) never calls `IsCurrentBoundary()`. It runs inline only when `ReferenceEquals(_executingDispatcher, this)` (`:166`); with `_context == null` it faults with "cannot marshal cross-thread UI work" for **every** other caller, on any thread (`:180-188`); otherwise it posts.
- `BreadcrumbBridgeCoordinator`'s public 2-argument constructor captures a context-backed dispatcher (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:39-43`); inbound messages go `OnMessageReceived` -> `ObserveInboundAsync` -> `_dispatcher.Dispatch(...)` (`:273-315`).
- `ItemViewer.ThrowIfOffUiBoundary` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:432-447`): returns when `UiDispatcher == null` (`:435-438`), else throws when `!owning.CheckAccess()` (`:440-446`). `_uiDispatcher = Dispatcher.CurrentDispatcher` is captured at `QuickFiler/Viewers/ItemViewer.cs:27`. WPF `CheckAccess()` is `Thread` object identity.
- `BreadcrumbDropDownHost.Close` (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:247-263`), `Reset` (`:275-280`) and `Dispose` (`:283-290`) contain no thread guard; they schedule through the open lifetime, which dispatches through the context-backed operations.
- `EmailMoveMonitor.UnhookItem` marshals unconditionally through `_marshalToSta` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:67-76`).

| # | Site (repo-relative, `QuickFiler.Test/`) | Containing test method | Verdict | Reason |
|---|---|---|---|---|
| 1 | `Helper Classes/EmailMoveMonitorTests.cs:298` | `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread` | UNAFFECTED | The asserted property (`recordedBodyThreadId != callingThreadId`, `:308-314`) is guaranteed by the fresh `new Thread` inside the marshal delegate (`:281-287`), which is distinct from every live thread including an inlined caller; `UnhookItem` marshals unconditionally. Class carries a pre-existing `[DoNotParallelize]` at `:24` (leave as is). |
| 2 | `Viewers/BreadcrumbCoordinatorLifecycleTests.cs:350` | `QueuedCompletion_DisposedBeforeOwnerDrain_DoesNotPublish` | UNAFFECTED | Completes a `TaskCompletionSource` (`gate.SetResult(key)`); continuations run asynchronously (`RunContinuationsAsynchronously`). |
| 3 | `Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:58` | `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` | **AFFECTED** | Owner-only dispatcher (`CreateOwnerOnlyDispatcher`, `:124-134`, null context, owner id = test thread). `Dispatch` reaches the thread-id branch (`BreadcrumbUiDispatcher.cs:276-277`). The blocking `GetAwaiter().GetResult()` at `:59` can inline the work item on the test thread, in which case the action runs inline, `executions == 1`, `errors` empty, and the test fails spuriously (`:60-61`). Outcome is decided by scheduling. |
| 4 | `Viewers/BreadcrumbPopupControlDispatchTests.cs:29` | `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` | UNAFFECTED | Operations built with a context-backed dispatcher (`:311`); every stage goes through `RunAsync` -> `DispatchValue`, which posts unless inside an executing callback and never reads thread identity. The sibling test at `:53` calls `CreateSurface` directly on the test thread and asserts the same stage sequence. No blocking wait at the start (`Drain` waits on a monitor, `:249-279`). |
| 5 | `Viewers/BreadcrumbPopupControlDispatchTests.cs:111` | `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment` | UNAFFECTED | `readiness.Dispose()` -> `Cancel` -> `dispatcher.Dispatch(detachHandlers)` (`BreadcrumbPopupUiOperations.cs:418`); with a captured context the guard compares ambient context by reference; the body sets ambient null itself (`:113`), so the post occurs on any thread. `fixture.Drain(disposing)` is not a task wait, so the body is not inlined and the unrestored null context lands on a pool thread only. |
| 6 | `Viewers/BreadcrumbPopupControlDispatchTests.cs:300` | helper `CompleteOnWorker` (used by test 1 at `:31,33`) | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| 7 | `Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192` | `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` | UNAFFECTED | `captured` is a `CaptureCurrent()` dispatcher whose `_context` is the `PumpSynchronizationContext`; `PostAsync` -> `Dispatch` compares ambient context by reference. On the test thread the ambient context is the restored previous one (`WithContext`, `:136-148`), never `context`, so the post happens on any thread; `PostCount == 1` (`:197`) pins the posted path. |
| 8 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:61` | `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary` | UNAFFECTED | Completes a `TaskCompletionSource`. |
| 9 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:75` | same as 8 | UNAFFECTED | Coordinator built via the public constructor (context-backed). `messenger.Receive` -> inbound `Dispatch(HandleSelectorMessage)` posts whenever ambient context is not the captured one; the body forces ambient null through `InvokeAmbientNull` (`:325-337`). `context.WaitForPost()` (`:76`) precedes the blocking `GetResult()` (`:77`) and is a semaphore wait, so the body has already run on another pool thread before any wait-inlining is possible. |
| 10 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:137` | `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary` | UNAFFECTED | Completes a `TaskCompletionSource` (factory completion). |
| 11 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:148` | same as 10 | UNAFFECTED | Completes a `TaskCompletionSource` (readiness). |
| 12 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:154` | same as 10 | UNAFFECTED | `host.Close(...)` has no thread guard (`BreadcrumbDropDownHost.cs:247-263`); the test asserts that callbacks and drained work ran on the creator thread (`:164-169`), which the drain enforces; the origin thread of `Close` is not asserted. |
| 13 | `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:161` | same as 10 | UNAFFECTED | `host.Reset` has no thread guard (`:275-280`); same reasoning as 12. |
| 14 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs:37` | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` | UNAFFECTED | Completes a `TaskCompletionSource` (`SetException`). |
| 15 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs:210` | `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity` | UNAFFECTED | Completes a `TaskCompletionSource`. |
| 16 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs:218` | same as 15 | UNAFFECTED | `host.Reset` has no thread guard; assertions are about operation counts after drain (`:222-229`). |
| 17 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs:219` | same as 15 | UNAFFECTED | `host.Dispose` has no thread guard (`:283-290`). |
| 18 | `Viewers/BreadcrumbUiThreadDispatchTests.cs:51` | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` | UNAFFECTED | Completes a `TaskCompletionSource`; awaited, so no wait-inlining. |
| 19 | `Viewers/BreadcrumbUiThreadDispatchTests.cs:90` | `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` | UNAFFECTED | Awaited (no wait-inlining). Coordinator is context-backed; `Dispatch` posts whenever ambient context is not the captured one; on the test thread the ambient context is the restored `previous` (`:86`), so the outcome is identical on any thread. |
| 20 | `Viewers/BreadcrumbUiThreadDispatchTests.cs:301` | `ProductionCaptureWithoutUiContext_FailsFast` | UNAFFECTED | `DispatchValue` on an owner-only dispatcher (`CreateForCurrentThreadTests()`, `:298-299`) faults for every caller that is not inside an executing dispatcher callback (`BreadcrumbUiDispatcher.cs:166-188`); it never reads `_ownerThreadId`. The expected `InvalidOperationException` is produced on the owner thread as well as on any other thread. This corrects the second citation of Entry 1 in `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md:40-42`, which attributed this site to the owner-thread-id check. The message text "cross-thread" is broader than the mechanism; a wording-only follow-up, not a fix under #931. |
| 21 | `Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:332` | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | **AFFECTED** | The blocking `GetAwaiter().GetResult()` at `:335-336` can inline the work item onto the owner thread. The remark at `:311-317` claims the test discriminates against the pre-#781 context-reference guard; that holds only when the call is genuinely off the owner thread (on the inlined branch the ambient context equals the captured one and the pre-fix guard would admit the call). Issue #931 lists this site as remaining; the #900 handoff records the same conditionality (Entry 2, `:63-94`). |
| 22 | `Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:222` | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | UNAFFECTED | Asserts the value observed by a second gate acquirer (`observedByB`, `:244-254`), not any thread identity; `secondCallerStarted.Wait()` (`:239`) is a `ManualResetEventSlim` wait, not a task wait, so the body runs on a separate pool thread. Its intermittency is tracked under #823 (`:197-202`) for unrelated reasons. |

Result: AFFECTED = {3, 21}; UNAFFECTED = the remaining 20. Of the 8 sites the issue named as candidates, 6 are UNAFFECTED (4, 5, 7, 9, 19, 20).

### A3. AFFECTED sites: guard, in-thread assertion, apartment state

**Site 3, `BreadcrumbPopupBoundaryCoverageTests.cs:58`**

- Guard under test: `BreadcrumbUiDispatcher.IsCurrentBoundary()` owner-thread branch (`BreadcrumbUiDispatcher.cs:274-277`), reached from `Dispatch` (`:78`) with `_context == null`; the rejection path reports "cannot marshal" and returns without running the action (`:97-105`).
- In-thread assertion: the dispatcher exposes no `CheckAccess`. Capture `int ownerThreadId = Environment.CurrentManagedThreadId;` in Arrange (this is the value `CreateOwnerOnlyDispatcher` passes at `:133`) and assert inside the delegate, before `dispatcher.Dispatch(...)`: `Environment.CurrentManagedThreadId.Should().NotBe(ownerThreadId, "...")`. Optionally also `ReferenceEquals(Thread.CurrentThread, ownerThread).Should().BeFalse()`.
- Apartment state: none required. No control is touched; a null-context dispatcher never posts. Use the same background MTA thread shape as the canonical helper.
- After the rewrite the existing assertions (`executions == 0`, one error containing "cannot marshal") stay unchanged.

**Site 21, `ItemViewerBreadcrumbThreadAffinityTests.cs:332`**

- Guard under test: the null-owner escape of `ThrowIfOffUiBoundary` (`ItemViewer.Breadcrumb.cs:434-438`), entered from `InitializeBreadcrumbPipeline` (`:51`); the call then takes the already-initialized early return (`:60-72`).
- In-thread assertion: `scope.Viewer.UiDispatcher` is null after `ClearViewerDispatcher` (`:361-371`), so capture the owner first: `Dispatcher owner = scope.Viewer.UiDispatcher;` before `ClearViewerDispatcher(scope.Viewer);`, then inside the delegate assert `owner.CheckAccess().Should().BeFalse("...")` before the guarded call. This is the same `Thread`-identity proof used at `:230-236`.
- Apartment state: none required. The call path touches no control (early return); the two existing dedicated-thread tests call the same member without STA.
- The `Action act` / `NotThrow` shape can be preserved by asserting `RunOnDedicatedWorkerThread(...)` returned null, plus `BreadcrumbCoordinator.Should().BeSameAs(before)`.

### A4. Demonstrating failure against a deliberately broken guard

Precedent: #900 recorded two reverted, test-only mutations with before/after census and TRX-derived summaries (`.../evidence/regression-testing/p3-t1-mutation-guard-disabled.2026-09-17T02-24.md`, `p3-t3-mutation-inline-precondition.2026-09-17T02-25.md`), each followed by a revert record. The same evidence discipline applies here: record the mutated file's line count and token census, build, run the scoped test with a `/TestCaseFilter`, record the TRX-derived summary, revert with `git checkout -- <file>`, and prove the revert with `git status --porcelain` showing no residual change to that file.

**Site 3**

- Test seam or fake: none. `BreadcrumbUiDispatcher` is `sealed`, `IsCurrentBoundary` is private, and every argument combination of the reflected 3-argument constructor yields a correct guard (null owner and null context -> reports; a different owner id -> a different owner, not a broken check).
- M1 (temporary production edit, reverted in the same task): replace `BreadcrumbUiDispatcher.cs:276-277` with `return true;`. `Dispatch` then runs the action inline on the worker thread: `executions == 1` and `errors` is empty, so the rewritten test fails at `executions.Should().Be(0)`. Expected failure text: "Expected executions to be 0, but found 1".
- M2 (test-only, reverted): insert `action();` as the first statement of the dedicated-thread helper, exactly as #900 P3-T3 did. The in-thread precondition fails ("Expected ... not to be <owner id>"), proving the precondition is live.
- Fallback without any production edit: construct the owner-only dispatcher inside the dedicated thread (owner = worker). `Dispatch` then runs inline and the same assertions fail. This models a wrong owner rather than a broken check, so M1 is preferred when a temporary production edit is acceptable.
- Feasibility: M1 and M2 both feasible; M1 requires the temporary production edit the delegation permits.

**Site 21**

- Test seam or fake: none for the escape itself. `ClearViewerDispatcher` (reflection on `_uiDispatcher`) is the existing seam that *selects* the escape; it cannot disable it. #900's P3-T1 mutation (inserting `ClearViewerDispatcher` into the throw-tests) is not applicable here because the escape is the branch under test.
- M1 (temporary production edit, reverted): at `ItemViewer.Breadcrumb.cs:435-438` replace the bare `return;` with the pre-#781 context-reference check (`if (!ReferenceEquals(SynchronizationContext.Current, UiSyncContext)) throw new InvalidOperationException(...)`). On the dedicated thread the ambient context is null and differs from `UiSyncContext`, so the guard throws and the rewritten test fails at `NotThrow`. On the owner thread (the inlined branch of the old shape) the ambient context equals `scope.Context`, so the old test would have passed; this is the discrimination the rewrite restores and the remark at `:311-317` can then state unconditionally.
- M1-alt: delete the null check so `owning.CheckAccess()` dereferences null. Fails on any thread with `NullReferenceException`; proves the escape is reached but not the thread dependence. Use only as a supplement.
- M2 (test-only, reverted): inline `action();` in the helper; `owner.CheckAccess()` is then true and the `BeFalse` precondition fails with the "vacuously" reason text, as in P3-T3.
- Feasibility: M1, M1-alt and M2 all feasible.

### A5. `ItemViewerBreadcrumbThreadAffinityTests.cs`: structure, split, line counts, csproj

Current file: **490 lines**, one `[TestClass] public sealed class ItemViewerBreadcrumbThreadAffinityTests` (`:29-30`), namespace `QuickFiler.Test.Viewers`.

| Lines | Member | Grouping |
|---|---|---|
| 39-75 | `InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext` | owner-thread admission |
| 89-122 | `InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow` | owner-thread admission |
| 129-160 | `InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow` | owner-thread admission |
| 168-197 | `ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow` | owner-thread admission |
| 219-251 | `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` | cross-thread (dedicated thread) |
| 269-304 | `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` | cross-thread (dedicated thread) |
| 319-346 | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | cross-thread (site 21, to be rewritten) |
| 352-355 | `InertOperations()` | shared helper |
| 361-371 | `ClearViewerDispatcher()` | used only by the null-owner test |
| 385-403 | `RunOnDedicatedWorkerThread()` | used only by the cross-thread tests |
| 406-434 | `InertDropDownHost` (nested) | used by tests at 176 and 274 |
| 442-461 | `DrainableSynchronizationContext` (nested) | used by `InertOperations` |
| 467-488 | `ViewerScope` (nested) | used by all tests except the first |

Proposed split (repository convention for continuation partials: `*.Part2.cs`, e.g. `BreadcrumbPopupBoundaryCoverageTests.Part2.cs:17,23`, and 16 further `.Part2.cs` entries in `QuickFiler.Test.csproj`):

- Make the class `partial`. Primary file `Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` keeps the four owner-thread admission tests (lines 1-197), `InertOperations` (348-355), and the three nested types (405-488). Estimated **about 290 lines**.
- New file `Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` holds the three cross-thread tests (199-346) and `ClearViewerDispatcher` (357-371). With the rewritten null-owner test (about +12 lines) and a file header (about 16 lines) the estimate is **about 200 lines**; about 230 if `RunOnDedicatedWorkerThread` stays in it. Private nested types and private static helpers remain accessible across partial declarations, so no accessibility is widened.
- `RunOnDedicatedWorkerThread` is needed by site 3 in another class. Recommended: move it to a new internal static helper `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (namespace `QuickFiler.Test.TestSupport`, matching `TestSupport/WinFormsPumpHost.cs:9`), keeping the same contract (`Exception Run(Action)`), and optionally adding a built-in generic precondition (`ReferenceEquals(Thread.CurrentThread, caller)` must be false) in addition to the site-specific precondition each test keeps in its delegate. Estimated 40-60 lines.

`QuickFiler.Test.csproj` uses explicit `<Compile Include>` entries. Existing entry for the file: `QuickFiler.Test/QuickFiler.Test.csproj:98` (`<Compile Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" />`). Neighbouring entries for the other candidate files: `:77-79` (`BreadcrumbUiThreadDispatchTests.cs`, `BreadcrumbSelectorToggleUiBoundaryTests.cs`, `BreadcrumbPopupControlDispatchTests.cs`), `:105-106` (`BreadcrumbPopupBoundaryCoverageTests.cs`, `.Part2.cs`); `TestSupport` entries at `:227-228`.

Line counts of every file the rewrite would edit:

| File | Now | After (estimate) | Under 500 |
|---|---|---|---|
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | 490 | about 290 | yes |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` (new) | 0 | about 200-230 | yes |
| `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (new) | 0 | about 40-60 | yes |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | about 375 (site 3 rewrite) | yes |
| `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | 359 | about 355-370 (`GetSolutionFile` removed, seam test added) | yes |
| `QuickFiler.Test/QuickFiler.Test.csproj` | build file | +2 `<Compile Include>` entries | not subject to the code-file limit |

Not edited (for reference): `BreadcrumbPopupControlDispatchTests.cs` 486, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` 483, `BreadcrumbSelectorToggleUiBoundaryTests.cs` 478, `BreadcrumbUiThreadDispatchTests.cs` 480. Each of these has fewer than 25 lines of headroom, which is a further reason not to classify their sites as affected without necessity and not to add helpers to them.

---

## B. File-handle defect (issue #906 portion)

### B6. The `TaskMaster.sln` fixture in `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`

Test (current lines 55-67; the issue cites 56-62):

```csharp
[TestMethod]
public void OpenRead_ShouldReturnReadableStreamForWrappedFile()
{
    // Arrange
    var wrapper = new FileInfoWrapper(GetSolutionFile());

    // Act
    using var stream = wrapper.OpenRead();

    // Assert
    stream.CanRead.Should().BeTrue();
    stream.Length.Should().BeGreaterThan(0);
}
```

Helper (current lines 339-357; the issue cites 340-352):

```csharp
private static FileInfo GetSolutionFile()
{
    var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

    while (current is not null)
    {
        var solutionPath = Path.Combine(current.FullName, "TaskMaster.sln");
        if (File.Exists(solutionPath))
        {
            return new FileInfo(solutionPath);
        }

        current = current.Parent;
    }

    throw new InvalidOperationException(
        "The TaskMaster solution file could not be located from the test assembly path."
    );
}
```

Tests in this file that use `GetSolutionFile()` (all through the public `FileInfoWrapper(FileInfo)` constructor):

| Lines | Test | Call site | Real-file dependency |
|---|---|---|---|
| 26-39 | `Properties_ShouldMirrorWrappedFileInfo` | `:29` | Reads `Exists`, `FullName`, `Name`, `Extension`, `DirectoryName`, `Directory.FullName` of `TaskMaster.sln` (metadata only, no handle). |
| 42-53 | `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory` | `:45` | Path-derived only. |
| 55-67 | `OpenRead_ShouldReturnReadableStreamForWrappedFile` | `:59` | **Opens a read handle on `TaskMaster.sln`** (`FileInfo.OpenRead()` requests `FileShare.Read`); this is the #906 failure (IOException observed once in the #900 run: `p5-t14-follow-up-handoff...md:157-164`). |
| 70-81 | `ToString_ShouldDelegateToWrappedFileInfo` | `:73` | Path-derived only. |

Other real-file use in the same file: `StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo` (184-295) opens the test's own loaded assembly six times (`typeof(FileInfoWrapper_Tests).Assembly.Location`, `FileMode.Open`, `FileAccess.Read`, `FileShare.ReadWrite`; lines 189-224) purely as identity sentinels returned by a strict `Mock<IFileInfo>`. This is a build output the test host already holds mapped, opened with a sharing mode that admits every other read or write holder; it is not a repository source file and it is not the #906 defect. The same shape is the repository's stated precedent (`UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs:247-248, 258-275`).

Out of #931 scope but the same defect class (record as follow-up potential entries, do not fix here): `PhysicalFileSystemAdapters_Tests.cs:173` and `:318` (`GetSolutionFile()`), `:186-198` (a `catch (IOException)` that swallows contention), `:213-234` (read opens on the real `.sln`); `DirectoryInfoWrapper_Tests.cs:60, 79, 381` (asserts that `TaskMaster.sln` is enumerated from the repository root).

### B7. Existing seam in production (no production change required)

- `UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs:14-24`: public `FileInfoWrapper(FileInfo)` wraps a `PhysicalFileInfoAdapter`; `internal FileInfoWrapper(IFileInfo fileInfo)` (`:21-24`) is the seam. Every member of the wrapper delegates to `_fileInfo` (`:26-209`); `OpenRead()` is `:153-156`.
- Visibility: `UtilitiesCS/Properties/AssemblyInfo.cs:19` `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` (also `DynamicProxyGenAssembly2` at `:18`, so Moq can proxy internal members).
- The seam is already exercised in this file at `:139`, `:271`, `:322` (`new FileInfoWrapper(fileInfo.Object)`), including `OpenRead` delegation at `:263` and `:287` (`wrapper.OpenRead().Should().BeSameAs(openReadStream)`).
- Second, narrower seam one level down: `PhysicalFileInfoAdapter`'s internal delegate constructor (`UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs:34-48`) seams `AppendText`, `Open(FileMode)`, `Open(FileMode, FileAccess)` and `OpenWrite` only (`:118, :146-149, :158`); `OpenRead()` is unseamed (`:154`). Therefore `OpenRead` cannot be exercised through the public `FileInfoWrapper(FileInfo)` path without a real file; through the `IFileInfo` seam it can.
- Prior art searched: `PhysicalFileInfoAdapter` (production, above, and `PhysicalFileSystemAdapters_Tests.cs:170-311`), "sentinel" (`PhysicalFileSystemAdapters_Tests.cs:244-282`, `Bootstrap/AssemblyBindingFallbackTests.cs:28-29`), injectable `Func<>` seams (`UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:106-133`).

Conclusion: the issue's condition "add a seam to the wrapper only if one does not already exist" resolves to **no seam addition**.

### B8. Replacement design and what each rewritten test verifies

Type constraint: `IFileInfo.OpenRead()`, `Open(...)`, `Create()`, `OpenWrite()` return the concrete `FileStream` (`IFileInfo.cs:16, 23-26, 28`). A `MemoryStream` is not a `FileStream`, so an in-memory stream cannot be returned through the seam for these members. `MemoryStream` is usable only behind `StreamReader`/`StreamWriter` members (`AppendText`, `CreateText`, `OpenText`, `:13, 17, 27`), which the existing test already does at `:186-188`. A pipe-backed `FileStream` is rejected as an option because it is non-seekable and the test asserts `Length`.

Recommended rewrite (all four tests keep their names and scenario intent; `GetSolutionFile()` and the `AppDomain` walk-up are deleted):

1. `Properties_ShouldMirrorWrappedFileInfo`, `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory`, `ToString_ShouldDelegateToWrappedFileInfo`: construct `new FileInfo(@"C:\Repo\fixture.sln")` (style matches the rooted literals already used at `:87, :113`). `FileInfo` construction, `FullName`, `Name`, `Extension`, `DirectoryName`, `Directory`, and `ToString()` are path computations with no handle; `Exists` is a metadata query with no handle, and the assertion mirrors `file.Exists` so it is deterministic either way. Do not assert `Length` on this fixture (it throws for a missing file). What is verified: the public constructor path `FileInfoWrapper(FileInfo)` -> `PhysicalFileInfoAdapter` (`FileInfoWrapper.cs:14-19`; adapter `:25-32, 68-108, 181`) and the explicit `DirectoryInfoWrapper` cast (`:211-214`), which the mocked tests cannot reach because a `Mock<IDirectoryInfo>` is not a `DirectoryInfoWrapper`. This keeps coverage of `FileInfoWrapper.cs:18` and the adapter's property lines without any file handle.
2. `OpenRead_ShouldReturnReadableStreamForWrappedFile`: construct the wrapper through the seam with a strict `Mock<IFileInfo>` whose `OpenRead()` returns a test-owned `FileStream` opened on `typeof(FileInfoWrapper_Tests).Assembly.Location` with `FileMode.Open`, `FileAccess.Read`, `FileShare.ReadWrite` (the pattern at `:189-224` and `PhysicalFileSystemAdapters_Tests.cs:258-275`), disposed by `using`. Assert `wrapper.OpenRead().Should().BeSameAs(sentinel)`, `CanRead` true and `Length > 0`. What is verified: the wrapper's own behaviour is delegation (`FileInfoWrapper.cs:153-156`); identity of the returned instance is the complete contract, and the readability assertions confirm the returned object is the live stream. No repository source file and no temporary file is involved. This test then overlaps `:263/:287`; deleting it instead would lose no production coverage (line 155 stays covered) and is an acceptable alternative if the reviewer prefers fewer tests, but the rewrite preserves the named scenario the issue cites.

Rejected alternatives: temporary files (prohibited by policy); adding an `OpenRead` delegate to `PhysicalFileInfoAdapter` (a production change the issue forbids when a seam exists); asserting through `PhysicalFileInfoAdapter` directly (that adapter's own tests are out of scope).

---

## C. Toolchain facts

### C9. Project files

- `QuickFiler.Test/QuickFiler.Test.csproj` (explicit `<Compile Include>` model) gains two entries: `Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` (adjacent to `:98`) and `TestSupport\DedicatedWorkerThread.cs` (adjacent to `:227-228`). Files present on disk but absent from the csproj are not compiled, so a missing entry fails silently at the test-count level; the executor should confirm the new test names appear in the TRX-derived summary.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` also uses explicit entries (`:232-234` list `DirectoryInfoWrapper_Tests.cs`, `PhysicalFileSystemAdapters_Tests.cs`, `FileInfoWrapper_Tests.cs`). The recommended rewrite creates no new file there, so no entry is needed.

### C10. Parallel regime and `[DoNotParallelize]`

- `TaskMaster.runsettings:3-8`: `<Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize>` (plus a Code Coverage collector configuration at `:9-29`). `scripts/vscode/TaskMaster.cli.runsettings:3-8` carries the same `Workers=0`, `Scope=ClassLevel` without the collector (this is the file the #900 evidence ran under).
- `[DoNotParallelize]` today (grep over `*.cs`): none on `ItemViewerBreadcrumbThreadAffinityTests`, `BreadcrumbPopupBoundaryCoverageTests` (either partial), `BreadcrumbPopupControlDispatchTests`, `BreadcrumbSelectorToggleUiBoundaryTests`, `BreadcrumbUiThreadDispatchTests`, `BreadcrumbSelectorOpenRetryTests`, `BreadcrumbCoordinatorLifecycleTests`, `QfcItemController.UiThreadDispatcherFixtureTests`, or `FileInfoWrapper_Tests`. In `QuickFiler.Test` it appears only on `Helper Classes/EmailMoveMonitorTests.cs:24` and `Helper Classes/ViewerQueueStaticWrapperTests.cs:11` (both unaffected by this issue; leave untouched). The fix must add none.

---

## D. Candidate approaches and recommendation

**Approach 1 (recommended): dedicated-thread helper shared through `TestSupport`, partial split of the affinity file, seam-only rewrite of the file tests.**
Advantages: one implementation of the #900 helper reused at both affected sites (no copy-paste); the affinity file lands near 290 lines with headroom; no production code changes anywhere; matches the issue's stated fix and existing `.Part2.cs`/`TestSupport` conventions. Limitations: two new files and two csproj entries.

**Approach 2 (rejected): duplicate the private helper into `BreadcrumbPopupBoundaryCoverageTests.cs` and split the affinity file.** Rejected for copy-paste of a 19-line helper whose rationale comment is the load-bearing part.

**Approach 3 (rejected): move the helper out and skip the split.** The primary file would land near 475 lines, below the "with headroom" bar and contrary to the issue's stated fix.

**Approach 4 (rejected): rewrite all eight cited candidate sites.** Six of them do not exercise a thread-identity guard (A2); rewriting them adds dedicated threads to files with fewer than 25 lines of headroom for no discriminating benefit.

## E. Behaviour semantics and requirements mapping

- AC "apply the #900 pattern at every triaged site": sites 3 and 21 only; record the UNAFFECTED verdicts for the six other candidates with the reasons in A2 so the reviewer can see the triage.
- AC "split and register": Part2 partial plus `TestSupport/DedicatedWorkerThread.cs`, both registered (C9).
- AC "replace the `.sln` fixture; add a seam only if none exists": B7/B8; no production edit.
- AC "each rewritten test shown to fail against a deliberately broken guard": A4 gives M1/M2 per site, each reverted in the same task with census and `git status --porcelain` evidence, never committed. For the file test the analogous demonstration is: point the mock's `OpenRead()` at a second sentinel (or make it throw) and observe `BeSameAs` fail, then restore.
- AC "run the full `QuickFiler.Test` and `UtilitiesCS.Test` suites in the parallel regime": run under `/Settings:TaskMaster.runsettings` (or the CLI twin) with `/InIsolation`, summarise from the TRX; no `Workers=1`, `[DoNotParallelize]`, or retry is permitted.
- Files changed: `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (make partial, remove cross-thread tests and helpers), new `...ThreadAffinityTests.Part2.cs`, new `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:52-62`, `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs:25-81, 339-357`. No file under `QuickFiler/` or `UtilitiesCS/` changes.

## F. Testing implications

- The rewritten tests are deterministic under `Workers=0`/`ClassLevel`: a constructed `Thread` is distinct from every live thread; the in-thread precondition makes an inlined or same-thread execution fail loudly instead of passing vacuously (site 21) or failing spuriously (site 3).
- No wall-clock waits are introduced: `Thread.Join()` is a completion wait on one bounded synchronous call (rationale at `ItemViewerBreadcrumbThreadAffinityTests.cs:377-384`).
- Coverage: production lines touched by these tests are unchanged in count; `FileInfoWrapper.cs:18, 44-78, 153-156, 211-214` remain covered by the rewritten tests. New test-support code is excluded from the production denominator.
- Evidence to commit: TRX-derived summaries for the mutation runs and the final parallel suite runs, the JaCoCo projection and one-line coverage summary; never the raw TRX or Cobertura documents (CLAUDE.md "Committed Test Evidence Format").
- Follow-ups to promote as potential entries, not fixed here: the `.sln` usages in `PhysicalFileSystemAdapters_Tests.cs` and `DirectoryInfoWrapper_Tests.cs` (B6); the "cross-thread" wording at `BreadcrumbUiThreadDispatchTests.cs:305` (A2 row 20); the mischaracterised second citation in the #900 handoff Entry 1.

## Numeric Derivation Evidence

- Complete Family: EmailMoveMonitorTests.cs:298, BreadcrumbCoordinatorLifecycleTests.cs:350, BreadcrumbPopupBoundaryCoverageTests.cs:58, BreadcrumbPopupControlDispatchTests.cs:29, BreadcrumbPopupControlDispatchTests.cs:111, BreadcrumbPopupControlDispatchTests.cs:300, BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192, BreadcrumbSelectorToggleUiBoundaryTests.cs:61, BreadcrumbSelectorToggleUiBoundaryTests.cs:75, BreadcrumbSelectorToggleUiBoundaryTests.cs:137, BreadcrumbSelectorToggleUiBoundaryTests.cs:148, BreadcrumbSelectorToggleUiBoundaryTests.cs:154, BreadcrumbSelectorToggleUiBoundaryTests.cs:161, BreadcrumbSelectorOpenRetryTests.cs:37, BreadcrumbSelectorOpenRetryTests.cs:210, BreadcrumbSelectorOpenRetryTests.cs:218, BreadcrumbSelectorOpenRetryTests.cs:219, BreadcrumbUiThreadDispatchTests.cs:51, BreadcrumbUiThreadDispatchTests.cs:90, BreadcrumbUiThreadDispatchTests.cs:301, ItemViewerBreadcrumbThreadAffinityTests.cs:332, QfcItemController.UiThreadDispatcherFixtureTests.cs:222
- Exhaustive Search Scope: the entire `QuickFiler.Test` source tree of the repository (every file under `QuickFiler.Test/`, all subdirectories: `Controllers/`, `Helper Classes/`, `TestSupport/`, `Viewers/`, and any other), searched with ripgrep from the directory root with no path filter beyond the tree itself; both strategies were run over this whole tree.
- Inclusion Rules: a line of C# code in which the identifier `Task.Run` is invoked (followed by `(` or a generic argument list), whether the result is awaited, blocked on, stored, or discarded, and whether inside a test method or a helper; every overload of `Task.Run` (`Action`, `Func<Task>`, `Func<TResult>`, `Func<Task<TResult>>`, with or without a `CancellationToken`) is in the family.
- Exclusion Rules: (a) occurrences inside XML documentation or `//` comments (`ItemViewerBreadcrumbThreadAffinityTests.cs:205, 260, 378`; `QfcItemController.FolderHandlingTests.Part2.cs:315`; `QfcItemController.EventHandlersTests.cs:261`; `QfcItemControllerTests.cs:62`); (b) occurrences inside string literals (`QfcItemController.FolderHandlingTests.Part2.cs:351`); (c) substring matches on other identifiers (`flagTask.Run(` at `QfcItemController.EventHandlersTests.cs:261`, `TimeOutTask.RunWithTimeout` at `QfcItemControllerTests.cs:62`); (d) sibling APIs that are not `Task.Run` (`Task.Factory.StartNew`, `ThreadPool.QueueUserWorkItem`, `ThreadPool.UnsafeQueueUserWorkItem`: a grep over the same tree returned zero matches, so no sibling site exists to exclude).
- Primary Search Strategy: ripgrep regex `Task\.Run` over every file under `QuickFiler.Test/` (no glob, no type filter, unlimited results), returning 29 matching lines, then applying exclusion rules (a)-(c) by reading each line; retained members: EmailMoveMonitorTests.cs:298, BreadcrumbCoordinatorLifecycleTests.cs:350, BreadcrumbPopupBoundaryCoverageTests.cs:58, BreadcrumbPopupControlDispatchTests.cs:29, BreadcrumbPopupControlDispatchTests.cs:111, BreadcrumbPopupControlDispatchTests.cs:300, BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192, BreadcrumbSelectorToggleUiBoundaryTests.cs:61, BreadcrumbSelectorToggleUiBoundaryTests.cs:75, BreadcrumbSelectorToggleUiBoundaryTests.cs:137, BreadcrumbSelectorToggleUiBoundaryTests.cs:148, BreadcrumbSelectorToggleUiBoundaryTests.cs:154, BreadcrumbSelectorToggleUiBoundaryTests.cs:161, BreadcrumbSelectorOpenRetryTests.cs:37, BreadcrumbSelectorOpenRetryTests.cs:210, BreadcrumbSelectorOpenRetryTests.cs:218, BreadcrumbSelectorOpenRetryTests.cs:219, BreadcrumbUiThreadDispatchTests.cs:51, BreadcrumbUiThreadDispatchTests.cs:90, BreadcrumbUiThreadDispatchTests.cs:301, ItemViewerBreadcrumbThreadAffinityTests.cs:332, QfcItemController.UiThreadDispatcherFixtureTests.cs:222 (7 lines excluded: 205, 260, 378 of ItemViewerBreadcrumbThreadAffinityTests.cs; 315, 351 of QfcItemController.FolderHandlingTests.Part2.cs; 261 of QfcItemController.EventHandlersTests.cs; 62 of QfcItemControllerTests.cs)
- Primary Member Set: EmailMoveMonitorTests.cs:298, BreadcrumbCoordinatorLifecycleTests.cs:350, BreadcrumbPopupBoundaryCoverageTests.cs:58, BreadcrumbPopupControlDispatchTests.cs:29, BreadcrumbPopupControlDispatchTests.cs:111, BreadcrumbPopupControlDispatchTests.cs:300, BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192, BreadcrumbSelectorToggleUiBoundaryTests.cs:61, BreadcrumbSelectorToggleUiBoundaryTests.cs:75, BreadcrumbSelectorToggleUiBoundaryTests.cs:137, BreadcrumbSelectorToggleUiBoundaryTests.cs:148, BreadcrumbSelectorToggleUiBoundaryTests.cs:154, BreadcrumbSelectorToggleUiBoundaryTests.cs:161, BreadcrumbSelectorOpenRetryTests.cs:37, BreadcrumbSelectorOpenRetryTests.cs:210, BreadcrumbSelectorOpenRetryTests.cs:218, BreadcrumbSelectorOpenRetryTests.cs:219, BreadcrumbUiThreadDispatchTests.cs:51, BreadcrumbUiThreadDispatchTests.cs:90, BreadcrumbUiThreadDispatchTests.cs:301, ItemViewerBreadcrumbThreadAffinityTests.cs:332, QfcItemController.UiThreadDispatcherFixtureTests.cs:222
- Primary Count: 22
- Cross-check Search Strategy: ripgrep regex `(^|[^A-Za-z0-9_.])Task\.Run\s*[(<]` with glob `*.cs` over every file under `QuickFiler.Test/` (unlimited results; the leading character class rejects identifier-suffix matches such as `flagTask.Run(` and the trailing class requires an invocation or generic list, which also rejects `TimeOutTask.RunWithTimeout`), returning 27 matching lines, then applying exclusion rules (a)-(b) by reading each line; retained members: EmailMoveMonitorTests.cs:298, BreadcrumbCoordinatorLifecycleTests.cs:350, BreadcrumbPopupBoundaryCoverageTests.cs:58, BreadcrumbPopupControlDispatchTests.cs:29, BreadcrumbPopupControlDispatchTests.cs:111, BreadcrumbPopupControlDispatchTests.cs:300, BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192, BreadcrumbSelectorToggleUiBoundaryTests.cs:61, BreadcrumbSelectorToggleUiBoundaryTests.cs:75, BreadcrumbSelectorToggleUiBoundaryTests.cs:137, BreadcrumbSelectorToggleUiBoundaryTests.cs:148, BreadcrumbSelectorToggleUiBoundaryTests.cs:154, BreadcrumbSelectorToggleUiBoundaryTests.cs:161, BreadcrumbSelectorOpenRetryTests.cs:37, BreadcrumbSelectorOpenRetryTests.cs:210, BreadcrumbSelectorOpenRetryTests.cs:218, BreadcrumbSelectorOpenRetryTests.cs:219, BreadcrumbUiThreadDispatchTests.cs:51, BreadcrumbUiThreadDispatchTests.cs:90, BreadcrumbUiThreadDispatchTests.cs:301, ItemViewerBreadcrumbThreadAffinityTests.cs:332, QfcItemController.UiThreadDispatcherFixtureTests.cs:222 (5 lines excluded: 205, 260, 378 of ItemViewerBreadcrumbThreadAffinityTests.cs, matched through the `<` of `</c>` in XML remarks; 315, 351 of QfcItemController.FolderHandlingTests.Part2.cs)
- Cross-check Member Set: EmailMoveMonitorTests.cs:298, BreadcrumbCoordinatorLifecycleTests.cs:350, BreadcrumbPopupBoundaryCoverageTests.cs:58, BreadcrumbPopupControlDispatchTests.cs:29, BreadcrumbPopupControlDispatchTests.cs:111, BreadcrumbPopupControlDispatchTests.cs:300, BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192, BreadcrumbSelectorToggleUiBoundaryTests.cs:61, BreadcrumbSelectorToggleUiBoundaryTests.cs:75, BreadcrumbSelectorToggleUiBoundaryTests.cs:137, BreadcrumbSelectorToggleUiBoundaryTests.cs:148, BreadcrumbSelectorToggleUiBoundaryTests.cs:154, BreadcrumbSelectorToggleUiBoundaryTests.cs:161, BreadcrumbSelectorOpenRetryTests.cs:37, BreadcrumbSelectorOpenRetryTests.cs:210, BreadcrumbSelectorOpenRetryTests.cs:218, BreadcrumbSelectorOpenRetryTests.cs:219, BreadcrumbUiThreadDispatchTests.cs:51, BreadcrumbUiThreadDispatchTests.cs:90, BreadcrumbUiThreadDispatchTests.cs:301, ItemViewerBreadcrumbThreadAffinityTests.cs:332, QfcItemController.UiThreadDispatcherFixtureTests.cs:222
- Cross-check Count: 22
- Member-set Comparison: the two sets are identical (22 members each, same files and line numbers, no member present in one set and absent from the other); the two strategies differ in their regular expression, their file filter, and the number of raw lines they returned (29 versus 27) and converge on the same member set after the declared exclusions, so the family count of 22 and the AFFECTED subset count of 2 (BreadcrumbPopupBoundaryCoverageTests.cs:58, ItemViewerBreadcrumbThreadAffinityTests.cs:332) are asserted.
