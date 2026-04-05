# P1-T5: Isolated Branch Diff (Post-Cleanup)

Timestamp: 2026-03-27T09-35
Command: git diff --name-status development...HEAD
EXIT_CODE: 0

## Verification

- VBFunctions.Test/ComputerInfo_Test.cs: NOT PRESENT ✅
- VBFunctions.Test/VBFunctions.Test.csproj: NOT PRESENT ✅
- docs/features/active/2026-03-25-quickfiler-gui-not-expanding-96/ paths: NOT PRESENT ✅
- docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/ paths: NOT PRESENT ✅

## Output Summary

Total lines: 274 (all issue #87 related)

```
A       UtilitiesCS.Test/Dialogs/FolderNotFoundViewer_Tests.cs
M       UtilitiesCS.Test/Dialogs/FunctionButton_Tests.cs
M       UtilitiesCS.Test/Dialogs/InputBox_Test.cs
A       UtilitiesCS.Test/Dialogs/MyBox_Tests.cs
A       UtilitiesCS.Test/Dialogs/NotImplementedDialog_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroup_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs
M       UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/Bayesian/CorpusInherit_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroupUtilities_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/MulticlassEngine_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs
M       UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/EmailFiler_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersController_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersViewer_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FolderInfoViewer_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FolderRemapViewer_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/FolderSelector_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/MovedMailInfo_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/OlFolderTools/FolderRemapTree_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/PeopleScoDictionaryNew_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/RecentsList_Tests.cs
M       UtilitiesCS.Test/EmailIntelligence/SmithWaterman_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/SubjectMapMetrics_Tests.cs
A       UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Tests.cs
M       UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs
A       UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs
A       UtilitiesCS.Test/Extensions/DfMLNet_Tests.cs
M       UtilitiesCS.Test/Extensions/WinFormsExtensions_Tests.cs
M       UtilitiesCS.Test/HelperClasses/ComStreamWrapper_Tests.cs
M       UtilitiesCS.Test/HelperClasses/DeepCompare_Tests.cs
M       UtilitiesCS.Test/HelperClasses/DispatchUtility_Tests.cs
A       UtilitiesCS.Test/HelperClasses/DvgForm_Tests.cs
M       UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs
A       UtilitiesCS.Test/HelperClasses/OlvExtension_Tests.cs
A       UtilitiesCS.Test/HelperClasses/QfcTipsDetails_Tests.cs
M       UtilitiesCS.Test/HelperClasses/ThemeHelpers/ThemeTests.cs
M       UtilitiesCS.Test/HelperClasses/TimedDiskWriterTests.cs
A       UtilitiesCS.Test/HelperClasses/TipsController_Tests.cs
M       UtilitiesCS.Test/Interfaces/PropertyStore_Tests.cs
M       UtilitiesCS.Test/NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionaryTests.cs
M       UtilitiesCS.Test/NewtonsoftHelpers/NonRecursiveConverter_Tests.cs
M       UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs
M       UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs
M       UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs
M       UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs
M       UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/AsyncLazy_Tests.cs
A       UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs
A       UtilitiesCS.Test/ReusableTypeClasses/ConfigController_Tests.cs
A       UtilitiesCS.Test/ReusableTypeClasses/ConfigGroupBox_Tests.cs
A       UtilitiesCS.Test/ReusableTypeClasses/ConfigViewer_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/LockingObservableLinkedListNode_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/LockingObservableLinkedList_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScBag_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScDictionary_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScoCollection_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScoDictionaryNew_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/ScoStack_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/SerializableList_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/SloLinkedList_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/SmartSerializable_Tests.cs
M       UtilitiesCS.Test/ReusableTypeClasses/TimedQueueOfActions_Tests.cs
A       UtilitiesCS.Test/TestData/sco-collection-valid.json
A       UtilitiesCS.Test/TestData/serializable-list-invalid.json
A       UtilitiesCS.Test/TestData/serializable-list-valid.json
M       UtilitiesCS.Test/Threading/ApplicationIdleTimer_Tests.cs
A       UtilitiesCS.Test/Threading/AsyncMultiTasker_Tests.cs
A       UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs
A       UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
A       UtilitiesCS.Test/Threading/ProgressPane_Tests.cs
M       UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
M       UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
A       UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs
A       UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs
A       UtilitiesCS.Test/Threading/TimeOutTask_InternalCoverageTests.cs
A       UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs
M       UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs
M       UtilitiesCS.Test/Threading/UiThread_Tests.cs
M       UtilitiesCS.Test/UtilitiesCS.Test.csproj
M       UtilitiesCS/Dialogs/InputBoxViewer.cs
M       UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianPerformanceMeasurement.cs
M       UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianSerializationHelper.cs
M       UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs
M       UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailTokenizer.cs
M       UtilitiesCS/EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs
M       UtilitiesCS/Extensions/DfDeedle.cs
M       UtilitiesCS/Extensions/IEnumerableExtensions.cs
M       UtilitiesCS/NewtonsoftHelpers/DerivedCompositionConverter_ConcurrentDictionary.cs
M       UtilitiesCS/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew.cs
M       UtilitiesCS/NewtonsoftHelpers/WrapperScDictionary.cs
M       UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs
M       UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs
M       UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs
M       UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs
M       UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoCollection.cs
M       UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs
M       UtilitiesCS/ReusableTypeClasses/Serializable/SerializableList.cs
D       UtilitiesCS/To Depricate/CSVDictUtilities.cs
D       UtilitiesCS/To Depricate/FlattenArray.cs
D       UtilitiesCS/To Depricate/StackObjectVB.cs
M       UtilitiesCS/UtilitiesCS.csproj
A       coverage_output.txt
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/code-review.2026-03-27T08-20.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-analyzer-build.md
M       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-build.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-csharpier.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-nullable-build.md
M       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-per-file-coverage.md
M       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/baseline-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-baseline-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-branch-split-source-map.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-current-diff-scope.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/final87-phase0-instructions-read.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/issue96-merged-precheck.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/issue97-merged-precheck.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/p0-t6-checklist-verification.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-branch-diff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-csharpier.md
M       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-instructions-read.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-remaining-ledger.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/phase0-tests-with-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/remaining-sub80-reconciliation.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/baseline/residual-merged-precheck.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t10-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t11-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t12-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t13-through-p2-t15-coverage-snapshot.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t5-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t7-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/p2-t8-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/archive-source-branch.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/final87-archive-source-branch.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-bootstrap-commit.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-bootstrap-restore-main.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-cherry-pick-batch1.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-cherry-pick-batch2.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-cherry-pick-batch3.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-cherry-pick-batch4.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-focused-diff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue87-worktree-created.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-cherry-pick-a19ac86.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-cherry-pick-ad4ae95.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-focused-diff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-pr.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-push.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/issue97-worktree-created.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/next-pass-handoff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-issue96-docs-cleanup.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-stale-audit-cleanup.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-vbfunctions-computerinfo-cleanup.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/p1-vbfunctions-csproj-cleanup.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-bootstrap-4634ac5.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-bootstrap-a8d24b2.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-bootstrap-ee92dd6.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-0c9a045.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-16d7d5d.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-4d5f476.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-52742b8.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-60408b0.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-66220df.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-cherry-pick-ea0206e.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-focused-diff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-next-pass-handoff.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-pr.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-push.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/residual-worktree-created.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-coverage-delta.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-final-coverage-verification.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-qc-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-qc-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-qc-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue87-qc-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue97-coverage-delta.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue97-qc-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue97-qc-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue97-qc-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/issue97-qc-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-coverage-delta.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-qc-actionlint.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-qc-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-qc-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-qc-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/residual-qc-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/baseline-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/branch-split-source-map.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/current-coverage-headline.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/current-diff-scope.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/phase0-instructions-read.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-actionlint.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-analyzers.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-format.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-nullable.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-baseline-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-commit-map.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-current-coverage-headline.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-current-diff-scope.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/remediation-baseline/residual-phase0-instructions-read.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/feature-audit.2026-03-27T08-20.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/policy-audit.2026-03-27T08-20.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-inputs.2026-03-27T08-20.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/remediation-plan.2026-03-27T08-20.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/baseline/baseline-build.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/baseline/baseline-per-file-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/baseline/baseline-test-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/baseline/phase0-instructions-read.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/baseline/remaining-sub80-reconciliation.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t10-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t11-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t12-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t13-through-p2-t15-coverage-snapshot.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t5-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t7-focused-coverage.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/p2-t8-focused-coverage.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/checkpoints/phase2-checkpoint.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/checkpoints/phase2-checkpoint.md
R098    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/skip-candidates.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/other/skip-candidates.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-coverage-verification.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-coverage-verification.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qa-analyzer-build.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-qa-analyzer-build.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qa-format.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-qa-format.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qa-nullable-build.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-qa-nullable-build.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qa-test-coverage.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-qa-test-coverage.md
R075    docs/features/active/2026-03-19-utilities-coverage-part-three-87/plan.2026-03-19T21-49.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/plan.2026-03-19T21-49.md
R098    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/other/skip-candidates.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/other/skip-candidates.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-coverage-verification.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/evidence/qa-gates/final-coverage-verification.md
R098    docs/features/active/2026-03-19-utilities-coverage-part-three-87/research.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/research.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/spec.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/user-story.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/user-story.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/utilitiescs-coverage-inventory.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/utilitiescs-coverage-missing.md
R100    docs/features/active/2026-03-19-utilities-coverage-part-three-87/issue.md  docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/issue.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/plan.2026-03-22T21-00.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/research.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/spec.md
A       docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/user-story.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-analyzers.md
M       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-format.md
M       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-nullable.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-targeted-test.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/baseline-test-coverage.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/current-coverage-headline.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/current-diff-scope.md
M       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/baseline/phase0-instructions-read.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/issue96-coverage-delta.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/issue96-qc-analyzers.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/issue96-qc-format.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/issue96-qc-nullable.md
A       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/evidence/qa-gates/issue96-qc-test-coverage.md
M       docs/features/archive/2026-03-25-quickfiler-gui-not-expanding-96/issue.md
```
