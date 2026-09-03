# FakeTimeProvider zero-due-time observation (superseded P2-T4 run, revision round 14)

Timestamp: 2026-09-03T01-52

Command: `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NonBlockingDelayTests"`

Tool resolution used the Block K prelude (`MSBUILD_FOUND: True`, `VSTEST_FOUND: True`).

EXIT_CODE: 1
ExpectedExitCode: 1

TotalCount: 3
PassedCount: 2
FailedCount: 1

## Per-node outcomes

| Test method | Outcome |
|---|---|
| `WaitAsync_WithNoDispatcher_CompletesAfterInterval` | Passed [46 ms] |
| `WaitAsync_ZeroDelay_CompletesWithoutPump` | Failed [102 ms] |
| `WaitAsync_SingleArgumentOverload_CompletesOnSystemTimeProvider` | Passed [1 ms] |

## Failure text

```
  Failed WaitAsync_ZeroDelay_CompletesWithoutPump [102 ms]
  Error Message:
   Expected waitTask.IsCompleted to be False because FakeTimeProvider fires a due timer on the next
   advance, not at creation, but found True.
  Stack Trace:
     at FluentAssertions.Primitives.BooleanAssertions`1.BeFalse(String because, Object[] becauseArgs)
     at TaskMaster.Test.AppGlobals.NonBlockingDelayTests.<WaitAsync_ZeroDelay_CompletesWithoutPump>d__1.MoveNext()
     in <repo-root>\TaskMaster.Test\AppGlobals\NonBlockingDelayTests.cs:line 76
```

## The authorized mechanical retry branch does not apply

P2-T4 authorizes exactly one retry branch:

> if `WaitAsync_ZeroDelay_CompletesWithoutPump` fails on its `waitTask.Status` assertion, change the
> single line `fakeTimeProvider.Advance(TimeSpan.Zero);` in that method to
> `fakeTimeProvider.Advance(TimeSpan.FromTicks(1));`

Two facts make that branch inapplicable here, and both were observed rather than inferred:

1. **The failure is not on the `waitTask.Status` assertion.** The stack trace names
   `BooleanAssertions.BeFalse` at `NonBlockingDelayTests.cs:line 76`. Line 76 is the second line of
   the pre-`Advance` statement
   `waitTask.IsCompleted.Should().BeFalse("FakeTimeProvider fires a due timer on the next advance, not at creation");`
   The `Status` assertion begins at line 83 and is never reached, because the test aborts at line 76.

2. **The prescribed edit cannot fix this failure.** The prescribed edit changes the argument of
   `fakeTimeProvider.Advance(...)` on line 79. Line 79 executes *after* line 76. Changing an
   argument on a line the test never reaches cannot change the outcome of the assertion that
   aborted it.

## Observed mechanism

The premise encoded in the assertion's `because` string — and stated in `spec.md` Assumptions and in
research 1.4 — is that `FakeTimeProvider` "fires a due timer on the next advance, not at creation".
The observed behaviour is the opposite: with a due time of `TimeSpan.Zero`, `CreateTimer` invokes the
callback during creation, so the returned task is already completed when control returns from
`NonBlockingDelay.WaitAsync(TimeSpan.Zero, fakeTimeProvider)`.

`spec.md` line 209 anticipated one direction of error — that the upstream comparison might be
*strict* rather than inclusive, for which `Advance(TimeSpan.FromTicks(1))` is the documented
fallback. The observed behaviour is the other direction: the timer is *more* eager than assumed, so
no advance is required at all. Neither `spec.md` nor P2-T4 carries a branch for that direction.

## Determinism confirmation

The failure is deterministic, not a race. A second scoped run filtered to the single node
`FullyQualifiedName~WaitAsync_ZeroDelay_CompletesWithoutPump` reproduced the identical failure at the
identical source line:

```
Total tests: 1
  at TaskMaster.Test.AppGlobals.NonBlockingDelayTests.<WaitAsync_ZeroDelay_CompletesWithoutPump>d__1.MoveNext()
  in <repo-root>\TaskMaster.Test\AppGlobals\NonBlockingDelayTests.cs:line 76
EXIT_CODE: 1
```

## Acceptance verdict

P2-T4's acceptance requires `EXIT_CODE: 0`, `PassedCount: 3`, and `FailedCount: 0`. Observed:
`EXIT_CODE: 1`, `PassedCount: 2`, `FailedCount: 1`. **The acceptance is not met and P2-T4 remains
unchecked.**

No workaround was attempted. Specifically, the following were considered and deliberately **not**
performed, because each would depart from the plan text without authorization:

- Editing the `Advance` argument on line 79 as the inapplicable retry branch prescribes — it cannot
  affect the assertion that fails.
- Deleting or weakening the line-76 `IsCompleted.Should().BeFalse(...)` pre-assertion. That
  assertion is authoritative Block B content, and `spec.md` AC5 requires the rewritten tests to
  "assert the returned task is not completed before `Advance`". Removing it would silently narrow an
  acceptance criterion.
- Changing the due time away from `TimeSpan.Zero`, which is the scenario the test exists to cover.

Output Summary: 2 of 3 tests passed. `WaitAsync_ZeroDelay_CompletesWithoutPump` fails
deterministically at `NonBlockingDelayTests.cs:line 76` because `FakeTimeProvider` fires a
zero-due-time one-shot timer at creation rather than on the next advance, falsifying the premise
Block B's pre-`Advance` assertion encodes. The plan's single authorized retry branch is scoped to a
different assertion and prescribes an edit to a line the failing test never reaches, so it does not
apply. Execution stops at P2-T4 pending a plan revision.
