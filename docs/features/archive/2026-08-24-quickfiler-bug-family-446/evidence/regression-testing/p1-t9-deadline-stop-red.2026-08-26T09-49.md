# [P1-T9] [expect-fail] Deadline Expiry Reports `DeadlineExpired`

Timestamp: 2026-08-26T09-49

Task: [P1-T9] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` — added
`DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop`, plus the
`using QuickFiler.Interfaces;` directive required to name `QfcDequeueStop`.

The test follows the established `FakeTimeProvider` pattern from
`QfcStreamingDequeueConfidenceGateTests.Part2.cs:36-69`: ten candidates that all score below the
cutoff, each score advancing the fake clock by one second against a three-second first-batch
deadline. It asserts `Stop == QfcDequeueStop.DeadlineExpired` and an empty `Accepted`.

No wall-clock wait, no `Thread.Sleep` and no `Task.Delay` is used, satisfying the determinism
requirements of `.claude/rules/general-unit-test.md`.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_DeadlineExpiresWithZeroAccepted_ReportsDeadlineExpiredStop" "/Logger:trx;LogFileName=p1-t9.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t9"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t9/p1-t9.trx`

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX:

```
Expected batch.Stop to be QfcDequeueStop.DeadlineExpired {value: 2} because an empty batch caused by the first-batch deadline is not source exhaustion, but found QfcDequeueStop.QuantitySatisfied {value: 0}.
```

This is a FluentAssertions assertion-failure message. It confirms the D-Plan-1 stub is in force
(`QuantitySatisfied` at every exit) and that the test discriminates against it, so the test is a
real gate rather than a vacuous one.

## Output Summary

Test lands RED by assertion on the stop reason. Compile exit 0, scoped run exit 1 with the single
test Failed. `[P2-T1]` maps the deadline exit to `DeadlineExpired` and turns it green.
