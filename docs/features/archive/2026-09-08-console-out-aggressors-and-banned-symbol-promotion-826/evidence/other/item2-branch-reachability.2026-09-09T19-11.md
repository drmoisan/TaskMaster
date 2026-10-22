# Item-2 branch reachability, re-measured against the post-825 tree (issue #826, [P1-T1])

Timestamp: 2026-09-09T19-11

Command: the six steps below were performed with the Read and Grep tools against the working tree of the
`-exec` worktree at base commit `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`. No shell command was
required and none was run, so this artifact records a derivation rather than a tool exit status.

EXIT_CODE: 0

## Step 1 — the `RunWithTimeout` call inside `GetTableInViewAsync`

File: `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, lines 74 to 81.

```csharp
table = await TimeOutTask.RunWithTimeout(
    view.GetTable,
    token,
    timeoutMs,
    1,
    false,
    resolvedTimeoutSourceFactory
);
```

Argument list, in order: `view.GetTable` (the `Func<TResult>` delegate), `token`, `timeoutMs`, `1`
(`maxAttempts`), **`false` (`strict`)**, and `resolvedTimeoutSourceFactory` as the timeout-source
factory. `resolvedTimeoutSourceFactory` is bound at lines 63 to 70 and is the caller-supplied
`timeoutSourceFactory` parameter whenever one is supplied, falling back to a `TimeProvider`-derived
factory otherwise. The `strict` argument is `false`, as plan decision D2 step 1 states.

## Step 2 — the overload the call binds, and the private overload it forwards to

File: `UtilitiesCS/Threading/TimeOutTask.cs`.

`view.GetTable` is a `Func<Outlook.Table>`, so the call binds the public overload whose first parameter
is `this Func<TResult>`, declared at lines 21 to 28. Its body, lines 30 to 37, forwards to the private
overload declared at lines 40 to 48, supplying `attempt: 0`:

```csharp
return await function.RunWithTimeout(token, milliseconds, maxAttempts, strict, 0, timeoutSourceFactory);
```

## Step 3 — factory invocation site versus the `try` block (this is the deciding step)

File: `UtilitiesCS/Threading/TimeOutTask.cs`, private overload.

- The factory is invoked at **lines 52 to 54**:

  ```csharp
  using var timeoutSource = (
      timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))
  )(milliseconds);
  ```

- The `try` block opens at **line 61** and its body runs to line 64; its `catch` clauses are at lines 65
  and 85 and the whole `try` statement closes at line 92.

**The factory invocation is OUTSIDE that `try`.** It sits nine lines above the `try` keyword, in the
method body proper. An exception thrown by an injected factory is therefore caught by neither
`catch (TaskCanceledException)` at line 65 nor `catch (System.Exception e)` at line 85, and leaves
`RunWithTimeout` carrying the type the factory threw. This is the condition plan decision D3 names as
the deciding test, and it holds.

## Step 4 — every `throw` and rethrow inside the `try` region, and its `strict` guard

Enumerated by reading `UtilitiesCS/Threading/TimeOutTask.cs` lines 61 to 92 in full:

| Line | Statement | Guarded by `strict`? | Effect with `strict = false` |
|---|---|---|---|
| 67 | `token.ThrowIfCancellationRequested();` inside `catch (TaskCanceledException)` | no | throws `OperationCanceledException` only when the outer token is already cancelled; that is the sole exception that escapes from inside the `try` region |
| 71 to 78 | recursive `function.RunWithTimeout(...)` with `attempt + 1`, inside `catch (TaskCanceledException)` | no | not a throw; propagates whatever the recursion propagates |
| 90 | bare `throw;` inside `catch (System.Exception e)` | **yes**, by `if (strict)` at line 88 | not taken, because this call site passes `strict = false`; the exception is logged at line 87 and absorbed |

Consequence: a `TimeoutException` raised by the delegate *inside* the `try` reaches line 85, is logged
and absorbed, and does not reach `GetTableInViewAsync`. Only an exception thrown by the injected factory,
which is raised before line 61, reaches `GetTableInViewAsync`. The two mechanisms are distinct and the
test must use the factory one.

## Step 5 — both catch clauses recurse and pass the factory through

File: `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`.

- `catch (TaskCanceledException)` opens at line 88. `token.IsCancellationRequested` is tested at line 90;
  the `else` branch begins at line 94 and holds the first edited statement at line 96. The recursive call
  is at **lines 99 to 105**, with `counter + 1` at line 101 and `timeoutSourceFactory` passed through at
  line 103.
- `catch (TimeoutException)` opens at line 113 and holds the second edited statement at line 115. The
  recursive call is at **lines 122 to 128**, with `counter + 1` at line 124 and `timeoutSourceFactory`
  passed through at line 126.

Both recursive calls therefore hand the same factory instance to the second attempt, so a factory that
throws only on its first invocation lets the second attempt run `view.GetTable` once and return the
mocked table.

## Step 6 — the 825 test that already pins the mechanism

File: `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`.

`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` still exists, declared at
**line 112** under the `[TestMethod]` attribute at line 111. Its factory, at lines 120 to 133, throws
`TimeoutException` on its first invocation and returns a fresh parameterless `CancellationTokenSource`
afterwards. The doc comment at lines 104 to 110 records the same outside-the-`try` mechanism in prose.

That file is named by a `<Compile Include>` item at `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
**line 549**:

```xml
<Compile Include="OutlookObjects\Table\GetTableInViewAsyncClockTests.cs" />
```

so the test is compiled and executed. Independent corroboration from this feature's own [P0-T9] run:
changed line 115, the statement inside `catch (TimeoutException)`, carries a baseline hit count of 2,
which is only reachable if that catch body executes.

## Verdict

BRANCH: REACHABLE

Recorded because step 3 finds the factory invocation outside the `try`. Plan decision D3's halt-verdict
branch is not taken. AC7 is satisfiable as written, no substitute test is authored, and execution
continues into Phase 2, where [P2-T2] authors two `[TestMethod]` methods.

Output Summary: all six steps re-derived against the post-825 tree with current line citations. The
`strict` argument is `false`; the injected factory is invoked at `TimeOutTask.cs` lines 52 to 54, outside
the `try` that opens at line 61; the only exception escaping from inside the `try` is the
`OperationCanceledException` from line 67; both catch clauses in `GetTableInViewAsync` recurse with
`counter + 1` and pass the factory through; and the 825 test at
`GetTableInViewAsyncClockTests.cs` line 112 is registered and compiled. The verdict recorded above is
the reachable one.
