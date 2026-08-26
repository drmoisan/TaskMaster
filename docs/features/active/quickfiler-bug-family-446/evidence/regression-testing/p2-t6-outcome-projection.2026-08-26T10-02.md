# [P2-T6] Real Outcome Projection at the Datamodel Boundary (Issue #446)

Timestamp: 2026-08-26T10-02

Task: [P2-T6]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`:

1. `DequeueNextItemGroupWithOutcomeAsync` no longer delegates to the `IList<MailItem>` four-argument
   overload and no longer hard-codes `QfcDequeueStop.QuantitySatisfied` (the `[P1-T15]` stub). It
   now branches on `HighConfidenceModeEnabled` itself:
   - high-confidence mode returns the new
     `DequeueWithHighConfidenceGateWithOutcomeAsync(...)` result verbatim;
   - normal mode calls `DequeueDirectAsync(quantity)` and reports
     `QfcDequeueStop.SourceExhausted` when `(items?.Count ?? 0) < quantity`, otherwise
     `QfcDequeueStop.QuantitySatisfied`, with an empty `PreScored`.
2. Added `private async Task<QfcDequeueBatch> DequeueWithHighConfidenceGateWithOutcomeAsync(...)`,
   which holds the gate construction previously inside `DequeueWithHighConfidenceGateAsync`, and
   returns `new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop)`. `Items` is
   therefore populated from the same accepted set as `PreScored`, after `UnhookDequeuedNodes` runs
   over it. `batch.Accepted` is captured into a local once, because the property materializes a
   fresh empty list on each read when the backing field is null.
3. `DequeueWithHighConfidenceGateAsync` now delegates to that method and returns `batch.Items`.

The three pre-existing `IQfcDatamodel` overloads (`DequeueNextItemGroupAsync(int, int)`,
`DequeueNextItemGroupAsync(int, int, TimeSpan, Action<int,int,int>)` and
`DequeueNextItemGroup(int)`) were not edited; they keep delegating internally and their observable
behaviour, including the pre-existing `null` return from `UnhookDequeuedNodes`, is unchanged.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop" "/Logger:trx;LogFileName=p2-t6.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t6"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t6/p2-t6.trx`

Counters: total 1, executed 1, **passed 1**, failed 0, error 0, timeout 0, aborted 0.

- `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` = **Passed**
  (was Failed at `[P1-T18]`).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test name and outcome unchanged. No `danmoisan` or `megalodon4` match
anywhere under the feature folder.

## Output Summary

The gate's stop reason now survives the datamodel boundary instead of being flattened. The
`[P1-T18]` `[expect-fail]` test transitions Failed -> Passed. Format EXIT_CODE 0, check EXIT_CODE 0,
compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 1 of 1 Passed and 0 Failed.
`QfcDatamodel.QueueProcessing.cs` is 288 lines, within the 500-line cap.
