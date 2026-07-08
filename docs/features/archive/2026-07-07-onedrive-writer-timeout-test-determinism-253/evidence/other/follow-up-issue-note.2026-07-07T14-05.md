# Follow-Up Issue Note — `TimeOutTask.cs` Exception-Type Mismatch (Out of Scope for Issue #253)

Timestamp: 2026-07-07T16-51

## Defect Description

`TimeOutTask.RunWithTimeout<T1, TResult>` (private implementation overload, `UtilitiesCS/Threading/TimeOutTask.cs:199`) catches `TimeoutException` in its exception-handling ladder:

```csharp
catch (TimeoutException)
{
    ...
}
```

Every sibling overload in the same file that wraps a `Task.Run`/awaited task under a real `CancellationTokenSource`-driven timeout instead catches `TaskCanceledException`:
- `RunWithTimeout<TResult>` (`Func<TResult>` overload), line 64: `catch (TaskCanceledException)`
- `RunWithTimeout<TResult>` (`Func<CancellationToken,Task<TResult>>` overload), line 129: `catch (TaskCanceledException)`
- `RunWithTimeout<T1,T2,TResult>` (`Func<T1,T2,TResult>` overload), line 350: `catch (TaskCanceledException)`
- `RunWithTimeout<T1,T2,T3,TResult>` (`Func<T1,T2,T3,TResult>` overload), line 580: `catch (TaskCanceledException)`

Because `System.Threading.Tasks.TaskCanceledException` (raised when `Task.Run(..., combinedToken)` is canceled by a real timer-linked `CancellationTokenSource`) is unrelated to `System.TimeoutException` (both derive independently from `SystemException`), the `catch (TimeoutException)` clause at line 199 can never match a genuine timer-driven cancellation in this overload. It only matches a `TimeoutException` that the wrapped delegate throws directly — which is exactly why the overload's own existing coverage tests simulate "timeout" via an explicitly thrown `TimeoutException` rather than a genuinely short `milliseconds` value.

## Source and Scope

This defect is cited and analyzed in the research artifact `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/research/2026-07-07T13-00-onedrive-writer-timeout-research.md`, Section 2.1 ("Exception-type mismatch confirmed against sibling overloads") and Section 3, Option (c) (the rejected alternative fix of changing the catch clause).

This defect is explicitly **NOT fixed** under issue #253. The adopted fix for issue #253 (research Section 3, Option (a)) resolves the test-determinism problem by introducing an injectable `WriterTimeoutRunner` seam in `OneDriveDownloader` so the wrapper contract can be verified without exercising the real timer/thread-pool path at all — it does not touch `TimeOutTask.cs` or change its exception-handling behavior. Per the repository's bugfix-workflow minimal-fix principle (CLAUDE.md, "Implement the minimal, targeted fix... If you uncover deeper design problems, open a new issue instead of widening scope"), the exception-type mismatch is out of scope for this plan.

Research Section 3 also notes that a prior proposed production fix (catching `TaskCanceledException` instead of `TimeoutException` at line 199) was shown to break other existing `TimeOutTask` retry tests, confirming this is a nontrivial, separately-scoped behavioral change that requires its own dedicated investigation and test updates.

## Follow-Up Tracking

A new GitHub issue should be filed to track this defect independently. No issue number has been assigned as of this writing; this is recorded as a pending follow-up item, not a fabricated issue number.

## Output Summary

Recorded and cross-referenced (research Sections 2.1 and 3, Option (c)) the `TimeOutTask.RunWithTimeout<T1,TResult>` exception-type mismatch (`catch (TimeoutException)` at line 199, inconsistent with all four sibling overloads' `catch (TaskCanceledException)`). Confirmed out of scope for issue #253 per the bugfix-workflow minimal-fix principle. No production or test change to `TimeOutTask.cs` was made under this plan. A new GitHub issue should be filed to track this defect (issue number not yet assigned).
