# [P1-T4] Negative Control — An Accepted Candidate Does Not Invoke the Rejection Sink

Timestamp: 2026-08-26T09-25

Task: [P1-T4] (not tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` — added
`DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected` as the AC13 negative control. It arranges
a single above-cutoff candidate with a recording rejection sink and asserts the candidate is
accepted while the sink stays empty.

This test is green in both the pre-fix and post-fix states by construction — pre-fix because no
sink is ever invoked, post-fix because `[P2-T2]` invokes the sink only in the `else` of the accept
decision — so it is deliberately not tagged `[expect-fail]`. Its value is as a guard against an
over-broad `[P2-T2]` implementation that fired the sink for every scanned candidate.

## Verification

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected" "/Logger:trx;LogFileName=p1-t4.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t4"`
EXIT_CODE: 0

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t4/p1-t4.trx`

Recorded outcome for `DequeueAsync_AcceptedCandidate_DoesNotInvokeOnRejected`: **Passed**
(`outcome="Passed"` on the `UnitTestResult` element; the run-level element records
`outcome="Completed"`).

## Output Summary

Negative control added and Passed on the first run, as the acceptance condition requires.
