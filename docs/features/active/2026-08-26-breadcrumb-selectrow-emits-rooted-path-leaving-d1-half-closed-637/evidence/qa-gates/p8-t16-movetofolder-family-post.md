Timestamp: 2026-08-31T14:03:00-04:00
Command 1: `rg -n "MoveToFolder" --glob "*.cs" .`
Command 2: `rg -n "MoveToFolderAsync\s*\(" --glob "*.cs" .`
EXIT_CODE: 0 for both commands.
Output Summary: The family-stem search returned 23 lines across 6 files. The syntax-anchored search returned 10 lines across 5 files: 3 declarations and 7 call sites. No new overload or signature change appears.

Syntax-anchored declarations:

- `QuickFiler/Controllers/EfcDataModel.cs:303`
- `QuickFiler/Controllers/EfcDataModel.cs:398`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:89`

Syntax-anchored call sites:

- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:78`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:98`
- `QuickFiler/Controllers/EfcDataModel.cs:408`
- `QuickFiler/Controllers/EfcFormController.cs:537`
- `QuickFiler/Controllers/EfcFormController.cs:844`
- `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:87`
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:314`

AC16 already carries these measured figures. P8-T32 list entry A9 re-verifies them. The 16-line figure retained at `spec.md:313` describes the pre-#638 tree and is recorded in P8-T32 list entry B6.
