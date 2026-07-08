# Remediation Focused Tests

TASK: P9-T13
COMMAND: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcHomeControllerDependenciesTests|FullyQualifiedName~EfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerLifecycleTests|FullyQualifiedName~ViewerQueueStaticWrapperTests|FullyQualifiedName~QfcThemeHelperTests"
EXIT_CODE: 0

OUTPUT:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed SetupFormThemes_ReturnsExpectedKeysAndControlGroups [60 ms]
  Passed SetupThemes_WithControlSet_ReturnsFourExpectedThemeKeys [103 ms]
  Passed SetupThemes_WithControlSet_MapsRepresentativeColorsAndHtmlStates [2 ms]
  Passed SetupThemes_WithNullController_ThrowsArgumentNullException [9 ms]
  Passed SetupThemes_WithNullViewer_ThrowsArgumentNullException [< 1 ms]
  Passed BuildProductionControlSet_MapsControllerAndViewerInputs [5 ms]
  Passed SetupFormThemes_ButtonGroups_ApplyLightAndDarkHoverBranches [2 ms]
  Passed QfcThemeControlSet_NullRequiredCollection_ThrowsArgumentNullException [< 1 ms]
  Passed SetTheme_Extensions_ApplyColorsToControls [< 1 ms]
  Passed EfcViewerQueue_BuildQueue_DelegatesToInjectedCore [13 ms]
  Passed EfcViewerQueue_Dequeue_UsesInjectedCoreAndRestoresReplacementCount [< 1 ms]
  Passed ItemViewerQueue_BuildMethods_DelegateToInjectedCore [< 1 ms]
  Passed ItemViewerQueue_DequeueAndChunk_DelegateToInjectedCore [1 ms]
  Passed EfcViewerQueue_CreateProductionCore_UsesProvidedDelegates [< 1 ms]
  Passed ItemViewerQueue_CreateProductionCore_UsesProvidedDelegates [< 1 ms]
  Passed Constructor_WithNoOverrides_InstallsProductionDefaults [1 ms]
  Passed Constructor_WithOverrides_PreservesInjectedDelegates [10 ms]
  Passed LoadSelection_WithExplicitMail_ReturnsOnlyExplicitMail [51 ms]
  Passed CreateDataModelWithFactory_ValidatesAndForwardsArguments [2 ms]
  Passed CreateKeyboardHandlerWithFactory_ValidatesViewerAndHomeController [1 ms]
  Passed CreateExplorerControllerWithFactory_ValidatesGlobalsAndHomeController [1 ms]
  Passed CreateInitializedFormControllerWithDataFactory_ValidatesRequiredArguments [2 ms]
  Passed CreateInitializedFormControllerWithoutDataFactory_ValidatesRequiredArguments [1 ms]
  Passed InitializeFormControllerDataFieldsWithFactory_ValidatesArguments [< 1 ms]
  Passed CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies [21 ms]
  Passed LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies [1 ms]
  Passed Run_WithoutMail_ShowsMessageThroughInjectedSeam [3 ms]
  Passed RunAsync_WithoutMail_ShowsMessageThroughInjectedSeam [< 1 ms]
  Passed Run_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed RunAsync_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed Cleanup_ClearsControllerFieldsAndInvokesParentCleanup [< 1 ms]
  Passed ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances [< 1 ms]
  Passed LoadedAndFilerQueue_PreserveNotImplementedContracts [< 1 ms]
  Passed OpenFolderMethods_DelegateToDataModelWithoutExternalServices [2 ms]
  Passed BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines [1 ms]
  Passed BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine [4 ms]
  Passed QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter [2 ms]
  Passed QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter [< 1 ms]
  Passed QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter [< 1 ms]
  Passed QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract [< 1 ms]

Test Run Successful.
Total tests: 40
     Passed: 40
 Total time: 0.6076 Seconds
```
