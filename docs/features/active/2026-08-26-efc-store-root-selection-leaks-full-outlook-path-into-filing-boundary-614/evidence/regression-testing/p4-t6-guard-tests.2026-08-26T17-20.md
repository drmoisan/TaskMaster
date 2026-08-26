# P4-T6 — EfcSelectionGuard tests (#614, D9; AC16 test half)

Timestamp: 2026-08-26T17-20

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p4-t6.trx" "/ResultsDirectory:coverage\trx\p4-t6"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 9; Passed: 9; Failed: 0; Skipped: 0.
- Discovery of `QuickFiler.Test.Controllers.EfcSelectionGuardTests` proves the explicit
  `<Compile Include>` wiring of both new files (`QuickFiler\Controllers\EfcSelectionGuard.cs` in
  `QuickFiler.csproj` and `QuickFiler.Test\Controllers\EfcSelectionGuardTests.cs` in
  `QuickFiler.Test.csproj`); these are non-SDK projects with no glob includes.
- Predicate rejects: `null`, `string.Empty`, whitespace, a `"===="`-prefixed banner sentinel, a
  store-rooted selection, a single-separator-leading selection, a drive-rooted selection, and a
  two-character selection. It accepts the valid relative stem `Clients\North`.
- `EfcFormController.ActionOkAsync` and `EfcFormController.IsValidSelection` both delegate to this
  one predicate (2 occurrences of `IsValidFilingSelection` in `EfcFormController.cs`), so a value
  can no longer be accepted by one path and rejected by the other (AC16).
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p4-t6\` tree.
