# Research: utilities-coverage-part-three (Issue #87)

**Date:** 2026-03-19  
**Issue:** #87  
**Researcher:** Task Researcher Agent  
**Scope:** UtilitiesCS project — raising all files below 80% line coverage to ≥80%

---

## Executive Summary

The UtilitiesCS project compiles **~230 production .cs files** (per explicit `<Compile Include>` entries in UtilitiesCS.csproj). Of these, **~155 files appear below 80% line coverage** in the current Cobertura report, plus approximately **16 Designer.cs files**, **~20+ pure-interface files** (no executable lines — not reported in coverage), and **~4 commented-out stubs** (no executable lines).

The total scope breaks down as:

| Category | Count | Typical Difficulty |
|---|---|---|
| Extensions (pure logic) | ~8 below 80% | Easy |
| HelperClasses (logic + file system + WinForms) | ~26 below 80% | Easy to Hard |
| ReusableTypeClasses (data structures, serialization) | ~33 below 80% | Easy to Medium |
| NewtonsoftHelpers (JSON converters) | ~14 below 80% | Medium |
| EmailIntelligence (Bayesian, classifiers, parsing) | ~60 below 80% | Medium to Hard |
| OutlookObjects (COM interop) | ~19 below 80% | Hard |
| Threading (async, UI-thread) | ~17 below 80% | Medium to Hard |
| Dialogs (WinForms UI) | ~9 below 80% | Hard |
| To Depricate (deprecated code) | 3 below 80% | Easy/Skip |
| OneDriveHelpers | 1 below 80% | Medium |
| Interfaces (PropertyStore class) | 1 below 80% | Easy |
| Designer.cs auto-generated | ~16 at 0% | Skip candidates |
| Commented stubs (no executable lines) | ~4 | Skip (already 100% or N/A) |
| Pure interface files (no executable lines) | ~40+ | Skip (no code to test) |

**Key finding:** The biggest bang for the buck is in Extensions, HelperClasses (pure logic subset), ReusableTypeClasses (data structures), and simple EmailIntelligence classes, where ~50+ files can be tested with straightforward unit tests. The hardest files are WinForms UI classes, Outlook COM interop classes, and deep serialization infrastructure.

---

## Project Structure

### UtilitiesCS.csproj
- **Framework:** .NET Framework 4.8.1 (old-style csproj with explicit `<Compile Include>`)
- **Language:** C# 12.0
- **Key dependencies:** Newtonsoft.Json, Microsoft.Office.Interop.Outlook, System.Windows.Forms, log4net, Deedle, Microsoft.ML, Mono.Cecil, Svg, AngleSharp, Microsoft.Graph
- **Project references:** SVGControl, UtilitiesSwordfish, VBFunctions

### UtilitiesCS.Test.csproj
- **Framework:** .NET Framework 4.8.1
- **Test framework:** MSTest 4.1.0 + Moq 4.20.72 + FluentAssertions 8.3.0
- **Important:** Uses explicit `<Compile Include>` entries — every new test file must be added to the csproj
- **Existing test files:** ~120+ test files already registered
- **Project references:** TaskMaster, UtilitiesCS, UtilitiesSwordfish

---

## Existing Test Patterns

Based on examination of existing test files, the established pattern is:

```csharp
using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UtilitiesCS;               // or specific sub-namespace
using Moq;                       // when mocking is needed

namespace UtilitiesCS.Test.{SubFolder}
{
    [TestClass]
    public class {ClassName}_Tests
    {
        [TestMethod]
        public void {MethodName}_{Scenario}_{ExpectedResult}()
        {
            // Arrange
            ...
            // Act
            ...
            // Assert
            result.Should().Be(expected);
        }
    }
}
```

**Key conventions:**
- Namespace mirrors subfolder: `UtilitiesCS.Test.Extensions`, `UtilitiesCS.Test.ReusableTypeClasses`, etc.
- Test class name: `{ProductionClass}_Tests` (matching existing pattern)
- Method naming: descriptive `{Method}_{Scenario}_{Expected}` style
- Uses FluentAssertions `.Should()` syntax throughout
- AAA (Arrange-Act-Assert) pattern with comments
- Async tests use `[TestMethod] public async Task ...`
- Moq is used for COM interop mocking (e.g., `Mock<Microsoft.Office.Interop.Outlook.MailItem>`)

---

## File Inventory by Category

### 1. Extensions (8 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| ArrayExtensions.cs | 77.7% | **Easy** | Close to 80%, small uplift needed |
| AsyncSerialization.cs | 11.6% | **Medium** | Async serialization helpers, needs mocking of streams |
| DfDeedle.cs | 0% | **Medium** | Deedle DataFrame extensions, needs Deedle test data |
| DfMLNet.cs | 0% | **Medium** | ML.NET DataFrame extensions, needs ML context |
| DrawingExtensions.cs | 0% | **Medium** | System.Drawing extensions, needs GDI+ objects |
| IAsyncEnumerableExtensions.cs | 60.9% | **Easy** | Async LINQ-style helpers, straightforward |
| IEnumerableExtensions.cs | 70.6% | **Easy** | Close to 80%, straightforward LINQ extensions |
| ImageExtensions.cs | 35.9% | **Medium** | Image manipulation, needs Bitmap/Image objects |
| WinFormsExtensions.cs | 13% | **Hard** | WinForms control extensions, needs UI thread |

### 2. HelperClasses (26 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| DeepCompare.cs | 31.2% | **Easy** | Object comparison logic, pure logic |
| DispatchUtility.cs | 10.5% | **Hard** | COM dispatch, uses Marshal/IDispatch |
| DvgForm.cs | 0% | **Hard** | WinForms Form subclass |
| DvgForm.Designer.cs | 0% | **Skip** | Auto-generated designer |
| DirectoryInfoWrapper.cs | 20.3% | **Medium** | File system wrapper with interface, mockable |
| FileInfoWrapper.cs | 17.8% | **Medium** | File system wrapper with interface, mockable |
| FilePathHelper.cs | 18.8% | **Medium** | Path manipulation, some file-system deps |
| FileSystemInfoWrapper.cs | 0% | **Medium** | Base class for FileInfo/DirectoryInfo wrappers |
| MyFileSystemInfo.cs | 71% | **Easy** | Close to 80%, data class |
| ShellUtilities.cs | 0% | **Hard** | Shell32 COM interop, P/Invoke |
| ShellUtilitiesStatic.cs | 33.3% | **Medium** | Static shell helpers, some testable parts |
| Initializer.cs | 60.3% | **Easy** | Property initialization helpers |
| DebugTextWriter.cs | 63.6% | **Easy** | TextWriter subclass, pure logic |
| TraceUtility.cs | 60.6% | **Easy** | Logging utility, trace-based |
| PrettyPrint.cs | 67.2% | **Easy** | String formatting helpers |
| SystemThemeDetector.cs | 62.5% | **Medium** | Registry-based theme detection |
| Theme.cs | 3.3% | **Hard** | WinForms theme application |
| ThemeControlGroup.cs | 0% | **Hard** | WinForms theme group |
| QfcTipsDetails.cs | 0% | **Medium** | Tooltip data class |
| TipsController.cs | 0% | **Hard** | WinForms tooltip controller |
| ControlPosition.cs | 0% | **Hard** | WinForms positioning |
| ControlResizer.cs | 0% | **Hard** | WinForms resizing |
| ImageHelper.cs | 0% | **Hard** | WinForms image handling |
| MouseDownFilter.cs | 0% | **Hard** | WinForms message filter |
| OlvExtension.cs | 0% | **Hard** | ObjectListView extensions |
| ScreenHelper.cs | 0% | **Hard** | WinForms screen utilities |
| TableLayoutHelper.cs | 0% | **Hard** | WinForms TableLayoutPanel helpers |
| ComStreamWrapper.cs | 0% | **Hard** | COM IStream wrapper, WIP/unfinished |

### 3. ReusableTypeClasses (33 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| AsyncLazy.cs | 25% | **Easy** | Lazy async initialization pattern |
| SimpleActionBagObserver.cs | 0% | **Easy** | Observer pattern impl, straightforward |
| ConcurrentObservableDictionary.cs | 77.4% | **Easy** | Close to 80%, small uplift |
| LockingLinkedList.cs | 58.2% | **Easy** | Thread-safe linked list, existing tests |
| LockingLinkedListNode.cs | 61.7% | **Easy** | Node class, pair with above |
| LockingObservableLinkedList.cs | 24.8% | **Medium** | Observable variant, needs observer setup |
| LockingObservableLinkedListNode.cs | 20.4% | **Medium** | Node for observable list |
| SimpleActionLockingLinkedListObserver.cs | 0% | **Easy** | Observer impl |
| ConfigController.cs | 0% | **Hard** | WinForms config UI controller |
| ConfigGroupBox.cs | 0% | **Hard** | WinForms component |
| ConfigViewer.cs | 0% | **Hard** | WinForms Form |
| ConfigViewer.Designer.cs | 0% | **Skip** | Auto-generated designer |
| NewSmartSerializableConfig.cs | 29.5% | **Medium** | Config data class for serialization |
| SmartSerializable.cs | 15.9% | **Medium** | Generic serialization framework |
| SmartSerializableBase.cs | 0% | **Medium** | Base class for smart serialization |
| SmartSerializableLoader.cs | 7.1% | **Medium** | File-based deserialization loader |
| SmartSerializableNonTyped.cs | 72% | **Medium** | Non-typed serialization variant |
| SmartSerializableStatic.cs | 0% | **Medium** | Static serialization helpers |
| AbstractCloneable.cs | 77.8% | **Easy** | Close to 80%, cloning base class |
| StackGeek.cs | 72.2% | **Easy** | Simple stack implementation |
| StackObjectCS.cs | 72% | **Easy** | Simple stack variant |
| TreeNodeOfT.cs | 76.8% | **Easy** | Generic tree node, close to 80% |
| ScBag.cs | 20.4% | **Medium** | Serializable concurrent bag |
| ScoCollection.cs | 4.4% | **Medium** | Serializable collection, complex generics |
| SCODictionary.cs | 4.3% | **Medium** | Serializable concurrent ordered dictionary |
| ScoSortedDictionary.cs | 7.4% | **Medium** | Sorted dictionary variant |
| ScoStack.cs | 40.2% | **Medium** | Serializable concurrent stack |
| SerializableList.cs | 35.9% | **Medium** | Serializable list with file persistence |
| ScoDictionaryNew.cs | 15.5% | **Medium** | New-gen serializable dictionary |
| SloLinkedList.cs | 29.7% | **Medium** | Serializable linked list |
| ScDictionary.cs | 8.6% | **Medium** | Serializable concurrent dictionary |
| TimedDiskWriter.cs | 66.3% | **Medium** | Timer-based disk writer |
| TimedQueueOfActions.cs | 58.8% | **Easy** | Queued timed actions |

### 4. NewtonsoftHelpers (14 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| DerivedCompositionConverter_ConcurrentDictionary.cs | 0% | **Medium** | JSON converter, has existing test file |
| FilePathHelperConverter.cs | 72% | **Easy** | Close to 80%, existing tests |
| MonoExtension.cs | 39.1% | **Medium** | Mono.Cecil-based type inspection |
| NConsoleTraceWriter.cs | 0% | **Easy** | Simple console trace writer |
| NonRecursiveConverter.cs | 0% | **Medium** | Custom JSON converter |
| PeopleScoConverter.cs | 66.7% | **Medium** | Domain-specific JSON converter |
| PeopleScoRemainingObjectConverter.cs | 40% | **Medium** | Domain-specific JSON converter |
| ScDictionaryConverter.cs | 9.1% | **Medium** | JSON converter for ScDictionary |
| ILGlobals.cs | 0% | **Medium** | IL instruction constants/data |
| ILInstruction.cs | 0% | **Medium** | IL instruction model |
| MethodBodyReader.cs | 0% | **Hard** | Mono.Cecil-based IL reader |
| WrapperPeopleScoDictionaryNew.cs | 12.6% | **Medium** | JSON wrapper class |
| WrapperScDictionary.cs | 70.7% | **Easy** | Close to 80%, JSON wrapper |
| WrapperScoDictionary.cs | 76% | **Easy** | Close to 80%, JSON wrapper |

### 5. EmailIntelligence (~60 files below 80%)

#### Bayesian (12 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| BayesianClassifierExtensions.cs | 20.5% | **Medium** | Extension methods for classifier |
| BayesianClassifierGroup.cs | 22.5% | **Medium** | Group management logic |
| BayesianClassifierShared.cs | 63.8% | **Medium** | Core classification logic |
| Corpus.cs | 33.9% | **Medium** | Text corpus management |
| CorpusInherit.cs | 0% | **Medium** | Corpus inheritance logic |
| Obsolete\BayesianClassifier.cs | 65.1% | **Medium** | Legacy classifier |
| Obsolete\ClassifierGroup.cs | 8.2% | **Medium** | Legacy group |
| Obsolete\DedicatedToken.cs | 22.2% | **Easy** | Token data class |

#### Bayesian\Performance (6 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| BayesianMetricTypes.cs | 0% | **Easy** | Metric enums/data classes |
| BayesianPerformanceMeasurement.cs | 0% | **Medium** | Performance measurement logic |
| BayesianSerializationHelper.cs | 0% | **Medium** | Serialization helpers |
| ConfusionViewer.cs | 0% | **Hard** | WinForms confusion matrix viewer |
| ConfusionViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| MetricChartViewer.cs | 0% | **Hard** | WinForms chart viewer |
| MetricChartViewer.Designer.cs | 0% | **Skip** | Auto-generated |

#### ClassifierGroups (11 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| ActionableClassifierGroup.cs | 0% | **Hard** | Depends on IApplicationGlobals, COM |
| CategoryClassifierGroup.cs | 0% | **Hard** | Depends on IApplicationGlobals, COM |
| ClassifierGroupUtilities.cs | 0% | **Medium** | Utility methods |
| ConditionalItemEngine.cs | 0% | **Hard** | Depends on COM Outlook items |
| ManagerAsyncLazy.cs | 0% | **Hard** | Async lazy with globals dependency |
| MulticlassEngine.cs | 0% | **Hard** | Multiclass classification engine |
| OlFolderClassifierGroup.cs | 0% | **Hard** | Depends on Outlook folders |
| SpamBayes.cs | 0% | **Hard** | SpamBayes implementation |
| Triage_OlLogic.cs | 40.4% | **Medium** | Triage sorting logic, partial tests exist |
| Triage.cs | 8.5% | **Hard** | Triage classifier with IApplicationGlobals |
| TristateEngine.cs | 0% | **Hard** | Tristate classification engine |

#### Ctf (2 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| CtfIncidenceList.cs | 64.5% | **Easy** | Data class/list, existing tests close |
| CtfMap.cs | 68.6% | **Easy** | Dictionary-like map, existing tests close |

#### EmailParsingSorting (7 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| AutoFile.cs | 0% | **Hard** | Depends on COM Outlook |
| EmailDataMiner.cs | 0% | **Hard** | Depends on COM Outlook |
| EmailFiler.cs | 0% | **Hard** | Depends on COM Outlook + globals |
| EmailFilerConfig.cs | 0% | **Easy** | Config data class |
| EmailTokenizer.cs | 74.8% | **Easy** | Close to 80%, text tokenization |
| ImageStripper.cs | 53.5% | **Easy** | HTML image stripping, existing tests |
| MovedMailInfo.cs | 35.4% | **Easy** | Data class, existing tests |

#### Flags (1 file)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| FlagTranslator.cs | 41.2% | **Medium** | Flag translation logic |

#### Other EmailIntelligence (11 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| FilterEntry.cs | 0% | **Easy** | Simple data class (ICloneable) |
| IntelligenceConfig.cs | 7.3% | **Medium** | Configuration with globals deps |
| PeopleScoDictionaryNew.cs | 3.2% | **Medium** | Specialized dictionary |
| RecentsList.cs | 0% | **Medium** | Recents tracking |
| SubjectMapEncoder.cs | 0% | **Medium** | Subject encoding logic |
| SubjectMapEntry.cs | 50.8% | **Easy** | Data class, existing tests |
| SubjectMapSco.cs | 4.1% | **Medium** | Serializable subject map |
| SmithWaterman.cs | 48.6% | **Easy** | Pure algorithm (string alignment) |

#### OlFolderTools (12 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| FilterOlFoldersController.cs | 0% | **Hard** | COM + UI controller |
| FilterOlFoldersViewer.cs | 0% | **Hard** | WinForms |
| FilterOlFoldersViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| FolderInfoViewer.cs | 0% | **Hard** | WinForms |
| FolderInfoViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| OSBrowser.cs | 0% | **Hard** | WinForms WebView2 browser |
| OSBrowser.Designer.cs | 0% | **Skip** | Auto-generated |
| FolderRemapController.cs | 0% | **Hard** | COM + UI controller |
| FolderRemapTree.cs | 0% | **Hard** | Tree logic with COM deps |
| FolderRemapViewer.cs | 0% | **Hard** | WinForms |
| FolderRemapViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| FolderSelector.cs | 0% | **Hard** | WinForms |
| FolderSelector.Designer.cs | 0% | **Skip** | Auto-generated |

#### SubjectMap viewers (2 files)
| File | Coverage | Testability | Notes |
|---|---|---|---|
| SubjectMapMetrics.cs | 0% | **Hard** | WinForms metrics viewer |
| SubjectMapMetrics.Designer.cs | 0% | **Skip** | Auto-generated |

### 6. OutlookObjects (19 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| AttachmentHelper.cs | 69.9% | **Medium** | COM mocking via existing patterns |
| AttachmentSerializable.cs | 54.7% | **Medium** | Serialization with COM data |
| CreateCategory.cs | 65.5% | **Medium** | COM namespace operations |
| ConversationHelper.cs | 4% | **Hard** | Deep COM conversation traversal |
| UserDefinedFields.cs | 26% | **Medium** | COM property accessor |
| OlItemPseudoInterface.cs | 55.4% | **Medium** | COM reflection-based item access |
| OutlookItem.cs | 54.5% | **Medium** | Core item wrapper, COM mocking |
| OutlookItemExtensions.cs | 44.9% | **Medium** | Extension methods on COM types |
| OutlookItemFlaggable.cs | 58.2% | **Medium** | Flag operations on Outlook items |
| OutlookItemFlaggableTry.cs | 51% | **Medium** | Try-pattern flag operations |
| OutlookItemTry.cs | 35.5% | **Medium** | Try-pattern COM operations |
| OutlookItemTryGet.cs | 21.6% | **Medium** | TryGet pattern for COM props |
| MailItemHelper.cs | 45.8% | **Hard** | Core mail processing, deep COM |
| RecipientStatic.cs | 46.7% | **Medium** | Static recipient helpers |
| StoreWrapper.cs | 71.7% | **Medium** | Close to 80%, existing tests |
| StoreWrapperController.cs | 33.9% | **Hard** | Store navigation with COM |
| OlTableExtensions.cs | 4.7% | **Hard** | COM Table iteration/filtering |
| OlToDoTable.cs | 0% | **Hard** | COM ToDo table operations |
| CaptureEmailAddressesModule2.cs | N/A | **Hard** | Not in coverage report |

### 7. Threading (17 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| ApplicationIdleTimer.cs | 0% | **Hard** | WinForms application idle detection |
| AsyncMultiTasker.cs | 0% | **Medium** | Async task orchestration |
| IdleActionQueue.cs | 0% | **Hard** | Idle-time action queue, UI-thread |
| IdleAsyncQueue.cs | 0% | **Hard** | IAsyncIdleQueue variant |
| ProgressMultiStepViewer.cs | 0% | **Hard** | WinForms progress viewer |
| ProgressMultiStepViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| ProgressPane.cs | 0% | **Hard** | WinForms UserControl |
| ProgressPane.Designer.cs | 0% | **Skip** | Auto-generated |
| ProgressTracker.cs | 47% | **Medium** | Progress tracking logic |
| ProgressTrackerAsync.cs | 0% | **Medium** | Async progress tracking |
| ProgressTrackerPane.cs | 0% | **Hard** | UI thread progress pane |
| ProgressViewer.cs | 0% | **Hard** | WinForms progress viewer |
| ProgressViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| SyncContextForm.Designer.cs | 78.6% | **Skip** | Auto-generated, close to 80% |
| ThreadMonitor.cs | 0% | **Medium** | Thread monitoring logic |
| ThreadSafeFunctions.cs | 54.8% | **Easy** | Thread-safe function wrappers |
| TimeOutTask.cs | 24.1% | **Easy** | Timeout extension methods |
| UiThread.cs | 60% | **Hard** | UI thread marshalling |

### 8. Dialogs (9 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| DelegateButton.cs | 51.6% | **Medium** | Button logic, some WinForms deps |
| FolderNotFoundViewer.cs | 0% | **Hard** | WinForms Form |
| FolderNotFoundViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| FunctionButton.cs | 0% | **Medium** | Generic button with Func delegate |
| InputBox.cs | 0% | **Hard** | Static dialog display |
| InputBoxViewer.cs | 0% | **Hard** | WinForms Form |
| InputBoxViewer.Designer.cs | 0% | **Skip** | Auto-generated |
| MyBox.cs | 0% | **Hard** | Static dialog display |
| MyBoxViewer.cs | 28.1% | **Hard** | WinForms Form |
| NotImplementedDialog.cs | 0% | **Hard** | Static MessageBox-based dialog |
| YesNoToAll.cs | 28.8% | **Medium** | Dialog result tracking, partial logic |

### 9. To Depricate (3 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| CSVDictUtilities.cs | 0% | **Medium** | CSV file operations, depends on FileIO2 |
| FileIO2.cs | 0% | **Medium** | File I/O operations (deprecated) |
| StringManipulation.cs | 0% | **Easy** | Simple regex-based string cleanup |

### 10. Other (2 files below 80%)

| File | Coverage | Testability | Notes |
|---|---|---|---|
| OneDriveDownloader.cs | 0% | **Hard** | Microsoft Graph API calls |
| PropertyStore.cs (Interfaces) | 0% | **Easy** | Pure data structure, no external deps |

---

## Skip Candidates

These files should be excluded or given special treatment:

### Designer.cs Auto-Generated Files (~16 below 80%)
Designer files are auto-generated by Visual Studio WinForms designer. They contain `InitializeComponent()` calls and control property assignments. Testing them directly provides minimal value since they're regenerated on any UI change.

**Files:** DvgForm.Designer.cs, ConfusionViewer.Designer.cs, MetricChartViewer.Designer.cs, FilterOlFoldersViewer.Designer.cs, FolderInfoViewer.Designer.cs, OSBrowser.Designer.cs, FolderRemapViewer.Designer.cs, FolderSelector.Designer.cs, SubjectMapMetrics.Designer.cs, ConfigViewer.Designer.cs, ProgressMultiStepViewer.Designer.cs, ProgressPane.Designer.cs, ProgressViewer.Designer.cs, SyncContextForm.Designer.cs, FolderNotFoundViewer.Designer.cs, InputBoxViewer.Designer.cs

**Recommendation:** Exclude from coverage gate or accept indirect coverage from parent form tests.

### Commented-Out Stubs (no executable code)
- `ReusableTypeClasses\Observable\ObservableDictionary.cs` — entirely commented out (live impl is in UtilitiesSwordfish)
- `ReusableTypeClasses\Concurrent\Observable\Bag\ConcurrentObservableBag.cs` — entirely commented out
- `To Depricate\StackObjectVB.cs` — entirely commented out
- `To Depricate\FlattenArray.cs` — entirely commented out

These have **zero executable lines** so they don't appear in coverage reports and don't need tests.

### Pure Interface Files (no executable code)
~40+ files in `Interfaces/` folder that define only interfaces with no implementations. These have no executable lines and don't appear in coverage reports.

### Files Not in Csproj
From repo memory: `UtilitiesCS/OutlookObjects/MailResolution.cs` exists on disk but is **not** included in the csproj. The live compiled version is `UtilitiesCS/OutlookObjects/MailItem/MailResolution.cs` (already at 100%).

---

## Testability Summary

| Difficulty | File Count | Description |
|---|---|---|
| **Easy** | ~45 | Pure logic, data classes, extension methods, close-to-80% files |
| **Medium** | ~55 | Needs mocking (interfaces, file system, Newtonsoft, Bayesian) |
| **Hard** | ~55 | WinForms UI, COM interop, deep Outlook integration |
| **Skip** | ~20+ | Designer.cs, commented stubs, pure interfaces |

---

## Recommended Phasing Strategy

### Phase 1: Quick Wins — Easy files (Target: ~45 files)
**Goal:** Maximum coverage uplift per effort, establish patterns

Priority order within Phase 1:
1. **Close-to-80% files** (small delta to close): ArrayExtensions (77.7%), IEnumerableExtensions (70.6%), ConcurrentObservableDictionary (77.4%), AbstractCloneable (77.8%), TreeNodeOfT (76.8%), StackGeek (72.2%), StackObjectCS (72%), WrapperScDictionary (70.7%), WrapperScoDictionary (76%), MyFileSystemInfo (71%), FilePathHelperConverter (72%), SyncContextForm.Designer.cs (78.6%), EmailTokenizer (74.8%)
2. **Pure-logic helpers:** PrettyPrint, DeepCompare, Initializer, DebugTextWriter, TraceUtility, SmithWaterman (pure algorithm), StringManipulation
3. **Data classes at 0%:** FilterEntry, BayesianMetricTypes, EmailFilerConfig, NConsoleTraceWriter, PropertyStore
4. **Simple extension methods:** IAsyncEnumerableExtensions, AsyncSerialization
5. **Data structures:** LockingLinkedList/Node, TimedQueueOfActions, ThreadSafeFunctions, TimeOutTask, AsyncLazy, SimpleActionBagObserver, SimpleActionLockingLinkedListObserver
6. **EmailIntelligence data:** CtfIncidenceList, CtfMap, SubjectMapEntry, MovedMailInfo, ImageStripper, DedicatedToken

### Phase 2: Medium Difficulty (~55 files)
**Goal:** Cover mocking-dependent classes using established patterns

1. **Newtonsoft converters:** ScDictionaryConverter, NonRecursiveConverter, MonoExtension, PeopleScoConverter, PeopleScoRemainingObjectConverter, WrapperPeopleScoDictionaryNew
2. **Serializable collections:** ScBag, ScoCollection, SCODictionary, ScoSortedDictionary, ScoStack, SerializableList, ScoDictionaryNew, SloLinkedList, ScDictionary
3. **SmartSerializable framework:** SmartSerializable, SmartSerializableBase, SmartSerializableLoader, SmartSerializableStatic, NewSmartSerializableConfig
4. **Bayesian core:** BayesianClassifierShared, BayesianClassifierGroup, Corpus, BayesianClassifierExtensions
5. **OutlookObjects (mocked COM):** Items already partially tested — extend AttachmentHelper, AttachmentSerializable, CreateCategory, StoreWrapper, OutlookItem*, RecipientStatic, UserDefinedFields
6. **Threading:** ProgressTracker, ProgressTrackerAsync, AsyncMultiTasker, ThreadMonitor
7. **EmailIntelligence domain:** FlagTranslator, IntelligenceConfig, PeopleScoDictionaryNew, SubjectMapEncoder, SubjectMapSco, RecentsList

### Phase 3: Hard Files (~55 files)
**Goal:** Cover WinForms and deep COM classes, may require testability refactoring

1. **Outlook COM-heavy:** ConversationHelper, MailItemHelper, StoreWrapperController, OlTableExtensions, OlToDoTable, CaptureEmailAddressesModule2
2. **ClassifierGroups (all at 0%):** These depend heavily on IApplicationGlobals and COM — may need facade extraction
3. **WinForms dialogs:** InputBox, MyBox, YesNoToAll logic extraction, DelegateButton, FunctionButton
4. **WinForms helper classes:** ControlPosition, ControlResizer, ImageHelper, MouseDownFilter, OlvExtension, ScreenHelper, TableLayoutHelper, Theme, ThemeControlGroup, TipsController
5. **WinForms viewers:** FilterOlFoldersViewer, FolderInfoViewer, OSBrowser, FolderRemapViewer, FolderSelector, ConfigViewer, ProgressViewer, ProgressPane, SubjectMapMetrics
6. **Other hard:** DispatchUtility (COM dispatch), ComStreamWrapper (WIP), OneDriveDownloader (Graph API), ShellUtilities (Shell32), IdleActionQueue/IdleAsyncQueue (UI thread)

### Phase 4: Evaluate Skips
Review whether Designer.cs files, deprecated "To Depricate" code, and WIP stubs should be:
- Excluded from coverage gate via coverage config
- Given minimal constructor/smoke tests
- Removed from the project if truly dead code

---

## Risks and Special Considerations

### 1. WinForms Testing Challenges
~30+ files are WinForms Forms, UserControls, or depend on WinForms controls. These require:
- Running tests with `[STAThread]` attribute or SynchronizationContext
- Creating control instances that may need `ISite`, `IContainer`, or message pumps
- Potential `ObjectDisposedException` or `InvalidOperationException` on cross-thread access
- **Mitigation:** Extract testable logic from UI code-behind into testable helper classes; use `[TestMethod]` with `STATestMethodAttribute` if needed

### 2. COM Interop (Outlook)
~30+ files depend on `Microsoft.Office.Interop.Outlook` types. Existing test patterns use Moq to mock COM interfaces, but:
- Some COM types require `EmbedInteropTypes=True` making mocking harder
- Some code uses runtime COM dispatch (`Type.InvokeMember`, `IDispatch`)
- **Mitigation:** Existing test files (e.g., OutlookItemTests, FolderWrapperStateTests) already demonstrate successful COM mocking patterns — follow those patterns

### 3. Explicit Compile Include Requirement
Every new test file must be manually added to `UtilitiesCS.Test.csproj` `<Compile Include>` section. Missing this will cause the test to silently not compile or run.

### 4. Serialization Testing Complexity
SmartSerializable and SCO collection classes involve file-based serialization. Tests must avoid actual file I/O per policy (no temp files). Options:
- Mock file system via constructor injection
- Test serialization logic in isolation via streams/StringWriter
- Focus on in-memory operations

### 5. Deprecated Code ("To Depricate")
Three files (CSVDictUtilities, FileIO2, StringManipulation) are in a "To Depricate" folder but still compiled. Consider whether these should simply be removed rather than tested.

### 6. Obsolete Bayesian Code
Files in `EmailIntelligence\Bayesian\Obsolete\` are legacy implementations. Testing them is lower priority since they're superseded by newer implementations.

### 7. SDIL Reader
`NewtonsoftHelpers\SDIL Reader\` contains Mono.Cecil-based IL reading code (ILGlobals, ILInstruction, MethodBodyReader). These require reflection/IL emission test patterns.

### 8. File Count Discrepancy
The issue states "196 files below 80%", but the coverage report shows ~155 files with explicit line-rate below 80%. The discrepancy is likely due to:
- ~16 Designer.cs files at 0%
- ~4 commented stubs counted but having no lines
- Files compiled but not appearing in coverage (zero instrumented lines)
- CaptureEmailAddressesModule2.cs not appearing in coverage
- Possible enumeration of interface files with `IGenericTimer.cs` type implementations

---

## Key Metrics

| Metric | Value |
|---|---|
| Total compiled files (csproj) | ~230 |
| Files already ≥ 80% coverage | ~75 |
| Files below 80% (from coverage data) | ~155 |
| Designer.cs files below 80% | ~16 |
| Commented stubs (no executable code) | ~4 |
| Pure interfaces (no executable code) | ~40+ |
| Existing test files in UtilitiesCS.Test | ~120+ |
| Files at 0% coverage | ~80 |
| Files between 1–50% | ~35 |
| Files between 50–79% | ~40 |
