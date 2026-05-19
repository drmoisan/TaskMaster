# Baseline Per-File Coverage — UtilitiesCS files below 80% line rate

Timestamp: 2026-03-23T00:20:00Z

Source: coverage/coverage.cobertura.xml (run captured in baseline-test-coverage.md)

Total sub-80% UtilitiesCS files: 105

---

## 0% (48 files — completely untested)

Categorized:

### Designer files — SKIP (auto-generated, no meaningful logic)
- Threading\ProgressMultiStepViewer.Designer.cs — 0%
- Threading\ProgressPane.Designer.cs — 0%
- Threading\ProgressViewer.Designer.cs — 0%
- EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.Designer.cs — 0%
- EmailIntelligence\OlFolderTools\FilterOlFolders\FolderInfoViewer.Designer.cs — 0%
- EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.Designer.cs — 0%
- EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.Designer.cs — 0%
- EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapViewer.Designer.cs — 0%
- EmailIntelligence\SubjectMap\SubjectMapMetrics.Designer.cs — 0%
- EmailIntelligence\Bayesian\Performance\MetricChartViewer.Designer.cs — 0%
- EmailIntelligence\Bayesian\Performance\ConfusionViewer.Designer.cs — 0%
- ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.Designer.cs — 0%
- Dialogs\FolderNotFoundViewer.Designer.cs — 0%
- Dialogs\InputBoxViewer.Designer.cs — 0%
- HelperClasses\DvgForm.Designer.cs — 0%

### Skip-evaluated by plan (constructor-only shells or untestable)
- EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs — 0% — SKIP (P6-T1: constructor-only WinForms designer shell)
- EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs — 0% — SKIP (P7-T1: constructor-only WinForms designer shell)
- Threading\ProgressMultiStepViewer.cs — 0% — SKIP (P28-T1: constructor-only designer shell)
- Threading\ThreadMonitor.cs — 0% — SKIP (P31-T1: obsolete Thread.Suspend/Resume APIs, timing-sensitive)
- To Depricate\FileIO2.cs — 0% — SKIP (P33-T1: deprecated, direct static file I/O)

### Easy — implementation tasks in P1–P3
- Dialogs\FolderNotFoundViewer.cs — 0% — Phase 1 (P1-T1 to P1-T4)
- Dialogs\InputBox.cs — 0% — Phase 2 (P2-T1 to P2-T3)
- Dialogs\InputBoxViewer.cs — 0% — Phase 3 (P3-T1 to P3-T3)

### Medium — implementation tasks in P4–P29
- Dialogs\MyBox.cs — 0% — Phase 4 (P4-T1 to P4-T3)
- Dialogs\NotImplementedDialog.cs — 0% — Phase 5 (P5-T1 to P5-T3)
- EmailIntelligence\EmailParsingSorting\AutoFile.cs — 0% — Phase 8 (P8-T1 to P8-T3)
- EmailIntelligence\EmailParsingSorting\SortEmail.cs — 0% — Phase 9 (P9-T1 to P9-T3)
- EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs — 0% — Phase 10 (P10-T1 to P10-T4)
- EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs — 0% — Phase 11 (P11-T1 to P11-T4)
- EmailIntelligence\OlFolderTools\FilterOlFolders\FolderInfoViewer.cs — 0% — Phase 12 (P12-T1 to P12-T2)
- EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs — 0% — Phase 13 (P13-T1 to P13-T4)
- EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs — 0% — Phase 14 (P14-T1 to P14-T5)
- EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapViewer.cs — 0% — Phase 15 (P15-T1 to P15-T3)
- EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs — 0% — Phase 16 (P16-T1 to P16-T3)
- EmailIntelligence\SubjectMap\SubjectMapEncoder.cs — 0% — Phase 17 (P17-T1 to P17-T3)
- EmailIntelligence\SubjectMap\SubjectMapMetrics.cs — 0% — Phase 18 (P18-T1 to P18-T2)
- Extensions\DfDeedle.cs — 0% — Phase 19 (P19-T1 to P19-T3)
- HelperClasses\DvgForm.cs — 0% — Phase 20 (P20-T1)
- HelperClasses\ToolTips\QfcTipsDetails.cs — 0% — Phase 21 (P21-T1 to P21-T3)
- HelperClasses\ToolTips\TipsController.cs — 0% — Phase 22 (P22-T1 to P22-T3)
- HelperClasses\Windows Forms\OlvExtension.cs — 0% — Phase 23 (P23-T1 to P23-T2)
- ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs — 0% — Phase 24 (P24-T1 to P24-T2)
- ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs — 0% — Phase 25 (P25-T1 to P25-T3)
- Threading\IdleActionQueue.cs — 0% — Phase 26 (P26-T1 to P26-T3)
- Threading\IdleAsyncQueue.cs — 0% — Phase 27 (P27-T1 to P27-T3)
- Threading\ProgressPane.cs — 0% — Phase 29 (P29-T1 to P29-T3)
- Threading\ProgressViewer.cs — 0% — Phase 30 (P30-T1 to P30-T2)

---

## Partially covered (57 files, 1.4% – 78.6%)

### Easy/Medium implementation tasks
- EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs — 1.4% — Phase 34 (P34-T1 to P34-T3)
- HelperClasses\Windows Forms\ScreenHelper.cs — 3.2% — SKIP (P35-T1: machine monitor topology dependency)
- EmailIntelligence\SubjectMap\SubjectMapSco.cs — 4.1% — Phase 36 (P36-T1 to P36-T3)
- HelperClasses\ThemeHelpers\Theme.cs — 5.6% — SKIP (P37-T1: broad UI/control graph)
- EmailIntelligence\IntelligenceConfig.cs — 7.3% — Phase 38 (P38-T1 to P38-T3)
- EmailIntelligence\EmailParsingSorting\EmailFiler.cs — 8.1% — Phase 39 (P39-T1 to P39-T3)
- ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs — 8.5% — Phase 40 (P40-T1 to P40-T3)
- Threading\AsyncMultiTasker.cs — 8.5% — Phase 41 (P41-T1 to P41-T3)
- EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs — 8.8% — Phase 42 (P42-T1 to P42-T3)
- EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs — 10.6% — Phase 43 (P43-T1 to P43-T3)
- EmailIntelligence\People\PeopleScoDictionaryNew.cs — 13.2% — Phase 44 (P44-T1 to P44-T3)
- ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs — 14.3% — Phase 45 (P45-T1 to P45-T2)
- HelperClasses\FileSystem\FileInfoWrapper.cs — 17.8% — Phase 46 (P46-T1 to P46-T2)
- HelperClasses\FileSystem\DirectoryInfoWrapper.cs — 20.3% — Phase 47 (P47-T1)
- Extensions\DfMLNet.cs — 22.8% — Phase 48 (P48-T1 to P48-T3)
- HelperClasses\Windows Forms\TableLayoutHelper.cs — 24.0% — Phase 49 (P49-T1 to P49-T2)
- EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs — 24.1% — Phase 50 (P50-T1 to P50-T3)
- ReusableTypeClasses\Serializable\Concurrent\ScBag.cs — 24.3% — Phase 51 (P51-T1 to P51-T3)
- EmailIntelligence\Bayesian\CorpusInherit.cs — 24.6% — Phase 52 (P52-T1 to P52-T3)
- Dialogs\FunctionButton.cs — 26.0% — Phase 53 (P53-T1 to P53-T3)
- Dialogs\MyBoxViewer.cs — 28.1% — Phase 54 (P54-T1 to P54-T3)
- Dialogs\YesNoToAll.cs — 28.8% — Phase 55 (P55-T1 to P55-T2)
- EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs — 29.8% — Phase 56 (P56-T1 to P56-T3)
- HelperClasses\Windows Forms\MouseDownFilter.cs — 30.0% — Phase 57 (P57-T1 to P57-T3)
- HelperClasses\FileSystem\ShellUtilities.cs — 31.2% — SKIP (P58-T1: Win32 shell interop, no DI seam)
- HelperClasses\FileSystem\ShellUtilitiesStatic.cs — 33.3% — SKIP (P59-T1: same as ShellUtilities)
- HelperClasses\ThemeHelpers\ThemeControlGroup.cs — 33.6% — Phase 60 (P60-T1 to P60-T3)
- OutlookObjects\Table\OlTableExtensions.cs — 34.5% — Phase 61 (P61-T1 to P61-T3)
- Threading\ProgressTrackerAsync.cs — 35.0% — Phase 62 (P62-T1 to P62-T3)
- Extensions\WinFormsExtensions.cs — 37.5% — Phase 63 (P63-T1 to P63-T3)
- EmailIntelligence\ClassifierGroups\MulticlassEngine.cs — 42.0% — Phase 64 (P64-T1 to P64-T3)
- EmailIntelligence\ClassifierGroups\Triage\Triage.cs — 42.3% — Phase 65 (P65-T1 to P65-T3)
- Threading\ProgressTrackerPane.cs — 42.7% — Phase 66 (P66-T1 to P66-T3)
- EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs — 43.6% — Phase 67 (P67-T1 to P67-T3)
- Threading\ApplicationIdleTimer.cs — 44.8% — Phase 68 (P68-T1 to P68-T3)
- EmailIntelligence\Recents\RecentsList.cs — 46.5% — Phase 69 (P69-T1 to P69-T3)
- OneDriveHelpers\OneDriveDownloader.cs — 46.6% — Phase 70 (P70-T1 to P70-T3)
- EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs — 49.8% — Phase 71 (P71-T1 to P71-T3)
- HelperClasses\FileSystem\FileSystemInfoWrapper.cs — 50.0% — Phase 72 (P72-T1 to P72-T2)
- HelperClasses\CloningFunctions\DispatchUtility.cs — 53.9% — Phase 73 (P73-T1 to P73-T3)
- Threading\ProgressTracker.cs — 54.4% — Phase 74 (P74-T1 to P74-T3)
- HelperClasses\WipUnfinished\ComStreamWrapper.cs — 57.9% — Phase 75 (P75-T1 to P75-T3)
- EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs — 59.8% — Phase 76 (P76-T1 to P76-T3)
- OutlookObjects\Store\StoreWrapperController.cs — 60.0% — Phase 77 (P77-T1 to P77-T3)
- EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs — 62.2% — Phase 78 (P78-T1 to P78-T3)
- HelperClasses\ThemeHelpers\SystemThemeDetector.cs — 62.5% — SKIP (P79-T1: static registry reads, environment-dependent)
- EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs — 62.7% — Phase 80 (P80-T1 to P80-T3)
- ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedListNode.cs — 65.3% — Phase 81 (P81-T1 to P81-T3)
- Extensions\AsyncSerialization.cs — 65.3% — Phase 82 (P82-T1 to P82-T3)
- Dialogs\DelegateButton.cs — 65.6% — Phase 83 (P83-T1 to P83-T3)
- ReusableTypeClasses\TimedActions\TimedDiskWriter.cs — 66.3% — Phase 84 (P84-T1 to P84-T3)
- Threading\UiThread.cs — 69.1% — Phase 85 (P85-T1 to P85-T3)
- EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs — 70.7% — Phase 86 (P86-T1 to P86-T3)
- ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs — 72.3% — Phase 87 (P87-T1 to P87-T3)
- OutlookObjects\Table\OlToDoTable.cs — 75.6% — Phase 88 (P88-T1 to P88-T3)
- HelperClasses\FileSystem\FilePathHelper.cs — 76.4% — Phase 89 (P89-T1 to P89-T3)
- Threading\SyncContextForm.Designer.cs — 78.6% — DESIGNER file (auto-generated; coverage from .cs companion)

---

## Summary counts
- Total sub-80% files: 105
- Designer (.Designer.cs) — SKIP: 16
- Constructor-only shell — SKIP: 3 (ConfusionViewer, MetricChartViewer, ProgressMultiStepViewer)
- Untestable/deprecated — SKIP: 7 (ThreadMonitor, FileIO2, ScreenHelper, Theme, ShellUtilities, ShellUtilitiesStatic, SystemThemeDetector)
- Implementation tasks assigned in P1–P89: 78 files
