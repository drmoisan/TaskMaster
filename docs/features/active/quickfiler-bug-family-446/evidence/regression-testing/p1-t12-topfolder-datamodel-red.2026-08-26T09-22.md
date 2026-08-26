# [P1-T12] [expect-fail] Datamodel Scorer Must Return Score AND Top Folder

Timestamp: 2026-08-26T09-22

Task: [P1-T12] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcDatamodelTests.cs` — added
`ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder` in a new
`#region Issue #446 — Top-folder propagation from the master-queue admission scorer`.

The test builds the datamodel with `CreateUninitializedDatamodel`, sets `_globals` through
`SetPrivateField`, and assigns the `ScoringServiceFactory` seam added by `[P1-T5]` to a
`Mock<IFolderScoringService>` (`MockBehavior.Strict`) whose `ScoreAsync` returns the known pair
`(875L, @"Inbox\Projects\Alpha")`. The private `ScoreRemainingQueueMailItemAsync` is invoked
through a small reflection helper, `InvokeScoreRemainingQueueMailItemAsync`, using the file's
existing `NonPublicInstance` binding flags. No live Outlook COM, no filesystem and no wall-clock
wait is involved, per `.claude/rules/general-unit-test.md` UT4.

The test asserts both halves of the returned tuple: `Score` (already propagated) and `TopFolder`
(discarded today).

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~ScoreRemainingQueueMailItemAsync_ReturnsScoreAndTopFolder" "/Logger:trx;LogFileName=p1-t12.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t12"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t12/p1-t12.trx`

TRX counters: `total="1" executed="1" passed="0" failed="1"`.

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX:

```
Expected result.TopFolder to be "Inbox\Projects\Alpha" with a length of 20 because the top-ranked folder the scorer already computed must reach the caller instead of being discarded and re-derived downstream, but "" has a length of 0, differs near "" (index 0).
```

This is a FluentAssertions assertion-failure message, not a build error and not an unhandled
exception. The RED state is exactly the D-Plan-1 stub: `[P1-T11]` widened the return type to
`(long Score, string TopFolder)` but hard-codes `TopFolder` to `string.Empty` at
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:192`. The `Score` half of the assertion
already passes, which confirms the test reaches the real method and fails only on the undelivered
behaviour.

## Output Summary

Failing-first test for AC5 lands RED by assertion on `TopFolder`. Compile EXIT_CODE 0; scoped run
EXIT_CODE 1 with 1 executed and 1 Failed. `[P2-T4]` replaces the `string.Empty` stub with the real
`score.TopFolder` and turns this test green.
