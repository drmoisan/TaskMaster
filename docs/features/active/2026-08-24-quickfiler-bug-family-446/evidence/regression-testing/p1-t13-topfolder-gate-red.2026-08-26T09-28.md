# [P1-T13] [expect-fail] Accepted Candidate Must Carry Its Top Folder

Timestamp: 2026-08-26T09-28

Task: [P1-T13] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` — added
`DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult`.

The test drives the reflective nine-parameter `CreateGate` seam with a single high-scoring
candidate and a score loader returning `(950L, @"Inbox\Projects\Alpha")`, then reads the gate
result through `DequeueBatchAsync`. It asserts the single entry in `Accepted` is the same
`MailItem` instance and that its `PredeterminedFolder` equals the folder the score loader
returned. `FakeTimeProvider` supplies the clock, so no wall-clock wait occurs.

## Verification

Command: `dotnet tool run csharpier check "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs"`
EXIT_CODE: 0

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_AcceptedCandidate_CarriesTopFolderInPreScoredResult" "/Logger:trx;LogFileName=p1-t13.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t13"`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t13/p1-t13.trx`

TRX counters: `total="1" executed="1" passed="0" failed="1"`.

Recorded outcome: **Failed**.

Failure message, quoted verbatim from the TRX:

```
Expected accepted.PredeterminedFolder to be "Inbox\Projects\Alpha" with a length of 20 because the folder the score loader already returned must travel with the accepted candidate instead of being discarded and re-derived downstream, but "" has a length of 0, differs near "" (index 0).
```

This is a FluentAssertions assertion-failure message, not a build error and not an unhandled
exception. The RED state is the D-Plan-1 stub in the gate's accept branch: the gate destructures
`(long score, string topFolder)` from the widened loader but constructs the carrier as
`new QfcPreScoredItem(mailItem, string.Empty)`. The `MailItem` half of the assertion already
passes, confirming the test reaches the real accept path and fails only on the undelivered
behaviour.

## Output Summary

Failing-first test for the gate-side half of top-folder propagation lands RED by assertion on
`PredeterminedFolder`. Format check EXIT_CODE 0, compile EXIT_CODE 0, scoped run EXIT_CODE 1 with
1 executed and 1 Failed. Phase 2 replaces the `string.Empty` argument with the loader's
`topFolder` and turns this test green.
