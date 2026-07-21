# Phase 5 — KbdActions Regression Net

Timestamp: 2026-07-10T23-48
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~KbdActions"`
EXIT_CODE: 0
Output Summary: 1 test file matched. Total tests: 13. Passed: 13. Failed: 0. All tests from
both `QuickFiler.Test/Controllers/KbdActionsTests.cs` (3 `[TestMethod]`s) and
`QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` (10 `[TestMethod]`s) pass
unchanged after the `List<UClass>` swap, including the `FindIndex`/`Add`/`RemoveAt` branch
tests (`FindIndex_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException`,
`Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate`,
`Remove_PresentKey_RemovesAndReturnsTrue`, `Remove_AbsentKey_ReturnsFalse`, etc.).

`git diff --name-only | grep -i "KbdActionsTests\|KbdActionsRemainingBranchesTests"` returned no
matches (`EXIT_CODE: 1`), confirming both test files remain unmodified.
