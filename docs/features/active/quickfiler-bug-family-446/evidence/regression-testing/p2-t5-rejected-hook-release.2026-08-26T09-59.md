# [P2-T5] Release the Rejected Candidate's Monitor Hook (Issue #426)

Timestamp: 2026-08-26T09-59

Task: [P2-T5]
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`:

1. Added `private void TryReleaseRejectedHook(MailItem item)`, which calls the datamodel's own
   `_moveMonitor.UnhookItem(item)` inside a `try`/`catch (System.Exception e)` that logs at error
   level and returns.
2. Wired `onRejected: TryReleaseRejectedHook` into the `QfcStreamingDequeueConfidenceGate`
   construction inside `DequeueWithHighConfidenceGateAsync`.

Exactly one `UnhookItem` call is made per rejected item, preserving the
one-marshal-hop-per-operation contract; no unhooks are batched into a single hop.

`QuickFiler/Helper Classes/EmailMoveMonitor.cs` gained no member and is absent from the change set
(`git diff --stat` against it produces no output).

## Verification

Command: `dotnet tool run csharpier format "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `dotnet tool run csharpier check "QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor|FullyQualifiedName~DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue" "/Logger:trx;LogFileName=p2-t5.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p2-t5"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t5/p2-t5.trx`

Counters: total 2, executed 2, **passed 2**, failed 0, error 0, timeout 0, aborted 0.

- `DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor` = **Passed**
  (was Failed at `[P1-T6]`). Its `Times.Once` verification is what pins the marshal-hop contract.
- `DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue` = **still Passed**
  (normal-mode path unaffected).

TRX hygiene: scrubbed of the absolute worktree path, account name and machine name, then re-parsed
as XML; `<Counters .../>`, test names and outcomes unchanged. No `danmoisan` or `megalodon4` match
anywhere under the feature folder.

## Output Summary

The #426 leak is closed end to end: the gate reports each discarded candidate through `_onRejected`
(`[P2-T2]`) and the datamodel releases the hook through its own monitor instance exactly once. The
`[P1-T6]` `[expect-fail]` test transitions Failed -> Passed and the normal-mode control stays
Passed. Format EXIT_CODE 0, check EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 0 with 2
of 2 Passed and 0 Failed.
