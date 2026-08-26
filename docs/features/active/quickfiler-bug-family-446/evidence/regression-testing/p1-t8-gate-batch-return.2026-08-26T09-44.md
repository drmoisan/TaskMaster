# [P1-T8] Gate Returns `QfcGateBatch` With the Stop Reason Stubbed

Timestamp: 2026-08-26T09-44

Task: [P1-T8]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`:

- `DequeueAsync` now returns `Task<QfcGateBatch>`.
- `Accepted` carries each accepted `MailItem` wrapped as `new QfcPreScoredItem(mailItem, string.Empty)`
  — the deliberate D-Plan-1 stub that `[P2-T3]` replaces with the real folder.
- `Scanned` carries the existing `scanned` counter (its declaration moved above the degenerate
  `quantity <= 0` exit so every exit can report it).
- `Stop` is hard-coded to `QfcDequeueStop.QuantitySatisfied` at all four exits per D-Plan-1.
  `QuantitySatisfied` is chosen deliberately over `SourceExhausted`: with `SourceExhausted`
  stubbed, `[P1-T10]`'s `DequeueAsync_SourceDrained_ReportsSourceExhaustedStop` would pass
  vacuously in the RED state and would gate nothing.

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` — the single call site now reads the batch
and projects `Accepted` back to `IList<MailItem>` before `UnhookDequeuedNodes`, so the datamodel's
observable behaviour is unchanged at this point.

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` — the reflective
`DequeueAsync` helper now casts to `Task<QfcGateBatch>` (in a new `DequeueBatchAsync` helper that
performs the single `GetMethod` lookup) and the existing `DequeueAsync` helper projects
`Accepted` back to `IList<MailItem>`, so the pre-existing gate tests keep their current shape.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcQueuePurePathsTests" "/Logger:trx;LogFileName=p1-t8.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t8"`
EXIT_CODE: 1
ExpectedExitCode: 1

- Total: `32`
- Passed: `29`
- Failed: `3`

The three failures are exactly the tests already tagged `[expect-fail]`:

| Failed test | Tagged by |
| --- | --- |
| `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` | `[P1-T2]` |
| `DequeueAsync_OnRejectedThrows_ScanContinues` | `[P1-T3]` |
| `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` | `[P1-T6]` |

There is **no** failure other than those three, which is the acceptance condition. In particular
all 23 pre-existing gate tests and the pre-existing
`DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue` still pass through
the projected helper.

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t8/p1-t8.trx`

## Output Summary

Gate return type widened to `QfcGateBatch` with `Stop` stubbed to `QuantitySatisfied` and
`PredeterminedFolder` stubbed to `string.Empty`. Compile exit 0; scoped run 29 passed, 3 failed,
all three pre-tagged `[expect-fail]`.
