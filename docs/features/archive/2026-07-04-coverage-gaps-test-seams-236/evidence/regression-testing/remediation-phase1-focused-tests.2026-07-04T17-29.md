# Remediation Phase 1 Focused Tests

Timestamp: 2026-07-04T17:49:47.7526740-04:00
Command: vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~RelativePathCoverageTests|FullyQualifiedName~ArrayExtensionsCoverageTests|FullyQualifiedName~IEnumerableExtensionsCoverageTests|FullyQualifiedName~PrettyPrintCoverageTests|FullyQualifiedName~SerializableListCoverageTests|FullyQualifiedName~TimeOutTaskCoverageTests" /InIsolation
EXIT_CODE: 0

Output Summary:
A total of 2 test files matched the specified pattern.
Test Run Successful.
Total tests: 58
     Passed: 58
 Total time: 1.9871 Seconds

Full Output:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 2 test files matched the specified pattern.
  Passed MakeRelativePath_WhenPathsShareRoot_ReturnsDecodedRelativeFilePath [37 ms]
  Passed MakeRelativePath_WhenSchemesDiffer_ReturnsOriginalTarget [3 ms]
  Passed GetRelativeUri_WhenTargetIsDescendant_AddsCurrentDirectoryPrefix [< 1 ms]
  Passed GetRelativeUri_WhenTargetRequiresTraversal_PreservesTraversalSegments [< 1 ms]
  Passed AbsoluteFromUri_WhenUriIsRelativeTraversal_NormalizesSegments [25 ms]
  Passed AbsoluteFromUri_WhenUriIsAbsoluteUri_ReturnsOriginalValue [< 1 ms]
  Passed GetFullPath_WhenRelativePathIsDriveRooted_UsesBaseDrive [< 1 ms]
  Passed GetFullPath_WhenBasePathIsNotFullyQualified_ThrowsArgumentException [43 ms]
  Passed PublicPathMethods_WhenRequiredInputsAreEmpty_ThrowArgumentNullException [< 1 ms]
Test Parallelization enabled for C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed GetFullPath_WhenPathIsAlreadyFullyQualified_ReturnsNormalizedPath [< 1 ms]
  Passed TimeoutAfter_WithCompletedGenericTask_ReturnsCompletedResult [32 ms]
  Passed ToStringArray_WhenOneDimensionalValuesContainNull_UsesReplacement [35 ms]
  Passed ConstructorsAndListOperations_HandleConstructionAddRemoveAndEmptyLists [35 ms]
  Passed CastNullSafe_WithNonGenericSource_ConvertsValuesAndNulls [35 ms]
  Passed ToStringArray_WhenTwoDimensionalArrayIsEmpty_ReturnsEmptyArray [< 1 ms]
  Passed CastNullSafe_WithTypedSource_ReturnsExistingEnumerable [< 1 ms]
  Passed SliceRowAndSliceColumn_WhenBoundaryIndexIsUsed_ReturnExpectedValues [1 ms]
  Passed PropertyChangedAndCollectionMembers_ReportExpectedState [2 ms]
  Passed To2D_WhenSourceIsEmpty_ReturnsZeroByZeroArray [< 1 ms]
  Passed CompareTo_WithEmptyAndNullInputs_ReturnsExpectedDifferences [7 ms]
  Passed TimeoutAfter_WithZeroTimeout_FaultsGenericAndNonGenericTasks [9 ms]
  Passed WithProgressReporting_IsDeferredUntilEnumeration [1 ms]
  Passed TimeoutAfter_WithInfiniteTimeout_ReturnsOriginalTasks [< 1 ms]
  Passed ToJustifiedText_WithScalarInputs_HandlesPaddingTruncationAndInvalidWidth [51 ms]
  Passed WithProgressReporting_WithCountCallback_HandlesEmptySource [4 ms]
  Passed WithProgressReporting_WithNullSource_ThrowsArgumentNullException [< 1 ms]
  Passed WithAction_IsDeferredAndRunsOncePerItem [1 ms]
  Passed WithAction_WithNullSource_ThrowsArgumentNullException [< 1 ms]
  Passed To2D_WhenSourceContainsNullRow_ThrowsInvalidOperationException [16 ms]
  Passed TimeoutAfter_MarshalsFaultAndCancellationFromControlledTasks [8 ms]
  Passed IsInitialized_WhenArraysAreNull_ReturnsFalse [< 1 ms]
  Passed SearchArry4Str_WhenSearchStringIsBlank_ReturnsOriginalArray [< 1 ms]
  Passed ToFormattedText_WithCollectionRows_FormatsHeadersAggregatorsAndBoundaries [8 ms]
  Passed FilepathFilenameAndFolderpath_ComposePathsAndRejectFolderPathInput [19 ms]
  Passed SentenceJoin_WhenCustomSeparatorsAreProvided_UsesConfiguredSeparators [1 ms]
  Passed Chunk_ValidatesNullEmptyAndBoundaryInputs [9 ms]
  Passed ToFormattedText_WithNestedDictionaryValues_UsesConverters [7 ms]
  Passed RunWithTimeout_WithImmediateCompletion_ReturnsResult [8 ms]
  Passed RunWithTimeout_WithPreCanceledToken_ThrowsOperationCanceledException [1 ms]
  Passed ToFormattedText_WithNullCells_NormalizesToEmptyText [2 ms]
  Passed ToFormattedText_WithEmptyInputs_ReturnsBoundaryMessages [< 1 ms]
  Passed RunWithTimeout_WithTaskCancellation_ReturnsDefaultAfterAttempts [3 ms]
  Passed RunWithTimeout_WithStrictException_PropagatesException [4 ms]
  Passed RunWithTimeout_WithNonStrictException_ReturnsDefault [2 ms]
  Passed RunWithTimeout_WithAsyncActionRetry_CompletesAfterCancellation [1 ms]
  Passed ArrayToDatatable_WithNestedAndNullValues_BuildsExpectedRows [32 ms]
  Passed ArrayToDatatable_WithHeaderMismatch_ThrowsArgumentException [< 1 ms]
  Passed ToFormattedTextAndMarkdown_WithTwoDimensionalArray_RenderCollectionBoundaries [< 1 ms]
  Passed Chunk_DoesNotEnumerateSourceBeforeResultIsEnumerated [54 ms]
  Passed SplitTestTrain_UsesDeterministicBoundaryPercentages [1 ms]
  Passed SplitTestTrain_ValidatesNullEmptyAndInvalidPercentages [4 ms]
  Passed JsonSerializationShape_RoundTripsWithoutDiskIo [76 ms]
  Passed SerializeThreadSafe_WritesJsonThroughInjectedFileSystem [106 ms]
  Passed SerializeAndDeserialize_WithNoConfiguredPath_DoNotChangeList [< 1 ms]
  Passed Deserialize_WithValidInjectedJson_LoadsItemsWithoutPrompt [5 ms]
  Passed Deserialize_WithMissingFileAndBackupLoader_UsesBackupWithoutDiskIo [1 ms]
  Passed Deserialize_WithPromptNoThenYes_CreatesEmptyListWithoutBackupLoader [4 ms]
  Passed FromListAndFindIndex_ReplaceStateAndFindExpectedItems [1 ms]

Test Run Successful.
Total tests: 58
     Passed: 58
 Total time: 1.9871 Seconds
```
