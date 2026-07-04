# Remediation Cycle 3 Focused Tests

Timestamp: 2026-07-04T17:04:46.9379107-04:00
Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcHomeControllerExecuteMovesTests|FullyQualifiedName~EfcHomeControllerLifecycleTests|FullyQualifiedName~TlpCellStatesTests" /InIsolation
EXIT_CODE: 0

Output:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed EmptyConstructor_CreatesEmptyStateDictionary [28 ms]
  Passed TypedCollectionConstructor_PreservesSnapshotListsByKey [4 ms]
  Passed RawCollectionConstructor_ConvertsListsToTlpCellSnapShotLists [5 ms]
  Passed CollectionConstructors_WithEmptyInputs_CreateEmptyStateDictionary [< 1 ms]
  Passed TypedCollectionConstructor_WithDuplicateKeys_ThrowsArgumentException [4 ms]
  Passed TryAddState_WithoutSnapshots_AddsOnlyMissingState [2 ms]
  Passed TryAddState_WithSnapshots_AddsConvertedListOnlyForMissingState [< 1 ms]
  Passed TypedCollectionConstructor_WithNullInput_ThrowsArgumentNullException [< 1 ms]
  Passed RawCollectionConstructor_WithNullInput_ThrowsArgumentNullException [< 1 ms]
  Passed SnapshotConstructor_CapturesControlCellState [94 ms]
  Passed RowAndColumnAccessors_UpdateCellPosition [< 1 ms]
  Passed ApplyState_WhenControlHasDifferentParent_ReparentsAndRestoresCell [141 ms]
  Passed SelectMoveMetricsItems_WhenMovingConversation_ReturnsAllSameFolderItems [23 ms]
  Passed SelectMoveMetricsItems_WhenMovingSingleItem_FiltersByCurrentMailEntryId [< 1 ms]
  Passed TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset [< 1 ms]
  Passed MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions [< 1 ms]
  Passed HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction [2 ms]
  Passed HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction [< 1 ms]
  Passed CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies [47 ms]
  Passed LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies [1 ms]
  Passed Run_WithoutMail_ShowsMessageThroughInjectedSeam [2 ms]
  Passed RunAsync_WithoutMail_ShowsMessageThroughInjectedSeam [< 1 ms]
  Passed Run_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed RunAsync_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed Cleanup_ClearsControllerFieldsAndInvokesParentCleanup [< 1 ms]
  Passed ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances [< 1 ms]
  Passed LoadedAndFilerQueue_PreserveNotImplementedContracts [< 1 ms]
  Passed OpenFolderMethods_DelegateToDataModelWithoutExternalServices [2 ms]

Test Run Successful.
Total tests: 28
     Passed: 28
 Total time: 1.2509 Seconds
```
