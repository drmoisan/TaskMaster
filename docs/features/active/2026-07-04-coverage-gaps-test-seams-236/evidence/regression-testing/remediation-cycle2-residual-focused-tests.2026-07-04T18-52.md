# Residual Focused Tests Evidence

Timestamp: 2026-07-04T20:10:03.9556319-04:00
Command: vstest.console.exe SVGControl.Test\bin\Debug\SVGControl.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /TestCaseFilter:"FullyQualifiedName~RelativePathCoverageTests|FullyQualifiedName~ToDoItemCoverageExpansionTests|FullyQualifiedName~QfcQueueCoverageExpansionTests|FullyQualifiedName~TagControllerCoverageExpansionTests|FullyQualifiedName~ProjectDataCoverageExpansionTests|FullyQualifiedName~AppAutoFileObjectsCoverageExpansionTests" /InIsolation
EXIT_CODE: 0

Output Summary:
Test Run Successful.
Total tests: 76
     Passed: 76

Raw Output:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 5 test files matched the specified pattern.
  Passed MakeRelativePath_WhenPathsShareRoot_ReturnsDecodedRelativeFilePath [37 ms]
  Passed MakeRelativePath_WhenSchemesDiffer_ReturnsOriginalTarget [3 ms]
  Passed GetRelativeUri_WhenTargetIsDescendant_AddsCurrentDirectoryPrefix [< 1 ms]
  Passed GetRelativeUri_WhenTargetRequiresTraversal_PreservesTraversalSegments [< 1 ms]
  Passed AbsoluteFromUri_WhenUriIsRelativeTraversal_NormalizesSegments [27 ms]
  Passed AbsoluteFromUri_WhenUriIsAbsoluteUri_ReturnsOriginalValue [< 1 ms]
  Passed GetFullPath_WhenRelativePathIsDriveRooted_UsesBaseDrive [< 1 ms]
  Passed GetFullPath_WhenBasePathIsNotFullyQualified_ThrowsArgumentException [42 ms]
  Passed PublicPathMethods_WhenRequiredInputsAreEmpty_ThrowArgumentNullException [< 1 ms]
  Passed GetFullPath_WhenPathIsAlreadyFullyQualified_ReturnsNormalizedPath [< 1 ms]
  Passed GetFullPath_WithRelativeForms_NormalizesExpectedPath (C:Child\Icon.svg,C:\Root\Parent\,C:\Root\Parent\Child\Icon.svg) [2 ms]
  Passed GetFullPath_WithRelativeForms_NormalizesExpectedPath (D:Child\Icon.svg,C:\Root\Parent\,D:\Child\Icon.svg) [< 1 ms]
  Passed GetFullPath_WithRelativeForms_NormalizesExpectedPath (Child\..\Sibling\.\Icon.svg,C:\Root\Parent\,C:\Root\Parent\Sibling\Icon.svg) [< 1 ms]
  Passed GetFullPath_WhenInputsContainNullCharacter_ThrowsArgumentException [< 1 ms]
  Passed RemoveRelativeSegments_NormalizesTraversalAndSeparators (C:\Root\Child\..\Sibling\.\Icon.svg,3,C:\Root\Sibling\Icon.svg) [2 ms]
  Passed RemoveRelativeSegments_NormalizesTraversalAndSeparators (C:\Root\\Child//Icon.svg,3,C:\Root\Child\\Icon.svg) [< 1 ms]
  Passed RemoveRelativeSegments_NormalizesTraversalAndSeparators (C:\Root\Child\Icon.svg,3,C:\Root\Child\Icon.svg) [< 1 ms]
  Passed GetRootLength_DetectsDosUncAndDeviceRoots (C:\Root\Child,3) [1 ms]
  Passed GetRootLength_DetectsDosUncAndDeviceRoots (C:Root\Child,2) [< 1 ms]
  Passed GetRootLength_DetectsDosUncAndDeviceRoots (\\Server\Share\Folder,14) [< 1 ms]
  Passed GetRootLength_DetectsDosUncAndDeviceRoots (\\?\UNC\Server\Share\Folder,20) [< 1 ms]
  Passed GetRootLength_DetectsDosUncAndDeviceRoots (\\?\C:\Root,7) [< 1 ms]
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (2,missing.svg,System.IO.FileNotFoundException) [2 ms]
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (3,C:\Missing,System.IO.DirectoryNotFoundException) [< 1 ms]
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (5,C:\Denied,System.UnauthorizedAccessException) [< 1 ms]
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (995,,System.OperationCanceledException) [< 1 ms]
  Passed GetExceptionForWin32Error_ReturnsSpecificExceptionTypes (206,,System.IO.PathTooLongException) [< 1 ms]
  Passed ErrorCodeHelpers_ConvertBetweenWin32AndHResultForms [< 1 ms]
  Passed Constructors_WithDefaultListAndEnumerableInputs_LoadEntriesWithoutFileSystem [57 ms]
  Passed SetIdUpdateAction_WithEntries_PropagatesActionToEveryEntry [107 ms]
  Passed IsCorrupt_WithEmptyValidAndNullEntry_ReturnsExpectedIndices [7 ms]
  Passed Queries_WithDuplicateMissingAndCaseVariants_ReturnExpectedMatches [7 ms]
  Passed ProgramsByProjectNames_WithNullInput_ReturnsEmptyString [1 ms]
  Passed UpdateProjectID_WithDuplicateAndNewIds_ReturnsExpectedValues [< 1 ms]
  Passed FilterToProjectIDs_WithNullAndMixedRowKeys_ReturnsOnlyFourCharacterRows [117 ms]
  Passed DfToListEntries_WithProjectCategories_ParsesProjectAndProgramNames [83 ms]
  Passed ConstructorWithString_LoadsIdentifierDefaultsWithoutOutlookAccess [19 ms]
  Passed ConstructorWithFlaggableItem_LoadsFieldsAndCustomState [229 ms]
  Passed SettableProperties_UpdateCachedStateAndSaveBackToFlaggableItem [12 ms]
  Passed DateAndStatusBoundaries_UseDefaultsAndBooleanTransitions [6 ms]
  Passed VisibilityAndExpandedStateTransitions_SetBitsAndClearChangeMarker [7 ms]
  Passed CloneAndEquals_UseReferenceEqualityForDistinctInstances [6 ms]
  Passed ConstructorWithNullFlaggableItem_ThrowsBeforeOutlookAccess [4 ms]
  Passed ForceSave_WhenReadOnlyPersistsCachedState_RestoresReadOnlyState [18 ms]
  Passed WriteFlagsBatch_WithTranslatorValues_UpdatesUserDefinedFields [41 ms]
  Passed AutoCodeIdAsync_WithNullAndEmptyInputs_ReturnsWithoutIdListAccess [20 ms]
  Passed AutoCodeIdAsync_WithEmptyAndExistingIds_UsesBranchSpecificTransitions [27 ms]
  Passed ReadOnlyPropertyTransitions_UpdateCachedStateWithoutWritingFields [8 ms]
  Passed Dequeue_WithQueuedEntry_UnhooksItemsRaisesRemoveAndUpdatesCount [265 ms]
  Passed TryDequeueAsync_WithCompletedPendingEntry_UnhooksItemsAndRaisesRemove [3 ms]
  Passed TryDequeueAsync_WithRunningJobAndCancellation_ReturnsDefault [42 ms]
  Passed CompleteAddingAsync_WhenFunctionTimeoutExpires_ThrowsAndLeavesQueueOpen [48 ms]
  Passed Dequeue_WithHighConfidenceCarrier_PreservesPredeterminedFolder [6 ms]
  Passed AdjustTlp_WhenRowsIncrease_GrowsRowCountAndMinimumHeight [2 ms]
  Passed RenumberGroups_WithTenItems_UsesTwoDigitNumbersAndSequentialIndexes [1 ms]
  Passed GrowEntry_WhenTargetHasCapacity_MovesControlAndGroupThenResetsSourceState [52 ms]
  Passed AddOption_WhenNewDuplicateAndEmptyInputs_UpdatesSelectionState [224 ms]
  Passed ToggleMethods_WhenOptionExists_AddRemoveAndUpdateSelectionState [10 ms]
  Passed SearchAndParse_WhenInputIsEmptyMissingOrWildcard_ReturnsExpectedMatches [16 ms]
  Passed FilterArchive_WhenAutoAssignerHasExclusions_RemovesMatchesCaseInsensitively [102 ms]
  Passed ResolvePrefix_WhenMissingOrInvalid_UsesDefaultOrThrows [7 ms]
  Passed FilterToSelected_AfterStateTransitions_ReloadsOnlySelectedControls [2 ms]
  Passed LoadSelections_WhenExistingSelectionsUseBothForms_TogglesMatchingOptions [1 ms]
  Passed SearchAndReload_WhenFilterChanges_ReplacesVisibleCheckboxes [6 ms]
  Passed UpdateSelections_AfterFiltering_SynchronizesPrivateSelectionLists [1 ms]
  Passed SelectControlMethods_WhenPositionsChange_UpdateFocusIndexOrThrow [23 ms]
  Passed HideArchive_WhenToggled_ReloadsFilteredAndOriginalOptions [60 ms]
  Passed AutoAssignClick_WhenExistingAndNewAssignmentsReturned_UpdatesSelections [105 ms]
  Passed Constructor_WhenCreated_InitializesDefaultStateAndCancelToken [465 ms]
  Passed PropertyDecisions_WhenAssigned_RetainProvidedValues [7 ms]
  Passed FileBackedProperties_WhenPythonStagingMissing_ReturnNullWithoutFilesystemAccess [12 ms]
  Passed FileBackedProperties_WhenOnlyFlowFolderExists_StillSkipPythonStagingLoads [4 ms]
  Passed ScalarSettingsProperties_WhenRoundTripped_UpdateDefaultsAndReturnValues [31 ms]
  Passed CommonWordsSetter_WhenFlowFolderMissing_DoesNotSerializeOrAssignFolder [5 ms]
  Passed PrivateAsyncLoaders_WhenConfigurationMissing_FailDeterministically [56 ms]
  Passed BackupLoaders_WithMissingFiles_ReturnEmptyCollections [10 ms]

Test Run Successful.
Total tests: 76
     Passed: 76
 Total time: 4.8521 Seconds
```
