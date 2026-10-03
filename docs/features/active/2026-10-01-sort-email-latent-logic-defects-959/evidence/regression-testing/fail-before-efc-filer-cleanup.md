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
