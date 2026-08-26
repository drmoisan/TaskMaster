# [P1-T2] [expect-fail] #426 Rejection Seam and Its Failing-First Test

Timestamp: 2026-08-26T09-20

Task: [P1-T2] (tagged `[expect-fail]`)
Feature: docs/features/active/quickfiler-bug-family-446

## Change

Production — `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`:

- Added a final optional constructor parameter `Action<MailItem> onRejected = null` to the
  widest constructor, with an XML `<param>` doc stating the #426 rationale.
- Added `private readonly Action<MailItem> _onRejected;` and assigned it in that constructor.
- **No invocation of `_onRejected` was added.** Per D5 and D-Plan-1 the invocation lands in
  `[P2-T2]`, which is what turns this test green.

Test — `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`:

- Both `CreateGate` overloads gained an `Action<MailItem> onRejected = null` parameter and
  forward it.
- The exact reflective lookup was widened from the 8-type to the 9-type shape, with the guard
  message updated to name the nine-parameter seam. The lookup remains a single `GetConstructor`
  call, so the `[P4-T20]` count of 1 is unaffected.
- Added `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce`, asserting the sink is
  invoked exactly once for a single below-cutoff candidate and that the discarded item is the
  same instance. It also re-asserts the drop-on-reject contract (`result.Should().BeEmpty()`).

## Verification

### Intra-phase compile

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

The tree compiles. The RED state below is therefore an assertion failure against a compiling
tree, which is exactly what AC4 requires.

### Scoped run (expected to fail)

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce" "/Logger:trx;LogFileName=p1-t2.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t2"`
EXIT_CODE: 1
ExpectedExitCode: 1

- Total: `1`
- Failed: `1`
- Passed: `0`

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t2/p1-t2.trx`

Recorded outcome for `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce`: **Failed**.

Failure message, quoted verbatim from the TRX `<Message>` element:

```
Expected rejected to contain a single item because the gate must report each discarded candidate exactly once, but the collection is empty.
```

This is a FluentAssertions assertion-failure message. It is not a build error and not an
unhandled exception.

## Output Summary

Seam landed without an invocation; regression test lands RED by assertion against a compiling
tree. Compile exit 0, scoped run exit 1 with 1 failed as designed.
