# Fail-Before: EfcDataModel Prompt-State Reset When the Filer Throws (P5-T7)

Timestamp: 2026-10-03T12-24
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelFilerCleanupTests" "/ResultsDirectory:coverage\test-results\959\p5-t7" "/Logger:trx;LogFileName=p5-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: After the seam extraction (P5-T6) and before the `try`/`finally` (P5-T8), the run is red with exactly the expected failing row (MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates) and the two control rows passing. The message reports zero resets, so the sticky prompt answers were not released before the filer's exception propagated (the defect).

```
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=3 executed=3 passed=2 failed=1
RESULT_COUNT: 3
RESULT MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce = Passed
RESULT MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates = Failed
RESULT MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState = Passed
MESSAGE MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates :: Expected probe.ResetCalls to be 1, but found 0 (difference of -1).
```

## Acceptance (P5-T7, all five required)

1. `EXIT_CODE:` non-zero and equal to `ExpectedExitCode:` (1 = 1): met.
2. `COUNTERS total=3 executed=3 passed=2 failed=1`: met.
3. The `Failed` row is exactly `MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates`, and the two `Passed` rows are the other `NAMES-TEF` names: met.
4. The `MESSAGE` line contains `but found 0`: met.
5. Every `SANDBOX-` value is `False`: met.

Result: FAIL-BEFORE OBSERVED for the expected reason.

## Pass-after (P5-T9)

Runs after Edit E-E-FINALLY (P5-T8): vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelFilerCleanupTests" "/ResultsDirectory:coverage\test-results\959\p5-t9" "/Logger:trx;LogFileName=p5-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None", then the same command with "/TestCaseFilter:FullyQualifiedName~Controllers.EfcDataModelArchiveRootTests", "/ResultsDirectory:coverage\test-results\959\p5-t9-archive" and "/Logger:trx;LogFileName=p5-t9-archive.trx" (both resolved through vswhere); then git status --porcelain -- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs and git diff --name-only MERGE-BASE -- QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs (MERGE-BASE 94287369908cc920b21b0e3256314f988ad7d2f5).

### Filer-cleanup run (p5-t9)

```
PASS-AFTER-VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=3 executed=3 passed=3 failed=0
RESULT MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates = Passed
RESULT MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce = Passed
RESULT MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState = Passed
```

### Archive-root pins (p5-t9-archive)

```
ARCHIVE-VSTEST_EXIT_CODE: 0
SANDBOX-959-EXISTS-BEFORE: False
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
SANDBOX-959-EXISTS-AFTER: False
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
COUNTERS total=11 executed=11 passed=11 failed=0
RESULT MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
RESULT OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress = Passed
RESULT OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing = Passed
RESULT MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot = Passed
RESULT MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce = Passed
RESULT MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates = Passed
RESULT OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns = Passed
ART-PORCELAIN: EMPTY
ART-DIFF: EMPTY
```

Acceptance (P5-T9, all four required): `PASS-AFTER-VSTEST_EXIT_CODE: 0` with `COUNTERS total=3 executed=3 passed=3 failed=0` and the rows exactly `NAMES-TEF`, each `= Passed`: met; `ARCHIVE-VSTEST_EXIT_CODE: 0` with `COUNTERS total=11 executed=11 passed=11 failed=0` and the rows exactly `NAMES-EFC-ARCHIVE`, each `= Passed`: met; every `SANDBOX-` value is `False`: met; `ART-PORCELAIN: EMPTY` and `ART-DIFF: EMPTY`: met.
