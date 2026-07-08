# Remediation Controller Focused Tests

Timestamp: 2026-07-04T14:45:53.0838572-04:00
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~EfcHomeControllerDependenciesTests|FullyQualifiedName~EfcHomeControllerMetricsTests|FullyQualifiedName~EfcHomeControllerLifecycleTests"
ResolvedRunner: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
EXIT_CODE: 0
Output Summary:
```text
Test Run Successful.
Total tests: 25
     Passed: 25
```

Full Output:
```text
VSTest version 18.7.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
  Passed Constructor_WithNoOverrides_InstallsProductionDefaults [29 ms]
  Passed Constructor_WithOverrides_PreservesInjectedDelegates [90 ms]
  Passed LoadSelection_WithExplicitMail_ReturnsOnlyExplicitMail [66 ms]
  Passed CreateDataModelWithFactory_ValidatesAndForwardsArguments [10 ms]
  Passed CreateKeyboardHandlerWithFactory_ValidatesViewerAndHomeController [1 ms]
  Passed CreateExplorerControllerWithFactory_ValidatesGlobalsAndHomeController [2 ms]
  Passed CreateInitializedFormControllerWithDataFactory_ValidatesRequiredArguments [3 ms]
  Passed CreateInitializedFormControllerWithoutDataFactory_ValidatesRequiredArguments [2 ms]
  Passed InitializeFormControllerDataFieldsWithFactory_ValidatesArguments [1 ms]
  Passed CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies [13 ms]
  Passed LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies [2 ms]
  Passed Run_WithoutMail_ShowsMessageThroughInjectedSeam [6 ms]
  Passed RunAsync_WithoutMail_ShowsMessageThroughInjectedSeam [1 ms]
  Passed Run_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed RunAsync_WithMail_ShowsViewerThroughInjectedSeam [< 1 ms]
  Passed Cleanup_ClearsControllerFieldsAndInvokesParentCleanup [1 ms]
  Passed ExplorerControllerAndKeyboardHandler_SettersStoreAssignedInstances [< 1 ms]
  Passed LoadedAndFilerQueue_PreserveNotImplementedContracts [1 ms]
  Passed OpenFolderMethods_DelegateToDataModelWithoutExternalServices [3 ms]
  Passed BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines [2 ms]
  Passed BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine [6 ms]
  Passed QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter [4 ms]
  Passed QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter [< 1 ms]
  Passed QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter [< 1 ms]
  Passed QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract [< 1 ms]

Test Run Successful.
Total tests: 25
     Passed: 25
 Total time: 0.5658 Seconds
```
