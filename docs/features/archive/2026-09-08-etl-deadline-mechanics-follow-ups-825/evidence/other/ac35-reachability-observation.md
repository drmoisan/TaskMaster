# AC35 — Reachability Observation and Deferred Follow-Up Handoff

Timestamp: 2026-09-09T17-29

PromotionWrittenOnThisBranch: false

## The traced conclusion

**On the ordinary timeout path, `TimeOutTask.RunWithTimeout` returns `default(TResult)` — that is,
null for a reference type — without throwing anything at all. No exception escapes it. Consequently
neither `catch` block in `GetTableInViewAsync` is entered by that path, neither of its retry
recursions runs, and `GetTableInViewAsync` returns null through its `return table!`.**

This feature does not change that. It is recorded as an observation about a pre-existing condition,
and no code change in this feature acts on it.

## The trace, step by step

Line citations into UtilitiesCS/Threading/TimeOutTask.cs are unmoved by this feature: Phase 4's
deletions sat at pre-change lines 824 to 940, below every line named here.

1. **TimeOutTask.cs line 52** constructs the deadline source from the resolved factory, in a
   statement spanning lines 52 to 54 that sits outside the try opened at line 61.
2. **TimeOutTask.cs line 60** initialises `TResult result = default(TResult)!;`. That value is what
   the method returns if no branch assigns another.
3. **TimeOutTask.cs line 63** runs the work as `await Task.Run(() => function(), combinedToken.Token)`.
   When the deadline expires the timeout source cancels, the linked token cancels, and this await
   faults with a `TaskCanceledException`.
4. **TimeOutTask.cs line 65** catches that `TaskCanceledException`.
5. **TimeOutTask.cs line 67** calls `token.ThrowIfCancellationRequested()`. On the ordinary timeout
   path the **caller's** token is not cancelled — only the internal timeout source is — so this does
   not throw.
6. **TimeOutTask.cs line 69** tests `attempt < maxAttempts`. The public entry overload passes an
   initial `attempt` of 0, and `GetTableInViewAsync` passes `maxAttempts` of 1, so on the first pass
   this is true and the method recurses internally at lines 71 to 78 with `attempt + 1`. The
   recursion arms a second deadline from the same factory and times out the same way; on that pass
   `1 < 1` is false.
7. **TimeOutTask.cs line 82** is then reached, in the `else`. It calls
   `logger.Warn($"Task timed out after {attempt} attempts.")` and throws nothing.
8. **TimeOutTask.cs line 94** returns `result!`, which is still the `default(TResult)` from line 60.

Nothing on that path throws, so the try at **TableAccess.cs line 72** completes normally, the
`catch (TaskCanceledException)` at **line 88** and the `catch (TimeoutException)` at **line 113** are
both skipped, and **line 138** returns `table!` with `table` null.

Those four TableAccess.cs line numbers are re-derived against the post-change file; their pre-change
values were 55, 71, 95 and 118, and Phase 3's additions moved all four.

## The adjacent path that does throw, recorded to prevent a misreading

If the **caller's** token is already cancelled, step 5 above throws an `OperationCanceledException`.
That throw happens inside the `catch` block rather than inside the try, so it escapes
`RunWithTimeout` and reaches TableAccess.cs line 74. The `catch (TaskCanceledException)` at line 88
does **not** catch it: `TaskCanceledException` derives from `OperationCanceledException`, not the
other way round, so a catch clause typed to the derived class cannot receive the base. The exception
therefore propagates to the caller of `GetTableInViewAsync`. That is the behaviour the existing test
`GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` pins.

That is the pre-cancelled-token path, not the ordinary deadline-expiry path, and the two must not be
conflated.

## What this feature's change does and does not do to the escaping exception

**It does nothing to it.** This feature changed which clock arms the deadline and which millisecond
value the retry uses. It did not change what `RunWithTimeout` does when the deadline expires:
`RunWithTimeout`'s signatures, its `strict` semantics and its retry behaviour are unchanged,
`strict` remains `false` at the `GetTableInViewAsync` call site, `maxAttempts` remains 1, and the
`catch (System.Exception e)` at TimeOutTask.cs lines 85 to 92 is neither narrowed nor widened.

Stated plainly for a later reader: **after this change, on the ordinary timeout path, nothing escapes
`RunWithTimeout` — not an `OperationCanceledException`, not a `TaskCanceledException`, and not a
`TimeoutException`. It returns null.** Both `Console.WriteLine` diagnostics in
`GetTableInViewAsync`, the one inside the `TaskCanceledException` catch and the one inside the
`TimeoutException` catch, therefore remain unreachable through the ordinary timeout path, exactly as
they were before this feature. Neither has become reachable.

The one way either catch block is entered remains an exception raised outside the try in
`RunWithTimeout` — in practice, one thrown by a supplied `timeoutSourceFactory` at line 52. That is
the mechanism the regression test
`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` uses deliberately, and it is
the only mechanism found in the current tree that reaches the `TimeoutException` branch
deterministically.

## The adjacent defect, recorded and not fixed

`GetTableInViewAsync` returning null on timeout instead of throwing, while its public contract is
non-null and its callers dereference the result, is a known adjacent defect. It is a pre-existing
condition documented in the comment above the `return table!` and is explicitly outside this
feature's scope. It is not fixed here, no test in this feature asserts on it, and no acceptance
criterion turns on it.

## DeferredHandoff

Addressed to the epic. Each item below is to be filed through the issue-promotion lifecycle **after
this feature merges**. None is filed on this branch: a promotion writes a record under
docs/features/potential/promoted/, which would put a file under docs/features/** that is not one of
this feature's own documents and would falsify AC20 on the same branch. The two obligations cannot
both hold here, and the on-branch write is the one that is dropped.

1. Capture real ETL durations against folder size from a live Outlook session using the recipe
   recorded in the budget comment in UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs, and
   revisit the 250 ms per-row budget with that measurement in hand.
2. Reduce UtilitiesCS/Threading/TimeOutTask.cs below the 500-line cap. This feature took it from
   1011 lines to 966, which is a reduction and not a resolution; see
   evidence/other/file-size-accounting.md.
3. The reachability observation recorded above, concerning the two `catch` blocks in
   `GetTableInViewAsync` under `strict: false` and `maxAttempts: 1`, together with the adjacent
   null-return defect it exposes.
4. Convert the `TimeOutTask_Tests` wall-clock races to an injected clock, which would then allow that
   class's `[DoNotParallelize]` attribute to be removed rather than documented.

## Scope confirmation

No file outside docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/ was created or
modified by this task, and no promotion tool was called.
