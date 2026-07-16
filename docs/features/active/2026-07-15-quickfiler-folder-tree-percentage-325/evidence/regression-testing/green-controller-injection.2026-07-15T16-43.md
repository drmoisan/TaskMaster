# Green — Controller Injection (P5-T6)

Timestamp: 2026-07-16T11-05
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:FullyQualifiedName~QfcItemController_FolderSuggestionsTests|FullyQualifiedName~QfcItemController_FolderHandlingTests
EXIT_CODE: 0

Output Summary: `Test Run Successful.` 18 tests pass — the 4 new QfcItemController_FolderSuggestionsTests
plus the 14 existing QfcItemController_FolderHandlingTests (no regression). Total tests: 18 | Passed: 18 | Failed: 0.

New tests (QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs):
- AssignFolderComboBox_HandsPredictorRowArrayToSetFolderSuggestions — verifies the FolderRow[] from
  the predictor's FolderRowArray reaches IItemViewer.SetFolderSuggestions with contract-correct
  classification (Suggestion row carries a non-null FolderScore with Probability 0.9; separators and
  recents carry null Score).
- AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection — the retained SetFolderItems(string[])
  population and index-1 top-suggestion selection remain.
- AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates — predetermined
  preselection retained alongside the new SetFolderSuggestions injection.
- MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems — the retained "Trash to Delete"
  append via SetFolderItems is unchanged.

The FolderRow[] source is a COM-free in-memory IFolderSearchHandler fake; the controller consumes it
via _folderHandler.FolderRowArray (the #324 contract member, now exposed on the narrow consuming seam
IFolderSearchHandler and already implemented by FolderPredictor).
