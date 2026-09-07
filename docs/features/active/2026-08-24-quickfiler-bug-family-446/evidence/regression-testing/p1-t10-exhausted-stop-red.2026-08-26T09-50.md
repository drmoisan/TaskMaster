# [P1-T10] [expect-fail] Source Drain Reports `SourceExhausted`

Timestamp: 2026-08-26T09-50

Task: [P1-T10] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` — added
`DequeueAsync_SourceDrained_ReportsSourceExhaustedStop`. The take delegate returns null and
`sourceActive` reports false, which is the take-returned-null exit. The test asserts
`Stop == QfcDequeueStop.SourceExhausted` and an empty `Accepted`.

This is the complementary exit to `[P1-T9]`. Together the two tests are what make the caller-side
guard in `[P2-T7]` meaningful: one stop reason must close the queue and the other must not.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_SourceDrained_ReportsSourceExhaustedStop" "/Logger:trx;LogFileName=p1-t10.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t10"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t10/p1-t10.trx`

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX:

```
Expected batch.Stop to be QfcDequeueStop.SourceExhausted {value: 1} because a drained source with no active producer is genuine exhaustion, but found QfcDequeueStop.QuantitySatisfied {value: 0}.
```

This is a FluentAssertions assertion-failure message. Note the D-Plan-1 reasoning is borne out
here: had `[P1-T8]` stubbed `Stop` to `SourceExhausted` instead of `QuantitySatisfied`, this test
would have passed vacuously in the RED state and gated nothing.

## Output Summary

Test lands RED by assertion on the stop reason. Compile exit 0, scoped run exit 1 with the single
test Failed. `[P2-T1]` maps the take-returned-null exit to `SourceExhausted` and turns it green.
