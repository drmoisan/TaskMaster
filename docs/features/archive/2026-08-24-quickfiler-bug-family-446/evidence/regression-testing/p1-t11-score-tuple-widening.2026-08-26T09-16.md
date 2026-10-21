# [P1-T11] Widen the Score Path to the Tuple Shape with `TopFolder` Stubbed

Timestamp: 2026-08-26T09-16

Task: [P1-T11]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:179` — the relocated
  `ScoreRemainingQueueMailItemAsync` now returns `Task<(long Score, string TopFolder)>` and returns
  `(score.Score, string.Empty)` at `:192`. `TopFolder` is deliberately stubbed to `string.Empty` per
  D-Plan-1; `[P2-T4]` discriminates the real value.
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` — `_scoreLoader` (`:61`) and both
  constructor parameters (`:73`, `:105`) widened to
  `Func<MailItem, CancellationToken, Task<(long Score, string TopFolder)>>`.
- `QuickFiler/Controllers/QfcDatamodel.cs:355` — the method-group conversion is repaired with the
  in-file adapter lambda `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score,`.
- `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` is not owned and is unchanged: it declares
  its `scoreLoader` locally, null-checks it and never invokes it, so the widening does not reach it.
- Score-loader lambdas updated to the tuple shape in
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`,
  `...Part2.cs` and `...Part3.cs`. No line was added to `...Part2.cs` (D-Plan-8).

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcDatamodelTests|FullyQualifiedName~QfcQueuePurePathsTests" "/Logger:trx;LogFileName=p1-t11.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t11"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t11/p1-t11.trx`

### Line-count conditions

| File | Recorded by `[P0-T14]` | Post-change | Condition | Result |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 496 | **480** | at most 500 | PASS |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 460 | **460** | equals `[P0-T14]` count | PASS |

### Scoped run — no newly failing test

TRX counters: `total="42" executed="42" passed="37" failed="5"`.

All five failures are tests this plan already tagged `[expect-fail]` in an earlier task:

| Failed test | Tagged by |
| --- | --- |
| `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` | `[P1-T2]` |
| `DequeueAsync_OnRejectedThrows_ScanContinues` | `[P1-T3]` |
| `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` | `[P1-T6]` |
| `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop` | `[P1-T9]` |
| `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` | `[P1-T10]` |

No test outside that set failed, so the widening introduced no newly failing test.

## Output Summary

Score path widened to `(long Score, string TopFolder)` end to end with `TopFolder` stubbed to
`string.Empty`. Intra-phase compile EXIT_CODE 0. `QfcDatamodel.cs` at 480 of the 500 cap;
`...Part2.cs` unchanged at 460, exactly the `[P0-T14]` figure. Scoped gate run: 42 executed,
37 passed, 5 failed, and all five failures are the previously `[expect-fail]`-tagged tests from
`[P1-T2]`, `[P1-T3]`, `[P1-T6]`, `[P1-T9]` and `[P1-T10]`. No newly failing test.
