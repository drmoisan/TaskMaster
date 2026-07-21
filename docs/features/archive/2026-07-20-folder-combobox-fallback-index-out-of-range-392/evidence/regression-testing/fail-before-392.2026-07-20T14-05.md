Timestamp: 2026-07-20T14-05
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing|FullyQualifiedName~AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero"`
(preceded by `MSBuild.exe TaskMaster.sln /t:QuickFiler_Test /p:Configuration=Debug /p:Platform="Any CPU"`, EXIT_CODE 0, to compile the two new test methods before running them; `/InIsolation` added per prior-session repo guidance for Moq-based test assemblies)
EXIT_CODE: 1
Output Summary: 2 failed, 0 passed, as expected before the fix.
- `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing` failed with
  an unhandled `System.ArgumentOutOfRangeException: InvalidArgument=Value of '1' is not valid for
  'SelectedIndex'` thrown from `System.Windows.Forms.ComboBox.set_SelectedIndex`, raised inside
  `QfcItemController.PopulateAndSelectFolder` at `QfcItemController.FolderHandling.cs:228` — exactly
  the diagnosed defect (unguarded `predeterminedIndex >= 0 ? predeterminedIndex : 1` fallback with a
  single-item combo box).
- `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` failed on
  `mock.Verify(v => v.SetFolderSelectedIndex(0), Times.Once())`: the Moq exception log shows the
  actual invocation was `IItemViewer.SetFolderSelectedIndex(1)` — exactly the diagnosed defect at
  `QfcItemController.FolderHandling.cs:202` (unconditional `SetFolderSelectedIndex(1)` regardless of
  `FolderArray.Length`).

This satisfies AC-1's fail-before requirement: both regression tests reproduce the defect and fail,
for the reasons diagnosed in P1-T1, before any production-code change.
