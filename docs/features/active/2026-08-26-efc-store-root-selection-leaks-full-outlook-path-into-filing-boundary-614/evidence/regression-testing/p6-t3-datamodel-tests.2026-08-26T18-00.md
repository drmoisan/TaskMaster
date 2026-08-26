# P6-T3 — EfcDataModel stem-derivation tests (#614, D8; AC15 test half)

Timestamp: 2026-08-26T18-00

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelIssue614Tests|FullyQualifiedName~EfcDataModelTests" "/Logger:trx;LogFileName=p6-t3.trx" "/ResultsDirectory:coverage\trx\p6-t3"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 14; Passed: 14; Failed: 0; Skipped: 0.
- Per-class counts from the TRX: `QuickFiler.Test.Controllers.EfcDataModelIssue614Tests` 8 passed;
  the pre-existing `QuickFiler.Controllers.Tests.EfcDataModelTests` 6 passed, unedited.
- New `EfcDataModel.ToArchiveRelativeStem` cases, all PASSED: under-root folder returns the
  relative stem; the same through a `Mock<MAPIFolder>` `FolderPath` seam; store-root folder throws
  without leaking the mailbox address; the archive root itself throws (an empty stem would file to
  the root); cross-store folder through a `Mock<Folder>` seam throws; case-differing ancestor still
  matches; `Archive2` separator-boundary near-miss throws; repeated ancestor substring is stripped
  only at the prefix.
- Both live callers of the rewired `MoveToFolderAsync(MAPIFolder, olAncestor, ...)` overload keep
  compiling unchanged in signature (`EfcFormController.cs` create-folder paths); the solution build
  exits 0.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p6-t3\` tree.
