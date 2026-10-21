# Queue-level regression suite (P5-T10)

Timestamp: 2026-09-01T10-58
Task: [P5-T10]
Working directory: WORKTREE

## Command 1 — build

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /fl "/flp:logfile=FEATURE/evidence/other/p5-t10-build.msbuild.txt;verbosity=normal"
```

EXIT_CODE: 0

File log: `FEATURE/evidence/other/p5-t10-build.msbuild.txt` (11952 lines).
Summary lines: `Build succeeded.`, `5 Warning(s)`, `0 Error(s)`.
Count of `Skipping target "CoreCompile"` occurrences: 0.
Count of CS/CA/IDE/SA/MA/RCS/S-prefixed diagnostic lines: 0.

## Command 2 — scoped queue suite

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FilerQueueTests" "/Logger:trx;LogFileName=p5-t10.trx" /ResultsDirectory:FEATURE\evidence\regression-testing\p5-t10
```

EXIT_CODE: 0

Count of `outcome="Passed"` occurrences in the produced TRX: **12**.
Count of `outcome="Failed"` occurrences: **0**.

## Results

| # | Outcome | Test | Origin |
|---|---|---|---|
| 1 | Passed | `FilerQueueItem_Constructor_StoresFilerAndHelpers` | pre-existing |
| 2 | Passed | `FilerQueueItem_Constructor_NullFiler_ThrowsArgumentNullException` | pre-existing |
| 3 | Passed | `FilerQueueItem_Constructor_NullHelpers_ThrowsArgumentNullException` | pre-existing |
| 4 | Passed | `FilerQueueItem_Constructor_HelpersContainingNull_ThrowsArgumentNullException` | pre-existing |
| 5 | Passed | `FilerQueue_NewInstance_HasCompletedConsumerByDefault` | pre-existing |
| 6 | Passed | `WhenDrainedAsync_OnFreshQueue_ReturnsCompletedTask` | P5-T2 |
| 7 | Passed | `WhenDrainedAsync_WithGatedItem_DoesNotCompleteBeforeItemCompletes` | P5-T3 |
| 8 | Passed | `WhenDrainedAsync_AfterGateReleased_CompletesAndItemRanOnce` | P5-T4 |
| 9 | Passed | `WhenDrainedAsync_WithTwoGatedItems_CompletesOnlyAfterBothComplete` | P5-T5 |
| 10 | Passed | `WhenDrainedAsync_AwaitedTwice_BothWaitersComplete` | P5-T6 |
| 11 | Passed | `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` | P5-T7 |
| 12 | Passed | `ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` | P5-T8 |

No `ErrorInfo` message is present in the TRX.

Output Summary: Both commands exited 0. The passed count of 12 is exactly the arithmetic the acceptance
condition specifies: the five pre-existing tests recorded in the P1-T5 artifact plus the seven added by
P5-T2 through P5-T8. The failed count is 0.

Every one of the five pre-existing tests still passes unmodified after the handshake repair removed the
`ThreadSafeSingleShotGuard` field and rewrote both `Enqueue` overloads and `ConsumeAsync`. That includes
`FilerQueue_NewInstance_HasCompletedConsumerByDefault`, which pins the retained `Consumer` default that
AC11 protects, and the three constructor-validation tests, which pin the synchronous
`ArgumentNullException` behaviour that the P3-T4 overload-delegation shape had to preserve.

This artifact supplies the passed-test evidence that the AC1 through AC6 check-offs in P8-T5 through
P8-T10 depend on.
