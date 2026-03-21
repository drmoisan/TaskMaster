# 2026-03-19-utilities-coverage-part-three - Plan

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-20T10-00
- **Status:** In Progress
- **Version:** 1.0

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

Raise every production .cs file compiled by UtilitiesCS.csproj to >= 80% line coverage by adding or extending MSTest unit tests in UtilitiesCS.Test. Work is phased by testability difficulty (Easy → Medium → Hard), followed by skip evaluation for untestable files (Designer.cs, commented stubs, pure interfaces) and a final QA loop verifying the full C# toolchain passes clean. Approximately 155 files have explicit line-rate below 80% in the Cobertura report, plus ~16 Designer.cs files, ~4 commented stubs, and ~40+ pure interface files with no executable code.

## Acceptance Criteria Traceability

| AC | Source (issue.md) | Plan Coverage |
|---|---|---|
| AC1 | Every .cs file compiled by UtilitiesCS.csproj >= 80% line coverage | P1–P3 implementation tasks + P5-T5 verification |
| AC2 | No pre-existing tests broken or removed | P0-T3 baseline + P5-T6 verification |
| AC3 | All new tests follow MSTest + Moq + FluentAssertions conventions | P0-T1 policy read + all P1–P3 implementation tasks |
| AC4 | All new tests deterministic, isolated, no external deps | P0-T1 policy read + all P1–P3 implementation tasks |
| AC5 | All new test files registered in UtilitiesCS.Test.csproj | P1-T13, P2-T24, P3-T67 registration tasks |
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

**Goal:** Cover classes requiring Moq-based mocking for interfaces, file system, Newtonsoft JSON, Bayesian logic, and COM interop with established mock patterns. Tests must use MemoryStream/StringWriter injection for any serialization testing (no temp files per policy).

- [x] [P2-T1] Create or extend tests for JSON converters batch 1: `ScDictionaryConverter.cs` (9.1%), `NonRecursiveConverter.cs` (0%), `PeopleScoConverter.cs` (66.7%), `PeopleScoRemainingObjectConverter.cs` (40%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T2] Create or extend tests for JSON converters batch 2: `DerivedCompositionConverter_ConcurrentDictionary.cs` (0%), `MonoExtension.cs` (39.1%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T3] Create tests for SDIL Reader data models: `ILGlobals.cs` (0%), `ILInstruction.cs` (0%)
  - Acceptance: Test classes exist; coverage report shows both files at >= 80% line rate

- [x] [P2-T4] Create or extend tests for NewtonsoftHelpers wrapper: `WrapperPeopleScoDictionaryNew.cs` (12.6%)
  - Acceptance: Coverage report shows file at >= 80% line rate

- [ ] [P2-T5] Create or extend tests for SCO core collections: `ScBag.cs` (20.4%), `ScDictionary.cs` (8.6%), `SCODictionary.cs` (4.3%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P2-T6] Create or extend tests for SCO variant collections: `ScoCollection.cs` (4.4%), `ScoSortedDictionary.cs` (7.4%), `ScoStack.cs` (40.2%), `ScoDictionaryNew.cs` (15.5%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T7] Create or extend tests for serializable lists: `SerializableList.cs` (35.9%), `SloLinkedList.cs` (29.7%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [x] [P2-T8] Create or extend tests for SmartSerializable core: `SmartSerializable.cs` (15.9%), `SmartSerializableBase.cs` (0%), `SmartSerializableNonTyped.cs` (72%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [x] [P2-T9] Create or extend tests for SmartSerializable infrastructure: `SmartSerializableLoader.cs` (7.1%), `SmartSerializableStatic.cs` (0%), `NewSmartSerializableConfig.cs` (29.5%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [ ] [P2-T10] Extend tests for Bayesian classifiers: `BayesianClassifierShared.cs` (63.8%), `BayesianClassifierGroup.cs` (22.5%), `BayesianClassifierExtensions.cs` (20.5%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [ ] [P2-T11] Create or extend tests for Corpus and legacy Bayesian: `Corpus.cs` (33.9%), `CorpusInherit.cs` (0%), `Obsolete/BayesianClassifier.cs` (65.1%), `Obsolete/ClassifierGroup.cs` (8.2%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T12] Create tests for Bayesian performance measurement: `BayesianPerformanceMeasurement.cs` (0%), `BayesianSerializationHelper.cs` (0%)
  - Acceptance: Test classes exist; coverage report shows both files at >= 80% line rate

- [ ] [P2-T13] Extend tests for OutlookItem core using Moq COM mocking: `OutlookItem.cs` (54.5%), `OutlookItemExtensions.cs` (44.9%), `OlItemPseudoInterface.cs` (55.4%)
  - Acceptance: Coverage report shows all three files at >= 80% line rate

- [ ] [P2-T14] Extend tests for OutlookItem try-patterns: `OutlookItemTry.cs` (35.5%), `OutlookItemTryGet.cs` (21.6%), `OutlookItemFlaggable.cs` (58.2%), `OutlookItemFlaggableTry.cs` (51%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [ ] [P2-T15] Extend tests for OutlookObjects helpers: `AttachmentHelper.cs` (69.9%), `AttachmentSerializable.cs` (54.7%), `CreateCategory.cs` (65.5%), `RecipientStatic.cs` (46.7%), `UserDefinedFields.cs` (26%), `StoreWrapper.cs` (71.7%)
  - Acceptance: Coverage report shows all six files at >= 80% line rate

- [x] [P2-T16] Create or extend tests for file system wrappers with mocked I/O: `FileInfoWrapper.cs` (17.8%), `DirectoryInfoWrapper.cs` (20.3%), `FileSystemInfoWrapper.cs` (0%), `FilePathHelper.cs` (18.8%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [ ] [P2-T17] Create or extend tests for progress and thread tracking: `ProgressTracker.cs` (47%), `ProgressTrackerAsync.cs` (0%), `AsyncMultiTasker.cs` (0%), `ThreadMonitor.cs` (0%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [ ] [P2-T18] Create or extend tests for EmailIntelligence domain logic: `FlagTranslator.cs` (41.2%), `IntelligenceConfig.cs` (7.3%), `SubjectMapEncoder.cs` (0%), `SubjectMapSco.cs` (4.1%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [x] [P2-T19] Create or extend tests for EmailIntelligence collections: `PeopleScoDictionaryNew.cs` (3.2%), `RecentsList.cs` (0%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T20] Extend tests for observable linked lists: `LockingObservableLinkedList.cs` (24.8%), `LockingObservableLinkedListNode.cs` (20.4%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T21] Extend tests for timed and system helpers: `TimedDiskWriter.cs` (66.3%), `SystemThemeDetector.cs` (62.5%)
  - Acceptance: Coverage report shows both files at >= 80% line rate

- [ ] [P2-T22] Create or extend tests for miscellaneous medium helpers: `QfcTipsDetails.cs` (0%), `ShellUtilitiesStatic.cs` (33.3%), `ClassifierGroupUtilities.cs` (0%), `Triage_OlLogic.cs` (40.4%)
  - Acceptance: Coverage report shows all four files at >= 80% line rate

- [ ] [P2-T23] Create or extend tests for data-dependent Extensions: `DrawingExtensions.cs` (0%), `ImageExtensions.cs` (35.9%), `AsyncSerialization.cs` (11.6%), `DfDeedle.cs` (0%), `DfMLNet.cs` (0%)
  - Acceptance: Coverage report shows all five files at >= 80% line rate

- [x] [P2-T24] Register all new Phase 2 test files in `UtilitiesCS.Test.csproj` via `<Compile Include>` entries
  - Acceptance: Every new `.cs` test file created in Phase 2 has a corresponding `<Compile Include>` entry in `UtilitiesCS.Test.csproj`; `msbuild` resolves all test files without missing-reference errors

- [ ] [P2-T25] Run Phase 2 checkpoint: build solution and run tests with coverage; verify all Phase 2 target files reach >= 80% line coverage
  - Preconditions: P2-T1 through P2-T24 complete
  - Acceptance: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits 0; `vstest.console.exe` exits 0 with no test failures; coverage report confirms all Phase 2 target files at >= 80%

### Phase 3 — Hard Files: WinForms & Deep COM (~66 files)

**Goal:** Cover WinForms UI classes and deep COM interop classes. Strategy: extract testable logic from code-behind where needed, use Moq for COM interfaces, use STAThread context for control instantiation where required. Testability seams are permitted only when required to reach 80% coverage (per spec non-goals). Each task targets exactly one production file for true atomicity.

- [x] [P3-T1] Create or extend tests for `ConversationHelper.cs` by mocking COM conversation traversal APIs
  - Acceptance: Coverage report shows `ConversationHelper.cs` (4%) at >= 80% line rate

- [x] [P3-T2] Create or extend tests for `MailItemHelper.cs` by mocking COM mail operations
  - Acceptance: Coverage report shows `MailItemHelper.cs` (45.8%) at >= 80% line rate

- [x] [P3-T3] Create or extend tests for `StoreWrapperController.cs` by mocking store/session COM objects
  - Acceptance: Coverage report shows `StoreWrapperController.cs` (33.9%) at >= 80% line rate

- [x] [P3-T4] Create or extend tests for `OlTableExtensions.cs` by mocking COM Table interface
  - Acceptance: Coverage report shows `OlTableExtensions.cs` (4.7%) at >= 80% line rate

- [x] [P3-T5] Create or extend tests for `OlToDoTable.cs` by mocking COM Table interface
  - Acceptance: Coverage report shows `OlToDoTable.cs` (0%) at >= 80% line rate

- [x] [P3-T6] Create tests for `ActionableClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `ActionableClassifierGroup.cs` (0%) at >= 80% line rate

- [x] [P3-T7] Create tests for `CategoryClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `CategoryClassifierGroup.cs` (0%) at >= 80% line rate

- [x] [P3-T8] Create tests for `OlFolderClassifierGroup.cs` with mocked IApplicationGlobals
  - Acceptance: Coverage report shows `OlFolderClassifierGroup.cs` (0%) at >= 80% line rate

- [x] [P3-T9] Create tests for `ConditionalItemEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `ConditionalItemEngine.cs` (0%) at >= 80% line rate

- [x] [P3-T10] Create tests for `MulticlassEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `MulticlassEngine.cs` (0%) at >= 80% line rate

- [x] [P3-T11] Create tests for `TristateEngine.cs` with mocked COM items
  - Acceptance: Coverage report shows `TristateEngine.cs` (0%) at >= 80% line rate

- [ ] [P3-T12] Create tests for `SpamBayes.cs` with mocked COM items
  - Acceptance: Coverage report shows `SpamBayes.cs` (0%) at >= 80% line rate

- [ ] [P3-T13] Create tests for `ManagerAsyncLazy.cs` with mocked globals
  - Acceptance: Coverage report shows `ManagerAsyncLazy.cs` (0%) at >= 80% line rate

- [x] [P3-T14] Create or extend tests for `Triage.cs` with mocked globals
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

- [x] [P3-T67] Register all new Phase 3 test files in `UtilitiesCS.Test.csproj` via `<Compile Include>` entries; register any new production helper classes extracted for testability
  - Acceptance: Every new `.cs` file created in Phase 3 has a corresponding `<Compile Include>` entry in the appropriate `.csproj`; `msbuild` resolves all files without missing-reference errors

- [ ] [P3-T68] Run Phase 3 checkpoint: build solution and run tests with coverage; verify all Phase 3 target files reach >= 80% line coverage
  - Preconditions: P3-T1 through P3-T67 complete
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

- [x] [P4-T19] Evaluate `To Depricate/StackObjectVB.cs` and confirm zero executable lines
  - Acceptance: `evidence/other/skip-candidates.md` confirms file has zero executable lines with explanation (entirely commented out or dead code)

- [x] [P4-T20] Evaluate `To Depricate/FlattenArray.cs` and confirm zero executable lines
  - Acceptance: `evidence/other/skip-candidates.md` confirms file has zero executable lines with explanation (entirely commented out or dead code)

#### Deprecated Files (P4-T21 through P4-T22)

- [x] [P4-T21] Evaluate `CSVDictUtilities.cs` (0%) and document skip or removal decision
  - Acceptance: `evidence/other/skip-candidates.md` records decision for `CSVDictUtilities.cs` (skip with rationale or removal deferred to separate cleanup issue)

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

#### Finalization (P4-T33)

- [x] [P4-T33] Finalize skip evaluation for any Phase 3 WinForms viewers deferred as untestable
  - Acceptance: `evidence/other/skip-candidates.md` is complete; every UtilitiesCS production file is either at >= 80% coverage or documented as a skip candidate with rationale; no file is left unevaluated

### Phase 5 — Final QA Loop

**Goal:** Run the full C# toolchain loop and verify all coverage targets are met. If any step fails or changes files, restart the loop from step 1 (format check) until a clean pass completes.

- [x] [P5-T1] Run csharpier format check on all modified and new `.cs` files: `csharpier .`
  - Acceptance: `csharpier .` exits 0 with no files changed; evidence artifact at `evidence/qa-gates/final-qa-format.md` with `Timestamp:`, `Command: csharpier .`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P5-T2] Run analyzer build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: Build exits 0 with zero analyzer errors; evidence artifact at `evidence/qa-gates/final-qa-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P5-T3] Run nullable build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: Build exits 0 with zero nullable warnings; evidence artifact at `evidence/qa-gates/final-qa-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P5-T4] Run full test suite with coverage: `vstest.console.exe <all-test-assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
  - Acceptance: All tests pass (zero failures); evidence artifact at `evidence/qa-gates/final-qa-test-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total test count, pass count, and UtilitiesCS line coverage percentage

- [ ] [P5-T5] Verify all UtilitiesCS production files reach >= 80% line coverage (excluding documented skip candidates from Phase 4), and verify repo-wide coverage does not regress below P0-T3 baseline
  - Acceptance: Coverage analysis shows zero non-skip files below 80%; evidence artifact at `evidence/qa-gates/final-coverage-verification.md` comparing baseline per-file rates (from P0-T4) with post-change rates; repo-wide UtilitiesCS coverage >= baseline value from P0-T3

- [x] [P5-T6] Verify no pre-existing test regressions by comparing test counts and pass rates against P0-T3 baseline
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
- **Verification checkpoints:** Phase 1 (P1-T14), Phase 2 (P2-T25), Phase 3 (P3-T68), and final QA (Phase 5)

## Open Questions / Notes

- **File count discrepancy:** Issue states ~196 files below 80%, but research identifies ~155 with explicit line-rate below 80% in Cobertura plus ~16 Designer.cs at 0%, ~4 commented stubs, and ~40+ pure interfaces. The plan covers all categories; Phase 4 reconciles the full count.
- **Obsolete Bayesian code:** Files in `EmailIntelligence/Bayesian/Obsolete/` are legacy but still compiled. Included in Phase 2 testing (P2-T11).
- **CaptureEmailAddressesModule2.cs:** Not in coverage report. Phase 3 task P3-T25 will verify whether it is compiled by UtilitiesCS.csproj before testing.
- **WinForms viewer testability:** Some viewers (P3-T22, P3-T23) may have zero extractable logic. If so, they are documented as skip candidates in Phase 4 rather than blocking Phase 3 completion.
- **UtilitiesCS.Test explicit Compile Include:** Per repo convention (old-style csproj), every new test .cs file must be registered in `UtilitiesCS.Test.csproj` or it silently fails to compile. Enforced by registration tasks P1-T13, P2-T24, P3-T26.
- **Rollback strategy:** Each phase is independently verifiable. If a phase introduces test failures, revert that phase's changes and re-examine the failing files before retrying.
- **Silent-failure risk:** Unregistered test files compile silently as absent. The registration tasks and phase checkpoints catch this by verifying build resolution.
