# [P4-T4] AC4 Verification — #426 Gate Test Was Red Against a Compiling Tree

Timestamp: 2026-08-26T10-32

Task: [P4-T4]
Acceptance criterion: AC4
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC4 text (spec.md:878)

> AC4 — #426 gate: `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce` is present in
> `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, and its pre-fix state
> is an **assertion failure against a compiling tree** (the seam and the test land in one task, the
> `else` invocation in the next), not a compile error.

## 1. Presence

Command: `grep -n "DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce" "QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs"`
EXIT_CODE: 0
Output: `345:        public async Task DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce()`

## 2. The tree that produced the Failed outcome compiled

Both facts below are recorded in one artifact,
`docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t2-onrejected-seam-red.2026-08-26T09-20.md`,
and therefore describe one and the same tree state: the state left by `[P1-T2]`, in which the
`Action<MailItem> onRejected` constructor seam exists but is never invoked (the `else`
invocation is deferred to `[P2-T2]`).

### 2a. Compile of that tree state

Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

This is the `[P1-T2]` intra-phase compile. It is a compile check, not the analyzer or nullable
gate; those gates run in Phase 5 with `/t:Rebuild` and are not asserted here.

### 2b. Scoped test run of the same tree state

Command: `& $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/Settings:scripts\vscode\TaskMaster.cli.runsettings" "/TestCaseFilter:FullyQualifiedName~DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce" "/Logger:trx;LogFileName=p1-t2.trx" "/ResultsDirectory:docs\features\active\quickfiler-bug-family-446\evidence\regression-testing\p1-t2"`
EXIT_CODE: 1 (ExpectedExitCode: 1)

TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p1-t2/p1-t2.trx`
Recorded outcome for `DequeueAsync_BelowThresholdCandidate_InvokesOnRejectedOnce`: **Failed**.

Failure message, verbatim from that TRX:

```
Expected rejected to contain a single item because the gate must report each discarded candidate exactly once, but the collection is empty.
```

That message is a FluentAssertions assertion failure. It is not a compiler diagnostic (no `CS`
code, no file/line build error) and not an unhandled exception.

## 3. Pass-after

`docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p2-t2/p2-t2.trx` —
outcome `Passed`, after `[P2-T2]` added the `else`-branch invocation. Also `Passed` in the
whole-assembly runs `evidence/regression-testing/p2-t8/p2-t8.trx` and
`evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC4 holds. The test is present at
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:345`. The `[P1-T2]`
artifact records a compile `EXIT_CODE: 0` for the same tree state that produced the `Failed`
outcome in `evidence/regression-testing/p1-t2/p1-t2.trx`, and the recorded failure text is an
assertion message rather than a compile error. The AC4 checkbox in `spec.md` is checked.

EXIT_CODE: 0
