<!-- markdownlint-disable-file -->

# Task Research Notes: UtilitiesCS Unit Test Coverage Implementation Strategy

## Research Executed

### File Analysis

- **UtilitiesCS/** — 359 production .cs files (excluding Designer, bin, obj, Properties)
- **UtilitiesCS.Test/** — 74 existing test files across all directories
- **coverage/coverage.cobertura.xml** — Package-level: line-rate=0.145 (14.5%), branch-rate=0.131 (13.1%)

### Code Search Results

- Complete directory listing of all 13 top-level subdirectories and all nested subdirectories
- Per-class coverage data extracted for every UtilitiesCS class from Cobertura XML
- All existing test files inventoried with patterns analyzed

### Project Conventions

- Standards referenced: csharp-code-change.instructions.md, csharp-unit-test.instructions.md, general-unit-test.instructions.md
- Framework: MSTest, Moq, FluentAssertions
- Toolchain: `dotnet format` → MSBuild analyzers → MSBuild nullable → vstest.console.exe
- File limit: 500 lines per file (production and test)
- Target: ≥80% per-file line coverage; ≥90% for new test modules

---

## Key Discoveries

### 1. Production File Inventory by Directory

| Directory | Production Files | Existing Test Files | Gap |
|-----------|-----------------|--------------------|----|
| EmailIntelligence/ | 72 | 20 | 52 |
| Interfaces/ | 63 | 0 | N/A (Skip) |
| ReusableTypeClasses/ | 53 | 6 | 47 |
| OutlookObjects/ | 50 | 4 | 46 |
| HelperClasses/ | 38 | 10 | 28 |
| Extensions/ | 24 | 3 | 21 |
| Threading/ | 20 | 2 | 18 |
| NewtonsoftHelpers/ | 19 | 6 | 13 |
| Dialogs/ | 11 | 4 | 7 |
| To Depricate/ | 5 | 0 | N/A (Skip) |
| OneDriveHelpers/ | 2 | 2 | 0 |
| Examples/ | 1 | 0 | N/A (Skip) |
| WindowsAPI/ | 1 | 0 | N/A (Skip) |

### 2. Existing Test Pattern Analysis

Patterns observed across all existing test files:

- **Framework**: MSTest `[TestClass]`/`[TestMethod]` used consistently
- **Mocking**: `MockRepository` pattern with `MockBehavior.Loose` or `MockBehavior.Strict`; Moq used for interfaces (JsonReader, IFileSystemFolderPaths, IApplicationGlobals)
- **Assertions**: Mix of MSTest `Assert.*` (older tests) and FluentAssertions `.Should().*` (newer tests)
- **Structure**: Arrange-Act-Assert consistently; `[TestInitialize]` for setup; helper classes/methods in `#region` blocks
- **Naming**: Mix of `MethodName_StateUnderTest_ExpectedBehavior` and descriptive names
- **Debug output**: `Console.SetOut(new DebugTextWriter())` common in TestInitialize
- **Subclassing**: Protected members exposed via test subclasses (e.g., `BayesianClassifierSub`, `CorpusSub`, `ClassifierGroupSub`)
- **Unfinished stubs**: Files in `Z.Unfinished.*` namespaces contain commented-out test skeletons — not counted as coverage

### 3. Coverage Highlights from Cobertura Data

**Well-covered classes (>70% line rate):**
- `UtilitiesCS.ActionButton` — 97.5%
- `UtilitiesCS.AppGlobalsConverter` — 100%
- `UtilitiesCS.CtfMapEntry` — 100%
- `UtilitiesCS.DebugTextWriter` — 100%
- `UtilitiesCS.DASLFilterParser` — 90.3%
- `NLogTraceWriter` — 95.5%
- `UtilitiesCS.CtfIncidence` — 72.4%
- `ObjectListViewDemo.MyFileSystemInfo` — 80%
- `SysImageListHelper` — 78.8%
- `SyncContextForm` — 78.6%
- `DelegateButtonTemplate` — 92.1%
- `BayesianClassifier` — 66.3%
- `BayesianClassifierShared` — 62.2%
- `AttachmentHelper` — 58.2%

**Zero-coverage classes (0% line rate — large or important):**
- All `ArrayExtensions` (5.4%), `AsyncSerialization`, `DfDeedle`, `DfMLNet`
- `DictionaryExtensions` — 22% (partially tested)
- `EmailDetails` — 11.2%
- Most `ClassifierGroups/*`, `EmailDataMiner`, `ConvHelper`
- All `OutlookItem*`, `FolderWrapper`, `FolderNavigator`, `FolderTree`
- All `SmartSerializable*`, `Concurrent*` collections
- All `Threading/` classes except some partial
- Most `Dialogs/` (except ActionButton, DelegateButtonTemplate)

---

## 4. Full File Inventory with Testability Assessment

### Legend
- **Testability**: Easy (pure logic, no deps), Medium (needs mocking), Hard (COM/UI/heavy deps), Skip (generated/interface/deprecated)
- **Has Tests**: Y (has dedicated tests), P (partial), N (none)
- **Priority**: P1 (highest value, easiest), P2 (medium), P3 (hard/low value), Skip

---

### Extensions/ (24 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| ArrayExtensions.cs | Easy | N | 5.4% | 25-30 | P1 |
| StringExtensions.cs | Easy | N | ~0% | 20-25 | P1 |
| DictionaryExtensions.cs | Easy | P | 22% | 15-20 | P1 |
| IEnumerableExtensions.cs | Easy | N | ~0% | 15-20 | P1 |
| IListExtensions.cs | Easy | N | ~0% | 10-15 | P1 |
| EnumExtensions.cs | Easy | N | ~0% | 8-10 | P1 |
| NullExtensions.cs | Easy | N | ~0% | 8-10 | P1 |
| ExceptionExtensions.cs | Easy | N | ~0% | 5-8 | P1 |
| QueueExtensions.cs | Easy | N | ~0% | 5-8 | P1 |
| LazyExtension.cs | Easy | N | ~0% | 5-8 | P1 |
| ExtToChar.cs | Easy | N | ~0% | 5-8 | P1 |
| TraceExtensions.cs | Easy | N | ~0% | 5-8 | P1 |
| JsonExtensions.cs | Medium | N | ~0% | 10-12 | P1 |
| JsonSerializerExtensions.cs | Medium | N | ~0% | 8-10 | P1 |
| CompilerServicesExtensions.cs | Easy | N | ~0% | 3-5 | P1 |
| StreamExtensions.cs | Medium | N | ~0% | 5-8 | P2 |
| AsyncSerialization.cs | Medium | N | 0% | 10-15 | P2 |
| DfDeedle.cs | Hard | N | 0% | N/A | P3 |
| DfMLNet.cs | Hard | N | 0% | N/A | P3 |
| IAsyncEnumerableExtensions.cs | Medium | N | ~0% | 5-8 | P2 |
| DrawingExtensions.cs | Hard (UI) | N | 0% | N/A | Skip |
| ImageExtensions.cs | Hard (UI) | N | ~0% | N/A | Skip |
| IControlExtensions.cs | Hard (UI) | N | ~0% | N/A | Skip |
| WinFormsExtensions.cs | Hard (UI) | N | ~0% | N/A | Skip |

### HelperClasses/ (38 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| Tokenizer.cs | Easy | Y | good | 5 (expand) | P1 |
| SimpleRegex.cs | Easy | Y | partial | 8-10 | P1 |
| PrettyPrint.cs | Easy | Y | partial | 8-10 | P1 |
| MergeSortImplementations.cs | Easy | N | ~0% | 10-15 | P1 |
| ParamArray.cs | Easy | N | ~0% | 5-8 | P1 |
| ObjectSize.cs | Easy | N | ~0% | 3-5 | P1 |
| ReflectionHelper.cs | Medium | N | ~0% | 5-8 | P1 |
| SegmentStopWatch.cs | Easy | N | ~0% | 8-10 | P1 |
| Initializer.cs | Medium | N | ~0% | 5-8 | P2 |
| GenericBitwise.cs | Easy | N | ~0% | 8-10 | P1 |
| DeepCompare.cs | Easy | N | ~0% | 8-10 | P1 |
| ObjectCopier.cs | Medium | N | ~0% | 5-8 | P2 |
| DispatchUtility.cs | Hard (COM) | N | ~0% | N/A | Skip |
| FilePathHelper.cs | Medium | Y (via converter) | partial | 5-8 | P2 |
| MyFileSystemInfo.cs | Medium | Y | 80% | 3 (expand) | P2 |
| ShellUtilities.cs | Hard (shell) | Y | 0% | N/A | P3 |
| ShellUtilitiesStatic.cs | Hard (shell) | Y | 39% | N/A | P3 |
| SysImageListHelper.cs | Hard (native) | Y | 79% | 3 (expand) | P3 |
| DirectoryInfoWrapper.cs | Medium | N | ~0% | 5-8 | P2 |
| FileInfoWrapper.cs | Medium | N | ~0% | 5-8 | P2 |
| FileSystemInfoWrapper.cs | Medium | N | ~0% | 5-8 | P2 |
| DebugTextLogger.cs | Easy | N | ~0% | 3-5 | P2 |
| DebugTextWriter.cs | Easy | Y (indirect) | 100% | 0 | Done |
| TraceUtility.cs | Easy | N | ~0% | 3-5 | P2 |
| VerboseLogger.cs | Easy | N | ~0% | 3-5 | P2 |
| DvgForm.cs | Hard (UI) | N | 0% | N/A | Skip |
| Theme.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ThemeControlGroup.cs | Hard (UI) | N | ~0% | N/A | Skip |
| QfcTipsDetails.cs | Medium | N | ~0% | 3-5 | P3 |
| TipsController.cs | Hard (UI) | N | 0% | N/A | Skip |
| ControlPosition.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ControlResizer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ImageHelper.cs | Hard (UI) | N | ~0% | N/A | Skip |
| MouseDownFilter.cs | Hard (UI) | N | ~0% | N/A | Skip |
| OlvExtension.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ScreenHelper.cs | Hard (UI) | N | ~0% | N/A | Skip |
| TableLayoutHelper.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ComStreamWrapper.cs | Hard (COM) | N | 0% | N/A | Skip |

### Threading/ (20 files — excluding Designer .cs)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| TaskPriority.cs | Easy | N | ~0% | 5-8 | P1 |
| ThreadSafeSingleShotGuard.cs | Easy | N | ~0% | 5-8 | P1 |
| ThreadSafeFunctions.cs | Easy | N | ~0% | 5-8 | P1 |
| ThreadMonitor.cs | Medium | N | ~0% | 5-8 | P2 |
| TimeOutTask.cs | Medium | N | ~0% | 8-10 | P2 |
| ProgressPackage.cs | Easy | N | ~0% | 5-8 | P1 |
| ProgressTracker.cs | Easy | N | ~0% | 8-10 | P2 |
| ProgressTrackerAsync.cs | Medium | N | ~0% | 5-8 | P2 |
| ApplicationIdleTimer.cs | Medium | N | ~0% | 5-8 | P2 |
| AsyncMultiTasker.cs | Medium | N | ~0% | 8-10 | P2 |
| IdleActionQueue.cs | Medium | N | ~0% | 5-8 | P2 |
| IdleAsyncQueue.cs | Medium | N | ~0% | 5-8 | P2 |
| AsyncIdleQueue1.cs | Medium | N | ~0% | 5-8 | P2 |
| UiThread.cs | Hard (UI) | N | ~0% | N/A | Skip |
| IProgressViewer.cs | Skip (interface) | N | N/A | N/A | Skip |
| ProgressTrackerPane.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ProgressMultiStepViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ProgressPane.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ProgressViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| SyncContextForm.cs | Hard (UI) | N | 79% | 0 | Done |

### ReusableTypeClasses/ (53 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| **AsyncLazy/** | | | | | |
| AsyncLazy.cs | Easy | N | 18% | 8-10 | P1 |
| **LazyTry/** | | | | | |
| LazyTry.cs | Easy | N | ~0% | 5-8 | P1 |
| **Matrices/** | | | | | |
| Matrix.cs | Easy | N | ~0% | 10-15 | P1 |
| JaggedMatrix.cs | Easy | N | ~0% | 8-10 | P1 |
| DenMatrix.cs | Easy | N | ~0% | 8-10 | P1 |
| DataConverter2d.cs | Easy | N | ~0% | 5-8 | P1 |
| **Other/** | | | | | |
| StackGeek.cs | Easy | N | ~0% | 5-8 | P1 |
| StackObjectCS.cs | Easy | N | ~0% | 5-8 | P1 |
| TreeNodeOfT.cs | Easy | N | ~0% | 10-15 | P1 |
| AsyncQueue.cs | Medium | N | ~0% | 5-8 | P2 |
| AbstractCloneable.cs | Easy | N | ~0% | 3-5 | P2 |
| **Observable/** | | | | | |
| ObservableCollectionBatchUpdate.cs | Medium | N | ~0% | 8-10 | P2 |
| ObservableDictionary.cs | Medium | N | ~0% | 10-15 | P2 |
| ObserverHelper.cs | Easy | N | ~0% | 3-5 | P2 |
| **Serializable/** | | | | | |
| SerializableList.cs | Easy | N | ~0% | 10-15 | P1 |
| ScBag.cs | Easy | N | ~0% | 5-8 | P1 |
| **Serializable/Concurrent/SCO/** | | | | | |
| ScoCollection.cs | Easy | Y | partial | 5 (expand) | P1 |
| SCODictionary.cs | Easy | N | ~0% | 10-15 | P1 |
| ScoSortedDictionary.cs | Easy | N | ~0% | 8-10 | P2 |
| ScoStack.cs | Easy | N | ~0% | 5-8 | P1 |
| **SerializableNew/Concurrent/** | | | | | |
| ScoDictionaryNew.cs | Medium | Y | partial | 5 (expand) | P2 |
| ScoDictionaryStatic.cs | Easy | N | ~0% | 5-8 | P2 |
| SloLinkedList.cs | Medium | N | ~0% | 8-10 | P2 |
| **SerializableNew/Concurrent/Observable/** | | | | | |
| ScoDictionaryNew.cs (test exists) | Medium | Y | partial | expand | P2 |
| **Concurrent/Observable/Bag/** | | | | | |
| ConcurrentObservableBag.cs | Medium | N | ~0% | 8-10 | P2 |
| BagChangedEventArgs.cs | Easy | N | ~0% | 3-5 | P2 |
| SimpleActionBagObserver.cs | Easy | N | ~0% | 3-5 | P2 |
| ISimpleActionBagObserver.cs | Skip | N | N/A | N/A | Skip |
| **Concurrent/Observable/Dictionary/** | | | | | |
| ConcurrentObservableDictionary.cs | Medium | Y | partial | 5 (expand) | P2 |
| DictionaryChangedEventArgs.cs | Easy | N | ~0% | 3-5 | P2 |
| SimpleActionDictionaryObserver.cs | Easy | N | ~0% | 3-5 | P2 |
| **Locking/** | | | | | |
| LockingLinkedList.cs | Medium | N | ~0% | 10-15 | P2 |
| LockingLinkedListNode.cs | Easy | N | ~0% | 3-5 | P2 |
| ILockingLinkedList.cs | Skip | N | N/A | N/A | Skip |
| **Locking/Observable/LinkedList/** | | | | | |
| LockingObservableLinkedList.cs | Medium | N | ~0% | 8-10 | P3 |
| LockingObservableLinkedListNode.cs | Easy | N | ~0% | 3-5 | P3 |
| LockingObservableLinkedListChangedEventArgs.cs | Easy | N | ~0% | 2-3 | P3 |
| ILockingLinkedListObserver.cs | Skip | N | N/A | N/A | Skip |
| SimpleActionLockingLinkedListObserver.cs | Easy | N | ~0% | 3-5 | P3 |
| **NewSmartSerializable/** | | | | | |
| SmartSerializable.cs | Hard (file I/O) | N | ~0% | 5-8 | P3 |
| SmartSerializableBase.cs | Hard (file I/O) | N | ~0% | N/A | P3 |
| SmartSerializableLoader.cs | Hard (file I/O) | N | ~0% | N/A | P3 |
| SmartSerializableNonTyped.cs | Hard (file I/O) | N | ~0% | N/A | P3 |
| SmartSerializableStatic.cs | Medium | N | ~0% | 5-8 | P3 |
| **NewSmartSerializable/Config/** | | | | | |
| ConfigController.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ConfigViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| ConfigGroupBox.cs | Hard (UI) | N | ~0% | N/A | Skip |
| NewSmartSerializableConfig.cs | Medium | N | ~0% | 3-5 | P3 |
| **TimedActions/** | | | | | |
| TimerWrapper.cs | Medium | N | ~0% | 5-8 | P2 |
| TimedBatchAction.cs | Medium | N | ~0% | 5-8 | P2 |
| TimedQueueOfActions.cs | Medium | N | ~0% | 5-8 | P2 |
| TimedAsyncTask.cs | Medium | N | ~0% | 5-8 | P2 |
| TimedDiskWriter.cs | Hard (file I/O) | Y | partial | expand | P3 |

### NewtonsoftHelpers/ (19 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| FilePathHelperConverter.cs | Medium | Y | good | expand | P1 |
| DerivedCompositionConverter_ConcurrentDictionary.cs | Medium | Y | partial | expand | P1 |
| ScoDictionaryConverter.cs | Medium | Y | partial | expand | P1 |
| WrapperScDictionary.cs | Medium | Y | partial | expand | P1 |
| WrapperScoDictionary.cs | Medium | Y | partial | expand | P1 |
| AppGlobalsConverter.cs | Medium | Y (Threading/) | 100% | 0 | Done |
| AllInclusiveBinder.cs | Easy | N | ~0% | 5-8 | P1 |
| KnownTypesBinder.cs | Easy | N | ~0% | 5-8 | P1 |
| NLogTraceWriter.cs | Easy | Y | 95.5% | 0 | Done |
| NConsoleTraceWriter.cs | Easy | N | ~0% | 3-5 | P2 |
| NonRecursiveConverter.cs | Medium | N | 0% | 5-8 | P2 |
| PeopleScoConverter.cs | Medium | N | 60% | 5-8 | P2 |
| PeopleScoRemainingObjectConverter.cs | Medium | N | 40% | 5-8 | P2 |
| WrapperPeopleScoDictionaryNew.cs | Hard | N | 13% | 5-8 | P3 |
| MonoExtension.cs | Medium | N | ~0% | 3-5 | P3 |
| ILGlobals.cs | Medium | N | 0% | N/A | Skip |
| ILInstruction.cs | Medium | N | 0% | N/A | Skip |
| MethodBodyReader.cs | Hard | N | 0% | N/A | Skip |

### EmailIntelligence/ (72 files)

| Subdirectory/File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|-------------------|-------------|-----------|----------|------------|----------|
| **Bayesian/** | | | | | |
| BayesianClassifierShared.cs | Medium | Y | 62% | 10 (expand) | P1 |
| BayesianClassifierGroup.cs | Medium | Y | 25% | 10 (expand) | P2 |
| Corpus.cs | Medium | Y (indirect) | 39% | 8-10 | P1 |
| Prediction.cs | Easy | N | 0% | 5-8 | P1 |
| SpamBayes.cs | Easy | N | ~0% | 3-5 | P2 |
| BayesianClassifierExtensions.cs | Medium | N | partial | 5-8 | P2 |
| CorpusInherit.cs | Medium | N | 0% | 5-8 | P3 |
| DoNotSerializeContractResolver.cs | Easy | N | ~0% | 3-5 | P1 |
| **Bayesian/Obsolete/** (6 files) | Skip | N | N/A | N/A | Skip |
| **Bayesian/Performance/** | | | | | |
| BayesianMetricTypes.cs | Easy | N | ~0% | 5-8 | P2 |
| BayesianPerformanceMeasurement.cs | Hard (file I/O) | N | 0% | N/A | P3 |
| BayesianSerializationHelper.cs | Hard (file I/O) | N | 0% | N/A | P3 |
| ConfusionViewer.cs | Hard (UI) | N | 0% | N/A | Skip |
| MetricChartViewer.cs | Hard (UI) | N | 0% | N/A | Skip |
| **Ctf/** | | | | | |
| CtfIncidence.cs | Easy | Y (indirect) | 72% | 5 (expand) | P1 |
| CtfIncidenceList.cs | Easy | Y | 28% | 10 (expand) | P1 |
| CtfMap.cs | Easy | Y | 58% | 5 (expand) | P1 |
| CtfMapEntry.cs | Easy | Y | 100% | 0 | Done |
| **EmailParsingSorting/** | | | | | |
| EmailTokenizer.cs | Easy | Y | partial | 5 (expand) | P1 |
| ImageStripper.cs | Easy | N | ~0% | 5-8 | P1 |
| MinedMailInfo.cs | Easy | Y | 54% | 5 (expand) | P1 |
| MovedMailInfo.cs | Easy | N | ~0% | 3-5 | P1 |
| EmailFilerConfig.cs | Easy | N | ~0% | 3-5 | P2 |
| IEmailTokenizer.cs | Skip (interface) | N | N/A | N/A | Skip |
| EmailDataMiner.cs | Hard (Outlook) | N | 0% | N/A | P3 |
| EmailFiler.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| AutoFile.cs | Hard (Outlook) | N | 0% | N/A | P3 |
| SortEmail.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| **Flags/** | | | | | |
| FlagParser.cs | Easy | Y | partial | 5 (expand) | P1 |
| FlagClassNoItem.cs | Easy | N | ~0% | 5-8 | P1 |
| FlagConsolidator.cs | Medium | N | ~0% | 5-8 | P2 |
| FlagDetails.cs | Easy | N | ~0% | 3-5 | P1 |
| FlagTranslator.cs | Medium | N | ~0% | 5-8 | P2 |
| IFlagTranslator.cs | Skip (interface) | N | N/A | N/A | Skip |
| **SubjectMap/** | | | | | |
| CommonWords.cs | Easy | Y | partial | 5 (expand) | P1 |
| SubjectMapEncoder.cs | Medium | N | ~0% | 8-10 | P2 |
| SubjectMapEntry.cs | Easy | N | ~0% | 5-8 | P1 |
| SubjectMapSco.cs | Medium | N | ~0% | 5-8 | P2 |
| SubjectMapMetrics.cs | Hard (UI) | N | ~0% | N/A | Skip |
| FilterEntry.cs | Easy | N | ~0% | 3-5 | P2 |
| FolderConverter.cs | Medium | N | 60% | 5-8 | P2 |
| IntelligenceConfig.cs | Easy | N | ~0% | 3-5 | P2 |
| IntelligenceFilters.cs | Medium | N | ~0% | 5-8 | P3 |
| **People/** | | | | | |
| PeopleScoDictionaryNew.cs | Hard (complex deps) | N | 2.6% | N/A | P3 |
| PeopleScoDictionaryNewBackup.cs | Skip | N | N/A | N/A | Skip |
| **Recents/** | | | | | |
| RecentsList.cs | Medium | N | ~0% | 5-8 | P2 |
| **OlFolderTools/** | | | | | |
| SmithWaterman.cs | Medium | N | ~0% | 8-10 | P2 |
| FilterOlFoldersController.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| FilterOlFoldersViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| FolderInfoViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| OSBrowser.cs | Hard (UI) | N | ~0% | N/A | Skip |
| OSFolder.cs | Hard (UI) | N | ~0% | N/A | Skip |
| FolderRemapController.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| FolderRemapTree.cs | Medium | N | ~0% | 5-8 | P3 |
| FolderRemapViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| FolderSelector.cs | Hard (UI) | N | ~0% | N/A | Skip |
| **ClassifierGroups/** (13 files) | Hard (Outlook deps) | 1 (Triage_OlLogic) | ~0% | limited | P3 |
| **ManagerAsyncLazy.cs** | Hard | N | ~0% | N/A | P3 |

### Dialogs/ (11 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| ActionButton.cs | Easy | Y | 97.5% | 0 | Done |
| DelegateButton.cs | Easy | N | 0% | 5-8 | P1 |
| DelegateButtonTemplate.cs | Medium | Y (partial) | 92% | 2 (expand) | Done |
| YesNoToAll.cs | Medium | Y | partial | 5 (expand) | P1 |
| InputBox.cs | Medium | Y | partial | 5 (expand) | P2 |
| MyBox.cs | Medium | N | ~0% | 5-8 | P2 |
| FunctionButton.cs | Hard (UI) | N | 0% | N/A | Skip |
| NotImplementedDialog.cs | Hard (UI) | N | ~0% | N/A | Skip |
| InputBoxViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| MyBoxViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| FolderNotFoundViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |

### OutlookObjects/ (50 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| DASLFilterParser.cs | Easy | N (but 90% covered) | 90.3% | 3 (expand) | P1 |
| RecipientInfo.cs | Medium | N | ~0% | 5-8 | P2 |
| RecipientStatic.cs | Medium | Y | partial | 5 (expand) | P2 |
| OlItemSummary.cs | Easy | Y | partial | 5 (expand) | P1 |
| ItemComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| AttachmentSerializable.cs | Easy | N | ~0% | 5-8 | P1 |
| FolderConverter.cs | Medium | Y | partial | 5 (expand) | P2 |
| FolderMinimalWrapper.cs | Easy | N | ~0% | 3-5 | P2 |
| FolderWrapperNameComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| FolderWrapperNameAndParentNameComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| FolderWrapperNameCountSizeComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| FolderWrapperNodeComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| FolderWrapperNodeContentsComparer.cs | Easy | N | ~0% | 5-8 | P1 |
| EmailDetails.cs | Medium | Y | 11% | 10-15 | P2 |
| EmailDetailsWrapper.cs | Medium | N | ~0% | 5-8 | P2 |
| ItemInfo.cs | Easy | N | ~0% | 5-8 | P1 |
| MAPIFields.cs | Easy | N | ~0% | 3-5 | P2 |
| UserDefinedFields.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| StoresWrapper.cs | Medium | Y | partial | 5 (expand) | P2 |
| StoreWrapper.cs | Hard (COM) | N | ~0% | N/A | P3 |
| StoreWrapperController.cs | Hard (COM/UI) | N | ~0% | N/A | Skip |
| StoreWrapperViewer.cs | Hard (UI) | N | ~0% | N/A | Skip |
| IStoreWrapperViewer.cs | Skip (interface) | N | N/A | N/A | Skip |
| OlTableExtensions.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| OlToDoTable.cs | Hard (Outlook) | N | ~0% | N/A | P3 |
| FolderWrapper .cs | Hard (COM) | N | ~0% | N/A | P3 |
| FolderNavigator.cs | Hard (COM) | N | ~0% | N/A | P3 |
| FolderPredictor.cs | Hard (COM) | N | ~0% | N/A | P3 |
| FolderScorer.cs | Medium | N | ~0% | 5-8 | P2 |
| FolderTree.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItem.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItemExtensions.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItemFlaggable.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItemFlaggableTry.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItemTry.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OutlookItemTryGet.cs | Hard (COM) | N | ~0% | N/A | P3 |
| OlItemPseudoInterface.cs | Medium | N | ~0% | 3-5 | P3 |
| MailItemHelper.cs | Hard (COM) | Y | partial | N/A | P3 |
| MailItemExtensions.cs | Hard (COM) | N | ~0% | N/A | P3 |
| MailResolution.cs | Hard (COM) | N | ~0% | N/A | P3 |
| CaptureEmailAddressesModule2.cs | Hard (COM) | N | ~0% | N/A | P3 |
| MeetingItemHelper.cs | Hard (COM) | N | ~0% | N/A | P3 |
| Calendar.cs | Hard (COM) | N | 0% | N/A | P3 |
| CreateCategory.cs | Hard (COM) | N | 0% | N/A | P3 |
| ComType.cs | Hard (COM) | N | 0% | N/A | Skip |
| ConversationHelper.cs | Hard (COM) | N | 0% | N/A | P3 |
| ExplorerActions.cs | Hard (COM) | N | ~0% | N/A | P3 |
| MAPIMethods.cs | Hard (native) | N | 0% | N/A | Skip |

### OneDriveHelpers/ (2 files)

| File | Testability | Has Tests | Coverage | Est. Tests | Priority |
|------|-------------|-----------|----------|------------|----------|
| AngleSharpParsedEmailBody.cs | Easy | Y | partial | 3 (expand) | P1 |
| OneDriveDownloader.cs | Hard (network) | Y | partial | 3 (expand) | P3 |

### Interfaces/ (63 files) — **Skip All**

All 63 files are pure interface definitions with no implementation logic. No tests needed.

### To Depricate/ (5 files) — **Skip All**

| File | Reason |
|------|--------|
| FileIO2.cs | Deprecated |
| StringManipulation.cs | Deprecated |

### WindowsAPI/ (1 file) — **Skip**

ExtraDeclarations.cs — P/Invoke stubs, all commented out. No testable logic.

### Examples/ (1 file) — **Skip**

MSDemoConv.cs — Demo/example code, not production.

---

## 5. Files to Skip with Justification

| Category | Files | Justification |
|----------|-------|---------------|
| **Interfaces** | 63 files in Interfaces/ | Pure interface definitions; no logic to test |
| **Designer-generated** | ~20+ .Designer.cs files | Auto-generated WinForms code |
| **Deprecated** | 5 files in To Depricate/ | Marked for deprecation; investing test effort is wasteful |
| **Obsolete** | 6 files in Bayesian/Obsolete/ | Superseded by current implementations |
| **UI-heavy** | ~35 files (Viewers, Forms, Controls) | Require WinForms runtime; no isolated logic to test |
| **COM-heavy** | ~20 files (OutlookItem*, FolderWrapper, etc.) | Deep COM interop; require live Outlook to exercise |
| **WindowsAPI** | 1 file | Commented-out P/Invoke declarations |
| **Examples** | 1 file | Demo code, not production |
| **SDIL Reader** | 3 files | IL reading utilities; extremely low value to test |
| **Total Skip** | ~155 files | |

**Testable target**: ~204 production files need tests (359 - 155 skipped)

---

## 6. Mocking Strategies

### COM Interop (Outlook Objects)

For any OutlookObjects code with extractable logic:
- Mock `MailItem`, `MAPIFolder`, `Recipient`, etc. via their already-defined interfaces in `Interfaces/IOutlookObjects/` and `Interfaces/IEmailIntelligence/`
- Use `MockBehavior.Loose` for COM interfaces (existing pattern)
- Focus on testing the logic paths (comparers, parsers, converters) that don't require a live COM object
- Many Folder comparers (FolderWrapperNameComparer, etc.) work on `TreeNode<FolderWrapper>` — can construct FolderWrapper stubs

### UI Dependencies (WinForms)

- Dialog logic classes (ActionButton, DelegateButton, YesNoToAll, InputBox, MyBox) separate logic from viewer
- Test the logic/model classes; skip Viewer/Form classes
- For any testable WinForms code: construct controls programmatically in tests (already demonstrated in ActionButtonTests)

### File I/O Dependencies

- SmartSerializable classes, BayesianSerializationHelper — would need IFileSystem abstraction (not currently available)
- Mark as P3; would require refactoring to be testable without temporary files
- TimedDiskWriter already has tests (likely uses mocking)

### IApplicationGlobals and Related Interfaces

- Already well-established mock pattern: `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>`
- These interface mocks unlock testing of many classes that depend on app globals

---

## Recommended Approach

### Phased Implementation Plan

**Phase 1 — Pure Logic, High-Value (P1)** — ~70 test files, ~500 test methods

Focus on files with zero test dependencies (pure functions, extension methods, data structures):

1. **Extensions/** (15 files) — ArrayExtensions, StringExtensions, DictionaryExtensions, IEnumerableExtensions, IListExtensions, EnumExtensions, NullExtensions, ExceptionExtensions, QueueExtensions, LazyExtension, ExtToChar, TraceExtensions, JsonExtensions, JsonSerializerExtensions, CompilerServicesExtensions
2. **HelperClasses/** pure logic (10 files) — GenericBitwise, MergeSortImplementations, ParamArray, ObjectSize, PrettyPrint, SimpleRegex (expand), Tokenizer (expand), DeepCompare, SegmentStopWatch, ReflectionHelper
3. **ReusableTypeClasses/** pure data structures (12 files) — AsyncLazy, LazyTry, Matrix, JaggedMatrix, DenMatrix, DataConverter2d, StackGeek, StackObjectCS, TreeNodeOfT, SerializableList, ScBag, ScoCollection (expand), SCODictionary, ScoStack
4. **Dialogs/** logic classes (2 files) — DelegateButton, YesNoToAll (expand)
5. **EmailIntelligence/** pure logic (10 files) — Prediction, DoNotSerializeContractResolver, CtfIncidence (expand), CtfIncidenceList (expand), CtfMap (expand), FlagParser (expand), FlagClassNoItem, FlagDetails, CommonWords (expand), SubjectMapEntry, EmailTokenizer (expand), ImageStripper, MinedMailInfo (expand), MovedMailInfo
6. **OutlookObjects/** comparers & POCOs (8 files) — FolderWrapperNameComparer, FolderWrapperNameAndParentNameComparer, FolderWrapperNameCountSizeComparer, FolderWrapperNodeComparer, FolderWrapperNodeContentsComparer, ItemComparer, AttachmentSerializable, OlItemSummary (expand), ItemInfo
7. **NewtonsoftHelpers/** binders (2 files) — AllInclusiveBinder, KnownTypesBinder
8. **Threading/** pure logic (3 files) — ThreadSafeSingleShotGuard, ThreadSafeFunctions, TaskPriority, ProgressPackage

**Phase 2 — Medium Complexity (P2)** — ~40 test files, ~300 test methods

Files requiring Moq mocking of interfaces:

1. **Extensions/** async/stream (3 files) — AsyncSerialization, StreamExtensions, IAsyncEnumerableExtensions
2. **HelperClasses/** with deps (8 files) — Initializer, ObjectCopier, FilePathHelper, DirectoryInfoWrapper, FileInfoWrapper, FileSystemInfoWrapper, logging classes
3. **ReusableTypeClasses/** observable/concurrent (12 files) — ObservableDictionary, ConcurrentObservableBag, LockingLinkedList, TimerWrapper, TimedBatchAction, TimedQueueOfActions, TimedAsyncTask, etc.
4. **Threading/** with deps (8 files) — TimeOutTask, ProgressTracker, ProgressTrackerAsync, ApplicationIdleTimer, AsyncMultiTasker, IdleActionQueue, IdleAsyncQueue, AsyncIdleQueue1
5. **NewtonsoftHelpers/** converters (5 files) — expand existing, add NonRecursiveConverter, PeopleScoConverter, PeopleScoRemainingObjectConverter, NConsoleTraceWriter, MonoExtension
6. **EmailIntelligence/** medium (10 files) — Corpus, BayesianClassifierGroup (expand), SmithWaterman, SubjectMapEncoder, SubjectMapSco, FolderConverter, EmailFilerConfig, FilterEntry, IntelligenceConfig, RecentsList, FlagConsolidator, FlagTranslator
7. **OutlookObjects/** mocked (5 files) — RecipientInfo, RecipientStatic (expand), EmailDetails (expand), FolderScorer, MAPIFields, FolderMinimalWrapper, StoresWrapper (expand)

**Phase 3 — Hard/Low-Value (P3)** — ~15 test files, ~80 test methods

Files with heavy external dependencies; limited ROI unless refactored:

1. **EmailIntelligence/** Outlook-dependent classifiers — test any extractable logic
2. **OutlookObjects/** COM wrappers — test only non-COM logic paths
3. **ReusableTypeClasses/** SmartSerializable — test serialization logic via mocks
4. **NewtonsoftHelpers/** complex wrappers

### Estimated Totals

| Phase | Test Files | Test Methods | Effort |
|-------|-----------|-------------|--------|
| Phase 1 (P1) | ~70 | ~500 | Highest ROI |
| Phase 2 (P2) | ~40 | ~300 | Medium ROI |
| Phase 3 (P3) | ~15 | ~80 | Low ROI |
| Skip | ~155 prod files | 0 | N/A |
| **Total** | ~125 test files | ~880 methods | |

### Test Approach per Category

| Category | Strategy |
|----------|----------|
| Extension methods | Test each method: null input, empty, single, typical, boundary. Separate test file per extension class. |
| Data structures (collections, matrices) | CRUD operations, enumeration, boundary (empty, single, large), concurrency for concurrent types. |
| Comparers | Equals/GetHashCode: same, different, null, edge cases. |
| Newtonsoft converters | Round-trip: serialize → deserialize → verify equality. Malformed JSON. |
| Bayesian classifiers | Use test double subclasses (existing SubCorpus, SubClassifierGroup pattern). Train → classify cycles. |
| Flags/parsers | Known input → expected output for each parsing path. |
| Threading | Use ManualResetEvent/TaskCompletionSource for deterministic sync. |
| COM-dependent code | Mock via interfaces; test only logic, not COM calls. |

---

## Implementation Guidance

- **Objectives**: Bring UtilitiesCS per-file coverage to ≥80% via MSTest + Moq + FluentAssertions
- **Key Tasks**:
  1. Execute Phase 1 (~70 test files for pure logic)
  2. Execute Phase 2 (~40 test files requiring mocking)
  3. Optionally execute Phase 3 (~15 test files for hard dependencies)
  4. Expand existing partial-coverage test files throughout each phase
- **Dependencies**: No new package dependencies required; MSTest, Moq, FluentAssertions already in project
- **Success Criteria**:
  - Per-file line coverage ≥80% for each testable file
  - All tests pass, are independent, isolated, fast, deterministic
  - No file I/O, no network, no COM interop in tests
  - Each test file ≤500 lines
  - Full C# toolchain passes: format → analyzers → nullable → test
- **500-line constraint**: For large extension classes (ArrayExtensions, StringExtensions, IEnumerableExtensions), split into multiple test files: e.g., `ArrayExtensionsTests_SliceAndResize.cs`, `ArrayExtensionsTests_SearchAndFilter.cs`

**Mandatory unachievable objective callout**:
- Achieving ≥80% coverage on **COM-heavy** OutlookObjects files (FolderWrapper, OutlookItem*, MailItemHelper, etc.) is **not achievable** without either (a) live Outlook integration tests or (b) refactoring to inject interfaces. These ~20 files should be excluded from the ≥80% target or scheduled for a separate refactoring initiative.