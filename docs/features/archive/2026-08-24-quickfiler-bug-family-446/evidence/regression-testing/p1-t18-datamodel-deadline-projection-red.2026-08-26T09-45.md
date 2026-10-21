# [P1-T18] [expect-fail] Datamodel Must Project the Deadline-Expired Gate Stop

Timestamp: 2026-08-26T09-45

Task: [P1-T18] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` — added
`DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop`, and
`using Microsoft.Extensions.Time.Testing;`.

The test builds an uninitialized `QfcDatamodel`, injects a `FakeTimeProvider` through the
`TimeProvider` seam, fills the master queue with ten candidates, enables high-confidence mode with a
`0.90` threshold, and drives scoring through the `ScoringServiceFactory` seam added by `[P1-T5]` so
no live Outlook COM is touched (`.claude/rules/general-unit-test.md` UT4). Every score advances the
fake clock by one second and returns `100L`, which is below the cutoff, so a three-second
`firstBatchDeadline` expires after three scored candidates with an empty accepted set — the gate's
deadline exit. The test then asserts the datamodel projects that gate result as
`QfcDequeueStop.DeadlineExpired`.

This gates the datamodel-side projection that `[P2-T6]` implements.

## Verification

Command: `dotnet tool run csharpier format "QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop" "/Logger:trx;LogFileName=p1-t18.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t18"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t18/p1-t18.trx`

TRX counters: `total="1" executed="1" passed="0" failed="1"`.

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX:

```
Expected batch.Stop to be QfcDequeueStop.DeadlineExpired {value: 2} because a deadline-bounded empty batch must not be reported as quantity satisfaction, but found QfcDequeueStop.QuantitySatisfied {value: 0}.
```

This is a FluentAssertions assertion-failure message, not a build error and not an unhandled
exception; the TRX stack trace begins at `FluentAssertions.Primitives.EnumAssertions.Be`. The RED
state has two stubbed layers, both deliberate under D-Plan-1: the gate hard-codes
`QfcDequeueStop.QuantitySatisfied` at all four exits (`[P1-T8]`), and the datamodel's
`DequeueNextItemGroupWithOutcomeAsync` hard-codes the same value (`[P1-T15]`). The empty `Items`
half of the assertion already passes, confirming the test really drives the gate's deadline exit.

## Output Summary

Failing-first test for the datamodel-side stop projection lands RED by assertion. Format EXIT_CODE
0, compile EXIT_CODE 0, scoped run EXIT_CODE 1 with 1 executed and 1 Failed.
`QfcQueuePurePathsTests.cs` finishes at 262 lines. `[P2-T1]` discriminates the gate's exits and
`[P2-T6]` projects the gate stop through the datamodel, turning this test green.
