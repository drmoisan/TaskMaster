# 2026-09-28-tests-depend-on-uncontrolled-environment (Spec)

- **Issue:** #931 (consolidates #905 and #906)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-28T20-45
- **Status:** Ready for atomic planning
- **Version:** 1.0
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source for this feature. No user-story.md exists or is required.
- **Research record:** docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md (authoritative for site triage, line numbers, seam analysis and the split). Every line citation below was re-verified against the current tree of this worktree on 2026-09-28.

> Formatting contract for the blast-radius scheduler: the only backtick-delimited repository paths in this document are the entries under `## Write Set`. Every other file reference, including the unaffected triaged sites, the production files cited for the guards, the runsettings file, the sibling defect sites, and the research and evidence artifacts, is written in plain prose without backticks. Backticked tokens elsewhere are C# identifiers, attribute names, or the mandated toolchain command strings from CLAUDE.md (which contain spaces and are not harvestable). Do not "fix" this formatting.

## Context

Two consolidated defects share one root cause: a unit test depends on environment state it does not control, so its outcome is decided by the scheduler or by other processes rather than by the code under test.

**Distinct-thread defect (from #905).** Tests use `Task.Run` to obtain "another thread". `Task.Run` guarantees a thread-pool work item, not a different thread from the caller. When the test itself runs on a thread-pool thread, which is the case for every test under the parallel run configured in TaskMaster.runsettings (Workers zero, Scope ClassLevel), a blocking wait on the work item's task can execute the work item inline on the waiting thread. A guard that decides by thread identity is then either never exercised (it admits the call) or reports a result the test did not intend. PR #904 (issue #900) replaced two such sites in QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs with a dedicated, joined `Thread` and an in-thread distinctness precondition. The issue listed one remaining site (line 332 of that file) and six candidate sites to triage: BreadcrumbSelectorToggleUiBoundaryTests.cs line 75; BreadcrumbPopupControlDispatchTests.cs lines 29 and 111; BreadcrumbPopupBoundaryCoverageTests.cs line 58; BreadcrumbPopupBoundaryCoverageTests.Part2.cs line 192; BreadcrumbUiThreadDispatchTests.cs lines 90 and 301 (all under QuickFiler.Test/Viewers). The research enumerated every `Task.Run` call site in QuickFiler.Test (22 sites; derivation in the research record's Numeric Derivation Evidence section) and classified exactly two as AFFECTED: BreadcrumbPopupBoundaryCoverageTests.cs line 58 and ItemViewerBreadcrumbThreadAffinityTests.cs line 332. The remaining 20, including the other six candidates, either only complete a `TaskCompletionSource` or exercise a guard that decides by `SynchronizationContext` reference identity or by an executing-callback marker, so the identity of the calling thread cannot change their outcome.

**File-handle defect (from #906).** Four tests in UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs (lines 25 to 81) obtain the repository's own TaskMaster.sln through a private `GetSolutionFile()` helper (lines 339 to 357) that walks up from `AppDomain.CurrentDomain.BaseDirectory`. One of them, `OpenRead_ShouldReturnReadableStreamForWrappedFile` (lines 55 to 67), opens a read handle on that file with the default `FileShare.Read` sharing mode of `FileInfo.OpenRead()`. Any other process that holds the solution file with a share mode that excludes readers (resident MSBuild node-reuse workers, an IDE, a hook) makes the open throw `IOException`. The outcome depends on build history, not on the wrapper. An `IOException` from exactly this test was recorded once during the #900 run.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C#, MSTest, net48; Moq and FluentAssertions
- Command/flags used: parallel regime through the TaskMaster.runsettings file at the repository root (Workers zero, Scope ClassLevel); the CLI twin under scripts/vscode carries the same two values
- Data source or fixture: files listed above, main at 177b6d78e

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce (static, verified on 2026-09-28 in this worktree):
1. Open QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs at lines 52 to 62. `CreateOwnerOnlyDispatcher` (lines 124 to 134) builds a `BreadcrumbUiDispatcher` with a null context and the test thread's managed id as owner. The test queues `dispatcher.Dispatch(...)` through `Task.Run` (line 58) and blocks on it (line 59), then asserts the action did not run and one "cannot marshal" error was reported. If the work item is inlined onto the test thread, `IsCurrentBoundary()` (QuickFiler/Viewers/BreadcrumbUiDispatcher.cs lines 255 to 278; the thread-id branch is lines 276 to 277) returns true, the action runs inline, `executions` is one and no error is reported: the test fails spuriously.
2. Open QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs at lines 319 to 346. The test nulls the viewer's owning dispatcher through `ClearViewerDispatcher` (lines 361 to 371), then re-enters `InitializeBreadcrumbPipeline` from a `Task.Run` work item (line 332) with a blocking wait (lines 335 to 336). If the work item is inlined onto the owner thread, the call is on-thread and the pre-#781 context-reference guard the remark at lines 311 to 317 claims to discriminate against would also have admitted it: the test passes vacuously. The null-owner escape under test is at QuickFiler/Viewers/ItemViewer.Breadcrumb.cs lines 434 to 438.
3. Open UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs at lines 55 to 67 and 339 to 357 and confirm `GetSolutionFile()` resolves the repository's TaskMaster.sln and that `OpenRead()` opens it.

Expected:
- A test that needs a distinct thread uses a dedicated `Thread` that is started, joined, and asserts inside its own delegate that it is not the owner thread (for example `CheckAccess()` is false, or `Environment.CurrentManagedThreadId` differs from the captured owner id) before it exercises the guard. This is the pattern PR #904 introduced at lines 228 to 238 and 385 to 403 of ItemViewerBreadcrumbThreadAffinityTests.cs.
- A file-handle test uses a stream the test itself owns, supplied through the wrapper's existing `IFileInfo` seam. It never opens a repository-tracked file and never creates a temporary file.

Actual:
- Both thread-affinity guards can go unexercised (or produce a spurious result) depending on where the thread pool runs the work item; the file-open test can pass or fail depending on which other processes hold TaskMaster.sln.

Logs / Screenshots:
- Static findings, verified present on 2026-09-28. The single observed `IOException` for the file test is recorded in the #900 follow-up handoff under docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/ (plain reference, not modified by this feature).

## Scope & Non-Goals

- In scope:
  - Rewrite the two AFFECTED `Task.Run` sites (`Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` and `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`) to the dedicated-thread pattern with an in-thread distinctness assertion.
  - Move the private `RunOnDedicatedWorkerThread` helper into a shared internal test-support type so both affected classes use one implementation.
  - Split ItemViewerBreadcrumbThreadAffinityTests.cs (490 total lines today) into a partial class across the existing file and a new Part2 file, and register the new files in the QuickFiler.Test project file.
  - Remove `GetSolutionFile()` and every TaskMaster.sln dependency from FileInfoWrapper_Tests.cs; route the `OpenRead` scenario through the existing internal `FileInfoWrapper(IFileInfo)` seam with a test-owned `FileStream`; convert the three metadata-only tests to a rooted literal `FileInfo` that needs no handle.
  - Demonstrate, with committed Markdown projections, that each rewritten guard test fails against a deliberately broken guard and passes after the mutation is reverted.
  - Run both affected suites in the parallel regime and the full C# toolchain; commit projections only.
- Out of scope / non-goals (paths in this list are deliberately unbackticked; none of them is modified):
  - The 20 UNAFFECTED `Task.Run` sites in QuickFiler.Test enumerated in the Triage table below. They are not rewritten, not reformatted, and not annotated.
  - Any production file. No file under QuickFiler/ or UtilitiesCS/ changes; the seam already exists (UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs lines 21 to 24, reachable through the `InternalsVisibleTo("UtilitiesCS.Test")` attribute at UtilitiesCS/Properties/AssemblyInfo.cs line 19).
  - TaskMaster.runsettings and scripts/vscode/TaskMaster.cli.runsettings. Workers and Scope stay as they are.
  - The two pre-existing `[DoNotParallelize]` attributes in QuickFiler.Test (Helper Classes/EmailMoveMonitorTests.cs line 24 and Helper Classes/ViewerQueueStaticWrapperTests.cs line 11). They are unrelated to this issue and stay untouched.
  - Same-class defects in UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs (lines 173 and 318 call its own `GetSolutionFile()`; lines 186 to 198 swallow `IOException`; lines 213 to 234 open the real solution file) and UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs (lines 60, 79 and 381 assert that TaskMaster.sln is enumerated from the repository root). Recorded as follow-ups in Rollout & Follow-up.
  - The message wording "cannot marshal cross-thread UI work" asserted at QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs line 305 (broader than the mechanism; wording-only follow-up).
  - The .claude directory and the two JSON files under config.
- Explicitly excluded systems, integrations, or datasets: Outlook, VSTO, and any live UI pump. Neither rewritten test touches a control.

## Root Cause Analysis

**Distinct-thread defect.** `Task.Run` queues a work item to the thread pool. When the queuing thread is itself a pool thread, the item lands on that thread's local queue. A blocking wait (`GetAwaiter().GetResult()`) on a not-yet-started task attempts inline execution through the task scheduler, and the thread-pool scheduler permits inlining when the waiting thread is a pool thread. Under the parallel regime every test method runs on a pool thread, so the "other thread" can be the calling thread. A guard that decides by thread identity (`Dispatcher.CheckAccess()`, which is `Thread` object identity; or the `_ownerThreadId` comparison in `BreadcrumbUiDispatcher.IsCurrentBoundary()`) then sees the owner and admits the call. A dedicated `Thread` object constructed by the test is distinct from every live thread by construction, so an in-thread precondition asserted on it holds under any scheduler, and an untimed `Join()` on it parks no pool slot waiting on another pool slot.

The six other candidate sites do not share the defect because the guards they reach do not consult thread identity: `Dispatch` with a captured context compares `SynchronizationContext.Current` to the captured context by reference (BreadcrumbUiDispatcher.cs lines 269 to 272); `DispatchValue` never calls `IsCurrentBoundary()` and faults for every caller outside an executing callback when the context is null; `BreadcrumbDropDownHost.Close`, `Reset` and `Dispose` carry no thread guard; and `TaskCompletionSource` completions have no thread-dependent outcome. Full per-site reasoning is in the Triage table.

**File-handle defect.** The test reaches for a file that exists in every checkout, the solution file, because the public `FileInfoWrapper(FileInfo)` constructor wraps a `PhysicalFileInfoAdapter` whose `OpenRead()` is not seamed (UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs line 154), so a real file is the only way to exercise `OpenRead` through the public constructor. `FileInfo.OpenRead()` requests `FileShare.Read`, so any concurrent holder that denies shared reads makes the open fail. The internal `FileInfoWrapper(IFileInfo)` constructor removes the need for a real file for the wrapper's own contract, which is pure delegation (FileInfoWrapper.cs lines 153 to 156 for `OpenRead`). The three metadata tests never needed a handle: `FullName`, `Name`, `Extension`, `DirectoryName`, `Directory`, `ToString()` are path computations and `Exists` is a metadata query.

Both patterns were copied from earlier tests and survived because they usually pass. They must not be fixed by serialising the run: Workers one, `[DoNotParallelize]`, retries, sleeps and timeouts are prohibited as fixes for this issue.

## Proposed Fix

### Design summary (what changes where)

1. **Shared dedicated-thread helper.** New internal static class `DedicatedWorkerThread` in the QuickFiler.Test TestSupport folder (namespace `QuickFiler.Test.TestSupport`, matching the existing WinFormsPumpHost.cs). One member, `internal static Exception Run(Action action)`, with the identical body and XML remarks of the private `RunOnDedicatedWorkerThread` at ItemViewerBreadcrumbThreadAffinityTests.cs lines 373 to 403: construct a `Thread`, set `IsBackground = true`, `Start()`, untimed `Join()`, return the exception captured by the delegate or null. The helper adds no assertion of its own: each test states its site-specific distinctness precondition inside its delegate so that a failure names the guard under test. The helper contains no `Thread.Sleep`, `Task.Delay`, timeout, or retry. Estimated 40 to 60 total lines.
2. **Affinity file split.** `ItemViewerBreadcrumbThreadAffinityTests` becomes `partial`. The existing file keeps the four owner-thread admission tests (current lines 39 to 197), `InertOperations` (lines 348 to 355) and the three nested types `InertDropDownHost`, `DrainableSynchronizationContext`, `ViewerScope` (lines 405 to 488). The new Part2 file holds the three cross-thread tests (current lines 199 to 346) and `ClearViewerDispatcher` (lines 357 to 371). `RunOnDedicatedWorkerThread` is deleted from the class; the three cross-thread tests call `DedicatedWorkerThread.Run`. Private nested types and private static helpers remain visible across partial declarations, so no accessibility is widened. Estimated totals: about 290 lines for the existing file and 200 to 230 for the Part2 file, both under the 500 total-line ceiling.
3. **Site rewrite, `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (Part2 file).** Before `ClearViewerDispatcher(scope.Viewer)`, capture `Dispatcher owner = scope.Viewer.UiDispatcher;` (requires a `using System.Windows.Threading;` directive in the Part2 file). Replace the `Task.Run` block with `Exception captured = DedicatedWorkerThread.Run(() => { ... })` whose delegate first asserts `owner.CheckAccess().Should().BeFalse(reason)` and then calls `scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations)`. Assert `captured.Should().BeNull(reason)` and `scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before)` unchanged. Rewrite the remark at current lines 311 to 317 so it states unconditionally that the call is off the owner thread and therefore discriminates against the pre-#781 context-reference guard.
4. **Site rewrite, `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (BreadcrumbPopupBoundaryCoverageTests.cs, current lines 52 to 62).** Capture `int ownerThreadId = Environment.CurrentManagedThreadId;` in Arrange (this is the value `CreateOwnerOnlyDispatcher` passes as owner). Replace the `Task.Run` and its blocking wait with `Exception captured = DedicatedWorkerThread.Run(() => { ... })` whose delegate first asserts `Environment.CurrentManagedThreadId.Should().NotBe(ownerThreadId, reason)` and then calls `dispatcher.Dispatch(() => executions++)`. The rejection path reports and returns `Task.CompletedTask` synchronously (BreadcrumbUiDispatcher.cs lines 97 to 105), so no task wait is needed. Assert `captured.Should().BeNull()`, then the two existing assertions unchanged: `executions.Should().Be(0)` and a single error whose message contains "cannot marshal". The file is 361 total lines today and stays under the ceiling. No apartment state is set: no control is touched and a null-context dispatcher never posts.
5. **File tests (FileInfoWrapper_Tests.cs).** Delete `GetSolutionFile()` and the `AppDomain` walk-up. The four tests keep their names and scenario intent:
   - `Properties_ShouldMirrorWrappedFileInfo`, `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory`, `ToString_ShouldDelegateToWrappedFileInfo` construct a `FileInfo` over the rooted literal C:\Repo\fixture.sln (the rooted-literal style already used at lines 87 and 113 of the same file). The path is not expected to exist and does not point into the repository. The assertions stay as they are (`Exists` mirrors `file.Exists`, so it is deterministic either way; `Extension` is ".sln"; `FullName`, `Name`, `DirectoryName`, `Directory.FullName`, the explicit `DirectoryInfoWrapper` cast, and `ToString()` are path computations). `Length` is not asserted on this fixture because it throws for a missing file. These three tests are what keeps the public constructor path (FileInfoWrapper.cs lines 14 to 19 into `PhysicalFileInfoAdapter`) and the explicit cast operator covered; the mocked tests cannot reach them because a `Mock<IDirectoryInfo>` is not a `DirectoryInfoWrapper`.
   - `OpenRead_ShouldReturnReadableStreamForWrappedFile` constructs the wrapper through the internal seam with a strict `Mock<IFileInfo>` whose `OpenRead()` returns a test-owned `FileStream` sentinel, and asserts `wrapper.OpenRead().Should().BeSameAs(sentinel)`, `CanRead` is true, and `Length` is greater than zero. The wrapper's contract for `OpenRead` is delegation, so instance identity is the complete contract; the two readability assertions confirm the returned object is the live stream the test opened.

**Chosen stream design and justification (binding operator constraint).** The sentinel is `new FileStream(typeof(FileInfoWrapper_Tests).Assembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)`, disposed by a `using` declaration in the test.
- It is test-owned: the test constructs it, holds the only reference, passes it to a mock it also owns, and disposes it. Nothing outside the test method touches it.
- It is not a repository-tracked file: the test assembly is a build output under the test project's bin directory, which git ignores. It is not TaskMaster.sln or any source file.
- It is not a temporary file: the test creates nothing, writes nothing, and deletes nothing. The file pre-exists as the running test host's own loaded image and remains after the test. The unit-test policy's prohibition on temporary files is satisfied by construction, not by cleanup.
- It cannot be denied by another process's handle in any observed mode: the open requests read access only and `FileShare.ReadWrite`, so every concurrent reader and writer is admitted, and the running host's image mapping keeps the file in existence for the whole run. This exact open shape already executes six times in the same file (lines 189 to 224) and in PhysicalFileSystemAdapters_Tests.cs lines 258 to 275 with no recorded contention incident.
- Why not `MemoryStream`: `IFileInfo.OpenRead()` returns the concrete `FileStream` type (UtilitiesCS/Interfaces/IHelperClasses/IFileInfo.cs line 26), as do `Create()`, `Open(...)` and `OpenWrite()`; a `MemoryStream` cannot be returned through the seam for these members. `MemoryStream` remains usable only behind the `StreamReader` and `StreamWriter` members, which the existing test at lines 186 to 188 already does.
- Handle-free alternative evaluated and rejected: `FileStream` on net48 needs a path or a `SafeFileHandle`. The only file-less handle sources available are anonymous pipes and device pseudo-files; a `FileStream` over either is non-seekable, so `Length` throws `NotSupportedException`, and it is still an OS handle rather than "no file". Dropping the `Length` assertion to admit a pipe would weaken the scenario the issue names. The assembly-location `FileStream` is therefore the chosen design.

### Boundaries and invariants to preserve

- The parallel regime is unchanged: Workers zero, Scope ClassLevel. No `[DoNotParallelize]`, no `Workers` change, no retry attribute or loop, no `Thread.Sleep`, `Task.Delay`, timeout or wall-clock wait is introduced anywhere in the diff.
- No production file changes. The negative-control mutations in Test Strategy are applied and reverted inside the same task; the final diff contains no change under QuickFiler/ or UtilitiesCS/.
- Every existing test name in the three edited test files survives; no test is deleted or renamed.
- The two rewritten guard tests keep their original observable assertions and add only the in-thread precondition and the `captured` null check.
- The helper's contract is identical to the #900 private helper so the two existing worker-thread tests behave exactly as before.

### Dependencies or blocked work

- None. The seam exists in production; the split and the helper are test-project-only.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change
See `## Write Set`. Three existing test files are modified, two test files are created, one test project file gains two Compile entries, and three evidence directories are created under the feature folder.

#### Functions/classes/CLI commands impacted
- `ItemViewerBreadcrumbThreadAffinityTests` (becomes `partial`; loses `RunOnDedicatedWorkerThread`; three tests and `ClearViewerDispatcher` relocate to the Part2 file).
- `BreadcrumbPopupBoundaryCoverageTests.Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (rewritten body).
- `FileInfoWrapper_Tests` (four test bodies rewritten; `GetSolutionFile` removed).
- New `QuickFiler.Test.TestSupport.DedicatedWorkerThread.Run(Action)`.
- No CLI command changes.

#### Data flow and validation changes
None in production. In tests, the distinctness precondition moves from "assumed by `Task.Run`" to "asserted inside the dedicated thread".

#### Error handling and logging updates
None.

#### Rollback/feature-flag considerations (if applicable)
Not applicable; test-only change. Reverting the commit restores the prior tests.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats
- `DedicatedWorkerThread.Run(Action action)`: runs `action` on a new background thread, joins it, returns the `Exception` the delegate threw or null. Same contract as the removed private helper.

#### Required configuration keys and defaults
None.

#### Backward-compatibility expectations
No public API changes. `DedicatedWorkerThread` is `internal` to QuickFiler.Test.

#### Performance constraints (latency/throughput/memory)
Each rewritten test starts and joins exactly one thread; the join is a completion wait on one bounded synchronous call. No measurable suite-time change is expected.

## Triage of every Task.Run site in QuickFiler.Test

Verdicts and reasons follow the research record section A2 and were re-verified against the current tree. File paths are project-relative under QuickFiler.Test and deliberately unbackticked; only the two AFFECTED rows are edited.

| Site (file:line) | Containing test or helper | Verdict | Reason |
|---|---|---|---|
| Helper Classes/EmailMoveMonitorTests.cs:298 | `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread` | UNAFFECTED | The asserted property is guaranteed by a fresh `new Thread` inside the marshal delegate, distinct from every live thread including an inlined caller; `UnhookItem` marshals unconditionally. Class already carries `[DoNotParallelize]` for unrelated reasons; left as is. |
| Viewers/BreadcrumbCoordinatorLifecycleTests.cs:350 | `QueuedCompletion_DisposedBeforeOwnerDrain_DoesNotPublish` | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:58 | `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` | **AFFECTED** | Owner-only dispatcher (null context) reaches the thread-id branch of `IsCurrentBoundary()`; the blocking wait can inline the work item onto the owner thread, in which case the action runs and the test fails spuriously. |
| Viewers/BreadcrumbPopupControlDispatchTests.cs:29 | `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` | UNAFFECTED | Context-backed dispatcher; every stage goes through `DispatchValue`, which posts unless inside an executing callback and never reads thread identity. |
| Viewers/BreadcrumbPopupControlDispatchTests.cs:111 | `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment` | UNAFFECTED | Guard compares ambient context by reference; the body sets ambient null itself, so the post occurs on any thread. |
| Viewers/BreadcrumbPopupControlDispatchTests.cs:300 | helper `CompleteOnWorker` | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:192 | `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` | UNAFFECTED | `PostAsync` compares ambient context by reference; on the test thread the ambient context is the restored previous one, never the captured one, so the post happens on any thread; `PostCount` pins the posted path. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:61 | `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary` | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:75 | same test | UNAFFECTED | Context-backed coordinator; inbound `Dispatch` posts whenever ambient context is not the captured one and the body forces ambient null; a semaphore wait precedes the blocking wait, so the body has already run on another pool thread before inlining is possible. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:137 | `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary` | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:148 | same test | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:154 | same test | UNAFFECTED | `host.Close(...)` has no thread guard; the test asserts where drained work ran, not where `Close` originated. |
| Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:161 | same test | UNAFFECTED | `host.Reset` has no thread guard; same reasoning. |
| Viewers/BreadcrumbSelectorOpenRetryTests.cs:37 | `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly` | UNAFFECTED | Completes a `TaskCompletionSource` (`SetException`). |
| Viewers/BreadcrumbSelectorOpenRetryTests.cs:210 | `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity` | UNAFFECTED | Completes a `TaskCompletionSource` only. |
| Viewers/BreadcrumbSelectorOpenRetryTests.cs:218 | same test | UNAFFECTED | `host.Reset` has no thread guard; assertions are operation counts after drain. |
| Viewers/BreadcrumbSelectorOpenRetryTests.cs:219 | same test | UNAFFECTED | `host.Dispose` has no thread guard. |
| Viewers/BreadcrumbUiThreadDispatchTests.cs:51 | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` | UNAFFECTED | Completes a `TaskCompletionSource`; awaited, so no wait-inlining. |
| Viewers/BreadcrumbUiThreadDispatchTests.cs:90 | `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` | UNAFFECTED | Awaited; context-backed coordinator posts whenever ambient context is not the captured one; outcome identical on any thread. |
| Viewers/BreadcrumbUiThreadDispatchTests.cs:301 | `ProductionCaptureWithoutUiContext_FailsFast` | UNAFFECTED | `DispatchValue` on an owner-only dispatcher faults for every caller outside an executing callback and never reads `_ownerThreadId`; the expected exception is produced on the owner thread too. (The message wording is a follow-up, not a fix here.) |
| Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs:332 | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` | **AFFECTED** | The blocking wait can inline the work item onto the owner thread, on which branch the pre-#781 guard would also have admitted the call; the test then passes without discriminating. |
| Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:222 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | UNAFFECTED | Asserts a value observed by a second gate acquirer, not thread identity; the wait is a `ManualResetEventSlim`, not a task wait. Its intermittency is tracked under #823. |

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): the test host loads the UtilitiesCS.Test assembly from its bin output, so `typeof(FileInfoWrapper_Tests).Assembly.Location` is a readable on-disk path for the whole run (already relied on six times in the same file). The `InternalsVisibleTo("UtilitiesCS.Test")` attribute remains in place.
- Constraints (budget, performance, compatibility): temporary files prohibited; parallel regime unchanged; no production change; 500 total-line ceiling on every .cs file in the Write Set; MSTest, Moq, FluentAssertions only; committed evidence is projections only, with no absolute host path, account name or host name.
- External dependencies (services, libraries, releases): none.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): none. The project file gains two Compile Include entries in the existing explicit-include style.

## Test Strategy

**Baseline (before any edit).** Run both suites under the root runsettings file with the isolation switch and record a TRX-derived summary as a Markdown projection named test-run-baseline.md under the baseline evidence directory. Run the CLAUDE.md step-4 coverage route and record the package-level JaCoCo projection and the one-line first-party coverage summary as coverage-baseline.md in the same directory. Record total line counts of the three test files to be edited (490, 361, 359 today).

**Regression tests updated (no new production behaviour, so no new production test).**
- `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (BreadcrumbPopupBoundaryCoverageTests.cs): rewritten per Proposed Fix item 4.
- `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (moved to the Part2 file): rewritten per Proposed Fix item 3.
- `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic` and `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic` (moved to the Part2 file): body unchanged except the helper call target.
- `Properties_ShouldMirrorWrappedFileInfo`, `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory`, `OpenRead_ShouldReturnReadableStreamForWrappedFile`, `ToString_ShouldDelegateToWrappedFileInfo` (FileInfoWrapper_Tests.cs): rewritten per Proposed Fix item 5.

**Negative controls (each rewritten guard test must be observed failing).** Every mutation is applied, built, run with a scoped test-case filter on the fully qualified test name under the root runsettings file with the isolation switch, recorded, reverted with a git checkout of the mutated file, and the revert proven with the porcelain status output showing no residual change to that file. Each control is then re-run unmutated in the same artifact so the before/after pair is visible. Controls are never committed; the final diff contains no production change.
1. Owner-only dispatcher guard (temporary production edit): replace the thread-id comparison at BreadcrumbUiDispatcher.cs lines 276 to 277 with `return true;`. Expected: the rewritten `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` fails at the `executions` assertion ("Expected executions to be 0, but found 1") because the action ran inline on the worker. Projection: mutation-owner-only-dispatcher-guard.md.
2. Null-owner escape (temporary production edit): at ItemViewer.Breadcrumb.cs lines 435 to 438 replace the bare `return;` with the pre-#781 context-reference check that throws `InvalidOperationException` when `SynchronizationContext.Current` is not the captured context. Expected: the rewritten `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` fails at the `captured` null assertion because the dedicated thread's ambient context is null. This is the discrimination the old `Task.Run` shape only had conditionally. Projection: mutation-null-owner-escape.md.
3. Inline precondition (test-only edit): insert `action();` as the first statement of `DedicatedWorkerThread.Run` so the delegate also runs on the caller. Expected: both rewritten tests and the two existing worker-thread tests fail at their in-thread precondition with the stated reason text, proving the preconditions are live. Projection: mutation-inline-precondition.md.
4. OpenRead sentinel (test-only edit): point the mock's `OpenRead()` setup at a second, distinct `FileStream` sentinel. Expected: `OpenRead_ShouldReturnReadableStreamForWrappedFile` fails at `BeSameAs`. Projection: mutation-openread-sentinel.md.

Isolation note for the controls: each projection records the runsettings file used, the isolation switch, the exact test-case filter, the mutated file and hunk, the failed test names with the failure message excerpt, and the porcelain status output after revert. A control that passes while the mutation is applied means the test does not exercise the guard and is a blocking finding.

**Full suites in the parallel regime.** After the rewrite and after every control is reverted, run the full QuickFiler.Test and UtilitiesCS.Test assemblies under the root runsettings file (Workers zero, Scope ClassLevel) with the isolation switch. Record TRX-derived summaries as parallel-suite-quickfiler-test.md and parallel-suite-utilitiescs-test.md under the regression-testing evidence directory. The QuickFiler.Test summary must list the three cross-thread test names and `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`, which is the proof that the Part2 file and the helper compiled (an unregistered file is silently absent from the run). If the local machine's known shell-icon stall in UtilitiesCS.Test reproduces, the filter that excludes those classes must be recorded verbatim in the projection together with the excluded class names and a note that the stall is pre-existing on main; no other exclusion is permitted.

**Toolchain (CLAUDE.md order; restart from step one on any change or failure).**
1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. The `test: MSTest with Coverage (Koverage)` route (Invoke-MSTestWithCoverage.ps1 under scripts/vscode), which writes the fixed-name TRX under the coverage results directory that the summary is derived from.
Record commands, exit codes and the count of "Skipping target CoreCompile" lines (must be zero for both rebuilds, proving the analyzers and the nullable gate actually compiled) in toolchain-pass.md under the qa-gates evidence directory. Record the post-change package-level JaCoCo projection and one-line summary as coverage-final.md in the same directory.

**Coverage impact.** Changed lines are all test lines and sit outside the production denominator. Production lines the four file tests keep covered: FileInfoWrapper.cs lines 14 to 19 (public constructor), 21 to 24 (seam), 153 to 156 (`OpenRead`), 211 to 214 (explicit cast), and the `PhysicalFileInfoAdapter` property lines. The QuickFiler rewrite covers the same production branches as before. The final projection's per-package figures for UtilitiesCS and QuickFiler must be no lower than the baseline projection.

**Evidence hygiene.** Committed evidence is Markdown projections only (CLAUDE.md "Committed Test Evidence Format" and the maintainer decision on issue 671): no raw TRX, Cobertura XML or .coverage document is added anywhere. Every projection replaces absolute host paths, account names and host names with the placeholders repo-root, user-profile, user and host, each enclosed in angle brackets. Projection filenames are fixed (not timestamped); the run timestamp is a Timestamp field inside each artifact.

**Manual validation steps.** None.

## Write Set

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (modify: make partial; remove the three cross-thread tests, `ClearViewerDispatcher` and `RunOnDedicatedWorkerThread`)
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` (create: partial continuation with the three cross-thread tests and `ClearViewerDispatcher`)
- `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (create: shared dedicated-thread helper)
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` (modify: rewrite `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modify: two Compile Include entries)
- `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` (modify: rewrite four tests; remove `GetSolutionFile`)
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/` (create: baseline projections)
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/` (create: mutation and parallel-suite projections)
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/` (create: toolchain and coverage projections)

## Acceptance Criteria

- [ ] AC1. `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` in BreadcrumbPopupBoundaryCoverageTests.cs contains no `Task.Run` call and no `GetAwaiter().GetResult()` wait; it captures the owner thread id before constructing the owner-only dispatcher, runs the guarded `Dispatch` call through `DedicatedWorkerThread.Run`, and the delegate asserts that `Environment.CurrentManagedThreadId` differs from the captured owner id before `Dispatch` is invoked; the returned exception is asserted null and the two pre-existing assertions (zero executions; exactly one reported error whose message contains "cannot marshal") are present unchanged.
- [ ] AC2. `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`, now in the Part2 file, contains no `Task.Run` call and no `GetAwaiter().GetResult()` wait; it captures the owning `Dispatcher` before `ClearViewerDispatcher` is called, runs the guarded `InitializeBreadcrumbPipeline` call through `DedicatedWorkerThread.Run`, and the delegate asserts that the captured owner's `CheckAccess()` is false before the guarded call; the returned exception is asserted null and `BreadcrumbCoordinator` is asserted to be the same instance as before; the remark on the test states the off-owner-thread discrimination unconditionally.
- [ ] AC3. Every other `Task.Run` site in QuickFiler.Test, the twenty UNAFFECTED rows of the Triage table, is byte-identical to the merge base: a diff of QuickFiler.Test against the merge base touches no line of EmailMoveMonitorTests.cs, BreadcrumbCoordinatorLifecycleTests.cs, BreadcrumbPopupControlDispatchTests.cs, BreadcrumbPopupBoundaryCoverageTests.Part2.cs, BreadcrumbSelectorToggleUiBoundaryTests.cs, BreadcrumbSelectorOpenRetryTests.cs, BreadcrumbUiThreadDispatchTests.cs or QfcItemController.UiThreadDispatcherFixtureTests.cs, and the two pre-existing `[DoNotParallelize]` attributes in QuickFiler.Test are untouched.
- [ ] AC4. `ItemViewerBreadcrumbThreadAffinityTests` is declared `partial` in both ItemViewerBreadcrumbThreadAffinityTests.cs and ItemViewerBreadcrumbThreadAffinityTests.Part2.cs; the existing file holds the four owner-thread admission tests, `InertOperations` and the three nested types; the Part2 file holds the three cross-thread tests and `ClearViewerDispatcher`; no test method in the class is renamed or removed; the private `RunOnDedicatedWorkerThread` helper no longer exists in the class.
- [ ] AC5. Each of ItemViewerBreadcrumbThreadAffinityTests.cs, ItemViewerBreadcrumbThreadAffinityTests.Part2.cs, DedicatedWorkerThread.cs, BreadcrumbPopupBoundaryCoverageTests.cs and FileInfoWrapper_Tests.cs is at or under five hundred total lines, measured as total newline-terminated lines; the project file and Markdown artifacts are exempt from this ceiling by policy.
- [ ] AC6. The QuickFiler.Test project file contains a Compile Include entry for the Part2 file and a Compile Include entry for the DedicatedWorkerThread file, both in the project-relative backslash form used by the neighbouring entries, and the committed QuickFiler.Test parallel-suite projection lists `InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic`, `ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic`, `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` and `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` as executed and passed.
- [ ] AC7. `DedicatedWorkerThread` is an `internal static` class in namespace `QuickFiler.Test.TestSupport` exposing `internal static Exception Run(Action action)` whose body constructs a `Thread`, sets `IsBackground` true, starts it, joins it without a timeout, and returns the exception the delegate threw or null; it carries the dedicated-thread rationale remark from the removed private helper; and it contains no `Thread.Sleep`, `Task.Delay`, timeout argument, retry loop, or assertion. All four dedicated-thread tests in QuickFiler.Test call this member.
- [ ] AC8. FileInfoWrapper_Tests.cs contains no method named `GetSolutionFile`, no occurrence of the text "TaskMaster.sln", no reference to `AppDomain.CurrentDomain.BaseDirectory`, and no call that creates, writes to, or deletes a file: no `File.Create`, `File.WriteAll`, `File.Delete`, `Path.GetTempFileName`, `Path.GetTempPath`, no `FileMode` other than `Open`, and no `FileAccess` other than `Read`; the four tests `Properties_ShouldMirrorWrappedFileInfo`, `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory`, `OpenRead_ShouldReturnReadableStreamForWrappedFile` and `ToString_ShouldDelegateToWrappedFileInfo` keep their names.
- [ ] AC9. `OpenRead_ShouldReturnReadableStreamForWrappedFile` constructs the wrapper through the internal `FileInfoWrapper(IFileInfo)` constructor with a strict Moq mock of `IFileInfo` whose `OpenRead()` returns a `FileStream` the test opens over `typeof(FileInfoWrapper_Tests).Assembly.Location` with `FileMode.Open`, `FileAccess.Read` and `FileShare.ReadWrite` inside a `using` declaration; it asserts the wrapper's `OpenRead()` result is the same instance as that sentinel, that `CanRead` is true, and that `Length` is greater than zero.
- [ ] AC10. `Properties_ShouldMirrorWrappedFileInfo`, `ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory` and `ToString_ShouldDelegateToWrappedFileInfo` construct their `FileInfo` from a rooted literal path that does not point into the repository, open no stream, and assert none of `Length`, `OpenRead`, `Open`, `Create` or `OpenWrite`; their existing assertions on `Exists`, `FullName`, `Name`, `Extension`, `DirectoryName`, `Directory`, the explicit `DirectoryInfoWrapper` cast and `ToString()` are retained.
- [ ] AC11. A committed Markdown projection named mutation-owner-only-dispatcher-guard.md under the regression-testing evidence directory shows that, with the thread-id comparison in `BreadcrumbUiDispatcher.IsCurrentBoundary()` temporarily replaced by an unconditional true, the rewritten `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` fails at its `executions` assertion under the root runsettings file with the isolation switch and a scoped test-case filter, and that the same filter passes after the revert; the projection records the mutated file and hunk, the failed test name, the failure message excerpt, and porcelain status output after the revert showing no residual change.
- [ ] AC12. A committed projection named mutation-null-owner-escape.md under the regression-testing evidence directory shows that, with the null-owner `return` in `ItemViewer.ThrowIfOffUiBoundary` temporarily replaced by the pre-fix context-reference throw, the rewritten `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` fails at its `captured` null assertion, and passes after the revert, with the same recorded fields as AC11.
- [ ] AC13. A committed projection named mutation-inline-precondition.md under the regression-testing evidence directory shows that, with `action()` temporarily inserted as the first statement of `DedicatedWorkerThread.Run`, all four dedicated-thread tests fail at their in-thread distinctness precondition, and pass after the revert, with the same recorded fields as AC11.
- [ ] AC14. A committed projection named mutation-openread-sentinel.md under the regression-testing evidence directory shows that, with the mock's `OpenRead()` setup temporarily pointed at a second distinct `FileStream`, `OpenRead_ShouldReturnReadableStreamForWrappedFile` fails at its same-instance assertion, and passes after the revert, with the same recorded fields as AC11.
- [ ] AC15. The final diff against the merge base modifies no file under the QuickFiler or UtilitiesCS production directories, nothing under the .claude directory, and neither JSON file under config; the set of changed, added and deleted repository files equals the Write Set entries plus documents inside the feature folder.
- [ ] AC16. The full QuickFiler.Test and UtilitiesCS.Test assemblies pass under the root runsettings file (Workers zero, Scope ClassLevel) with the isolation switch, with zero failed tests, and the committed projections parallel-suite-quickfiler-test.md and parallel-suite-utilitiescs-test.md under the regression-testing evidence directory record the totals, the runsettings file, the switch and any test-case filter verbatim; the only permitted filter excludes the pre-existing local shell-icon stall classes in UtilitiesCS.Test and names them; the diff introduces no `[DoNotParallelize]`, no change to Workers or Scope, no retry attribute or loop, and no `Thread.Sleep`, `Task.Delay`, timeout or wall-clock wait.
- [ ] AC17. The full C# toolchain passes in one final pass in CLAUDE.md order (csharpier check; analyzer rebuild; TreatWarningsAsErrors rebuild; the MSTest-with-coverage route), and the committed projection toolchain-pass.md under the qa-gates evidence directory records each command, its exit code, and that both rebuild logs contain zero "Skipping target CoreCompile" lines.
- [ ] AC18. The committed projections coverage-baseline.md (baseline evidence directory) and coverage-final.md (qa-gates evidence directory) each contain a package-level JaCoCo projection and the one-line first-party coverage summary, and the final per-package line and branch figures for UtilitiesCS and QuickFiler are not lower than the baseline figures.
- [ ] AC19. The diff adds no file with a .trx, .xml or .coverage extension anywhere in the repository, and no committed evidence artifact contains an absolute host path, an account name or a host name; the placeholders repo-root, user-profile, user and host (each enclosed in angle brackets) are used in their place.

## Numeric Derivation Evidence

The acceptance criteria above contain no standalone digits; every count is written in words. The population claims they rely on (22 `Task.Run` sites in QuickFiler.Test, of which exactly two are AFFECTED and twenty UNAFFECTED) are derived in the research record's `## Numeric Derivation Evidence` section, which supplies the complete family, exhaustive scope, inclusion and exclusion rules, two independently constructed search strategies with distinct regular expressions and file filters, two independently enumerated member sets, both counts, and an explicit member-set comparison showing the sets are identical. That section is at docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md. The primary grep was re-run in this worktree on 2026-09-28 and returned the same 29 raw lines and the same 22 retained members.

## Risks & Mitigations

- Technical or operational risks:
  - The Part2 file or the helper is created on disk but not registered in the project file, so the moved tests silently disappear from the run. Mitigation: AC6 requires the four test names to appear as executed in the committed suite projection.
  - A negative control passes while the mutation is applied (the rewritten test does not exercise the guard). Mitigation: AC11 to AC14 require an observed failure with message excerpt; a passing control is a blocking finding.
  - A temporary production mutation is left in the tree. Mitigation: each control's projection records the porcelain status after revert, and AC15 forbids any production change in the final diff.
  - The rooted literal path used by the three metadata tests happens to exist on some machine. Mitigation: the assertions mirror `file.Exists` and never assert `Length`, so the outcome is identical either way.
  - The local shell-icon stall in UtilitiesCS.Test prevents a full local run. Mitigation: AC16 permits exactly that documented exclusion and no other; CI executes the excluded classes.
- Mitigations and rollbacks: test-only change; revert the commit to restore the previous tests.

## Rollout & Follow-up

- Release/rollout steps: merge through the normal PR gate after the toolchain and the parallel suite runs pass. No deployment impact.
- Post-fix monitoring or clean-up tasks: none beyond the follow-ups below.
- Follow-ups to record as potential entries through the promotion lifecycle (not fixed here; paths deliberately unbackticked):
  1. UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs: its own `GetSolutionFile()` (lines 173, 318, 373 to 376), the `catch (IOException)` blocks at lines 43 and 195 that swallow contention, and the read opens on the real solution file at lines 213 to 234. Same defect class as #906.
  2. UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs lines 60, 79 and 381: assertions that TaskMaster.sln is enumerated from the repository root. Same defect class as #906.
  3. QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs line 305: the asserted message "cannot marshal cross-thread UI work" describes the mechanism more broadly than `DispatchValue` implements (it faults for every caller outside an executing callback, on any thread). Wording-only.
  4. The #900 follow-up handoff (docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/) attributes BreadcrumbUiThreadDispatchTests.cs line 301 to the owner-thread-id check; the research record shows it reaches `DispatchValue`, which never reads the owner id. Documentation correction only.
- Links: issue #931 (https://github.com/drmoisan/TaskMaster/issues/931), consolidated issues #905 and #906, precedent PR #904 (issue #900), maintainer decision on issue 671 (projections-only evidence), research record cited in the header.
