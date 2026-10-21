Timestamp: 2026-08-31T10:28:53-04:00
Search 1: rg -n "MoveToFolder" --glob "*.cs" .
Search 2: rg -n "MoveToFolderAsync\\s*\\(" --glob "*.cs" .
EXIT_CODE: 0 for both searches
Output Summary: Search 1 returned 23 lines across 6 files. Search 2 returned 10 lines across 5 files. Search 1 minus Search 2 yields 13 non-member textual references.

Search 2 declarations (3):
- EfcDataModel.cs:303
- EfcDataModel.cs:398
- EfcHomeController.ExecuteMoves.cs:89

Search 2 call sites (7):
- EfcHomeController.ExecuteMoves.cs:78
- EfcHomeController.ExecuteMoves.cs:98
- EfcDataModel.cs:408
- EfcFormController.cs:537
- EfcFormController.cs:844
- EfcHomeControllerExecuteMovesTests.cs:87
- EfcDataModelArchiveRootTests.cs:314

Search 1 non-members (13): the delegate property, its assignments and invocations in EfcHomeController.ExecuteMoves.cs; test method names in EfcHomeControllerExecuteMovesTests.cs and EfcDataModelArchiveRootTests.cs; and the EfcHomeControllerTests.cs:55 comment. EfcHomeControllerTests.cs is the one stem-search-only file. EfcDataModelArchiveRootTests.cs appears in both searches and contributes call site :314. The prior 16-line research/spec figure predates issue #638.
