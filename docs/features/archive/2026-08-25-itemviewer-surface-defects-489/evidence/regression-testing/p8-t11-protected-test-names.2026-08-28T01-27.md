# P8-T11 — The two protected test method names survived the rename

Timestamp: 2026-08-28T01-27
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection|FullyQualifiedName~MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems" "/Logger:trx;LogFileName=p8-t11.trx" /ResultsDirectory:<temp-results-dir>
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

Both grep clauses return a match:

```
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:111:        public void AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection()
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:169:        public void MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems()
```

Both still carry `SetFolderItems` inside the method name, which is the point: P8-T7 renamed member
**invocations** only. Renaming either method would have changed its node ID and invalidated a
sibling acceptance condition.

The run: `Test Run Successful.` — `Total tests: 2`, `Passed: 2`, 0 failed, 0 skipped, in 1.40
seconds.

```
Passed AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection [249 ms]
Passed MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems [17 ms]
```

TRX: `evidence/regression-testing/p8-t11.trx` (sanitised; 2 `UnitTestResult` elements, matching the
total above; parses under a strict XML reader after redaction).

## Baseline comparison

`ExpectedExitCode: 0` is declared because **neither** named test was recorded `failed` in P0-T13's
`BaselineNamedPins:` block. That block records all nine pins as `passed`, including these two, so no
test needed attributing to a sibling child and the absolute count of 2 passed is asserted directly.
The no-regression comparison is therefore trivially satisfied: `passed` at baseline, `passed` here,
for both.

That both still pass is also the behavioural half of the rename's safety argument. Each verifies a
`Times.Once()` folder-population dispatch through the renamed member; had the rename changed the
call's arity, arguments or dispatch count rather than only its name, the `Verify` in each would have
failed even though the assembly compiled.

Output Summary: Both protected test method names survive verbatim at
`QfcItemController.FolderSuggestionsTests.cs:111` and `:169`, and both pass — `EXIT_CODE: 0`,
`Total tests: 2`, `Passed: 2`, 0 failed, 0 skipped. Both were recorded `passed` in P0-T13's
`BaselineNamedPins:` block, so `ExpectedExitCode: 0` applies and the absolute pass count is asserted
rather than a no-regression comparison.
