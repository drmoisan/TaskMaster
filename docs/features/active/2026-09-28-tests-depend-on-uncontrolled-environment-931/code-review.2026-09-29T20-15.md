# Code Review: tests-depend-on-uncontrolled-environment (Issue #931)

- Branch: bug/tests-depend-on-uncontrolled-environment-931
- Head under review: ce744bbba3db4701e070785fd782c8b45289f2da
- Base: origin/main (c4ff0e2be0bc9c51acc43dacd2cc5954a448676c, merged at 55a50e922)
- Review timestamp: 2026-09-29T20-15
- Files reviewed (full current content read; diff against origin/main read in full): `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs`, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`
- Total blocking findings in this artifact: 0
- Total non-blocking findings in this artifact: 8 (1 Low, 7 Informational)

## Executive Summary

Verdict: PASS. Zero blocking findings.

The change is a disciplined test-only fix. Both affected `Task.Run` sites are replaced by a dedicated joined `Thread` whose delegate first asserts, inside the worker, that it is not the owner thread and only then exercises the guard; the returned exception is asserted null so a precondition failure cannot be masked. The helper's body is identical to the private helper PR #904 introduced, now shared from `QuickFiler.Test.TestSupport`, and all four dedicated-thread tests call it. The 490-line affinity class is split into two partials that preserve every test name, keep private helpers reachable across the partial declarations, and stay well under 500 lines. The file-handle tests no longer locate or open `TaskMaster.sln`: three use a rooted literal that is never opened, and the `OpenRead` test goes through the existing internal `IFileInfo` seam with a strict Moq mock returning a stream the test itself owns.

Four negative controls demonstrate that each rewritten assertion is live. The code follows MSTest, Moq and FluentAssertions conventions, carries `<summary>`/`<remarks>` on every changed member, and introduces no sleep, delay, timeout, retry or parallelism change.

The findings below are observations for the record; none requires a change on this branch. CR-1 (Low) notes that the shared helper deliberately carries no null guard for its delegate, a trade the spec and plan D-1 made explicitly to keep the body identical and assertion-free.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (Non-blocking) | `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` | `Run(Action action)`, lines 28-46 | No `ArgumentNullException` guard on `action`. A null delegate would surface as a captured `NullReferenceException` returned to the caller rather than a fail-fast at the call site. | No change on this branch: spec Proposed Fix item 1 and plan D-1 require the body to be identical to the removed private helper and AC7 requires no assertion in the helper. If the helper is revised later, add `if (action == null) throw new ArgumentNullException(nameof(action));` before the thread is constructed. | The general policy prefers validating preconditions at entry; the deviation is deliberate, disclosed, and confined to an `internal` test helper whose four call sites all pass lambdas. | Plan D-1; spec AC7; all four call sites read. |
| Informational (Non-blocking) | `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` | lines 33-40 | `catch (Exception error)` on the worker thread. | None. | The broad catch is the capture mechanism that returns the delegate's exception to the calling test for assertion; it is documented in the `<remarks>` and is the only way an assertion thrown on a non-pool thread reaches the test. An unhandled exception on a dedicated thread would terminate the test host. | File read; M2 and M3 projections show the captured exception surfacing in the test's own assertion message. |
| Informational (Non-blocking) | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`, lines 66-87 | No `// Arrange` / `// Act` / `// Assert` markers. | None. | The file's other tests use the same compact unmarked style; the rewritten test keeps the file's existing style, which the general policy prefers over introducing a second style in one file. The Part2 tests, whose file uses markers, carry them. | File read. |
| Informational (Non-blocking) | `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | `OpenRead_ShouldReturnReadableStreamForWrappedFile`, lines 68-73 | The sentinel is an OS file handle on the test host's own loaded assembly image (`typeof(FileInfoWrapper_Tests).Assembly.Location`), opened `FileMode.Open`, `FileAccess.Read`, `FileShare.ReadWrite`. | None. | The spec evaluated and rejected `MemoryStream` (the seam's `OpenRead` returns the concrete `FileStream` type) and a handle-free alternative (pipes are non-seekable, so `Length` throws). The chosen shape is read-shared, pre-existing for the whole run, creates and deletes nothing, and already occurs six times in the same file (lines 207-242). `Assembly.Location` being a readable on-disk path is an assumption the existing tests already rely on. | Spec "Chosen stream design and justification"; file read. |
| Informational (Non-blocking) | `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | `FixturePath`, line 18 | Rooted Windows literal `C:\Repo\fixture.sln`. | None. | Tests are net48 / Windows-only and the same rooted style already appears at lines 105, 131 and 150. The leading comment states it is not expected to exist; the assertions mirror `file.Exists` and never read `Length`, so the outcome is identical whether or not the path exists. | File read; plan D-14. |
| Informational (Non-blocking) | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` | `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow`, line 158 | `owner.Should().NotBeNull(...)` is an Arrange-phase precondition beyond the spec's item 3 text. | None. | The added assertion makes a vacuous pass impossible if the viewer ever stops owning a dispatcher before the clear; it strengthens, not weakens, the test and is consistent with AC2. | File read. |
| Informational (Non-blocking) | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` | lines 71-73 and 124-126 (pre-existing text, relocated) | `captured.Should().BeOfType<InvalidOperationException>()` followed by `captured.Should().NotBeOfType<ObjectDisposedException>()` is logically redundant (`BeOfType` is an exact-type check). | None. | Pre-existing since PR #904 and documented in the `<remarks>` as intent-declaring; relocation without change is what AC4 requires. | Diff read (removed and added hunks are identical apart from the helper name). |
| Informational (Non-blocking) | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | `using` block, lines 1-11 | After the split the primary partial still imports `System.Threading` and `System.Threading.Tasks`. | None. | Both remain used (`SynchronizationContext`, `Task.FromResult`); `System.Reflection` was correctly removed with `ClearViewerDispatcher` and re-declared in Part2. No unused directive was found in any of the five `.cs` files; the analyzer rebuild reports 0 warnings. | File read; `p4-t3-msbuild-analyzers` WARNINGS 0. |

## Design and Correctness Review

### Dedicated-thread helper

- A `Thread` constructed by the test is by construction a different object from every live thread, so `Dispatcher.CheckAccess()` (thread-object identity) and `Environment.CurrentManagedThreadId` comparisons are decided independently of the scheduler. The untimed `Join()` is a completion wait on one bounded synchronous call; the waiting thread may be a pool thread but the waited-for thread never is, so the wait cannot starve the pool under Workers=0 / ClassLevel. The helper does not set apartment state, which is correct: no control is touched on the worker in any of the four call sites.
- Visibility of `captured` across threads: the write on the worker happens before `Join()` returns on the caller (`Thread.Join` is a full fence), so the read after `Join()` is well-defined.
- `IsBackground = true` ensures a hung delegate cannot keep the process alive past the test host's own shutdown; this matters only for a hang, which the blame collector would name.

### Rewritten owner-only dispatcher test

- `ownerThreadId` is captured at line 70 before `CreateOwnerOnlyDispatcher` at line 71, which passes `Environment.CurrentManagedThreadId` as the owner (line 158), so the precondition compares against the same value the dispatcher holds. The rejection path of `Dispatch` reports through the sink and returns synchronously, so no task wait is needed and the pre-existing `executions == 0` and single "cannot marshal" error assertions remain unchanged. Negative control M1 (`return true;` in the thread-id branch) fails exactly at the `executions` assertion, which confirms the guard is the deciding branch.

### Rewritten null-owner test

- Capturing `Dispatcher owner = scope.Viewer.UiDispatcher` before `ClearViewerDispatcher` and asserting `owner.CheckAccess()` is false inside the delegate is the correct way to prove the call is off the owner thread after the field is nulled. The remark now states the discrimination unconditionally and explains why seeding first is still required (first-time initialization throws at `CaptureCurrent()` under a null ambient context). Negative control M2 restores the pre-#781 context-reference throw and the test fails at the `captured` null assertion, confirming it discriminates.

### File-handle tests

- The wrapper's `OpenRead` contract is pure delegation, so instance identity (`BeSameAs(sentinel)`) is the complete contract; `CanRead` and `Length > 0` confirm the returned object is the live stream. Negative control M4 (second stream) fails at `BeSameAs`. The three metadata tests keep the public `FileInfoWrapper(FileInfo)` constructor and the explicit `DirectoryInfoWrapper` cast covered (the Cobertura class row for `FileInfoWrapper.cs` reads line-rate 1.000 in every document), which the mocked tests cannot reach.

### Project file

- Two `Compile Include` entries added in the neighbouring backslash form at lines 99 and 229. The analyzer `HintPath` version strings (MSTest.Analyzers 4.4.1, Meziantou.Analyzer 3.0.290) changed only through the merge from origin/main; the post-merge analyzer rebuild loaded the merged versions (36 and 32 occurrences in the log).

### Policy conformance of the test code

- MSTest / Moq / FluentAssertions: conformant throughout; no MSTest `Assert` introduced.
- Banned APIs in tests: none (`Thread.Sleep`, `Task.Delay`, wall-clock waits, `DateTime.Now` all absent from the diff).
- Parallel regime: no `[DoNotParallelize]`, no Workers/Scope change, no retry; `TaskMaster.runsettings` hash unchanged in every recorded run.
- Temporary files: none created, written or deleted.
- 500-line limit: 48 / 202 / 294 / 386 / 357 lines for the five `.cs` files.
- Documentation: XML `<summary>` and `<remarks>` on the helper and each rewritten test; primary partial's class remark updated to point to the continuation file.

## Summary

The implementation matches the spec's Proposed Fix items 1-5 literally, the evidence demonstrates each rewritten assertion is live, and no code-quality issue rises above informational. Blocking findings: 0. Non-blocking findings: 8, none requiring remediation.
