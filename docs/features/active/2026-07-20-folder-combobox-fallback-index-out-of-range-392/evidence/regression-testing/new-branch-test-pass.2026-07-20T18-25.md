Timestamp: 2026-07-20T18-25
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke"`
(preceded by `MSBuild.exe TaskMaster.sln /t:QuickFiler_Test /p:Configuration=Debug /p:Platform="Any CPU"`, EXIT_CODE 0, 1 Warning(s), 0 Error(s))
EXIT_CODE: 0
Output Summary: Total tests: 1. Passed: 1. Failed: 0. Total time: 1.2643 seconds. The new test
`PopulateFolderComboBox_WhenInvokeRequired_MarshalsAssignFolderComboBoxViaInvoke` passes, confirming
`_itemViewer.Invoke(It.IsAny<Delegate>())` is called exactly once when `InvokeRequired` is `true`,
exercising the previously-untested true-branch of `PopulateFolderComboBox`'s guard clause (line
139/140-142 in `QfcItemController.FolderHandling.cs`). No existing test's assertions, names, or
behavior were modified in this change (see `git diff` — only comment-line removals and this one new
test method were added).
