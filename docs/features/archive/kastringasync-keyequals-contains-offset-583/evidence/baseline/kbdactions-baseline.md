# KbdActionsTests Baseline (P0-T9)

- Timestamp: 2026-09-13T01-05
- Command: <resolved vstest executable> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
  /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings
  "/TestCaseFilter:FullyQualifiedName~KbdActionsTests"
- EXIT_CODE: 0

## Verbatim result

```
Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: 1.2679 Seconds
```

## Per-class reporting

| Class | Total | Passed | Failed | Exit |
|---|---|---|---|---|
| KbdActionsTests | 4 | 4 | 0 | 0 |

Individual tests: Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate,
Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException,
EnumerableConstructor_WhenStoredKeysDifferButKeyEqualsOverlaps_DoesNotThrow,
FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics (the pinned test).

## Output Summary

Exit code 0; verdict "Test Run Successful."; 4 Passed, 0 Failed for the KbdActionsTests class,
including the pinned test FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics.
This baseline count (4) is expected to differ from the archived precedent's count of 3 on
2026-08-22 (docs/features/archive/2026-08-07-quickfiler-keyboard-action-contract-defects-445/evidence/baseline/vstest-baseline.2026-08-22T09-18.md),
because one test method (Add_WhenSourceAndStoredKeyAreExactDuplicate_ThrowsArgumentException or
a sibling) has been added to the pinned file since that baseline was recorded; per the plan's
own acceptance text this task's count is not compared against that archived figure. No file was
diffed to obtain this result; it is derived solely from the test-run outcome.
