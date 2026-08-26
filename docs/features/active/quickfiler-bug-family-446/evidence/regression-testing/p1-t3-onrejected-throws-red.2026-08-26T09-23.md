# [P1-T3] [expect-fail] Rejection Sink Failure Must Not Abort the Scan

Timestamp: 2026-08-26T09-23

Task: [P1-T3] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` — added
`DequeueAsync_OnRejectedThrows_ScanContinues`. The test drives one below-cutoff candidate whose
rejection sink throws `InvalidOperationException`, followed by one above-cutoff candidate, and
asserts both:

1. the sink was invoked exactly once, for the below-cutoff item; and
2. the scan continued and accepted the following above-cutoff candidate.

Assertion (1) is what makes this a real gate. A test that asserted only (2) would pass vacuously
in the RED state, because no sink exists yet and the scan already continues past a rejected item.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_OnRejectedThrows_ScanContinues" "/Logger:trx;LogFileName=p1-t3.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t3"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t3/p1-t3.trx`

Recorded outcome for `DequeueAsync_OnRejectedThrows_ScanContinues`: **Failed**
(`outcome="Failed"` on the `UnitTestResult` element).

Failure message, quoted verbatim from the TRX `<Message>` element:

```
Expected invocations to contain a single item because the throwing sink must still be invoked once for the rejected item, but the collection is empty.
```

This is a FluentAssertions assertion-failure message, not a build error and not an unhandled
exception.

## Output Summary

Test lands RED by assertion on the invocation-count conjunct, exactly as intended. Compile exit 0,
scoped run exit 1 with the single test Failed.
