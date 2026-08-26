# [P4-T3] AC3 Verification — #448 Pre-Fix Failure Is an Assertion Failure

Timestamp: 2026-08-26T10-31

Task: [P4-T3]
Acceptance criterion: AC3
Feature: docs/features/active/quickfiler-bug-family-446
Merge base (`<mb>`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`

## AC3 text (spec.md:877)

> AC3 — #448: `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` is present in
> `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` and its pre-fix failure is an
> **assertion failure, not a hang or a test-host timeout**. Verified by running that single test
> against the pre-fix tree and recording the failure message.

## 1. Presence

Command: `grep -n "UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay" "QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs"`
EXIT_CODE: 0
Output: `397:        public void UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay()`

The test is declared at `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:397`.

## 2. Recorded pre-fix failure message, verbatim

Source TRX: `docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t2/p3-t2.trx`
(the `[P3-T2]` red step, run before the `[P3-T3]` loop rewrite).
Outcome: `Failed`. Duration: `00:00:00.3423838`.

```
Expected clock.DelayRequests to be greater than or equal to 1 because an idle iteration must wait through the injected TimeProvider, not Task.Delay, but found 0 (difference of -1).
```

## 3. Classification of that message

| Check | Result |
| --- | --- |
| Is a FluentAssertions assertion-failure message (`Expected ... but found ...` shape) | Yes |
| Contains the substring `timeout` (case-insensitive) | No |
| Contains the substring `timed out` (case-insensitive) | No |
| Contains the substring `cancel` (case-insensitive, covers `cancelled`, `canceled`, `OperationCanceledException`, `TaskCanceledException`) | No |
| Contains `Aborted` or a test-host crash signature | No |

The `[P3-T2]` TRX `<Counters>` node corroborates the classification:
`total="1" executed="1" passed="0" failed="1" timeout="0" aborted="0" passedButRunAborted="0"`.
The `timeout` and `aborted` counters are both zero, so the run terminated on an assertion, not on
a `[Timeout]` trip or a host abort. The recorded duration of 0.34 seconds is inconsistent with a
hang.

## 4. Pass-after

The same test is recorded `Passed` in
`docs/features/active/quickfiler-bug-family-446/evidence/regression-testing/p3-t3/p3-t3.trx`
(immediately after the loop rewrite), and again in
`evidence/regression-testing/p3-t7/p3-t7.trx` and `evidence/regression-testing/p3-t8/p3-t8.trx`.

## Output Summary

AC3 holds. `UndoConsumer_EveryIdleIteration_InvokesTimeProviderDelay` is present at
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:397`. Its recorded pre-fix outcome in
`evidence/regression-testing/p3-t2/p3-t2.trx` is a FluentAssertions assertion failure
(`Expected clock.DelayRequests to be greater than or equal to 1 ... but found 0`) containing no
timeout or cancellation wording, with TRX counters `timeout="0" aborted="0"` and a 0.34-second
duration. The AC3 checkbox in `spec.md` is checked.

EXIT_CODE: 0
