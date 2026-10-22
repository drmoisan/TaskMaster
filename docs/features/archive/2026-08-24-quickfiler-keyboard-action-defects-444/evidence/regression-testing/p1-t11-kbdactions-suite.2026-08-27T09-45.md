# [P1-T11] Both `KbdActions` test classes

Timestamp: 2026-08-27T09-45
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QuickFiler.Controllers.Tests.KbdActionsTests.|FullyQualifiedName~QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests."`
EXIT_CODE: 0

The two filter clauses are joined with `|`; `vstest` 18.x rejects `OR`.

## Summary (verbatim)

```
Test Run Successful.
Total tests: 18
     Passed: 18
```

| Measure | Value |
| --- | --- |
| Total | 18 |
| Passed | 18 |
| Failed | **0** |

## Baseline arithmetic

```
BaselineKbdActionsTestMethodCount (from [P0-T21]) = 13
Tests added by this phase                        =  5
  [P1-T1] EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException
  [P1-T7] EnumerableConstructor_WhenListIsNull_ThrowsArgumentNullException
  [P1-T8] EnumerableConstructor_WhenSeedIsDuplicateFree_DoesNotThrow
  [P1-T8] EnumerableConstructor_WhenSameKeyUnderDifferentSourceIds_DoesNotThrow
  [P1-T9] EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow
Expected passed count                            = 13 + 5 = 18
Observed passed count                            = 18
```

## Every executed test, fully qualified

`QuickFiler.Controllers.Tests.KbdActionsTests` (4):

```
QuickFiler.Controllers.Tests.KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate   Passed [45 ms]
QuickFiler.Controllers.Tests.KbdActionsTests.Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException       Passed [61 ms]
QuickFiler.Controllers.Tests.KbdActionsTests.EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow  Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsTests.FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics  Passed [48 ms]
```

`QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests` (14):

```
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EmptyRegistry_HasNoKeysAndFindReturnsDefault             Passed [43 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.AddInstance_ThenFind_ReturnsTheRegisteredInstance        Passed [1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.AddInstance_ExactDuplicate_ThrowsArgumentException       Passed [60 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.Find_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException      Passed [1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.FindIndex_WhenMultipleSourcesShareKey_ThrowsInvalidOperationException Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.Remove_PresentKey_RemovesAndReturnsTrue                  Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.Remove_AbsentKey_ReturnsFalse                            Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt     Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.Enumeration_YieldsAllRegisteredInstancesAndKeysProjection   Passed [37 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.FilterKeys_ReturnsOnlyMatchingInstances                  Passed [1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException  Passed [1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenListIsNull_ThrowsArgumentNullException     Passed [1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSeedIsDuplicateFree_DoesNotThrow           Passed [< 1 ms]
QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSameKeyUnderDifferentSourceIds_DoesNotThrow  Passed [< 1 ms]
```

## Acceptance evaluation

- `Failed: 0`. PASS.
- Passed count (18) equals `BaselineKbdActionsTestMethodCount` (13) plus 5. PASS.
- The listed names include
  `Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` as **passed**. PASS.

That last test is the pre-existing characterization test asserting that `"10"` and `"1"` legally
coexist under one `SourceId`. Its continued pass is the direct evidence that the new constructor guard
uses `StoredKeyEquals` and not the substring-matching `KeyEquals`.

Output Summary: 18 of 18 passed, 0 failed; count equals the Phase 0 baseline of 13 plus the 5 tests
this phase adds; the pre-existing substring characterization test passes unmodified.
