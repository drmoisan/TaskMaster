# Final Coverage Verification

Timestamp: 2026-03-20T22:36:17.0590710-04:00
Command: Coverage verification against `coverage/coverage.cobertura.xml`, `evidence/baseline/baseline-test-coverage.md`, and `evidence/other/skip-candidates.md`
EXIT_CODE: 1

## Verification Summary

- **Baseline UtilitiesCS line coverage:** 34.27%
- **Current UtilitiesCS line coverage:** 47.29%
- **Repo-wide UtilitiesCS coverage delta vs baseline:** +13.02 percentage points
- **Non-skip UtilitiesCS files below 80%:** 94
- **Baseline comparison result:** PASS
- **Per-file >=80% result (excluding documented skip candidates):** FAIL

## Acceptance Result

[P5-T5] does **not** satisfy its acceptance criteria on the current coverage artifact because 94 non-skip UtilitiesCS production files remain below 80% line coverage.

## Non-Skip UtilitiesCS Files Below 80%

| File | Current Line Rate |
|---|---|
| UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs | 0% |
| UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs | 0% |
| UtilitiesCS\Extensions\DfDeedle.cs | 0% |
| UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs | 0% |
| UtilitiesCS\Threading\ThreadMonitor.cs | 0% |
| UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs | 1.82% |
| UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs | 4.05% |
| UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianSerializationHelper.cs | 4.43% |
| UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs | 7.3% |
| UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs | 8.15% |
| UtilitiesCS\Threading\AsyncMultiTasker.cs | 8.54% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs | 10.61% |
| UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs | 11.84% |
| UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoCollection.cs | 12.22% |
| UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\ScDictionary.cs | 12.86% |
| UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs | 13% |
| UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs | 13.16% |
| UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs | 14.31% |
| UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs | 17.76% |
| UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs | 20.32% |
| UtilitiesCS\NewtonsoftHelpers\WrapperPeopleScoDictionaryNew.cs | 22.62% |
| UtilitiesCS\Extensions\DfMLNet.cs | 22.81% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs | 24.12% |
| UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs | 24.25% |
| UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs | 24.56% |
| UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs | 29.31% |
| UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs | 29.76% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs | 29.77% |
| UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNew.cs | 30.95% |
| UtilitiesCS\HelperClasses\CloningFunctions\DeepCompare.cs | 31.25% |
| UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs | 33.33% |
| UtilitiesCS\Threading\ProgressTrackerAsync.cs | 35% |
| UtilitiesCS\EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs | 35.44% |
| UtilitiesCS\ReusableTypeClasses\Serializable\SerializableList.cs | 35.87% |
| UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs | 36.64% |
| UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs | 38.02% |
| UtilitiesCS\ReusableTypeClasses\AsyncLazy\AsyncLazy.cs | 39.29% |
| UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoStack.cs | 40.23% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs | 42.02% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs | 42.35% |
| UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILInstruction.cs | 43.33% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs | 43.62% |
| UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs | 46.51% |
| UtilitiesCS\OutlookObjects\Recipient\RecipientStatic.cs | 46.72% |
| UtilitiesCS\Extensions\AsyncSerialization.cs | 47.74% |
| UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedListNode.cs | 48.98% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs | 49.82% |
| UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs | 50% |
| UtilitiesCS\Threading\TimeOutTask.cs | 50.5% |
| UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEntry.cs | 50.83% |
| UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloLinkedList.cs | 51.35% |
| UtilitiesCS\OutlookObjects\Item\OutlookItemTry.cs | 52.67% |
| UtilitiesCS\EmailIntelligence\EmailParsingSorting\ImageStripper.cs | 53.46% |
| UtilitiesCS\Threading\ProgressTracker.cs | 54.36% |
| UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggable.cs | 58.23% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs | 59.81% |
| UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs | 60% |
| UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs | 62.17% |
| UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs | 62.5% |
| UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs | 62.5% |
| UtilitiesCS\OutlookObjects\Attachment\AttachmentSerializable.cs | 62.5% |
| UtilitiesCS\OutlookObjects\Item\OutlookItemExtensions.cs | 62.86% |
| UtilitiesCS\EmailIntelligence\OlFolderTools\OlFolderHelper\SmithWaterman.cs | 62.96% |
| UtilitiesCS\OutlookObjects\Item\OutlookItem.cs | 63.51% |
| UtilitiesCS\HelperClasses\Logging\DebugTextWriter.cs | 63.64% |
| UtilitiesCS\EmailIntelligence\Ctf\CtfIncidenceList.cs | 64.48% |
| UtilitiesCS\OutlookObjects\Item\OutlookItemTryGet.cs | 64.86% |
| UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs | 65.1% |
| UtilitiesCS\OutlookObjects\Category\CreateCategory.cs | 65.52% |
| UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs | 66.32% |
| UtilitiesCS\EmailIntelligence\Ctf\CtfMap.cs | 68.61% |
| UtilitiesCS\HelperClasses\PrettyPrint.cs | 69.04% |
| UtilitiesCS\NewtonsoftHelpers\MonoExtension\MonoExtension.cs | 69.57% |
| UtilitiesCS\OutlookObjects\Attachment\AttachmentHelper.cs | 69.89% |
| UtilitiesCS\HelperClasses\Logging\TraceUtility.cs | 70.49% |
| UtilitiesCS\NewtonsoftHelpers\WrapperScDictionary.cs | 70.75% |
| UtilitiesCS\HelperClasses\FileSystem\MyFileSystemInfo.cs | 71.01% |
| UtilitiesCS\Extensions\IEnumerableExtensions.cs | 71.08% |
| UtilitiesCS\ReusableTypeClasses\TimedActions\TimedQueueOfActions.cs | 71.48% |
| UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs | 72.15% |
| UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs | 72.34% |
| UtilitiesCS\NewtonsoftHelpers\NonRecursiveConverter.cs | 72.41% |
| UtilitiesCS\EmailIntelligence\Bayesian\BayesianClassifierShared.cs | 73.78% |
| UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs | 74.44% |
| UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailTokenizer.cs | 74.78% |
| UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggableTry.cs | 75.51% |
| UtilitiesCS\OutlookObjects\Table\OlToDoTable.cs | 75.58% |
| UtilitiesCS\NewtonsoftHelpers\WrapperScoDictionary.cs | 76% |
| UtilitiesCS\ReusableTypeClasses\Other\StackObjectCS.cs | 76% |
| UtilitiesCS\NewtonsoftHelpers\DerivedCompositionConverter_ConcurrentDictionary.cs | 76.96% |
| UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs | 77.35% |
| UtilitiesCS\ReusableTypeClasses\Other\TreeNodeOfT.cs | 77.75% |
| UtilitiesCS\ReusableTypeClasses\Other\AbstractCloneable.cs | 77.78% |
| UtilitiesCS\ReusableTypeClasses\Concurrent\Observable\Dictionary\ConcurrentObservableDictionary.cs | 78.98% |
