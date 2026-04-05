# Remaining Sub-80 Reconciliation

Timestamp: 2026-03-22T09:05:48.3988731-04:00
Command: Manual reconciliation of `evidence/qa-gates/final-coverage-verification.md` against `plan.2026-03-19T21-49.md` and `evidence/other/skip-candidates.md`
EXIT_CODE: 0

## Summary

- Source file count from `final-coverage-verification.md`: 94 non-skip UtilitiesCS files below 80%
- Rows in reconciliation matrix: 94
- Files mapped to remaining implementation tasks: 77
- Files mapped to remaining Phase 4 skip tasks: 17
- Unmapped files: 0
- Multi-mapped files: 0

## Reconciliation Matrix

| File | Current Line Rate | Path Type | Remaining Task |
|---|---:|---|---|
| `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs` | 0% | Implementation Task | `P3-T67` |
| `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs` | 0% | Phase 4 Skip Task | `P4-T35` |
| `UtilitiesCS\Extensions\DfDeedle.cs` | 0% | Phase 4 Skip Task | `P4-T36` |
| `UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs` | 0% | Phase 4 Skip Task | `P4-T36` |
| `UtilitiesCS\Threading\ThreadMonitor.cs` | 0% | Phase 4 Skip Task | `P4-T34` |
| `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs` | 1.82% | Implementation Task | `P2-T12` |
| `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs` | 4.05% | Phase 4 Skip Task | `P4-T35` |
| `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianSerializationHelper.cs` | 4.43% | Implementation Task | `P2-T12` |
| `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs` | 7.3% | Phase 4 Skip Task | `P4-T35` |
| `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs` | 8.15% | Implementation Task | `P2-T11` |
| `UtilitiesCS\Threading\AsyncMultiTasker.cs` | 8.54% | Phase 4 Skip Task | `P4-T34` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs` | 10.61% | Implementation Task | `P2-T22` |
| `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs` | 11.84% | Implementation Task | `P2-T8` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoCollection.cs` | 12.22% | Implementation Task | `P2-T6` |
| `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\ScDictionary.cs` | 12.86% | Implementation Task | `P2-T5` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs` | 13% | Implementation Task | `P2-T6` |
| `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs` | 13.16% | Implementation Task | `P2-T19` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs` | 14.31% | Phase 4 Skip Task | `P4-T33` |
| `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs` | 17.76% | Implementation Task | `P2-T16` |
| `UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs` | 20.32% | Implementation Task | `P2-T16` |
| `UtilitiesCS\NewtonsoftHelpers\WrapperPeopleScoDictionaryNew.cs` | 22.62% | Implementation Task | `P2-T4` |
| `UtilitiesCS\Extensions\DfMLNet.cs` | 22.81% | Implementation Task | `P2-T23` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs` | 24.12% | Phase 4 Skip Task | `P4-T36` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs` | 24.25% | Phase 4 Skip Task | `P4-T33` |
| `UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs` | 24.56% | Phase 4 Skip Task | `P4-T33` |
| `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs` | 29.31% | Implementation Task | `P2-T8` |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs` | 29.76% | Implementation Task | `P3-T4` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` | 29.77% | Implementation Task | `P3-T7` |
| `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNew.cs` | 30.95% | Implementation Task | `P2-T6` |
| `UtilitiesCS\HelperClasses\CloningFunctions\DeepCompare.cs` | 31.25% | Implementation Task | `P1-T6` |
| `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs` | 33.33% | Phase 4 Skip Task | `P4-T36` |
| `UtilitiesCS\Threading\ProgressTrackerAsync.cs` | 35% | Phase 4 Skip Task | `P4-T34` |
| `UtilitiesCS\EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs` | 35.44% | Implementation Task | `P1-T9` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\SerializableList.cs` | 35.87% | Implementation Task | `P2-T7` |
| `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs` | 36.64% | Implementation Task | `P2-T15` |
| `UtilitiesCS\Interfaces\IWinForm\PropertyStore.cs` | 38.02% | Implementation Task | `P1-T8` |
| `UtilitiesCS\ReusableTypeClasses\AsyncLazy\AsyncLazy.cs` | 39.29% | Implementation Task | `P1-T10` |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoStack.cs` | 40.23% | Implementation Task | `P2-T6` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs` | 42.02% | Implementation Task | `P3-T10` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs` | 42.35% | Implementation Task | `P3-T14` |
| `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILInstruction.cs` | 43.33% | Implementation Task | `P2-T3` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs` | 43.62% | Implementation Task | `P3-T8` |
| `UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs` | 46.51% | Implementation Task | `P2-T19` |
| `UtilitiesCS\OutlookObjects\Recipient\RecipientStatic.cs` | 46.72% | Implementation Task | `P2-T15` |
| `UtilitiesCS\Extensions\AsyncSerialization.cs` | 47.74% | Phase 4 Skip Task | `P4-T33` |
| `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedListNode.cs` | 48.98% | Implementation Task | `P2-T20` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs` | 49.82% | Phase 4 Skip Task | `P4-T35` |
| `UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs` | 50% | Implementation Task | `P2-T16` |
| `UtilitiesCS\Threading\TimeOutTask.cs` | 50.5% | Implementation Task | `P1-T12` |
| `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEntry.cs` | 50.83% | Implementation Task | `P1-T4` |
| `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\SloLinkedList.cs` | 51.35% | Implementation Task | `P2-T7` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItemTry.cs` | 52.67% | Implementation Task | `P2-T14` |
| `UtilitiesCS\EmailIntelligence\EmailParsingSorting\ImageStripper.cs` | 53.46% | Implementation Task | `P1-T7` |
| `UtilitiesCS\Threading\ProgressTracker.cs` | 54.36% | Phase 4 Skip Task | `P4-T34` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggable.cs` | 58.23% | Implementation Task | `P2-T14` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs` | 59.81% | Implementation Task | `P3-T6` |
| `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` | 60% | Implementation Task | `P3-T3` |
| `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs` | 62.17% | Implementation Task | `P2-T22` |
| `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs` | 62.5% | Implementation Task | `P1-T8` |
| `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs` | 62.5% | Phase 4 Skip Task | `P4-T37` |
| `UtilitiesCS\OutlookObjects\Attachment\AttachmentSerializable.cs` | 62.5% | Implementation Task | `P2-T15` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItemExtensions.cs` | 62.86% | Implementation Task | `P2-T13` |
| `UtilitiesCS\EmailIntelligence\OlFolderTools\OlFolderHelper\SmithWaterman.cs` | 62.96% | Implementation Task | `P1-T7` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItem.cs` | 63.51% | Implementation Task | `P2-T13` |
| `UtilitiesCS\HelperClasses\Logging\DebugTextWriter.cs` | 63.64% | Implementation Task | `P1-T6` |
| `UtilitiesCS\EmailIntelligence\Ctf\CtfIncidenceList.cs` | 64.48% | Implementation Task | `P1-T4` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItemTryGet.cs` | 64.86% | Implementation Task | `P2-T14` |
| `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs` | 65.1% | Implementation Task | `P2-T11` |
| `UtilitiesCS\OutlookObjects\Category\CreateCategory.cs` | 65.52% | Implementation Task | `P2-T15` |
| `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs` | 66.32% | Implementation Task | `P2-T21` |
| `UtilitiesCS\EmailIntelligence\Ctf\CtfMap.cs` | 68.61% | Implementation Task | `P1-T4` |
| `UtilitiesCS\HelperClasses\PrettyPrint.cs` | 69.04% | Implementation Task | `P1-T5` |
| `UtilitiesCS\NewtonsoftHelpers\MonoExtension\MonoExtension.cs` | 69.57% | Implementation Task | `P2-T2` |
| `UtilitiesCS\OutlookObjects\Attachment\AttachmentHelper.cs` | 69.89% | Implementation Task | `P2-T15` |
| `UtilitiesCS\HelperClasses\Logging\TraceUtility.cs` | 70.49% | Implementation Task | `P1-T6` |
| `UtilitiesCS\NewtonsoftHelpers\WrapperScDictionary.cs` | 70.75% | Implementation Task | `P1-T3` |
| `UtilitiesCS\HelperClasses\FileSystem\MyFileSystemInfo.cs` | 71.01% | Implementation Task | `P1-T5` |
| `UtilitiesCS\Extensions\IEnumerableExtensions.cs` | 71.08% | Implementation Task | `P1-T1` |
| `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedQueueOfActions.cs` | 71.48% | Implementation Task | `P1-T10` |
| `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs` | 72.15% | Implementation Task | `P1-T2` |
| `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs` | 72.34% | Implementation Task | `P2-T20` |
| `UtilitiesCS\NewtonsoftHelpers\NonRecursiveConverter.cs` | 72.41% | Implementation Task | `P2-T1` |
| `UtilitiesCS\EmailIntelligence\Bayesian\BayesianClassifierShared.cs` | 73.78% | Implementation Task | `P2-T10` |
| `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs` | 74.44% | Implementation Task | `P2-T16` |
| `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailTokenizer.cs` | 74.78% | Implementation Task | `P1-T4` |
| `UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggableTry.cs` | 75.51% | Implementation Task | `P2-T14` |
| `UtilitiesCS\OutlookObjects\Table\OlToDoTable.cs` | 75.58% | Implementation Task | `P3-T5` |
| `UtilitiesCS\NewtonsoftHelpers\WrapperScoDictionary.cs` | 76% | Implementation Task | `P1-T3` |
| `UtilitiesCS\ReusableTypeClasses\Other\StackObjectCS.cs` | 76% | Implementation Task | `P1-T2` |
| `UtilitiesCS\NewtonsoftHelpers\DerivedCompositionConverter_ConcurrentDictionary.cs` | 76.96% | Implementation Task | `P2-T2` |
| `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs` | 77.35% | Implementation Task | `P3-T2` |
| `UtilitiesCS\ReusableTypeClasses\Other\TreeNodeOfT.cs` | 77.75% | Implementation Task | `P1-T2` |
| `UtilitiesCS\ReusableTypeClasses\Other\AbstractCloneable.cs` | 77.78% | Implementation Task | `P1-T2` |
| `UtilitiesCS\ReusableTypeClasses\Concurrent\Observable\Dictionary\ConcurrentObservableDictionary.cs` | 78.98% | Implementation Task | `P1-T2` |

## Verification Notes

- Every row from the `Non-Skip UtilitiesCS Files Below 80%` table in `evidence/qa-gates/final-coverage-verification.md` is represented exactly once above.
- Each row maps to exactly one currently unchecked remaining plan task.
- Files routed to Phase 4 skip tasks are limited to the explicit policy-constrained groups added by the revised plan (`P4-T33` through `P4-T37`).

## Checklist Validation (P0-T6)

- Result: PASS
- Every file mapped to an `Implementation Task` points to a currently unchecked `P1`, `P2`, or `P3` task.
- Every file mapped to a `Phase 4 Skip Task` points to a currently unchecked `P4-T33` through `P4-T37` task.
- No file assigned to a checked task remains listed in `evidence/qa-gates/final-coverage-verification.md` under `Non-Skip UtilitiesCS Files Below 80%`.
- Revised checklist state and reconciliation matrix are aligned for implementation resumption.
