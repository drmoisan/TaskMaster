# 2026-03-19-utilities-coverage-part-three - Plan

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-22
- **Status:** In Progress
- **Version:** 1.2

## Required References

- General Coding Standards: [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- General Unit Test Policy: [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- C# Code Change Policy: [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- C# Unit Test Policy: [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)
- Spec: [`spec.md`](spec.md)
- User Story: [`user-story.md`](user-story.md)
- Research: [`../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md`](../../../../artifacts/research/20260319-utilities-coverage-part-three-87-research.md)

**All work must comply with these policies; do not duplicate their content here.**

## Overview

Raise every production `.cs` file compiled by `UtilitiesCS.csproj` to >= 80% line coverage by adding or extending MSTest unit tests in `UtilitiesCS.Test`, with evidence-backed skip evaluation only where repo policy and deterministic testability constraints make the 80% target unattainable. Work is phased by testability difficulty (Easy → Medium → Hard), now preceded by an explicit reconciliation gate that maps every currently sub-80 non-skip file to either a remaining implementation task or a Phase 4 skip task before further execution resumes. After the latest reconciliation pass, every remaining unchecked implementation task lists only implementation-routed files, and every Phase 4 constrained skip batch mirrors the reconciliation ledger exactly. Approximately 155 files have explicit line-rate below 80% in the Cobertura report, plus ~16 `Designer.cs` files, ~4 commented stubs, and ~40+ pure interface files with no executable code.

## Acceptance Criteria Traceability

| AC | Source (issue.md) | Plan Coverage |
|---|---|---|
| AC1 | Every .cs file compiled by UtilitiesCS.csproj >= 80% line coverage | P0-T5 through P0-T6 reconciliation + P1–P3 implementation tasks + P4-T1 through P4-T39 skip evaluation + P5-T5 verification |
| AC2 | No pre-existing tests broken or removed | P0-T3 baseline + P5-T6 verification |
| AC3 | All new tests follow MSTest + Moq + FluentAssertions conventions | P0-T1 policy read + all P1–P3 implementation tasks |
| AC4 | All new tests deterministic, isolated, no external deps | P0-T1 policy read + all P1–P3 implementation tasks |
| AC5 | All new test files registered in UtilitiesCS.Test.csproj | P1-T13, P2-T24, P3-T68 registration tasks |
| AC6 | C# toolchain loop passes clean | P5-T1 through P5-T4 |
| AC7 | Repo-wide coverage does not regress below baseline | P0-T3 baseline + P5-T5 comparison |

## Implementation Plan (Atomic Tasks)

### Phase 0 — Compliance & Baseline Capture

- [x] [P0-T1] Read all repo policy files in required order: `.github/copilot-instructions.md`, `general-code-change.instructions.md`, `general-unit-test.instructions.md`, `csharp-code-change.instructions.md`, `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact at `evidence/baseline/phase0-instructions-read.md` contains `Timestamp:`, `Policy Order:`, and explicit list of all five files read

- [x] [P0-T2] Capture baseline build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-build.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P0-T3] Capture baseline test results with coverage by running `vstest.console.exe` with `/EnableCodeCoverage` over all `*.Test.dll` assemblies
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-test-coverage.md` contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and repo-wide UtilitiesCS line coverage percentage

- [x] [P0-T4] Record per-file baseline coverage for all UtilitiesCS production files below 80% line rate from the current `coverage/coverage.cobertura.xml`
  - Acceptance: Evidence artifact at `evidence/baseline/baseline-per-file-coverage.md` lists each file with its current line-rate percentage, categorized by difficulty (Easy/Medium/Hard/Skip)

- [x] [P0-T5] Reconcile every currently sub-80 non-skip UtilitiesCS file from `evidence/qa-gates/final-coverage-verification.md` against the remaining plan and `evidence/other/skip-candidates.md`
  - Acceptance: Evidence artifact at `evidence/baseline/remaining-sub80-reconciliation.md` contains one row for every file listed under "Non-Skip UtilitiesCS Files Below 80%" in `evidence/qa-gates/final-coverage-verification.md`, and each row maps the file to exactly one remaining task path: `Implementation Task` or `Phase 4 Skip Task`

- [x] [P0-T6] Verify the revised checklist state matches the reconciliation matrix before additional implementation resumes
  - Preconditions: P0-T5 complete
  - Acceptance: Every file mapped to `Implementation Task` in `evidence/baseline/remaining-sub80-reconciliation.md` references an unchecked P1/P2/P3 task ID, every file mapped to `Phase 4 Skip Task` references an unchecked P4 task ID, and no checked task still depends on a file that remains below 80% in `evidence/qa-gates/final-coverage-verification.md`

### Phase 1 — Easy Files: Quick Wins (~45 files)

**Goal:** Maximum coverage uplift per effort. All tasks target files categorized as Easy difficulty in research. Tests must follow MSTest + Moq + FluentAssertions conventions with AAA pattern, deterministic, isolated, no external dependencies, no temp files.

- [x] [P1-T1] Extend tests for close-to-80% Extensions: `ArrayExtensions.cs` (77.7%), `IEnumerableExtensions.cs` (70.6%), `IAsyncEnumerableExtensions.cs` (60.9%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P1-T2] Extend tests for close-to-80% ReusableTypeClasses: `ConcurrentObservableDictionary.cs` (77.4%), `AbstractCloneable.cs` (77.8%), `TreeNodeOfT.cs` (76.8%), `StackGeek.cs` (72.2%), `StackObjectCS.cs` (72%)
  - Acceptance: Coverage report shows all five files at >= 80% line rate

- [x] [P1-T3] Extend tests for close-to-80% NewtonsoftHelpers wrappers: `WrapperScDictionary.cs` (70.7%), `WrapperScoDictionary.cs` (76%), `FilePathHelperConverter.cs` (72%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P1-T4] Extend tests for close-to-80% EmailIntelligence: `EmailTokenizer.cs` (74.8%), `CtfIncidenceList.cs` (64.5%), `CtfMap.cs` (68.6%), `SubjectMapEntry.cs` (50.8%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P1-T5] Extend tests for close-to-80% HelperClasses: `MyFileSystemInfo.cs` (71%), `PrettyPrint.cs` (67.2%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P1-T6] Create or extend tests for pure-logic HelperClasses: `DeepCompare.cs` (31.2%), `Initializer.cs` (60.3%), `DebugTextWriter.cs` (63.6%), `TraceUtility.cs` (60.6%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P1-T7] Create or extend tests for pure-algorithm classes: `SmithWaterman.cs` (48.6%), `ImageStripper.cs` (53.5%), `StringManipulation.cs` (0%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P1-T8] Create tests for zero-coverage data classes: `FilterEntry.cs` (0%), `BayesianMetricTypes.cs` (0%), `EmailFilerConfig.cs` (0%), `NConsoleTraceWriter.cs` (0%), `PropertyStore.cs` (0%)
  - Acceptance: Test classes exist for all five files; coverage report shows all five at >= 80% line rate

- [x] [P1-T9] Extend tests for partial-coverage data classes: `MovedMailInfo.cs` (35.4%), `DedicatedToken.cs` (22.2%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P1-T10] Create or extend tests for data structures: `LockingLinkedList.cs` (58.2%), `LockingLinkedListNode.cs` (61.7%), `AsyncLazy.cs` (25%), `TimedQueueOfActions.cs` (58.8%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P1-T11] Create tests for observer implementations: `SimpleActionBagObserver.cs` (0%), `SimpleActionLockingLinkedListObserver.cs` (0%)
  - Acceptance: Test classes exist for both; coverage report shows both at >= 80% line rate

- [x] [P1-T12] Create or extend tests for easy threading utilities: `ThreadSafeFunctions.cs` (54.8%), `TimeOutTask.cs` (24.1%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P1-T13] Register all new Phase 1 test files in `UtilitiesCS.Test.csproj` via `<Compile Include>` entries
  - Acceptance: Every new `.cs` test file created in Phase 1 has a corresponding `<Compile Include>` entry in `UtilitiesCS.Test.csproj`; `msbuild` resolves all test files without missing-reference errors

- [x] [P1-T14] Run Phase 1 checkpoint: build solution and run tests with coverage; verify all Phase 1 target files reach >= 80% line coverage
  - Preconditions: P1-T1 through P1-T13 complete
  - Acceptance: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits 0; `vstest.console.exe` exits 0 with no test failures; coverage report confirms all ~39 Phase 1 target files at >= 80%

### Phase 2 — Medium Files: Mocking-Dependent (~55 files)

**Goal:** Cover classes requiring Moq-based mocking for interfaces, file system, Newtonsoft JSON, Bayesian logic, and COM interop with established mock patterns. Tests must use MemoryStream/StringWriter injection for any serialization testing (no temp files per policy). After reconciliation, each remaining unchecked Phase 2 implementation task is limited to files still mapped to `Implementation Task` rows in `evidence/baseline/remaining-sub80-reconciliation.md`.

- [x] [P2-T1] Create or extend tests for JSON converters batch 1: `ScDictionaryConverter.cs` (9.1%), `NonRecursiveConverter.cs` (0%), `PeopleScoConverter.cs` (66.7%), `PeopleScoRemainingObjectConverter.cs` (40%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T2] Create or extend tests for JSON converters batch 2: `DerivedCompositionConverter_ConcurrentDictionary.cs` (0%), `MonoExtension.cs` (39.1%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T3] Create tests for SDIL Reader data models: `ILGlobals.cs` (0%), `ILInstruction.cs` (0%)
  - Acceptance: Test classes exist; coverage report shows both files at >= 80% line rate

- [x] [P2-T4] Create or extend tests for NewtonsoftHelpers wrapper: `WrapperPeopleScoDictionaryNew.cs` (12.6%)
  - Acceptance: Coverage report shows file at >= 80% line rate

- [x] [P2-T5] Create or extend tests for the remaining SCO core collection implementation target: `ScDictionary.cs` (8.6%)
  - Note: `ScBag.cs` and `SCODictionary.cs` are deferred to `P4-T33` per `evidence/baseline/remaining-sub80-reconciliation.md`
  - Acceptance: Coverage report shows `ScDictionary.cs` at >= 80% line rate

- [x] [P2-T6] Create or extend tests for SCO variant collections: `ScoCollection.cs` (4.4%), `ScoSortedDictionary.cs` (7.4%), `ScoStack.cs` (40.2%), `ScoDictionaryNew.cs` (15.5%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T7] Create or extend tests for serializable lists: `SerializableList.cs` (35.9%), `SloLinkedList.cs` (29.7%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T8] Create or extend tests for remaining SmartSerializable core files: `SmartSerializable.cs` (15.9%), `SmartSerializableBase.cs` (0%)
 - [x] [P2-T8] Create or extend tests for remaining SmartSerializable core files: `SmartSerializable.cs` (15.9%), `SmartSerializableBase.cs` (0%)
  - Note: `SmartSerializableNonTyped.cs` is no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T9] Create or extend tests for SmartSerializable infrastructure: `SmartSerializableLoader.cs` (7.1%), `SmartSerializableStatic.cs` (0%), `NewSmartSerializableConfig.cs` (29.5%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P2-T10] Extend tests for the remaining Bayesian classifier implementation target: `BayesianClassifierShared.cs` (63.8%)
  - Note: `BayesianClassifierGroup.cs` and `BayesianClassifierExtensions.cs` are no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows `BayesianClassifierShared.cs` at >= 80% line rate

- [x] [P2-T11] Create or extend tests for remaining legacy Bayesian implementation targets: `Obsolete/BayesianClassifier.cs` (65.1%), `Obsolete/ClassifierGroup.cs` (8.2%)
  - Note: `CorpusInherit.cs` is deferred to `P4-T33`, and `Corpus.cs` is no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T12] Create tests for Bayesian performance measurement: `BayesianPerformanceMeasurement.cs` (0%), `BayesianSerializationHelper.cs` (0%)
  - Acceptance: Test classes exist; coverage report shows both files at >= 80% line rate

- [x] [P2-T13] Extend tests for remaining OutlookItem core implementation targets using Moq COM mocking: `OutlookItem.cs` (54.5%), `OutlookItemExtensions.cs` (44.9%)
  - Note: `OlItemPseudoInterface.cs` is no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T14] Extend tests for OutlookItem try-patterns: `OutlookItemTry.cs` (35.5%), `OutlookItemTryGet.cs` (21.6%), `OutlookItemFlaggable.cs` (58.2%), `OutlookItemFlaggableTry.cs` (51%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T15] Extend tests for remaining OutlookObjects helper implementation targets: `AttachmentHelper.cs` (69.9%), `AttachmentSerializable.cs` (54.7%), `CreateCategory.cs` (65.5%), `RecipientStatic.cs` (46.7%), `UserDefinedFields.cs` (26%)
  - Note: `StoreWrapper.cs` is no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows all five files at >= 80% line rate

- [ ] [P2-T16] Create or extend tests for file system wrappers with mocked I/O: `FileInfoWrapper.cs` (17.8%), `DirectoryInfoWrapper.cs` (20.3%), `FileSystemInfoWrapper.cs` (0%), `FilePathHelper.cs` (18.8%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T17] Retire the former progress and thread-tracking implementation batch after reconciliation routed all four files to `P4-T34`
  - Acceptance: `evidence/baseline/remaining-sub80-reconciliation.md` maps `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `AsyncMultiTasker.cs`, and `ThreadMonitor.cs` only to `P4-T34`, and no unchecked P1/P2/P3 implementation task references those files

- [x] [P2-T18] Retire the former EmailIntelligence constrained implementation batch after reconciliation routed its remaining sub-80 files to `P4-T35`
  - Acceptance: `evidence/baseline/remaining-sub80-reconciliation.md` maps `IntelligenceConfig.cs`, `SubjectMapEncoder.cs`, and `SubjectMapSco.cs` only to `P4-T35`; `FlagTranslator.cs` is no longer on the remaining sub-80 reconciliation ledger; and no unchecked P1/P2/P3 implementation task references those files

- [ ] [P2-T19] Create or extend tests for EmailIntelligence collections: `PeopleScoDictionaryNew.cs` (3.2%), `RecentsList.cs` (0%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T20] Extend tests for observable linked lists: `LockingObservableLinkedList.cs` (24.8%), `LockingObservableLinkedListNode.cs` (20.4%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T21] Extend tests for the remaining timed helper implementation target: `TimedDiskWriter.cs` (66.3%)
  - Note: `SystemThemeDetector.cs` is deferred to `P4-T37`
  - Acceptance: Coverage report shows `TimedDiskWriter.cs` at >= 80% line rate

- [ ] [P2-T22] Create or extend tests for remaining miscellaneous medium helper implementation targets: `ClassifierGroupUtilities.cs` (0%), `Triage_OlLogic.cs` (40.4%)
  - Note: `QfcTipsDetails.cs` and `ShellUtilitiesStatic.cs` are deferred to `P4-T36`
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T23] Create or extend tests for the remaining data-dependent extension implementation target: `DfMLNet.cs` (0%)
  - Note: `AsyncSerialization.cs` is deferred to `P4-T33`, `DfDeedle.cs` is deferred to `P4-T36`, and `DrawingExtensions.cs` plus `ImageExtensions.cs` are no longer on the remaining sub-80 reconciliation ledger
  - Acceptance: Coverage report shows `DfMLNet.cs` at >= 80% line rate

- [ ] [P2-T24] Register all new Phase 2 test files in `UtilitiesCS.Test.csproj` via `<Compile Include>` entries
  - Acceptance: Every new `.cs` test file created in Phase 2 has a corresponding `<Compile Include>` entry in `UtilitiesCS.Test.csproj`; `msbuild` resolves all test files without missing-reference errors

- [ ] [P2-T25] Run Phase 2 checkpoint: build solution and run tests with coverage; verify all remaining Phase 2 implementation target files reach >= 80% line coverage
  - Preconditions: P2-T1 through P2-T24 complete
  - Acceptance: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits 0; `vstest.console.exe` exits 0 with no test failures; coverage report confirms every file still assigned to unchecked Phase 2 implementation tasks is at >= 80%, while Phase 4-routed files remain deferred to `P4-T33` through `P4-T37`

### Phase 3 — Hard Files: WinForms & Deep COM (~66 files)

**Goal:** Cover WinForms UI classes and deep COM interop classes. Strategy: extract testable logic from code-behind where needed, use Moq for COM interfaces, use STAThread context for control instantiation where required. Testability seams are permitted only when required to reach 80% coverage (per spec non-goals). Each task targets exactly one production file for true atomicity, except documented reconciliation retirements that exist only to preserve task-order continuity after Phase 4 routing.

- [x] [P3-T1] Create or extend tests for `ConversationHelper.cs` by mocking COM conversation traversal APIs
  - Acceptance: Coverage report shows `ConversationHelper.cs` (4%) at >= 80% line rate

- [ ] [P3-T2] Create or extend tests for `MailItemHelper.cs` by mocking COM mail operations
  - Acceptance: Coverage report shows `MailItemHelper.cs` (45.8%) at >= 80% line rate

- [ ] [P3-T3] Create or extend tests for `StoreWrapperController.cs` by mocking store/session COM objects
  - Acceptance: Coverage report shows `StoreWrapperController.cs` (33.9%) at >= 80% line rate

- [ ] [P3-T4] Create or extend tests for `OlTableExtensions.cs` by mocking COM Table interface
  - Acceptance: Coverage report shows `OlTableExtensions.cs` (4.7%) at >= 80% line rate

- [ ] [P3-T5] Create or extend tests for `OlToDoTable.cs` by mocking COM Table interface
  - Acceptance: Coverage report shows `OlToDoTable.cs` (0%) at >= 80% line rate

- [ ] [P3-T6] Create tests for `ActionableClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `ActionableClassifierGroup.cs` (0%) at >= 80% line rate

- [ ] [P3-T7] Create tests for `CategoryClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `CategoryClassifierGroup.cs` (0%) at >= 80% line rate

- [ ] [P3-T8] Create tests for `OlFolderClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `OlFolderClassifierGroup.cs` (0%) at >= 80% line rate

- [x] [P3-T9] Create tests for `ConditionalItemEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `ConditionalItemEngine.cs` (0%) at >= 80% line rate

- [ ] [P3-T10] Create tests for `MulticlassEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `MulticlassEngine.cs` (0%) at >= 80% line rate

- [x] [P3-T11] Create tests for `TristateEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `TristateEngine.cs` (0%) at >= 80% line rate

- [x] [P3-T12] Retire the former `SpamBayes.cs` implementation task after reconciliation routed the file to `P4-T36`
  - Acceptance: `evidence/baseline/remaining-sub80-reconciliation.md` maps `SpamBayes.cs` only to `P4-T36`, and no unchecked P1/P2/P3 implementation task references `SpamBayes.cs`

- [x] [P3-T13] Retire the former `ManagerAsyncLazy.cs` implementation task after reconciliation routed the file to `P4-T35`
  - Acceptance: `evidence/baseline/remaining-sub80-reconciliation.md` maps `ManagerAsyncLazy.cs` only to `P4-T35`, and no unchecked P1/P2/P3 implementation task references `ManagerAsyncLazy.cs`

- [ ] [P3-T14] Create or extend tests for `Triage.cs` with mocked globals
  - Acceptance: Coverage report shows `Triage.cs` (8.5%) at >= 80% line rate

- [x] [P3-T15] Extract testable logic from `InputBox.cs` and create tests
  - Acceptance: Coverage report shows `InputBox.cs` (0%) at >= 80% line rate

- [x] [P3-T16] Extract testable logic from `MyBox.cs` and create tests
  - Acceptance: Coverage report shows `MyBox.cs` (0%) at >= 80% line rate

- [x] [P3-T17] Extract testable logic from `NotImplementedDialog.cs` and create tests
  - Acceptance: Coverage report shows `NotImplementedDialog.cs` (0%) at >= 80% line rate

- [x] [P3-T18] Extract testable logic from `MyBoxViewer.cs` and create tests
  - Acceptance: Coverage report shows `MyBoxViewer.cs` (28.1%) at >= 80% line rate

- [x] [P3-T19] Extend tests for `DelegateButton.cs` button and dialog result logic
  - Acceptance: Coverage report shows `DelegateButton.cs` (51.6%) at >= 80% line rate

- [x] [P3-T20] Create tests for `FunctionButton.cs` button and dialog result logic
  - Acceptance: Coverage report shows `FunctionButton.cs` (0%) at >= 80% line rate

- [x] [P3-T21] Create or extend tests for `YesNoToAll.cs` button and dialog result logic
  - Acceptance: Coverage report shows `YesNoToAll.cs` (28.8%) at >= 80% line rate

- [x] [P3-T22] Create tests for `ControlPosition.cs` with WinForms control instantiation
  - Acceptance: Coverage report shows `ControlPosition.cs` (0%) at >= 80% line rate

- [x] [P3-T23] Create tests for `ControlResizer.cs` with WinForms control instantiation
  - Acceptance: Coverage report shows `ControlResizer.cs` (0%) at >= 80% line rate

- [x] [P3-T24] Create tests for `TableLayoutHelper.cs` with WinForms control instantiation
  - Acceptance: Coverage report shows `TableLayoutHelper.cs` (0%) at >= 80% line rate

- [x] [P3-T25] Create tests for `ScreenHelper.cs` with WinForms control instantiation
  - Acceptance: Coverage report shows `ScreenHelper.cs` (0%) at >= 80% line rate

- [x] [P3-T26] Create tests for `MouseDownFilter.cs` WinForms interaction helper
  - Acceptance: Coverage report shows `MouseDownFilter.cs` (0%) at >= 80% line rate

- [x] [P3-T27] Create tests for `ImageHelper.cs` WinForms interaction helper
  - Acceptance: Coverage report shows `ImageHelper.cs` (0%) at >= 80% line rate

- [x] [P3-T28] Create tests for `OlvExtension.cs` WinForms interaction helper
  - Acceptance: Coverage report shows `OlvExtension.cs` (0%) at >= 80% line rate

- [x] [P3-T29] Create or extend tests for `Theme.cs` with control instantiation
  - Acceptance: Coverage report shows `Theme.cs` (3.3%) at >= 80% line rate

- [x] [P3-T30] Create tests for `ThemeControlGroup.cs` with control instantiation
  - Acceptance: Coverage report shows `ThemeControlGroup.cs` (0%) at >= 80% line rate

- [x] [P3-T31] Create tests for `TipsController.cs` with control instantiation
  - Acceptance: Coverage report shows `TipsController.cs` (0%) at >= 80% line rate

- [x] [P3-T32] Extract and test controller logic from `FilterOlFoldersController.cs`
  - Acceptance: Coverage report shows `FilterOlFoldersController.cs` (0%) at >= 80% line rate

- [x] [P3-T33] Extract and test controller logic from `FolderRemapController.cs`
  - Acceptance: Coverage report shows `FolderRemapController.cs` (0%) at >= 80% line rate

- [x] [P3-T34] Extract and test controller logic from `FolderRemapTree.cs`
  - Acceptance: Coverage report shows `FolderRemapTree.cs` (0%) at >= 80% line rate

- [x] [P3-T35] Extract and test controller logic from `ConfigController.cs`
  - Acceptance: Coverage report shows `ConfigController.cs` (0%) at >= 80% line rate

- [x] [P3-T36] Create tests for `DispatchUtility.cs` COM dispatch interop
  - Acceptance: Coverage report shows `DispatchUtility.cs` (10.5%) at >= 80% line rate

- [x] [P3-T37] Create tests for `ShellUtilities.cs` system interop
  - Acceptance: Coverage report shows `ShellUtilities.cs` (0%) at >= 80% line rate

- [x] [P3-T38] Create tests for `ComStreamWrapper.cs` COM stream interop
  - Acceptance: Coverage report shows `ComStreamWrapper.cs` (0%) at >= 80% line rate

- [x] [P3-T39] Create tests for `OneDriveDownloader.cs` by mocking Microsoft.Graph API calls
  - Acceptance: Coverage report shows `OneDriveDownloader.cs` (0%) at >= 80% line rate

- [x] [P3-T40] Create tests for `IdleActionQueue.cs` idle queue logic
  - Acceptance: Coverage report shows `IdleActionQueue.cs` (0%) at >= 80% line rate

- [x] [P3-T41] Create tests for `IdleAsyncQueue.cs` async queue logic
  - Acceptance: Coverage report shows `IdleAsyncQueue.cs` (0%) at >= 80% line rate

- [x] [P3-T42] Create tests for `ApplicationIdleTimer.cs` idle timer logic
  - Acceptance: Coverage report shows `ApplicationIdleTimer.cs` (0%) at >= 80% line rate

- [x] [P3-T43] Extend tests for `UiThread.cs` UI thread utilities
  - Acceptance: Coverage report shows `UiThread.cs` (60%) at >= 80% line rate

- [x] [P3-T44] Create tests for `DvgForm.cs` by extracting testable logic
  - Acceptance: Coverage report shows `DvgForm.cs` (0%) at >= 80% line rate

- [x] [P3-T45] Create tests for `AutoFile.cs` by mocking Outlook APIs
  - Acceptance: Coverage report shows `AutoFile.cs` (0%) at >= 80% line rate

- [x] [P3-T46] Create tests for `EmailDataMiner.cs` by mocking Outlook APIs
  - Acceptance: Coverage report shows `EmailDataMiner.cs` (0%) at >= 80% line rate

- [x] [P3-T47] Create tests for `EmailFiler.cs` by mocking Outlook APIs
  - Acceptance: Coverage report shows `EmailFiler.cs` (0%) at >= 80% line rate

- [x] [P3-T48] Create tests for `MethodBodyReader.cs` by mocking IL reflection APIs
  - Acceptance: Coverage report shows `MethodBodyReader.cs` (0%) at >= 80% line rate

- [x] [P3-T49] Create tests for `ProgressPane.cs` by extracting testable logic from WinForms component
  - Acceptance: Coverage report shows `ProgressPane.cs` (0%) at >= 80% line rate

- [x] [P3-T50] Create tests for `ProgressViewer.cs` by extracting testable logic from WinForms component
  - Acceptance: Coverage report shows `ProgressViewer.cs` (0%) at >= 80% line rate

- [x] [P3-T51] Create tests for `ProgressMultiStepViewer.cs` by extracting testable logic from WinForms component
  - Acceptance: Coverage report shows `ProgressMultiStepViewer.cs` (0%) at >= 80% line rate

- [x] [P3-T52] Create tests for `ProgressTrackerPane.cs` by extracting testable logic from WinForms component
  - Acceptance: Coverage report shows `ProgressTrackerPane.cs` (0%) at >= 80% line rate

- [x] [P3-T53] Create tests for `FolderInfoViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `FolderInfoViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T54] Create tests for `FolderSelector.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `FolderSelector.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T55] Create tests for `ConfigViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `ConfigViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T56] Create tests for `ConfigGroupBox.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `ConfigGroupBox.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T57] Create tests for `SubjectMapMetrics.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `SubjectMapMetrics.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T58] Create tests for `OSBrowser.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `OSBrowser.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T59] Create tests for `FolderNotFoundViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `FolderNotFoundViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T60] Create tests for `InputBoxViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `InputBoxViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T61] Create tests for `FolderRemapViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `FolderRemapViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T62] Create tests for `FilterOlFoldersViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `FilterOlFoldersViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T63] Create tests for `ConfusionViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `ConfusionViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T64] Create tests for `MetricChartViewer.cs` by extracting testable logic
  - Note: If zero extractable logic after inspection, document as skip candidate in Phase 4
  - Acceptance: Coverage report shows `MetricChartViewer.cs` (0%) at >= 80% line rate; or file is documented as skip candidate in `evidence/other/skip-candidates.md` with rationale

- [x] [P3-T65] Create or extend tests for `WinFormsExtensions.cs`
  - Acceptance: Coverage report shows `WinFormsExtensions.cs` (13%) at >= 80% line rate

- [x] [P3-T66] Create tests for `CaptureEmailAddressesModule2.cs` with mocked COM interfaces
  - Acceptance: Coverage report shows `CaptureEmailAddressesModule2.cs` at >= 80% line rate; or confirmed not compiled by UtilitiesCS.csproj (excluded from scope)

- [ ] [P3-T67] Create or extend tests for `SortEmail.cs` by extracting or mocking the path-resolution, attachment-filtering, message-save, and sanitization branches that do not require live Outlook state
  - Acceptance: Coverage report shows `SortEmail.cs` (0%) at >= 80% line rate

- [ ] [P3-T68] Register all new Phase 3 test files in `UtilitiesCS.Test.csproj` via `<Compile Include>` entries; register any new production helper classes extracted for testability
  - Acceptance: Every new `.cs` file created in Phase 3 has a corresponding `<Compile Include>` entry in the appropriate `.csproj`; `msbuild` resolves all files without missing-reference errors

- [ ] [P3-T69] Run Phase 3 checkpoint: build solution and run tests with coverage; verify all Phase 3 target files reach >= 80% line coverage
  - Preconditions: P3-T1 through P3-T68 complete
  - Acceptance: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits 0; `vstest.console.exe` exits 0 with no test failures; coverage report confirms all Phase 3 target files at >= 80% (excluding files deferred to Phase 4 skip evaluation)

### Phase 4 — Skip Evaluation & Documentation

**Goal:** Evaluate files that cannot meaningfully be tested and document skip decisions with per-file rationale. No coverage gap may remain unevaluated. Each task evaluates one file or one coherent subdirectory of pure-interface files and records the result in `evidence/other/skip-candidates.md`.

#### Designer.cs Auto-Generated Files (P4-T1 through P4-T16)

- [x] [P4-T1] Evaluate `DvgForm.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `DvgForm.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T2] Evaluate `ConfusionViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `ConfusionViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T3] Evaluate `MetricChartViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `MetricChartViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T4] Evaluate `FilterOlFoldersViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `FilterOlFoldersViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T5] Evaluate `FolderInfoViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `FolderInfoViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T6] Evaluate `OSBrowser.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `OSBrowser.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T7] Evaluate `FolderRemapViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `FolderRemapViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T8] Evaluate `FolderSelector.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `FolderSelector.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T9] Evaluate `SubjectMapMetrics.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `SubjectMapMetrics.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T10] Evaluate `ConfigViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `ConfigViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T11] Evaluate `ProgressMultiStepViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `ProgressMultiStepViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T12] Evaluate `ProgressPane.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `ProgressPane.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T13] Evaluate `ProgressViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `ProgressViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T14] Evaluate `SyncContextForm.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `SyncContextForm.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T15] Evaluate `FolderNotFoundViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `FolderNotFoundViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

- [x] [P4-T16] Evaluate `InputBoxViewer.Designer.cs` and document skip rationale
  - Acceptance: `evidence/other/skip-candidates.md` lists `InputBoxViewer.Designer.cs` with rationale (auto-generated by WinForms designer, no testable logic)

#### Commented-Out Stubs (P4-T17 through P4-T20)

- [x] [P4-T17] Evaluate `ReusableTypeClasses/Observable/ObservableDictionary.cs` and confirm zero executable lines
  - Acceptance: `evidence/other/skip-candidates.md` confirms file has zero executable lines with explanation (entirely commented out; live implementation exists in UtilitiesSwordfish)

- [x] [P4-T18] Evaluate `ReusableTypeClasses/Concurrent/Observable/Bag/ConcurrentObservableBag.cs` and confirm zero executable lines
  - Acceptance: `evidence/other/skip-candidates.md` confirms file has zero executable lines with explanation (entirely commented out; no live implementation found in solution)

#### Deprecated Files (P4-T21 through P4-T22)

- [x] [P4-T22] Evaluate `FileIO2.cs` (0%) and document skip or removal decision
  - Acceptance: `evidence/other/skip-candidates.md` records decision for `FileIO2.cs` (skip with rationale or removal deferred to separate cleanup issue)

#### Pure-Interface Files by Subdirectory (P4-T23 through P4-T32)

- [x] [P4-T23] Evaluate `Interfaces/` root-level files and confirm no executable code: `PrefixInterface.cs`, `ITimerWrapper.cs`, `IGenericTimer.cs`, `Enums.cs`
  - Note: `Enums.cs` contains enum definitions rather than interfaces; confirm zero executable method bodies
  - Acceptance: `evidence/other/skip-candidates.md` lists each root-level file with confirmation of zero executable lines

- [x] [P4-T24] Evaluate `Interfaces/IWinForm/` files and confirm no executable code: `IUserControl.cs`, `IScrollableControl.cs`, `IForm.cs`, `IControlCollection.cs`, `IControl.cs`, `IContainerControl.cs`
  - Note: `PropertyStore.cs` in this directory was already covered in P1-T8; exclude from skip evaluation
  - Acceptance: `evidence/other/skip-candidates.md` lists each IWinForm interface file with confirmation of zero executable lines

- [x] [P4-T25] Evaluate `Interfaces/IToDo/` files and confirm no executable code: `IToDoItem.cs`, `ISubjectMapSco.cs`, `ISubjectMapEntry.cs`, `ISubjectMapEncoder.cs`, `IProjectInfoLegacy.cs`, `IProjectEntry.cs`, `IProjectData.cs`, `IPrefix.cs`, `IPeopleScoDictionaryNew.cs`, `IPeopleScoDictionary.cs`, `IIDList.cs`, `IFlagChangeTrainingQueue.cs`, `IFlagChangeItem.cs`, `IFlagChangeGroup.cs`, `IAutoAssign.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each IToDo interface file (15 files) with confirmation of zero executable lines

- [x] [P4-T26] Evaluate `Interfaces/IReusableTypeClasses/` root-level files and confirm no executable code: `IPercentageMatchable.cs`, `IOutlookItem.cs`, `ISerializableDictionary.cs`, `ISCODictionary.cs`, `IScoCollection2.cs`, `IScoCollection.cs`, `ISmartSerializable.cs`, `ISerializableList.cs`, `ISmartSerializableConfig.cs`, `ISmartSerializableNonTyped.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each root-level IReusableTypeClasses interface file (10 files) with confirmation of zero executable lines

- [x] [P4-T27] Evaluate `Interfaces/IReusableTypeClasses/` nested subdirectory files and confirm no executable code: `Concurrent/IConcurrentDictionary.cs`, `Observable/IObservableDictionary.cs`, `Concurrent/Observable/Dictionary/IDictionaryObserver.cs`, `Concurrent/Observable/Dictionary/IConcurrentObservableDictionary.cs`, `SerializableNew/Concurrent/Observable/IScoDictionaryNew.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each nested IReusableTypeClasses interface file (5 files) with confirmation of zero executable lines

- [x] [P4-T28] Evaluate `Interfaces/IEmailIntelligence/` files and confirm no executable code: `IMovedMailInfo.cs`, `IItemInfo.cs`, `IFolderWrapper.cs`, `IAttachment.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each IEmailIntelligence interface file (4 files) with confirmation of zero executable lines

- [x] [P4-T29] Evaluate `Interfaces/IHelperClasses/` files and confirm no executable code: `IFileSystemInfo.cs`, `IFileInfo.cs`, `IDirectoryInfo.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each IHelperClasses interface file (3 files) with confirmation of zero executable lines

- [x] [P4-T30] Evaluate `Interfaces/IGlobals/` files and confirm no executable code: `IToDoObjects.cs`, `IToDoObj.cs`, `IOlObjects.cs`, `IFileSystemFolderPaths.cs`, `IConditionalEngine.cs`, `IAppStagingFilenames.cs`, `IAppQuickFilerSettings.cs`, `IApplicationGlobals.cs`, `IAppItemEngines.cs`, `IAppEvents.cs`, `IAppAutoFileObjects.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each IGlobals interface file (11 files) with confirmation of zero executable lines

- [x] [P4-T31] Evaluate `Interfaces/IQuickFiler/` files and confirm no executable code: `IQfcTipsDetails.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists `IQfcTipsDetails.cs` with confirmation of zero executable lines

- [x] [P4-T32] Evaluate `Interfaces/IOutlookObjects/` files and confirm no executable code: `IRecipientInfo.cs`, `IOutlookItemFlaggable.cs`, `IEmailDetailsWrapper.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each IOutlookObjects interface file (3 files) with confirmation of zero executable lines

#### Reconciled Non-Skip Files with Evidence-Backed Skip Constraints (P4-T33 through P4-T38)

- [ ] [P4-T33] Evaluate file-I/O constrained coverage candidates: `ScBag.cs`, `SCODictionary.cs`, `CorpusInherit.cs`, `AsyncSerialization.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each of the four files with a deterministic rationale tied to the repo no-temp-files policy and the specific unreachable file-system branches documented in current coverage evidence

- [ ] [P4-T34] Evaluate UI-thread and runtime-constrained threading candidates: `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `AsyncMultiTasker.cs`, `ThreadMonitor.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each of the four files with a deterministic rationale tied to UI-thread dispatch, COM/runtime coupling, or deprecated thread APIs documented in current coverage evidence

- [ ] [P4-T35] Evaluate resource-loading and manager-coupled candidates: `IntelligenceConfig.cs`, `SubjectMapEncoder.cs`, `SubjectMapSco.cs`, `ManagerAsyncLazy.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each of the four files with a deterministic rationale tied to resource-manager loading, configuration persistence side effects, or live globals/manager dependencies documented in current coverage evidence

- [ ] [P4-T36] Evaluate runtime-bound helper and classifier candidates: `QfcTipsDetails.cs`, `DfDeedle.cs`, `SpamBayes.cs`, `ShellUtilitiesStatic.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists each of the four files with a deterministic rationale tied to WinForms runtime requirements, Outlook COM requirements, stub/no-op structure, or live shell execution documented in current coverage evidence

- [ ] [P4-T37] Evaluate environment-bound detection candidate: `SystemThemeDetector.cs`
  - Acceptance: `evidence/other/skip-candidates.md` lists `SystemThemeDetector.cs` with a deterministic rationale tied to non-injectable registry state and the unreachable negative/error branches documented in current coverage evidence

- [ ] [P4-T38] Cross-check the reconciled remainder after Phase 4 additions
  - Preconditions: P0-T5 through P0-T6 and P4-T1 through P4-T37 complete
  - Acceptance: `evidence/baseline/remaining-sub80-reconciliation.md` maps every file still listed below 80% to exactly one unchecked implementation task or one completed Phase 4 skip task; no unmapped file remains

#### Finalization (P4-T39)

- [ ] [P4-T39] Finalize skip evaluation for all files deferred as untestable under current repo policy constraints
  - Acceptance: `evidence/other/skip-candidates.md` is complete; every UtilitiesCS production file is either at >= 80% coverage or documented as a skip candidate with rationale; no file is left unevaluated

### Phase 5 — Final QA Loop

**Goal:** Run the full C# toolchain loop and verify all coverage targets are met. If any step fails or changes files, restart the loop from step 1 (format check) until a clean pass completes.

- [ ] [P5-T1] Run csharpier format check on all modified and new `.cs` files: `csharpier .`
  - Acceptance: `csharpier .` exits 0 with no files changed; evidence artifact at `evidence/qa-gates/final-qa-format.md` with `Timestamp:`, `Command: csharpier .`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P5-T2] Run analyzer build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: Build exits 0 with zero analyzer errors; evidence artifact at `evidence/qa-gates/final-qa-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P5-T3] Run nullable build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: Build exits 0 with zero nullable warnings; evidence artifact at `evidence/qa-gates/final-qa-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [ ] [P5-T4] Run full test suite with coverage: `vstest.console.exe <all-test-assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
  - Acceptance: All tests pass (zero failures); evidence artifact at `evidence/qa-gates/final-qa-test-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and UtilitiesCS line coverage percentage

- [ ] [P5-T5] Verify all UtilitiesCS production files reach >= 80% line coverage (excluding documented skip candidates from Phase 4), and verify repo-wide coverage does not regress below P0-T3 baseline
  - Acceptance: Coverage analysis shows zero non-skip files below 80%; evidence artifact at `evidence/qa-gates/final-coverage-verification.md` comparing baseline per-file rates (from P0-T4) with post-change rates; repo-wide UtilitiesCS coverage >= baseline value from P0-T3

- [ ] [P5-T6] Verify no pre-existing test regressions by comparing test counts and pass rates against P0-T3 baseline
  - Acceptance: Total test count >= baseline count from P0-T3; pass rate >= baseline pass rate; zero previously-passing tests now failing

- [ ] [P5-T7] Store final coverage evidence and update feature folder status
  - Acceptance: Updated `coverage/coverage.cobertura.xml` reflects final state; `issue.md` acceptance criteria checkboxes updated to reflect verified state; plan status updated to "Complete"

## Test Plan

- **Unit:** MSTest test classes in UtilitiesCS.Test for every UtilitiesCS production file, organized by namespace mirroring subfolder structure (e.g., `UtilitiesCS.Test.Extensions`, `UtilitiesCS.Test.HelperClasses`, `UtilitiesCS.Test.ReusableTypeClasses`)
- **Naming:** Test class `{ProductionClass}_Tests`; test method `{Method}_{Scenario}_{Expected}`; AAA comments
- **Coverage gate:** >= 80% line rate per file as reported by Cobertura XML from `vstest.console.exe /EnableCodeCoverage`
- **Mocking:** Moq for COM interop (`Mock<Outlook.MailItem>`, `Mock<Outlook.MAPIFolder>`, etc.), file system wrappers, and IApplicationGlobals
- **No integration tests:** All tests are unit-level with Moq mocking for COM, file system, and external dependencies
- **No temp files:** All file I/O is mocked via MemoryStream/StringWriter injection per repo policy
- **Verification checkpoints:** Phase 1 (P1-T14), Phase 2 (P2-T25), Phase 3 (P3-T69), and final QA (Phase 5)

## Open Questions / Notes

- **File count discrepancy:** Issue states ~196 files below 80%, but research identifies ~155 with explicit line-rate below 80% in Cobertura plus ~16 Designer.cs at 0%, ~4 commented stubs, and ~40+ pure interfaces. The plan covers all categories; Phase 4 reconciles the full count.
- **Obsolete Bayesian code:** Files in `EmailIntelligence/Bayesian/Obsolete/` are legacy but still compiled. Included in Phase 2 testing (P2-T11).
- **CaptureEmailAddressesModule2.cs:** Not in coverage report. Phase 3 task P3-T66 will verify whether it is compiled by UtilitiesCS.csproj before testing.
- **WinForms viewer testability:** Some Phase 3 viewer tasks (P3-T53 through P3-T64) may have zero extractable logic. If so, they are documented as skip candidates in Phase 4 rather than blocking Phase 3 completion.
- **UtilitiesCS.Test explicit Compile Include:** Per repo convention (old-style csproj), every new test .cs file must be registered in `UtilitiesCS.Test.csproj` or it silently fails to compile. Enforced by registration tasks P1-T13, P2-T24, P3-T68.
- **Reconciliation authority:** `evidence/baseline/remaining-sub80-reconciliation.md` is the authoritative ledger for the remaining execution path; after P0-T5/P0-T6, each file still below 80% must point to exactly one implementation task or one Phase 4 skip task.
- **Execution-order boundary:** Remaining unchecked Phase 2 and Phase 3 implementation tasks now contain only implementation-routed files; formerly mixed or fully rerouted batches have been narrowed or retired in place so executor task order no longer crosses into Phase 4-owned files.
- **Rollback strategy:** Each phase is independently verifiable. If a phase introduces test failures, revert that phase's changes and re-examine the failing files before retrying.
- **Silent-failure risk:** Unregistered test files compile silently as absent. The registration tasks and phase checkpoints catch this by verifying build resolution.
