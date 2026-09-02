# P3-T9 — Test-File Hygiene Census on `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`

Timestamp: 2026-09-01T08-28

Command: the same PowerShell census used by P0-T12, applying ordinal `String.IndexOf` scanning for
simple-match (occurrence) counts. Run after the final format pass.

EXIT_CODE: 0

## Census

| Measurement | **Post-change** | Required | Met |
| --- | --- | --- | --- |
| Simple-match `Task.Delay` | **0** | 0 | yes |
| Simple-match `Thread.Sleep` | **0** | 0 | yes |
| Simple-match `Thread.SpinWait` | **0** | 0 | yes |
| Simple-match `milliseconds: 30_000` | **1** | 1 | yes |
| Simple-match `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` | **1** | 1 | yes |
| Simple-match `timeoutSourceFactory: timeoutSourceFactory` | **1** | 1 | yes |
| File-level simple-match `CancellationToken.None` | **17** | 17 | yes |

**P0-T12 baseline for `CancellationToken.None`: 16.** Post-change: **17**, an increase of exactly
one. That delta is the arithmetic consequence of the appended method carrying exactly one caller
token and no existing method being edited, which P2-T6 independently confirms by showing the diff is
a pure insertion with zero deletion lines.

**All seven assertions are met.**

## Reading of the Results

- **The three banned timing APIs are all absent.** `.claude/rules/general-unit-test.md` names
  `Thread.Sleep` and `Task.Delay` as prohibited APIs in test code and bans real wall-clock waits;
  `Thread.SpinWait` is included as a busy-wait equivalent. The new regression test introduces none of
  them. Determinism comes from the injected pre-cancelled `CancellationTokenSource`, which fixes the
  outcome before any scheduling decision is made, rather than from a timing assumption. The observed
  55 ms execution time at P2-T4 corroborates that no wall-clock wait occurs.
- **`milliseconds: 30_000` occurs once.** The timeout value is deliberately large and never armed, so
  the test cannot depend on the real `System.Threading.Timer` that a short value would start. This
  avoids the flakiness class the spec attributes to issue #253.
- **The test name occurs once**, confirming exactly one new test method was added, not a duplicated
  or renamed pair.
- **`timeoutSourceFactory: timeoutSourceFactory` occurs once**, confirming the seam is bound by named
  argument on the public wrapper. This is the binding that requires the P1-T2 wrapper parameter to
  exist and would raise CS1739 if it did not.

This artifact is the evidence cited by the AC1 and AC3 check-offs at P4-T1 and P4-T3.

Output Summary: The test file contains zero occurrences of `Task.Delay`, `Thread.Sleep`, and
`Thread.SpinWait`; exactly one occurrence each of `milliseconds: 30_000`, the new test name, and the
`timeoutSourceFactory: timeoutSourceFactory` named argument; and 17 occurrences of
`CancellationToken.None` against the P0-T12 baseline of 16.

Acceptance: met. The first three counts are each 0; the `milliseconds: 30_000` count is 1; the
test-name count is 1; the named-argument count is 1; and the file-level `CancellationToken.None`
count is 17.
