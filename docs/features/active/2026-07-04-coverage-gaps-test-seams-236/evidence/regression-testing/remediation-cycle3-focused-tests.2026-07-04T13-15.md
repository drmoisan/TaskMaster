# Remediation Cycle 3 Focused Tests

Timestamp: 2026-07-04T17:14:55.7539477-04:00
Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcHomeControllerExecuteMovesTests|FullyQualifiedName~EfcHomeControllerDependenciesTestsProductionFactory|FullyQualifiedName~EfcHomeControllerLifecycleTests|FullyQualifiedName~TlpCellStatesTests" /InIsolation
EXIT_CODE: 0

Output:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed EmptyConstructor_CreatesEmptyStateDictionary [29 ms]
  Passed TypedCollectionConstructor_PreservesSnapshotListsByKey [5 ms]
  Passed RawCollectionConstructor_ConvertsListsToTlpCellSnapShotLists [5 ms]
  Passed CollectionConstructors_WithEmptyInputs_CreateEmptyStateDictionary [< 1 ms]
  Passed TypedCollectionConstructor_WithDuplicateKeys_ThrowsArgumentException [4 ms]
  Passed TryAddState_WithoutSnapshots_AddsOnlyMissingState [2 ms]
  Passed TryAddState_WithSnapshots_AddsConvertedListOnlyForMissingState [< 1 ms]
  Passed TypedCollectionConstructor_WithNullInput_ThrowsArgumentNullException [< 1 ms]
  Passed RawCollectionConstructor_WithNullInput_ThrowsArgumentNullException [< 1 ms]
  Passed SnapshotConstructor_CapturesControlCellState [95 ms]
  Passed RowAndColumnAccessors_UpdateCellPosition [< 1 ms]
  Passed ApplyState_WhenControlHasDifferentParent_ReparentsAndRestoresCell [148 ms]
  Passed Constructor_WithNoOverrides_UsesResettableProductionFactories [46 ms]
  Passed WithFactoryHelpers_ValidateFactoryArguments [30 ms]
  Passed LoadSelection_WithExplicitMail_DoesNotTraverseOutlookSelection [7 ms]
  Passed ConstructorDefaults_InvokeProductionConstructionAdapters [1 ms]
  Passed ResetProductionFactories_ConstructorDelegatesCreateConcreteInstances [94 ms]
  Passed SelectMoveMetricsItems_WhenMovingConversation_ReturnsAllSameFolderItems [3 ms]
  Passed SelectMoveMetricsItems_WhenMovingSingleItem_FiltersByCurrentMailEntryId [< 1 ms]
  Passed TryBeginExecuteMoves_ReturnsFalseUntilExecutionStateIsReset [< 1 ms]
  Passed MoveToFolderAsync_WithInjectedAction_UsesCapturedMoveOptions [< 1 ms]
  Passed ExecuteMovesCoreAsync_UsesFormOptionsAndRoutesSuccessfulMetrics [11 ms]
  Passed HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction [< 1 ms]
  Passed HandleMoveResult_WhenMoveSucceeds_RoutesMetricsThroughInjectedAction [< 1 ms]
  Passed CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies [5 ms]
  Passed LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies [2 ms]
  Passed Run_WithoutMail_ShowsMessageThroughInjectedSeam [1 ms]
  Passed RunAsync_WithoutMail_ShowsMessageThroughInjectedSeam [< 1 ms]
  Passed Run_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed RunAsync_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed Cleanup_ClearsControllerFieldsAndInvokesParentCleanup [< 1 ms]
  Passed ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances [< 1 ms]
  Passed LoadedAndFilerQueue_PreserveNotImplementedContracts [< 1 ms]
  Passed OpenFolderMethods_DelegateToDataModelWithoutExternalServices [1 ms]

Test Run Successful.
Total tests: 34
     Passed: 34
 Total time: 1.4003 Seconds
```
