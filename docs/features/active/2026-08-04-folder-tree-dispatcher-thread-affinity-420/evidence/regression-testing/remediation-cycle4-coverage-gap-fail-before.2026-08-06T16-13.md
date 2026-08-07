Timestamp: 2026-08-06T16-13
Command: $deltaPath='docs\\features\\active\\2026-08-04-folder-tree-dispatcher-thread-affinity-420\\evidence\\qa-gates\\remediation-cycle3-coverage-and-quality-delta.2026-08-05T05-48.md'; $coveragePath='docs\\features\\active\\2026-08-04-folder-tree-dispatcher-thread-affinity-420\\evidence\\qa-gates\\remediation-cycle3-coverage-final.cobertura.xml'; $delta=Get-Content -Raw $deltaPath; $coverage=[xml](Get-Content -Raw $coveragePath); $historical=[regex]::Match($delta,'759/876').Value; $targets='AppOlObjects\\.FolderTreeService|FilterOlFoldersController\\.Lifecycle|OutlookFolderTreeService|WpfUiDispatcher'; $classes=@($coverage.coverage.packages.package.classes.class | Where-Object { $_.filename -match $targets }); "HistoricalChangedProduction=$historical"; foreach($class in $classes){ "CLASS=$($class.filename)"; foreach($method in @($class.methods.method)){ $unhit=@($method.lines.line | Where-Object {[int]$_.hits -eq 0} | ForEach-Object {$_.number}) -join ','; if($unhit){ "METHOD=$($method.name);UNHIT_LINES=$unhit" } } }; $parts=$historical.Split('/'); if(([int]$parts[0]/[int]$parts[1]) -lt 0.90){ Write-Error "EXPECTED_THRESHOLD_FAILURE: $historical is below 90%"; exit 1 }; exit 0
EXIT_CODE: 1
Output Summary: Expected static threshold failure: historical changed-production coverage is 759/876 (86.64%), below the >=90% gate. The command read only the Cycle-3 delta and Cobertura XML; it did not launch tests, access live Outlook, or create a viewer. Historical module measures are AppOlObjects.FolderTreeService.cs 268/301 (89.04%) and FilterOlFoldersController.Lifecycle.cs 255/325 (78.46%).

## Exact unhit coverage inventory

| Production file | Method | Unhit lines |
| --- | --- | --- |
| `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` | `get_FolderTreeService` | 131, 132, 133, 134 |
| | `CompleteFolderTreeServiceComposition` | 214-222, 236-240 |
| | `DisposeFolderTreeServiceCandidate` | 258 |
| | `ObserveFolderTreeServiceDispatchTerminal` | 298 |
| | `IsFolderTreeServiceDispatcherThread` | 352 |
| | `OnFolderTreeServiceBeforeInitializationCompletion` | 358 |
| | `OnFolderTreeServiceInitializationTerminal` | 362 |
| | `LoadFolderTreeService` | 368-374, 378-380 |
| `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs` | `CloseViewerAfterInitializationFailure` | 148-150, 155-161 |
| | `CreateAndShowViewer` | 165-166, 168-170, 172-173, 175-184, 186, 188 |
| | `ObserveFolderTreeRefreshFault` | 284-289 |
| | `CreateFolderTreeRequest` | 297-298, 303-304 |
| | `CreateCompatibilityView` | 322-323, 335-336 |
| | `CreateArchiveRootSnapshot` | 349-350, 355-356 |
| | `TryCommitFolderTreeView` | 375-377 |
| | `TryAttachSnapshotSubscription` | 388-389 |
| | `Dispose` | 453-454 |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs` | `TryAuthorizeBuild` | 128-131 |
| | `MergeRefreshRequests` | 322 |
| | `ExecuteCleanup` | 419 |
| | `NotifyObserver` | 476-479 |
| `UtilitiesCS/Threading/WpfUiDispatcher.cs` | `InvokeAsync(Action)` | 43 |

`CreateFolderTreeServiceDispatcher` is included in the inspected current Cobertura method inventory and is recorded there as 1/1 covered. It has no unhit Cobertura line in this static source. The current baseline still proves the overall gate failure and the listed AppOlObjects disposed/composition, controller lifecycle, Outlook cleanup-observer, and WPF dispatch gaps.

## Non-live proof

This task used only `Get-Content` and XML parsing of existing evidence artifacts. It did not invoke a test runner, Outlook, a WPF dispatcher, or a viewer. The prior timed-out wrapper record remains historical test-infrastructure evidence at `remediation-cycle4-coverage-gap-fail-before.2026-08-06T16-11.md` and is not used as a product PASS/FAIL result.
