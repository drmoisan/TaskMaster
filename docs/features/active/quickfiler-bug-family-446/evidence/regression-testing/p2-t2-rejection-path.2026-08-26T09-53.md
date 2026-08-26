# [P2-T2] Rejection Path on the Below-Cutoff Branch (Issue #426)

Timestamp: 2026-08-26T09-53

Task: [P2-T2]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` — the `score >= _cutoff` accept
decision inside `DequeueAsync` gained an `else` branch that invokes the `[P1-T2]` `_onRejected`
seam:

```
else
{
    try { _onRejected?.Invoke(mailItem); }
    catch (System.Exception e) { logger.Error("Rejection sink threw ...", e); }
}
```

The catch is deliberate and narrow in effect: a failing move monitor must not abort the scan,
because aborting would strand the remaining candidates of the batch. The candidate is still
discarded on both paths, so the drop-on-reject contract is unchanged.

`TryUnhookOrReplace` was deliberately NOT reused: its recovery path pulls a replacement item out of
`_masterQueue`, which is meaningless for a candidate the gate is discarding.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce|FullyQualifiedName~DequeueAsync_OnRejectedThrows_ScanContinues|FullyQualifiedName~DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected|FullyQualifiedName~DequeueAsync_BelowThresholdItemsAreDiscarded" "/Logger:trx;LogFileName=p2-t2.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t2"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t2/p2-t2.trx`

Counters: total 4, executed 4, **passed 4**, failed 0, error 0, timeout 0, aborted 0.

- `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` = **Passed** (was Failed at `[P1-T2]`).
- `DequeueAsync_OnRejectedThrows_ScanContinues` = **Passed** (was Failed at `[P1-T3]`).
- `DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected` = **Passed** (negative control, `[P1-T4]`).
- `DequeueAsync_BelowThresholdItemsAreDiscarded` = **Passed** (pre-existing drop-on-reject contract).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test names and outcomes unchanged. No `danmoisan` or `megalodon4` match
anywhere under the feature folder.

## Output Summary

Both `[expect-fail]` #426 tests transition Failed -> Passed while the accepted-path negative control
and the pre-existing discard contract stay Passed. Format EXIT_CODE 0, check EXIT_CODE 0, compile
EXIT_CODE 0, scoped run EXIT_CODE 0 with 4 of 4 Passed and 0 Failed.
