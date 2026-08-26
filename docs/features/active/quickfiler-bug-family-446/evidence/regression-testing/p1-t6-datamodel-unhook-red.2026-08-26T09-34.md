# [P1-T6] [expect-fail] Datamodel Releases the Rejected Candidate's Monitor Hook

Timestamp: 2026-08-26T09-34

Task: [P1-T6] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` — added
`DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor`.

It follows the `Mock<IEmailMoveMonitor>` plus reflection-injection pattern already established in
this file by `DequeueNextItemGroupAsync_HighConfidenceDisabled_PreservesDirectBatchDequeue`,
reusing that file's own `CreateUninitializedDatamodel` and `SetPrivateField` helpers (the same
helpers `QfcDatamodelTests.cs` declares). High-confidence mode is enabled, the threshold is `0.90`
and the single candidate scores `100`, so it is discarded.

Scoring is supplied by a `Mock<IFolderScoringService>` injected through the `ScoringServiceFactory`
seam added by `[P1-T5]`, so the test touches no live Outlook COM, satisfying
`.claude/rules/general-unit-test.md` UT4.

The test asserts the datamodel's own `_moveMonitor.UnhookItem` is invoked exactly once for the
below-cutoff candidate, and re-asserts that the candidate is still absent from the result and
still removed from the master queue.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor" "/Logger:trx;LogFileName=p1-t6.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t6"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t6/p1-t6.trx`

Recorded outcome: **Failed** (`outcome="Failed"`).

Failure message, quoted verbatim from the TRX `ErrorInfo/Message` element:

```
Test method QuickFiler.Controllers.Tests.QfcQueuePurePathsTests.DequeueNextItemGroupAsync_HighConfidenceRejectedItem_UnhooksFromMoveMonitor threw exception:
Moq.MockException: the datamodel must release the rejected candidate's monitor hook exactly once
Expected invocation on the mock once, but was 0 times: x => x.UnhookItem(Mock<MailItem:1>.Object)

Performed invocations:

   Mock<IEmailMoveMonitor:1> (x):
   No invocations performed.
```

This is a `Moq.MockException` raised by the test's own `Verify` call — Moq's assertion-failure
mechanism, carrying the `because` reason supplied by the test. It is not a build error and it is
not an unhandled exception escaping the system under test: the dequeue call itself completed
normally and the two preceding FluentAssertions checks (`result` empty, `masterQueue.Count == 0`)
both passed before the verification ran. MSTest renders every non-`AssertFailedException`
verification failure with the "threw exception" preamble; the substantive content is the
"Expected invocation on the mock once, but was 0 times" assertion.

## Output Summary

Test lands RED on the unhook-invocation assertion against a compiling tree. Compile exit 0,
scoped run exit 1 with the single test Failed. `[P2-T5]` wires `onRejected: TryReleaseRejectedHook`
and turns it green.
