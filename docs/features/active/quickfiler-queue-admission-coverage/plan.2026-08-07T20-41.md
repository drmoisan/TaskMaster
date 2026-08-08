# quickfiler-queue-admission-coverage - Plan

- **Issue:** #431
- **Parent (optional):** #136 (`quickfiler-per-file-coverage` epic, child F2)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T20-41
- **Status:** Planned
- **Version:** 0.2

## Required References

- `CLAUDE.md` (policy compliance order, § UT2 COM/VSTO/WinForms coverage exemption, C# toolchain)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/epics/quickfiler-per-file-coverage/epic.md` (Shared Design §1-6, F2 file assignment)
- `docs/features/active/quickfiler-queue-admission-coverage/spec.md` and `user-story.md` (AC source, `full-feature` mode)
- `docs/features/active/quickfiler-queue-admission-coverage/research/*.research.md` (per-file candidate test cases)

**All work must comply with these policies; do not duplicate their content here.**

## Implementation Plan (Atomic Tasks)

Work mode: `full-feature`. AC source: `spec.md` and `user-story.md` (7 identical AC items each, per `acceptance-criteria-tracking`). One phase per production file per issue #136's per-file mandate; each research candidate-test-case row is its own atomic task (never batched).

### Phase 0 — Compliance & Baseline

- [ ] [P0-T1] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `docs/features/epics/quickfiler-per-file-coverage/epic.md` (Shared Design §1-6), `docs/features/active/quickfiler-queue-admission-coverage/issue.md`, `spec.md`, and `user-story.md` in that order, and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: file exists with `Timestamp:`, `Policy Order:`, and an explicit list of every file read.
- [ ] [P0-T2] Run `dotnet tool run csharpier check QuickFiler QuickFiler.Test` as the pre-change formatting baseline and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/csharpier-baseline.<timestamp>.md`
  - Acceptance: artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail + diff-file count).
- [ ] [P0-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` as the pre-change analyzer baseline and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/analyzer-build-baseline.<timestamp>.md`
  - Acceptance: artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning/error counts).
- [ ] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` as the pre-change nullable baseline and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/nullable-build-baseline.<timestamp>.md`
  - Acceptance: artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P0-T5] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to the current `QuickFiler.Test.dll` (coverage-enabled, full assembly, `-SearchRoot .`) as the pre-change coverage baseline and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/coverage-baseline.<timestamp>.md`
  - Acceptance: artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing the repo-wide/assembly line-coverage headline plus the current per-file line-coverage percentage for each of `QfcQueue.cs`, `FilerQueue.cs`, `QfcRemainingQueueAdmission.cs`, `QfcStreamingDequeueConfidenceGate.cs`, `QfcHighConfidencePreFilter.cs`, `QfcScanProgressBandMapper.cs`, `BreadcrumbOutboundQueue.cs`, `EmailSorter.cs`, `QfcItemGroup.cs`.
- [ ] [P0-T6] Confirm F1's dependency state — `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` absence and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` presence — and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/baseline/f1-dependency-check.<timestamp>.md`
  - Acceptance: artifact states whether the ledger exists (path checked) and confirms the harness script exists and is non-placeholder, with `Timestamp:`.

### Phase 1 — `QfcQueue.cs`

- [ ] [P1-T1] Add a private delegate field `_itemViewerFactory` (default-bound to `ItemViewerQueue.Dequeue`) to `QuickFiler/Controllers/QfcQueue.cs` and route `AddAsync` through it instead of the direct static call
  - Acceptance: `AddAsync` invokes `_itemViewerFactory(...)`, not `ItemViewerQueue.Dequeue(...)` directly; the default binding preserves identical runtime behavior; `QuickFiler.csproj` build succeeds.
- [ ] [P1-T2] Create `QuickFiler/Controllers/QfcQueue.TlpManipulation.cs` as a new `public partial class QfcQueue` file and move the "Tlp Manipulation" region (`_tlpTemplate` field, `TlpTemplate` property, `ActivateTlpTemplate`, `_tlpStates` field, `TlpStates` property, `AddAsync`, `AddViewerToTlp`, `AdjustTlp`, `LoadControllersViewersAsync`, `ChangeIterationSize`, `RenumberGroups`, `GrowEntry`) verbatim out of `QfcQueue.cs`
  - Acceptance: all twelve listed members exist only in the new file; the new file declares `public partial class QfcQueue` with no primary-constructor parameter list.
- [ ] [P1-T3] Remove the moved "Tlp Manipulation" region from `QuickFiler/Controllers/QfcQueue.cs`, leaving only the primary constructor, "Queue Functions", "INotify", and "Helper Methods" regions
  - Acceptance: `QfcQueue.cs` no longer declares any of the twelve moved members; still declares the primary constructor and `: IQfcQueue`.
- [ ] [P1-T4] Add `<Compile Include="Controllers\QfcQueue.TlpManipulation.cs" />` to `QuickFiler/QuickFiler.csproj` adjacent to the existing `Controllers\QfcQueue.cs` entry
  - Acceptance: `msbuild` resolves and compiles `QfcQueue.TlpManipulation.cs` as part of `QuickFiler.csproj`.
- [ ] [P1-T5] Verify `QuickFiler/Controllers/QfcQueue.cs` is <= 500 lines after the split
  - Acceptance: recorded line count <= 500.
- [ ] [P1-T6] Verify `QuickFiler/Controllers/QfcQueue.TlpManipulation.cs` is <= 500 lines
  - Acceptance: recorded line count <= 500.
- [ ] [P1-T7] Add test `RemoveItem_WithMatchingEntry_RemovesRowRenumbersUnhooksAndRaisesCollectionChanged` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes; asserts row removal, a `RenumberGroups` effect, `IEmailMoveMonitor` unhook call, and one `CollectionChanged` raise.
- [ ] [P1-T8] Add test `RemoveItem_WithNoMatchingEntry_LeavesQueueContentUnchangedAndStillRaisesCollectionChanged` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes; asserts unchanged queue content and one `CollectionChanged` raise per re-added entry.
- [ ] [P1-T9] Add test `EnqueueAsync_NullItems_ThrowsArgumentNullException` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T10] Add test `EnqueueAsync_EmptyItems_ThrowsArgumentException` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T11] Add test `EnqueueAsync_HappyPath_HooksItemsTogglesJobsRunningAndRaisesCollectionChangedAdd` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`, using the `QfcItemController.TestSupport` dispatcher helpers and the P1-T1 `_itemViewerFactory` seam
  - Acceptance: test passes; asserts the hook-and-hydrate happy path, `_jobsRunning` increments then decrements, one queue entry added, `CollectionChanged` raised with `Add`.
- [ ] [P1-T12] Add test `EnqueueAsync_WhenLoadControllersViewersThrowsOperationCanceledException_SwallowsAndStillDecrementsJobsRunning` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes; asserts no rethrow and `_jobsRunning` decremented in `finally`.
- [ ] [P1-T13] Add test `EnqueueAsync_WhenLoadControllersViewersThrowsGenericException_LogsAndStillDecrementsJobsRunning` to `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs`
  - Acceptance: test passes; asserts the exception is logged and swallowed and `_jobsRunning` is decremented.
- [ ] [P1-T14] Create `QuickFiler.Test/Controllers/QfcQueueTlpManipulationTests.cs` as a new `[TestClass]` targeting the non-STA members of `QfcQueue.TlpManipulation.cs`
  - Acceptance: file compiles.
- [ ] [P1-T15] Add `<Compile Include="Controllers\QfcQueueTlpManipulationTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: `msbuild` compiles the new test file.
- [ ] [P1-T16] Add test `ChangeIterationSize_GrowingRowCount_MovesEntriesViaGrowEntryAndAppendsDatamodelDequeuedEntry` to `QfcQueueTlpManipulationTests.cs`, using the uninitialized-`QfcHomeController` + reflection seam and a mocked `IQfcDatamodel.DequeueNextItemGroupAsync`
  - Acceptance: test passes.
- [ ] [P1-T17] Add test `ChangeIterationSize_WhenDatamodelDequeueReturnsZeroItems_DiscardsDuplicateTopElementAndCompletes` to `QfcQueueTlpManipulationTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T18] Add test `TlpTemplate_Setter_ClonesAssignedPanelAndRenamesItLeavingOriginalPanelNameUnchanged` to `QfcQueueTlpManipulationTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T19] Add test `ActivateTlpTemplate_AnyPanelArgument_DoesNotThrow` to `QfcQueueTlpManipulationTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T20] Add test `NotifyPropertyChanged_WithSubscriberAttached_RaisesPropertyChangedWithCallerMemberName` to `QfcQueueTlpManipulationTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T21] Add test `NotifyPropertyChanged_WithNoSubscriberAttached_IsNoOp` to `QfcQueueTlpManipulationTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T22] Create `QuickFiler.Test/Controllers/QfcQueueTlpManipulation.StaTests.cs` with `[STATestClass]`, documenting in a file-level comment why no further seam is feasible for `AddAsync`/`AddViewerToTlp` per the epic's STA last-resort clause, and add `<Compile Include="Controllers\QfcQueueTlpManipulation.StaTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: file compiles, is registered in the csproj, and contains the required STA-rationale comment.
- [ ] [P1-T23] Add `[STATestMethod]` test `AddAsync_ObtainsViewerThroughInjectedFactory_AssignsToReturnedGroupAndCallsAddViewerToTlpOnUiIdleDispatcher` to `QfcQueueTlpManipulation.StaTests.cs`, using a real never-shown `ItemViewer` obtained through the `_itemViewerFactory` seam and the `QfcItemController.TestSupport` dispatcher helpers
  - Acceptance: test passes.
- [ ] [P1-T24] Add `[STATestMethod]` test `AddViewerToTlp_SetsParentCellPositionColumnSpanAutoSizeDockAndBorderStyleExactlyOnce` to `QfcQueueTlpManipulation.StaTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T25] Create `QuickFiler.Test/Controllers/QfcQueueDispatcherTests.cs`, add `<Compile Include="Controllers\QfcQueueDispatcherTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`, and add test `UiIdleCallAsync_ActionOverload_ExecutesDelegateOnDedicatedDispatcher` using `QfcItemController.TestSupport`'s `EnsureUiThreadDispatcher`/`StartRunningDispatcher`/`ShutdownDispatcher` helpers
  - Acceptance: file and csproj entry exist; test passes.
- [ ] [P1-T26] Add test `UiIdleCallAsync_FuncOverload_ExecutesDelegateOnDedicatedDispatcherAndReturnsResult` to `QfcQueueDispatcherTests.cs`
  - Acceptance: test passes.
- [ ] [P1-T27] Add test `UiIdleAsyncCallAsync_AwaitsInnerTaskAndYieldsBeforeReturning` to `QfcQueueDispatcherTests.cs`, asserting ordering with a `TaskCompletionSource`-gated inner func (no `Thread.Sleep`/`Task.Delay`)
  - Acceptance: test passes.
- [ ] [P1-T28] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `QfcQueue.cs` and `QfcQueue.TlpManipulation.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcQueue-coverage.<timestamp>.md`
  - Acceptance: both files report >= 80% line coverage with numeric values recorded (not placeholders).

### Phase 2 — `FilerQueue.cs`

- [ ] [P2-T1] Add a test-only `TestableEmailFiler` subclass overriding `SortAsync()` (order-recording, `TaskCompletionSource`-gated) to `QuickFiler.Test/Controllers/FilerQueueTests.cs`, reusing `EmailFiler`'s existing `virtual SortAsync()` seam
  - Acceptance: subclass compiles; makes no production change to `EmailFiler.cs` or `FilerQueue.cs`.
- [ ] [P2-T2] Add test `Enqueue_FilerQueueItem_AddsItemAndStartsConsumerOnFirstCall` to `FilerQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P2-T3] Add test `Enqueue_SecondCallWhileConsumerRunning_DoesNotStartSecondConsumerTask` to `FilerQueueTests.cs`
  - Acceptance: test passes; asserts `Consumer` task reference identity is unchanged on the second call.
- [ ] [P2-T4] Add test `Enqueue_EmailFilerAndHelpersOverload_ConstructsFilerQueueItemInternallyAndEnqueues` to `FilerQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P2-T5] Add test `ConsumeAsync_DrainsAllQueuedItems_InvokingSortAsyncOnEachInEnqueueOrder` to `FilerQueueTests.cs`, using the P2-T1 subclass's recorded invocation order
  - Acceptance: test passes.
- [ ] [P2-T6] Add test `ConsumeAsync_ContinuesDrainingSubsequentItems_AfterOneItemsSortAsyncThrows` to `FilerQueueTests.cs`
  - Acceptance: test passes; asserts no unhandled exception escapes `ConsumeAsync` and later items are still processed.
- [ ] [P2-T7] Add test `ConsumeAsync_ResetsGuardAfterDraining_AllowingSubsequentEnqueueToStartNewConsumerTask` to `FilerQueueTests.cs`
  - Acceptance: test passes; asserts a new `Consumer` task starts after re-enqueue post-drain.
- [ ] [P2-T8] Add test `ConsumeAsync_WithEmptyQueue_CompletesImmediatelyWithoutInvokingSortAsync` to `FilerQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P2-T9] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `FilerQueue.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/FilerQueue-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically.

### Phase 3 — `QfcRemainingQueueAdmission.cs`

- [ ] [P3-T1] Create `QuickFiler.Test/Controllers/QfcRemainingQueueAdmissionTests.cs` as a new `[TestClass]`
  - Acceptance: file compiles.
- [ ] [P3-T2] Add `<Compile Include="Controllers\QfcRemainingQueueAdmissionTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: `msbuild` compiles the new test file.
- [ ] [P3-T3] Add test `Constructor_NullScoreLoader_ThrowsArgumentNullExceptionNamingScoreLoader` to `QfcRemainingQueueAdmissionTests.cs`
  - Acceptance: test passes.
- [ ] [P3-T4] Add test `Constructor_NullAddToQueue_ThrowsArgumentNullExceptionNamingAddToQueue` to `QfcRemainingQueueAdmissionTests.cs`
  - Acceptance: test passes.
- [ ] [P3-T5] Add test `Constructor_NullHookItem_ThrowsArgumentNullExceptionNamingHookItem` to `QfcRemainingQueueAdmissionTests.cs`
  - Acceptance: test passes.
- [ ] [P3-T6] Add test `Constructor_NullRemoveFromQueue_ThrowsArgumentNullExceptionNamingRemoveFromQueue` to `QfcRemainingQueueAdmissionTests.cs`
  - Acceptance: test passes.
- [ ] [P3-T7] Add test `TryQueueAsync_WithAlreadyCancelledToken_ThrowsOperationCanceledExceptionBeforeInvokingDelegates` to `QfcRemainingQueueAdmissionTests.cs`, using delegates that throw `AssertFailedException` if invoked
  - Acceptance: test passes; asserts none of `addToQueue`/`hookItem`/`removeFromQueue` is invoked.
- [ ] [P3-T8] Add test `Constructor_HookItemsSecondArgument_IsExactlyTheConstructorsRemoveFromQueueDelegateReference` to `QfcRemainingQueueAdmissionTests.cs`
  - Acceptance: test passes; asserts delegate-reference identity, not just outcome.
- [ ] [P3-T9] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `QfcRemainingQueueAdmission.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcRemainingQueueAdmission-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically.

### Phase 4 — `QfcStreamingDequeueConfidenceGate.cs`

- [ ] [P4-T1] Create `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` as `[TestClass] public partial class QfcStreamingDequeueConfidenceGateTests` for the two narrow pre-#424 gaps
  - Acceptance: file compiles.
- [ ] [P4-T2] Add `<Compile Include="Controllers\QfcStreamingDequeueConfidenceGateTests.Part4.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: `msbuild` compiles the new partial file.
- [ ] [P4-T3] Add test `DequeueAsync_WithZeroQuantity_ReturnsEmptyListWithoutCallingTryTakeNextOrScoreLoader` to `.Part4.cs`, using the existing `CreateGate` reflection helper and throw-if-invoked delegates
  - Acceptance: test passes.
- [ ] [P4-T4] Add test `DequeueAsync_WithNegativeQuantity_ReturnsEmptyListWithoutCallingTryTakeNextOrScoreLoader` to `.Part4.cs`
  - Acceptance: test passes; confirms the guard is `<= 0`, not `== 0`.
- [ ] [P4-T5] Add test `Constructor_NullTryTakeNext_ThrowsArgumentNullExceptionNamingTryTakeNext` to `.Part4.cs`
  - Acceptance: test passes.
- [ ] [P4-T6] Add test `Constructor_NullScoreLoader_ThrowsArgumentNullExceptionNamingScoreLoader` to `.Part4.cs`
  - Acceptance: test passes.
- [ ] [P4-T7] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `QfcStreamingDequeueConfidenceGate.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcStreamingDequeueConfidenceGate-coverage.<timestamp>.md`, explicitly noting no re-testing of issue #424's delivered deadline/progress-callback/liveness surface
  - Acceptance: >= 80% line coverage recorded numerically (baseline already ~96.63% per the #424 audit); note confirms no #424 surface was re-tested.

### Phase 5 — `QfcHighConfidencePreFilter.cs`

- [ ] [P5-T1] Add test `QfcPreScoredItem_ConstructedWithNullPredeterminedFolder_ExposesPredeterminedFolderAsEmptyString` to `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`
  - Acceptance: test passes.
- [ ] [P5-T2] Add test `QfcPreScoredItem_ConstructedWithNonNullPredeterminedFolder_ExposesUnchangedValueAndSameMailItemReference` to `QfcHighConfidencePreFilterTests.cs`
  - Acceptance: test passes.
- [ ] [P5-T3] Add test `FilterAsync_WhenScoringCompletesOutOfIndexOrder_PreservesOriginalInputOrderAmongSurvivors` to `QfcHighConfidencePreFilterTests.cs`, using `TaskCompletionSource`-gated mock responses released in a controlled reversed sequence
  - Acceptance: test passes; proves the `OrderBy(result => result.index)` line is load-bearing.
- [ ] [P5-T4] Record a ledger-ratification RECOMMENDATION (not a removal) for `FolderScoringService`'s `[ExcludeFromCodeCoverage]` exemption as `docs/features/active/quickfiler-queue-admission-coverage/evidence/other/folderscoringservice-ledger-recommendation.<timestamp>.md`, citing the existing `IFolderScoringService` seam and the COM-bound/live-classifier body as the irreducible-remainder rationale
  - Acceptance: artifact exists with `Timestamp:`, the recommendation text, and an explicit statement that F1's actual ledger is authoritative at execution time.
- [ ] [P5-T5] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `QfcHighConfidencePreFilter.cs`'s testable surface (`QfcHighConfidencePreFilter`, `QfcPreScoredItem`, `IFolderScoringService`) as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcHighConfidencePreFilter-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically for the testable surface; `FolderScoringService` excluded per P5-T4's recommendation.

### Phase 6 — `QfcScanProgressBandMapper.cs`

- [ ] [P6-T1] Grep-confirm `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` contains no `[ExcludeFromCodeCoverage]` attribute (only the phrase inside a doc comment) and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/other/QfcScanProgressBandMapper-exclusion-check.<timestamp>.md`
  - Acceptance: artifact confirms absence of the attribute, distinguishing the doc-comment mention from an actual application.
- [ ] [P6-T2] Run the existing 12-test `QuickFiler.Test/Controllers/QfcScanProgressBandMapperTests.cs` suite (unmodified) and confirm all 12 tests pass
  - Acceptance: 12/12 pass; no test file changes made.
- [ ] [P6-T3] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcScanProgressBandMapper-coverage.<timestamp>.md` confirming the existing coverage figure holds under F1's harness
  - Acceptance: numeric line/branch coverage recorded (expected 100%/100% per the #424 audit); no new test authored, per spec.md's Implementation Strategy.

### Phase 7 — `BreadcrumbOutboundQueue.cs`

- [ ] [P7-T1] Create `QuickFiler.Test/Controllers/BreadcrumbOutboundQueueTests.cs` as a new dedicated `[TestClass]`
  - Acceptance: file compiles.
- [ ] [P7-T2] Add `<Compile Include="Controllers\BreadcrumbOutboundQueueTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: `msbuild` compiles the new test file.
- [ ] [P7-T3] Add test `PostOrQueue_WithHostAlreadyInitialized_PostsImmediatelyAndDoesNotBuffer` to `BreadcrumbOutboundQueueTests.cs`, using `Mock<IBreadcrumbWebHost>`
  - Acceptance: test passes; asserts `PostMessageJson` called and `PendingCount` stays 0.
- [ ] [P7-T4] Add test `PostOrQueue_WithHostNotInitialized_BuffersPayloadAndIncrementsPendingCountWithoutPosting` to `BreadcrumbOutboundQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P7-T5] Add test `OnInitializationCompleted_FlushesEveryBufferedPayloadInEnqueueOrderAndDrainsPendingCountToZero` to `BreadcrumbOutboundQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P7-T6] Add test `OnInitializationCompleted_OnEmptyBuffer_IsANoOpAndNeverCallsPostMessageJson` to `BreadcrumbOutboundQueueTests.cs`
  - Acceptance: test passes.
- [ ] [P7-T7] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `BreadcrumbOutboundQueue.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/BreadcrumbOutboundQueue-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically.

### Phase 8 — `EmailSorter.cs`

- [ ] [P8-T1] Add test `GetSortKey_WithDefaultSortOptions_ReturnsNegativeOneForAnyTriageAndDateInput` to `QuickFiler.Test/Controllers/EmailSorterTests.cs`
  - Acceptance: test passes.
- [ ] [P8-T2] Add test `GetSortKey_WithOnlyTriageImportantFirstSet_ReturnsNegativeOne` to `EmailSorterTests.cs`
  - Acceptance: test passes.
- [ ] [P8-T3] Add test `GetSortKey_WithOnlyDateRecentFirstSet_ReturnsNegativeOne` to `EmailSorterTests.cs`
  - Acceptance: test passes.
- [ ] [P8-T4] Add test `Options_Setter_ChangesPropertyValueObservableViaSubsequentGetSortKeyCall` to `EmailSorterTests.cs`
  - Acceptance: test passes; sets `Options` to `Default` after constructing with both flags and observes the `-1` fallback.
- [ ] [P8-T5] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `EmailSorter.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/EmailSorter-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically.

### Phase 9 — `QfcItemGroup.cs`

- [ ] [P9-T1] Create `QuickFiler.Test/Controllers/QfcItemGroupTests.cs` as a new `[TestClass]`
  - Acceptance: file compiles.
- [ ] [P9-T2] Add `<Compile Include="Controllers\QfcItemGroupTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: `msbuild` compiles the new test file.
- [ ] [P9-T3] Add test `Constructor_Parameterless_LeavesMailItemAsDefaultNull` to `QfcItemGroupTests.cs`
  - Acceptance: test passes.
- [ ] [P9-T4] Add test `Constructor_WithMailItem_StoresSuppliedReferenceRetrievableViaGetter` to `QfcItemGroupTests.cs`
  - Acceptance: test passes.
- [ ] [P9-T5] Add test `ItemViewer_PropertySetterGetter_RoundTripsReferenceWithoutTransformation` to `QfcItemGroupTests.cs`, using a null/dummy reference (no COM/live-Outlook dependency)
  - Acceptance: test passes.
- [ ] [P9-T6] Add test `ItemController_PropertySetterGetter_RoundTripsMockedIQfcItemControllerReference` to `QfcItemGroupTests.cs`
  - Acceptance: test passes.
- [ ] [P9-T7] Add test `PredeterminedFolder_DefaultsToNullAndRoundTripsAnyAssignedStringIncludingEmpty` to `QfcItemGroupTests.cs`
  - Acceptance: test passes.
- [ ] [P9-T8] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to `QuickFiler.Test.dll` and record per-file line coverage for `QfcItemGroup.cs` as `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/QfcItemGroup-coverage.<timestamp>.md`
  - Acceptance: >= 80% line coverage recorded numerically.

### Phase 10 — `IQfcQueue.cs` / `IQfcQueue1.cs` disposition

- [ ] [P10-T1] Grep-verify `IQfcQueue1`'s only production reference beyond its own declaration is the `<Compile Include="Controllers\IQfcQueue1.cs" />` entry in `QuickFiler/QuickFiler.csproj` (no implementer, no consumer) and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/other/IQfcQueue1-reference-check.<timestamp>.md`
  - Acceptance: artifact confirms zero implementers and zero consumers repo-wide.
- [ ] [P10-T2] Decide and record the `IQfcQueue1.cs` disposition as deletion (zero-risk removal of unreachable dead code) in `docs/features/active/quickfiler-queue-admission-coverage/evidence/other/IQfcQueue1-disposition-decision.<timestamp>.md`, citing spec.md's Implementation Strategy decision point and P10-T1's reference check
  - Acceptance: decision artifact exists and states the chosen disposition and rationale.
- [ ] [P10-T3] Delete `QuickFiler/Controllers/IQfcQueue1.cs`
  - Acceptance: file no longer exists on disk.
- [ ] [P10-T4] Remove the `<Compile Include="Controllers\IQfcQueue1.cs" />` entry from `QuickFiler/QuickFiler.csproj`
  - Acceptance: entry no longer present in the csproj.
- [ ] [P10-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and confirm zero errors after the `IQfcQueue1.cs` deletion
  - Acceptance: `EXIT_CODE: 0`.
- [ ] [P10-T6] Record `IQfcQueue.cs` as an interface-only module exempt from line-coverage measurement per the repository's coverage-exclusion clarification in `docs/features/active/quickfiler-queue-admission-coverage/evidence/other/IQfcQueue-coverage-exclusion-note.<timestamp>.md`
  - Acceptance: note exists; confirms no coverage evidence task is required for `IQfcQueue.cs`.

### Phase 11 — Final QC & Evidence Consolidation

- [ ] [P11-T1] Run `dotnet tool run csharpier format QuickFiler QuickFiler.Test` across every file touched by Phases 1-10 and record `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/final-csharpier-format.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; artifact has `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [ ] [P11-T2] Run `dotnet tool run csharpier check QuickFiler QuickFiler.Test` and confirm zero diffs, recording `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/final-csharpier-check.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; if P11-T1 or this task reports any changed/flagged file, restart the loop from P11-T1.
- [ ] [P11-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and confirm zero analyzer diagnostics, recording `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/final-analyzer-build.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; restart the loop from P11-T1 if this step fails or changes files.
- [ ] [P11-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and confirm zero nullable warnings-as-errors, recording `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/final-nullable-build.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; restart the loop from P11-T1 if this step fails or changes files.
- [ ] [P11-T5] Run `scripts/vscode/Invoke-MSTestWithCoverage.ps1` scoped to the full `QuickFiler.Test.dll` (coverage-enabled) and confirm every new/modified test passes, recording `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/final-vstest-coverage.<timestamp>.md`
  - Acceptance: `EXIT_CODE: 0`; artifact records the full pass count and the repo-wide/assembly coverage headline; restart the loop from P11-T1 if any test fails.
- [ ] [P11-T6] Compare Phase 0's baseline repo-wide coverage (`evidence/baseline/coverage-baseline.<timestamp>.md`) against P11-T5's final coverage and confirm repository-wide line coverage is retained or improved, recording `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/coverage-no-regression-check.<timestamp>.md`
  - Acceptance: numeric delta >= 0 recorded, citing both baseline and final values.
- [ ] [P11-T7] Consolidate the nine per-file coverage evidence artifacts from Phases 1-9 (`QfcQueue-coverage`, `FilerQueue-coverage`, `QfcRemainingQueueAdmission-coverage`, `QfcStreamingDequeueConfidenceGate-coverage`, `QfcHighConfidencePreFilter-coverage`, `QfcScanProgressBandMapper-coverage`, `BreadcrumbOutboundQueue-coverage`, `EmailSorter-coverage`, `QfcItemGroup-coverage`) into `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/f2-per-file-coverage-summary.<timestamp>.md`
  - Acceptance: summary lists all nine files with numeric coverage values, all >= 80%.
- [ ] [P11-T8] Check off each satisfied acceptance criterion in `docs/features/active/quickfiler-queue-admission-coverage/spec.md`'s `## Acceptance Criteria` section, changing `- [ ]` to `- [x]` per item verified by Phases 0-10's evidence
  - Acceptance: every satisfied AC item is `- [x]`; any unmet item remains `- [ ]` with the gap documented.
- [ ] [P11-T9] Check off each satisfied acceptance criterion in `docs/features/active/quickfiler-queue-admission-coverage/user-story.md`'s `## Acceptance Criteria` section, mirroring P11-T8's checkoffs
  - Acceptance: every satisfied AC item is `- [x]` in `user-story.md`.
- [ ] [P11-T10] Produce the AC Status Summary block (source files, total/checked/remaining counts, remaining item text if any) per `acceptance-criteria-tracking` in `docs/features/active/quickfiler-queue-admission-coverage/evidence/qa-gates/ac-status-summary.<timestamp>.md`
  - Acceptance: artifact contains `Source:`, `Total AC items:`, `Checked off (delivered):`, `Remaining (unchecked):`, and `Items remaining:`.

## Test Plan

- Unit: MSTest/Moq/FluentAssertions additions per phase above (18 new `QfcQueue.cs` tests across four test files + STA file; 7 `FilerQueue.cs` tests; 6 `QfcRemainingQueueAdmission.cs` tests in a new file; 4 `QfcStreamingDequeueConfidenceGate.cs` tests in a new `.Part4.cs`; 3 `QfcHighConfidencePreFilter.cs` tests; 0 new `QfcScanProgressBandMapper.cs` tests (defensive re-verification only); 4 `BreadcrumbOutboundQueue.cs` tests in a new file; 4 `EmailSorter.cs` tests; 5 `QfcItemGroup.cs` tests in a new file).
- Integration: none — host-neutral, COM-independent logic; no external services, per spec.md.
- Manual/CLI: n/a.
- Coverage evidence: baseline artifacts under `evidence/baseline/` (Phase 0); per-file artifacts under `evidence/qa-gates/<file-basename>-coverage.<timestamp>.md` (Phases 1-9); final-QC artifacts and the coverage no-regression check under `evidence/qa-gates/` (Phase 11); ledger-recommendation and dead-code-disposition artifacts under `evidence/other/` (Phases 5, 6, 10).

## Open Questions / Notes

- F1's ratified exemption ledger (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`) does not exist on disk at planning time; Phases 5, 6, and 10's dispositions are this plan's best-effort recommendation, superseded by F1's actual ledger at execution time (per the epic's wave-0/wave-1 dependency).
- Issue #424's changes to `QfcStreamingDequeueConfidenceGate.cs` are confirmed present and complete on disk; Phase 4 deliberately does not re-test that surface.
- `IQfcQueue1.cs` disposition (Phase 10) is resolved as deletion per spec.md's explicit decision point, based on the confirmed absence of any implementer or consumer.
