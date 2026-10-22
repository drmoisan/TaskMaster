# P1-T6 [expect-fail] — Red Run of the New Regression Test Against the Unfixed Handler

Timestamp: 2026-09-01T08-17

## Command

```text
<resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p1-t6 /TestCaseFilter:FullyQualifiedName=UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException
```

`<resolved-vstest>` is the vswhere-resolved path recorded in P0-T10.

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

```text
Total tests: 1
     Failed: 1
Test Run Failed.
 Total time: 1.5126 Seconds
```

| Count | Value |
| --- | --- |
| Total tests | **1** |
| Passed | **0** |
| **Failed** | **1** |

`Passed: 0` is reported by omission: vstest prints no `Passed:` summary line when that count is zero,
and `Total tests: 1` with `Failed: 1` fixes the passed count at 0.

**This is the expected outcome for this task.** A failing run here is the fail-before evidence the
Bugfix Workflow in `CLAUDE.md` requires. The observed exit code equals the declared
`ExpectedExitCode`, so this gate is a pass, not a failure to repair.

## Failure Message and Stack Frames (verbatim)

The worktree root is written as `<WT>` below so this artifact carries no host path; nothing else is
altered.

```text
  Failed RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException [56 ms]
  Error Message:
   Test method UtilitiesCS.Test.TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException threw exception:
System.Threading.Tasks.TaskCanceledException: A task was canceled.
  Stack Trace:
     at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at UtilitiesCS.TimeOutTask.<RunWithTimeout>d__6`2.MoveNext() in <WT>\UtilitiesCS\Threading\TimeOutTask.cs:line 240
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at UtilitiesCS.TimeOutTask.<RunWithTimeout>d__5`2.MoveNext() in <WT>\UtilitiesCS\Threading\TimeOutTask.cs:line 175
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at UtilitiesCS.Test.TimeOutTask_Tests.<RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException>d__49.MoveNext() in <WT>\UtilitiesCS.Test\Threading\TimeOutTask_OverloadCoverageTests.cs:line 412
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.Execution.TestMethodInfo.<ExecuteInternalAsync>d__58.MoveNext()
```

### The required literal is present

The recorded failure text contains the literal **`System.Threading.Tasks.TaskCanceledException`**, on
the line `System.Threading.Tasks.TaskCanceledException: A task was canceled.` The red gate is valid;
no workaround was applied.

### The failure mechanism matches the spec's predicted pre-fix failure mode

The stack frames confirm the defect exactly as the spec's Root Cause Analysis describes, and confirm
it is an escaping exception rather than a wrong return value:

1. `UtilitiesCS.TimeOutTask.<RunWithTimeout>d__6` is the **private** `Func<T1, TResult>`
   implementation. The injected pre-cancelled timeout source cancels the linked combined token before
   `Task.Run` is queued, so the awaited task completes `Canceled` and the awaiter throws
   `TaskCanceledException`.
2. That type does not match `catch (TimeoutException)`, so the retry ladder is skipped entirely.
3. It falls to the general handler `catch (System.Exception e)`, which logs it and — because the test
   passes `strict: true` — rethrows it with a bare `throw;`.
4. It propagates out of `<RunWithTimeout>d__5` (the **public** wrapper) and out of the `await` in the
   test at `TimeOutTask_OverloadCoverageTests.cs:line 412`.

No assertion in the test was reached. This is the escaping `TaskCanceledException` that AC1 requires
as evidence.

## Production State at the Time of This Run (load-bearing for AC1)

This red run was taken with the determinism seam present (P1-T1 and P1-T2) and the defective
`catch (TimeoutException)` clause **untouched**. The P1-T1 acceptance measurement, taken immediately
after the P1-T1 edits and before any handler change, recorded:

| Anchored pattern | Count at P1-T1 | Baseline (P0-T12) |
| --- | --- | --- |
| `^\s*catch \(TimeoutException\)\s*$` | **4** | 4 |
| `^\s*catch \(TaskCanceledException\)\s*$` | 9 | 9 |
| `^\s*catch \(System\.Exception e\)\s*$` | 10 | 10 |
| simple-match `when (e is TaskCanceledException \|\| e is TimeoutException)` | **0** | 0 |

The `catch (TimeoutException)` count was still 4 and no filter clause existed, so the handler under
test was the original defective one. The seam itself is behaviour-preserving by construction:
`timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` with the parameter defaulted to
`null` produces the identical `CancellationTokenSource` the pre-change line produced, and all seven
existing call sites bind unchanged. This is the plan's declared deviation from AC1's literal wording
`against unmodified production code`, and it is recorded again in the P4-T13 summary.

## Fail-Before Requirement

This is a **real failing run**, so no fail-before exception dossier is required or created.

SearchScope: not applicable — a failing run exists at this path.

Acceptance: met. The run reports `Total tests: 1` with `Failed: 1` and `Passed: 0`; the recorded
failure text contains the literal `System.Threading.Tasks.TaskCanceledException`; and the artifact
records `EXIT_CODE: 1` against `ExpectedExitCode: 1`.
