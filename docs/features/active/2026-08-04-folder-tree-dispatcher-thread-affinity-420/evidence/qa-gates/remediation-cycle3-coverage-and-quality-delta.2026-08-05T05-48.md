# Phase 6 coverage reconciliation

Timestamp: 2026-08-05T05:48:27-04:00
Command: pwsh coverage XML comparison and git diff reconciliation
EXIT_CODE: 1
Output Summary: The strict P6-T5 coverage gate failed because changed-production coverage is 759/876 (86.64%), below 90%, and changed modules and methods remain below 90%. The `lines-valid` denominator is exactly reconciled by Cobertura's class-level and method-level line nodes; it is not a blocker.

## Inputs

- Baseline: `evidence/remediation-baseline/coverage-baseline.cobertura.xml`
- Final: `evidence/qa-gates/remediation-cycle3-coverage-final.cobertura.xml`
- Tracked scope command: `git diff origin/main -- '*.cs'`
- Untracked production additions included in the scope:
  - `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs`
  - `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs`

## Comparable scope

| Scope item | Baseline | Final | Result |
| --- | ---: | ---: | --- |
| Test assemblies | 8 | 8 | PASS |
| Source roots | 1 | 1 (`.`) | PASS |
| Packages | 9 | 9 | PASS |
| Source files | 529 | 531 | Requires reconciliation |

The final coverage run reports 6,137/6,137 passing tests. The common assembly inventory is QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, and VBFunctions.Test.

## Repository coverage and baseline comparison

| Metric | Baseline | Final | Result |
| --- | ---: | ---: | --- |
| Lines covered | 92,417 | 93,441 | +1,024 |
| Lines valid | 109,324 | 110,477 | +1,153 |
| Line coverage | 84.5350% | 84.5796% | PASS: >=80%; no regression |
| Branches covered | 21,083 | 21,404 | +321 |
| Branches valid | 27,320 | 27,696 | +376 |
| Branch coverage | 77.1706% | 77.2819% | No regression |

## Changed-production coverage

The calculation includes executable line locations for tracked `origin/main` C# diff ranges and the complete executable contents of the two untracked production files listed above.

| Changed production file | Covered | Valid | Coverage |
| --- | ---: | ---: | ---: |
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` | 268 | 301 | 89.04% |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs` | 255 | 325 | 78.46% |
| Other tracked changed executable locations | 236 | 250 | 94.40% |
| Total | 759 | 876 | 86.64% |

The changed-production total fails the required >=90% threshold. The two untracked production modules independently fail the required >=90% class/module threshold. Changed methods also below 90% include `CompleteFolderTreeServiceComposition` (48/62, 77.42%), `CloseViewerAfterInitializationFailure` (6/16, 37.50%), `CreateAndShowViewer` (0/19, 0.00%), and `CreateFolderTreeRequest` (9/13, 69.23%).

## Per-method coverage inventory

The following inventory records every method emitted by the final Cobertura report for the two untracked added production modules and the modified service/dispatcher implementations. `RibbonViewer.cs`, `TryFunctionalityInConstruction.cs`, `IUiDispatcher.cs`, `FolderTreeSnapshotBuilder.cs`, and `OutlookFolderHierarchyReader.cs` have changed production source but no changed method body emitted by the report; their absence prevents a passing per-method gate rather than being treated as coverage.

| File | Method | Covered/valid | Coverage |
| --- | --- | ---: | ---: |
| `AppOlObjects.FolderTreeService.cs` | `.ctor` | 1/1 | 100.00% |
|  | `get_FolderTreeService` | 115/119 | 96.64% |
|  | `CompleteFolderTreeServiceComposition` | 48/62 | 77.42% |
|  | `DisposeFolderTreeServiceCandidate` | 9/10 | 90.00% |
|  | `CompleteFolderTreeServiceCompositionFailure` | 18/18 | 100.00% |
|  | `ObserveFolderTreeServiceDispatchTerminal` | 18/19 | 94.74% |
|  | `CompleteFolderTreeServiceCompositionCancellation` | 18/18 | 100.00% |
|  | `NotifyFolderTreeServiceInitializationTerminal` | 6/6 | 100.00% |
|  | `CreateFolderTreeServiceDispatcher` | 1/1 | 100.00% |
|  | `IsFolderTreeServiceDispatcherThread` | 0/1 | 0.00% |
|  | `OnFolderTreeServiceCompositionStarting` | 1/1 | 100.00% |
|  | `OnFolderTreeServiceBeforeInitializationCompletion` | 0/1 | 0.00% |
|  | `OnFolderTreeServiceInitializationTerminal` | 0/1 | 0.00% |
|  | `LoadFolderTreeService` | 0/10 | 0.00% |
|  | `Dispose` | 28/28 | 100.00% |
| `FilterOlFoldersController.Lifecycle.cs` | `.ctor` (globals) | 0/10 | 0.00% |
|  | `.ctor` (injected viewer) | 13/13 | 100.00% |
|  | `.ctor` (injected viewer and dispatcher) | 14/14 | 100.00% |
|  | `InitializeConstruction` | 15/15 | 100.00% |
|  | `CloseViewerAfterInitializationFailure` | 6/16 | 37.50% |
|  | `CreateAndShowViewer` | 0/19 | 0.00% |
|  | `get_FolderTreeView` | 1/1 | 100.00% |
|  | `get_IsDisposed` | 1/1 | 100.00% |
|  | `Viewer_FormClosed` | 1/1 | 100.00% |
|  | `CreateFolderTreeUiDispatcher` | 1/1 | 100.00% |
|  | `ObserveFolderTreeRefreshFault` | 0/6 | 0.00% |
|  | `OnFolderTreeRefreshViewApplied` | 1/1 | 100.00% |
|  | `CreateFolderTreeRequest` | 9/13 | 69.23% |
|  | `CreateCompatibilityView` | 15/19 | 78.95% |
|  | `CreateArchiveRootSnapshot` | 17/21 | 80.95% |
|  | `TryCommitFolderTreeView` | 7/10 | 70.00% |
|  | `TryAttachSnapshotSubscription` | 20/22 | 90.91% |
|  | `SetFolderTreeView` | 10/10 | 100.00% |
|  | `UnsubscribeFolderTreeView` | 9/9 | 100.00% |
|  | `Dispose` | 21/23 | 91.30% |
| `OutlookFolderTreeService.cs` | `TryAuthorizeBuild` | 6/10 | 60.00% |
|  | `HandleNotification` | 30/30 | 100.00% |
|  | `ObserveScheduledRefresh` | 1/1 | 100.00% |
|  | `ReportScheduledRefreshFailure` | 10/10 | 100.00% |
|  | `CreatePublishedSnapshot` | 23/23 | 100.00% |
|  | `MergeRefreshRequests` | 9/10 | 90.00% |
|  | `Dispose` | 50/50 | 100.00% |
|  | `GetPrimaryFailure` | 3/3 | 100.00% |
|  | `ObserveFault` | 6/6 | 100.00% |
|  | `ExecuteCleanup` | 14/15 | 93.33% |
|  | `TryCleanupStage` | 9/9 | 100.00% |
|  | `ReportCleanupFailure` | 8/8 | 100.00% |
|  | `NotifyObserver` | 5/9 | 55.56% |
| `WpfUiDispatcher.cs` | `.ctor` overloads | 5/5 | 100.00% |
|  | `get_Dispatcher` | 1/1 | 100.00% |
|  | `Invoke` | 1/1 | 100.00% |
|  | `InvokeAsync(Action)` | 0/1 | 0.00% |
|  | `InvokeAsync(Action, priority, token)` | 1/1 | 100.00% |
|  | `BeginInvoke` | 1/1 | 100.00% |
|  | `InvokeAsync(Func<Task>)` | 1/1 | 100.00% |
|  | `InvokeAsync<TResult>(Func<Task<TResult>>)` | 1/1 | 100.00% |

## Denominator reconciliation

The coverage XML file-level source-location comparison identifies six files with changed executable-location counts:

| File | Baseline locations | Final locations | Delta |
| --- | ---: | ---: | ---: |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | 247 | 213 | -34 |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` | 215 | 98 | -117 |
| `UtilitiesCS/Threading/WpfUiDispatcher.cs` | 6 | 12 | +6 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs` | 208 | 359 | +151 |
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` | 0 | 301 | +301 |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs` | 0 | 325 | +325 |
| **Net reconciled source-location delta** |  |  | **+632** |

The report-level `lines-valid` denominator is the sum of Cobertura class-level and method-level line nodes, rather than the class-level source-location count alone. The exact reconciliation is:

| Node category | Baseline | Final | Delta |
| --- | ---: | ---: | ---: |
| Class-level line nodes | 61,515 | 62,147 | +632 |
| Method-level line nodes | 47,809 | 48,330 | +521 |
| `lines-valid` total | 109,324 | 110,477 | +1,153 |

Both reports satisfy `lines-valid = class-level line nodes + method-level line nodes`. The +632 class-level source-location delta and +521 method-level node delta exactly reconcile the report-level +1,153 denominator delta. There is no coverage-scope denominator drift.

## Gate outcome

- Repository-wide line coverage >=80%: PASS.
- Changed production lines >=90%: FAIL (86.64%).
- Each new or changed method/class/module >=90%: FAIL.
- No regression versus comparable baseline: PASS.
- Exact denominator reconciliation: PASS.

P6-T5 remains unchecked. P6-T6 was not run. Phase 7 remains blocked pending a remediated coverage result and a passing P6-T5 rerun.
