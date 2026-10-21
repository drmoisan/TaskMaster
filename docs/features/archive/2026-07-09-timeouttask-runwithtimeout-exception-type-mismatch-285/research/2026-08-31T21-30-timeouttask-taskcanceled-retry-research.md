# Research — Issue #285: `TimeOutTask.RunWithTimeout<T1, TResult>` exception-type mismatch

- **Timestamp:** 2026-08-31T21-30
- **Issue:** #285
- **Branch:** `bug/timeouttask-runwithtimeout-exception-type-mismatch-285`
- **Tree analysed:** worktree HEAD (based on `origin/main` @ `2b85134b`)
- **Scope:** research only; no source, config, or test files were modified.

## 0. Verdict

**The defect reproduces and is NOT already fixed.** A real production diff is required.

`UtilitiesCS/Threading/TimeOutTask.cs` line 200 still reads `catch (TimeoutException)` inside the
private `Func<T1, TResult>` overload declared at line 177, whose awaited work at line 198 is
`Task.Run(() => function(arg1), combinedToken.Token)`. Timer-driven cancellation of that
`Task.Run` surfaces as `TaskCanceledException`, which is unrelated to `TimeoutException`, so the
retry ladder at lines 202-218 is unreachable for a genuine timeout.

**Tool constraint:** the Bash tool was disabled for this session, so `git show d208fa68` could not
be executed. The `TimeProvider` seam that commit introduced is instead evidenced directly from the
current tree (`TimeOutTask.cs:838-848, 870, 924-935, 957` and
`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs:5, 12-20, 27`). Every other citation in
this document was read directly from files at HEAD.

---

## 1. Corrections to the issue body / `spec.md`

Both `spec.md` and `docs/features/potential/promoted/2026-07-09-...md` inherit two errors. They
must be corrected in the plan, not restated.

### 1.1 Line citations have shifted by one

`TimeOutTask.cs` gained `#nullable enable` as line 1 during the nullable-remediation epic, shifting
every cited line by +1. Re-derived against HEAD:

| Cited in issue/spec | Correct line at HEAD | What is actually there |
|---|---|---|
| `164-199` (overload range) | **165-230** | public wrapper 165-175; private impl **177-230** |
| `:164` (declaration) | **165** (public wrapper) / **177** (private impl carrying the bug) | |
| `:199` (`catch (TimeoutException)`) | **200** | `catch (TimeoutException)` |
| sibling `:64` | **65** | `catch (TaskCanceledException)` in `Func<TResult>` impl |
| sibling `:129` | **130** | `catch (TaskCanceledException)` in `Func<CancellationToken,Task<TResult>>` impl |
| sibling `:350` | **351** | `catch (TaskCanceledException)` in `Func<T1,T2,TResult>` impl |
| sibling `:580` | **581** | `catch (TaskCanceledException)` in `Func<T1,T2,T3,TResult>` impl |
| other `TimeoutException` sites `:817, :907` | **818, 914** | in `TimeoutAfter` retry wrappers (see §7) |

The awaited call is at line **198**; the general handler is at line **220**.

### 1.2 "Propagates unhandled to the caller" is FALSE

`spec.md` "Actual Behavior" / repro step 3 claims the `TaskCanceledException` "propagates unhandled
to the caller." It does not. Read at HEAD, `TimeOutTask.cs:220-227`:

```csharp
catch (System.Exception e)
{
    logger.Error(e);
    if (strict)
    {
        throw;
    }
}
```

`TaskCanceledException` derives from `OperationCanceledException` -> `SystemException` ->
`Exception`, so it is caught here. The true behaviour, in both modes:

- **`strict == true`** — the exception is **caught and logged at ERROR** (`logger.Error(e)`), then
  **rethrown** by the bare `throw;` at line 225. It reaches the caller, but only after being
  handled and logged, and only in this mode. The `maxAttempts` retry ladder never runs.
- **`strict == false`** — the exception is **caught, logged at ERROR, and swallowed**. Control falls
  to `return result!;` at line 229, where `result` is still the `default!` assigned at line 195.
  **The caller silently receives `default(TResult)` (`null` for reference types) with zero retries
  and no exception.** The `logger.Warn($"Task timed out after {attempt} attempts.")` diagnostic at
  line 217 also never fires, so the log records a generic ERROR rather than a timeout.

**Both production call sites of this overload pass `strict: false`** (§5). Therefore, in shipped
behaviour today, a genuine timeout on this overload **never** propagates: it is mislogged as an
ERROR-level exception and silently degrades to `null`, with the configured `maxAttempts` value
(3 at both call sites) having no effect whatsoever.

---

## 2. Q1 — Correct fix shape

### 2.1 Complete catch-clause census of `TimeOutTask.cs`

See `## Numeric Derivation Evidence` (§9) for the derivation of every count below.

`TimeOutTask.cs` contains **23** `catch` clauses across 12 methods: **9** catch
`TaskCanceledException`, **4** catch `TimeoutException`, **10** catch `System.Exception e`.

| # | Method (private impl unless noted) | Impl line | Awaited work | Specific catch(es) | Retries? |
|---|---|---|---|---|---|
| 1 | `Func<TResult>` | 40 | `Task.Run(() => function(), combinedToken.Token)` (63) | `TaskCanceledException` @**65** | yes |
| 2 | `Func<CancellationToken,Task<TResult>>` | 108 | `task(combinedToken.Token)` (128) | `TaskCanceledException` @**130** | yes |
| 3 | **`Func<T1,TResult>` (DEFECT)** | **177** | `Task.Run(() => function(arg1), combinedToken.Token)` (198) | **`TimeoutException` @200** | yes, but unreachable |
| 4 | `Func<T1,CancellationToken,Task<TResult>>` | 244 | `task(arg1, combinedToken.Token)` (266) | `TaskCanceledException` @**268** (**no retry**); `TimeoutException` @**272** (retry) | mixed |
| 5 | `Func<T1,T2,TResult>` | 327 | `Task.Run(() => function(arg1, arg2), combinedToken.Token)` (349) | `TaskCanceledException` @**351** | yes |
| 6 | `Func<T1,T2,CancellationToken,Task<TResult>>` | 405 | `task(arg1, arg2, cancel)` (427) — passes `cancel`, **not** `combinedToken.Token` | `TaskCanceledException` @**429** | yes |
| 7 | `Func<T1,T2,CancellationToken,Task>` (void) | 475 | `task(arg1, arg2, cancel)` (496) — same | `TaskCanceledException` @**498** | yes |
| 8 | `Func<T1,T2,T3,TResult>` | 556 | `Task.Run(() => function(arg1, arg2, arg3), combinedToken.Token)` (579) | `TaskCanceledException` @**581** | yes |
| 9 | `Func<T1,T2,T3,CancellationToken,Task<TResult>>` | 638 | `task(arg1, arg2, arg3, cancel)` (661) — same | `TaskCanceledException` @**663** | yes |
| 10 | `Func<T1,T2,T3,CancellationToken,Task>` (void) | 720 | `task(arg1, arg2, arg3, cancel)` (742) — same | `TaskCanceledException` @**744** | yes |
| 11 | `TimeoutAfter<TResult>(Task<TResult>, int, int)` (public) | 806 | none (synchronous call) | `TimeoutException` @**818** | yes (dead — §7) |
| 12 | `TimeoutAfter(Task, int, int)` (public) | 906 | none (synchronous call) | `TimeoutException` @**914** | yes (dead — §7) |

Each of methods 1-10 also has a trailing `catch (System.Exception e)` that logs and conditionally
rethrows (lines 85, 149, 220, 290, 372, 450, 519, 603, 685, 766).

### 2.2 Which shape is correct for the line-177 overload

The defect method is **structurally identical** to methods **1, 5, and 8**: all four await
`Task.Run(<sync lambda>, combinedToken.Token)` where `combinedToken` links the caller token to a
timer-armed `CancellationTokenSource`. All three peers catch `TaskCanceledException` and retry.
Method 3 is the only member of that four-method family that does not.

Method 4 (line 244) is **not** the right model, for two reasons:
1. Its shape differs — it awaits a caller-supplied `Task`, not a `Task.Run` wrapper, so its
   delegate can legitimately raise either exception on its own.
2. Its `TaskCanceledException` clause deliberately does not retry, which is an unexplained
   asymmetry with its own `TimeoutException` clause. Copying that asymmetry into method 3 would
   make the fix a no-op for the retry contract this issue is about.

**Correct target shape:** the line-177 overload must handle a **`TaskCanceledException` by entering
the existing retry ladder**, exactly as methods 1, 5, and 8 do.

### 2.3 Replacement vs addition

Section 3 (Q2) establishes that a bare replacement invalidates two passing tests. The fix must be
**additive**. Recommended minimal edit — a single line changed, at line 200:

```csharp
// before (line 200)
catch (TimeoutException)

// after
// A timer-driven cancellation of Task.Run surfaces as TaskCanceledException, not
// TimeoutException (issue #285). TimeoutException is retained because a wrapped
// delegate may raise it directly, and existing callers/tests depend on that retry.
catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
```

The clause body (lines 202-218) is unchanged, including its leading
`token.ThrowIfCancellationRequested()`, which already guards caller-token cancellation.

**Rationale for the filter form over two clauses.** The two exception types share no base other
than `Exception`, so a single typed clause cannot cover both. Two sibling clauses would duplicate
the ~15-line retry body, which the General Code Change Policy ("Reusability — avoid copy-paste")
discourages. The filter is a one-line diff and keeps a single retry body.

**Style-consistent alternative** (if a reviewer objects to `catch (Exception …) when (…)`): two
clauses with the retry body factored into a private local function. This is a larger diff and is
not recommended for a minimal bugfix.

**Analyzer note.** CA1031 ("do not catch general exception types") is not enforced in this
repository — `catch (System.Exception e)` already appears 10 times in this very file and builds
clean under `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. `ex` is consumed by the
filter, so no unused-variable diagnostic arises.

### 2.4 `TaskCanceledException` vs `OperationCanceledException` — decided

`TaskCanceledException` derives from `OperationCanceledException`, so `catch (TaskCanceledException)`
does **not** catch a bare `OperationCanceledException`. Determining what `Task.Run` actually throws:

- **Token already cancelled when `Task.Run` is called.** The TPL checks cancellation before the
  work item executes, so **the delegate is never invoked** and the task transitions directly to
  `TaskStatus.Canceled` with no stored `OperationCanceledException`. Awaiting it runs
  `TaskAwaiter.ThrowForNonSuccess`, which finds no cancellation `ExceptionDispatchInfo` and throws
  **`new TaskCanceledException(task)`**. This is the deterministic path the regression test uses.
- **Token cancels while the synchronous delegate is already running.** `() => function(arg1)` has
  no access to `combinedToken.Token` — the combined token is created inside the method and never
  handed to `function`. The delegate therefore cannot observe cancellation, runs to completion, and
  the task ends `RanToCompletion`. **No exception at all.** This is confirmed in-repo by the
  explanatory comment at `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1276-1285`,
  which documents exactly this behaviour for the sibling `Func<TResult>` overload and asserts
  `callCount == 1` with no retry.
- **Bare `OperationCanceledException` path.** `ThrowForNonSuccess` rethrows a stored OCE only when
  the delegate threw an `OperationCanceledException` whose `CancellationToken` equals the task's own
  token. That requires the caller's `function` to hold the combined token instance, which is
  impossible here. Unreachable.

**Consequence for the fix: catch `TaskCanceledException`.** It is the exact type produced, it
matches all 9 sibling cancellation clauses, and widening to `OperationCanceledException` would both
diverge from every sibling and swallow unrelated caller-thrown OCEs into the retry ladder. A
pre-cancelled *caller* token is already short-circuited earlier by
`token.ThrowIfCancellationRequested()` at line 187.

---

## 3. Q2 — SIBLING INVALIDATION verdict

### 3.1 Test A — `TimeOutTask_AdditionalTests.cs:189-216`

`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`. The delegate throws
`new TimeoutException("timeout")` on its first invocation (line 198) and returns `$"result-{arg}"`
thereafter. Called with `maxAttempts: 1, strict: true`. Asserts:
- `result.Should().Be("result-42")` (line 214)
- `attempts.Should().Be(2)` (line 215)

This test **requires the `TimeoutException` retry ladder to fire.**

### 3.2 Test B — `TimeOutTask_OverloadCoverageTests.cs:105-122`

`RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`. The delegate
unconditionally throws `new TimeoutException("timeout")` (line 109). Called with
`maxAttempts: 0, strict: true`. Asserts `result.Should().BeNull()` (line 121).

This test **requires `TimeoutException` to be swallowed by the specific clause** (which logs `Warn`
and leaves `result` at `default`), **not** to reach the general handler.

### 3.3 Verdict — control-flow trace, not a guess

**If `catch (TimeoutException)` is REPLACED by `catch (TaskCanceledException)`, BOTH tests FAIL.**

- Test A: the delegate's `TimeoutException` propagates out of `Task.Run` and is rethrown at the
  `await` on line 198. It no longer matches the specific clause, so it falls to
  `catch (System.Exception e)` at line 220 -> `logger.Error(e)` -> `strict == true` -> `throw;`.
  The test's `await function.RunWithTimeout(...)` at line 205 throws `TimeoutException` instead of
  returning `"result-42"`. **FAIL** (unhandled `TimeoutException` in the test method), and
  `attempts` would be 1, not 2.
- Test B: identical path — falls to line 220, `strict == true` -> `throw;`. The test expects a
  `null` return, gets a thrown `TimeoutException`. **FAIL.**

**Therefore the fix is an ADDITION, not a replacement.** The `TimeoutException` clause must be
retained alongside a `TaskCanceledException` clause.

### 3.4 Both tests under the recommended additive fix

Under `catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)`:

- Test A: `TimeoutException` matches the filter -> `token.ThrowIfCancellationRequested()` on
  `CancellationToken.None` is a no-op -> `attempt (0) < maxAttempts (1)` -> recursive retry ->
  delegate returns `"result-42"`, `attempts == 2`. **PASSES, unchanged.**
- Test B: `TimeoutException` matches the filter -> no-op guard -> `attempt (0) < maxAttempts (0)`
  is false -> `logger.Warn(...)` -> `result` remains `default` -> returns `null`. **PASSES,
  unchanged.**

Neither test needs editing. Neither the existing production behaviour for a delegate-thrown
`TimeoutException`, nor either test's assertions, change.

---

## 4. Q3 — Determinism seam

### 4.1 Target frameworks (VERIFIED, not assumed)

| Project | `TargetFrameworkVersion` | Evidence |
|---|---|---|
| `UtilitiesCS/UtilitiesCS.csproj` | **`v4.8.1`** (net481) | line 16; `<LangVersion>12.0</LangVersion>` at line 10 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | **`v4.8.1`** (net481) | line 17 |

Both are .NET Framework 4.8.1. Consequently the `CancellationTokenSource(TimeSpan, TimeProvider)`
constructor (.NET 8+, an instance constructor on a BCL type that no package can add) **does not
exist** here.

### 4.2 Option (b) — `TimeProvider` / `FakeTimeProvider`: viable but rejected

`Microsoft.Bcl.TimeProvider` 10.0.11 **is** referenced by both projects
(`UtilitiesCS.csproj:96-97`, `UtilitiesCS.Test.csproj:591-592`), and
`Microsoft.Extensions.TimeProvider.Testing` 10.9.0 supplies `FakeTimeProvider` to the test project
(`UtilitiesCS.Test.csproj:643-644`). The seam that commit `d208fa68` established is visible in the
tree at `TimeOutTask.cs:844-848` and `931-935` (`TimeProvider? timeProvider = null` parameter) with
`(timeProvider ?? TimeProvider.System).CreateTimer(...)` at lines 870 and 957, consumed by
`UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs:20` (`FrozenClock()`).

It **was applied to `TimeoutAfter`, not `RunWithTimeout`.** Adapting it here would require
replacing `new CancellationTokenSource(milliseconds)` with
`(timeProvider ?? TimeProvider.System).CreateCancellationTokenSource(TimeSpan.FromMilliseconds(milliseconds))`
— an extension method that Microsoft confirms is present in `Microsoft.Bcl.TimeProvider` for
`netframework-4.8.1` (verified against the `TimeProviderTaskExtensions` API reference). So it is
technically viable. It is nonetheless **rejected**, because:

1. **It does not remove the race.** Even with the CTS cancelled deterministically by advancing a
   `FakeTimeProvider`, the test must advance the clock *after* `RunWithTimeout` has armed the timer
   and queued `Task.Run`. Whether the delegate has already been dequeued at that instant is
   thread-pool-scheduling-dependent. Per §2.4, if the delegate has started, cancelling produces
   **no exception at all** and the test silently passes/fails on scheduling luck. That is precisely
   the flakiness class that produced issue #253.
2. **Larger production diff.** It introduces a new `TimeProvider` parameter into an overload family
   that has no such parameter, diverging from the already-established seam on the immediate sibling.
3. **No in-repo precedent for `CreateCancellationTokenSource`.** Zero occurrences repo-wide.

### 4.3 Option (a) — `timeoutSourceFactory`: RECOMMENDED

The sibling `Func<TResult>` overloads at lines 21-38 and 40-95 already carry
`Func<int, CancellationTokenSource>? timeoutSourceFactory = null`, consumed at lines 52-54 as
`(timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds)` and threaded
through the recursion at line 77. `OlTableExtensions.GetTableInViewAsync` threads it through at
`OlTableExtensions.TableAccess.cs:36, 62, 85, 106`, and it is exercised by
`OlTableExtensions_Tests.cs:1286-1322`.

That existing test (read in full) does **not** drive a retry: it holds an
**un**-cancelled `injectedSource` (line 1286), returns it from the factory (line 1302), and has the
mocked `GetTable()` cancel it *from inside the running delegate* (lines 1292-1298). Its documented
point (comment, lines 1276-1285) is that mid-flight cancellation of an already-started synchronous
delegate does **not** cancel the task — hence `callCount == 1` and no retry. It is a valuable
negative precedent but is not the pattern this issue needs.

**The pattern this issue needs is a factory that returns an ALREADY-cancelled source.** Then
`CreateLinkedTokenSource` (line 190) yields an already-cancelled combined token,
`Task.Run(..., combinedToken.Token)` never dequeues the delegate, and the awaited task is `Canceled`
before any scheduling decision is made. There is **no wall-clock wait and no race** — the outcome
is fixed by construction.

**Why no seam-free alternative works.** Passing `milliseconds: 0` is not viable: on .NET Framework
4.8.1 `CancellationTokenSource(int)` arms a `System.Threading.Timer` with a zero due-time, which
fires asynchronously on the thread pool. It races `Task.Run` and is non-deterministic. (The
synchronous fast path for a zero delay exists only in .NET Core 3.0+.) The
`Task.FromCanceled<T>(...)` trick used at `TimeOutTaskCoverageTests.cs:129` and `:195` cannot be
applied either, because this overload's delegate returns `TResult`, not a `Task`.

### 4.4 Production diff required by the recommended seam

Mirroring lines 21-38 / 40-95 exactly:

```csharp
public static async Task<TResult> RunWithTimeout<T1, TResult>(
    this Func<T1, TResult> function,
    T1 arg1,
    CancellationToken token,
    int milliseconds,
    int maxAttempts,
    bool strict,
    Func<int, CancellationTokenSource>? timeoutSourceFactory = null   // ADDED
)
{
    return await function.RunWithTimeout(
        arg1, token, milliseconds, maxAttempts, strict, 0,
        timeoutSourceFactory                                          // ADDED
    );
}

private static async Task<TResult> RunWithTimeout<T1, TResult>(
    this Func<T1, TResult> function,
    T1 arg1,
    CancellationToken token,
    int milliseconds,
    int maxAttempts,
    bool strict,
    int attempt,
    Func<int, CancellationTokenSource>? timeoutSourceFactory = null   // ADDED
)
{
    token.ThrowIfCancellationRequested();

    using var timeoutSource = (                                       // CHANGED from line 189
        timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))
    )(milliseconds);
    using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
        token,
        timeoutSource.Token
    );

    TResult result = default!;
    try
    {
        result = await Task.Run(() => function(arg1), combinedToken.Token);
    }
    // A timer-driven cancellation of Task.Run surfaces as TaskCanceledException, not
    // TimeoutException (issue #285). TimeoutException is retained because a wrapped
    // delegate may raise it directly, and existing callers/tests depend on that retry.
    catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException) // CHANGED from line 200
    {
        token.ThrowIfCancellationRequested();

        if (attempt < maxAttempts)
        {
            result = await function.RunWithTimeout(
                arg1, token, milliseconds, maxAttempts, strict, attempt + 1,
                timeoutSourceFactory                                  // ADDED
            );
        }
        else
        {
            logger.Warn($"Task timed out after {attempt} attempts.");
        }
    }
    catch (System.Exception e)   // unchanged
    {
        logger.Error(e);
        if (strict)
        {
            throw;
        }
    }

    return result!;
}
```

**Factory-per-attempt confirmed.** The factory is invoked once at the top of each private-impl
activation (the `using var timeoutSource = ...` line), and the recursive call passes the factory
through, so activation *n* calls the factory for the *(n+1)*-th time. This is exactly the mechanism
already live at lines 52-54 and 77 for the `Func<TResult>` sibling. The factory receives only
`milliseconds`, which is constant across attempts, so a test must count invocations in its own
closure.

### 4.5 Regression test sketch (compiling)

```csharp
[TestMethod]
public async Task RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException()
{
    // Arrange
    // Determinism seam (mirrors the S7 timeoutSourceFactory precedent in
    // OlTableExtensions_Tests). Attempt 0 receives an ALREADY-cancelled source, so
    // CreateLinkedTokenSource yields an already-cancelled combined token and
    // Task.Run never dequeues the delegate -- the awaited task is Canceled by
    // construction and the await throws TaskCanceledException with no wall-clock
    // wait and no scheduling race. Attempt 1 receives a never-cancelled source so
    // the retry can succeed and be observed.
    // Before the issue #285 fix this test fails: the TaskCanceledException misses
    // catch (TimeoutException), reaches the general handler, and (strict: true) is
    // rethrown instead of retried.
    using var canceledSource = new CancellationTokenSource();
    canceledSource.Cancel();
    using var liveSource = new CancellationTokenSource();

    int factoryCalls = 0;
    Func<int, CancellationTokenSource> timeoutSourceFactory = _ =>
        Interlocked.Increment(ref factoryCalls) == 1 ? canceledSource : liveSource;

    int delegateCalls = 0;
    Func<int, string> function = arg =>
    {
        Interlocked.Increment(ref delegateCalls);
        return $"result-{arg}";
    };

    // Act
    var result = await function.RunWithTimeout(
        42,
        CancellationToken.None,
        milliseconds: 30_000,
        maxAttempts: 1,
        strict: true,
        timeoutSourceFactory: timeoutSourceFactory
    );

    // Assert
    result.Should().Be("result-42");
    delegateCalls.Should().Be(1);   // attempt 0 never ran the delegate
    factoryCalls.Should().Be(2);    // one source per attempt
}
```

Load-bearing details, all verified:

- **Caller token is `CancellationToken.None`**, so both `token.ThrowIfCancellationRequested()`
  guards (line 187 pre-flight and line 202 inside the clause) are no-ops and cannot abort the
  retry. This is essential: cancelling the *caller* token instead of the *timeout* source would be
  rethrown at line 202 and never retry.
- **`milliseconds: 30_000` is deliberately large.** The injected factory ignores it entirely; the
  large value guarantees the test cannot accidentally depend on the real timer if the seam is ever
  removed.
- **Disposal is safe.** The production method's `using var timeoutSource` disposes each returned
  source at the end of its activation; the test's `using` declarations dispose them again.
  `CancellationTokenSource.Dispose()` is idempotent. Attempt 1 executes lexically inside attempt 0's
  `using` scope, so `liveSource.Token` is never read after disposal. Pre-creating both sources (as
  above) rather than constructing them inside the lambda also avoids any CA2000 exposure.
- **`Interlocked.Increment(ref <captured local>)` is legal C#** — the local is hoisted to a display
  class field. Existing precedent: `TimeOutTask_OverloadCoverageTests.cs:18`.

**Optional second test** covering the `else { logger.Warn(...) }` exhaustion branch (returns the
always-cancelled source, `maxAttempts: 1`, asserts `result.Should().BeNull()` and
`factoryCalls.Should().Be(2)`). Recommended but not required for the RED gate.

---

## 5. Q4 — Public API impact

### 5.1 Source compatibility

Adding a **trailing optional parameter** to the public wrapper at line 165 is source-compatible:
every existing 6-argument call still binds with the parameter defaulted to `null`. It is **not**
binary-compatible (the metadata signature changes), but `UtilitiesCS` and all of its consumers build
from `TaskMaster.sln` in the same pass, so no stale-binary risk exists. This is the identical change
already shipped for the `Func<TResult>` overload (lines 21-38) with no caller edits.

### 5.2 Every call site of the `Func<T1, TResult>` overload

Derived from `\.RunWithTimeout\(` across all `*.cs` (§9, N6). **Two** production sites and **five**
test sites bind to this overload. None breaks.

| File:line | Receiver | Args | Binds? | Breaks? |
|---|---|---|---|---|
| `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:139` | `Func<string, Stream> factory` (parameter of the `_writerTimeoutRunner` lambda) | 6 | yes, `T1=string, TResult=Stream` | **No** |
| `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs:80` | **method group** `GetConversationTable` | 6 | yes, `T1=Outlook.Conversation?, TResult=Table` | **No** — see 5.3 |
| `UtilitiesCS.Test/.../TimeOutTask_AdditionalTests.cs:177, 205, 225` | `Func<int, string>` | 6 | yes | No |
| `UtilitiesCS.Test/.../TimeOutTask_OverloadCoverageTests.cs:112, 132` | `Func<int, string>` | 6 | yes | No |

Sites binding to **other** overloads, unaffected by this change:
`OneDriveDownloader.cs:48` (`Func<string,CancellationToken,Task<HttpResponseMessage>>` -> line 232
overload); `StreamExtensions.cs:27` (`Func<Stream,int,CancellationToken,Task>` -> line 462 overload);
`OlTableExtensions.TableAccess.cs:56`, `ConversationHelper.Formatting.cs:97`,
`ConversationHelper.cs:252`, `ConversationHelper.cs:305` (all `Func<TResult>` -> line 21 overload,
which already has the parameter).

### 5.3 Ambiguity risk with the line-232 `Func<T1, CancellationToken, Task<TResult>>` overload

**No ambiguity is introduced.** The two overloads differ in the type of the `this` parameter:
`Func<T1, TResult>` (1-arity delegate) vs `Func<T1, CancellationToken, Task<TResult>>` (2-arity
delegate). No delegate instance or method group can satisfy both.

The riskiest binding form present is the **method group** at
`ConversationHelper.Formatting.cs:80-87`. `GetConversationTable` has signature
`Table GetConversationTable(Conversation?)` — one parameter. Converting it to
`Func<T1, CancellationToken, Task<TResult>>` would require two parameters and a `Task<>` return, so
that candidate is inapplicable regardless of the new optional parameter. Verified by reading the
declaration at `ConversationHelper.Formatting.cs:111`.

Internal calls are also unambiguous: the wrapper's call to the private impl passes an `int` literal
`0` in the `attempt` position, and `int` has no conversion to
`Func<int, CancellationTokenSource>?`, so the public overload is never a candidate for a 7-argument
call. The `Func<TResult>` sibling already compiles this exact shape at line 30 today.

### 5.4 Behavioural consequence at the two production call sites (IMPORTANT)

The fix makes previously-dead retry logic **live**. Both sites pass `strict: false` and a non-zero
`maxAttempts`:

- `ConversationHelper.Formatting.cs:80-87` — `timeoutMs = 1000, maxAttempts = 3` around the COM call
  `conversation.GetTable()`. Today a genuine timeout returns `null` immediately, and
  `GetDataFrameAsync` short-circuits to `return null` (line 88-91). After the fix, it retries up to
  3 more times, so worst-case latency on a stalled conversation table rises from ~1 s to ~4 s while
  the failure rate falls. This is the intended contract but is a real, user-visible change in the
  QuickFiler conversation-dataframe path.
- `OneDriveDownloader.cs:139` — `maxAttempts = 3` around the file-writer factory. Same shape.

Neither change is a behaviour *regression*; both restore the documented `maxAttempts` semantics.
They should be called out in the PR description and, if the plan wishes, waived explicitly against
the "No unintended behavior changes outside the defined scope" acceptance criterion.

---

## 6. Q5 — Nullable and toolchain constraints

- `UtilitiesCS/Threading/TimeOutTask.cs` carries **`#nullable enable` at line 1**. Under
  `/p:TreatWarningsAsErrors=true` its `CS86xx` diagnostics are build **errors**.
- **The `?` annotation is mandatory** on the new parameter:
  `Func<int, CancellationTokenSource>? timeoutSourceFactory = null`. Omitting it yields
  `CS8625` (null literal to non-nullable reference type) -> build error. This is the form already
  used at lines 27 and 47 of the same file.
  - Historical note: archived evidence
    `docs/features/archive/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/regression-testing/p1-build.2026-06-09T18-00.md:45-55`
    recommended the **non-`?`** form. That guidance is **stale** — it was written in June 2026 when
    this file was nullable-*disabled*. The file has since been migrated (epic
    `utilitiescs-nullable-remediation`, issue #369). Do not follow it.
- No other nullable hazard: `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` is
  null-safe; `TResult result = default!;` (line 195) and `return result!;` (line 229) already carry
  the required suppressions and are unchanged.
- `UtilitiesCS.Test/Threading/*.cs` carry no `#nullable` directive, so the test file needs no
  annotations.
- **Do NOT add `/p:Nullable=enable`.** It is absent from `.github/workflows/ci.yml` and fails
  wholesale on this repository.
- **Toolchain order (CLAUDE.md, run in this exact order; restart from step 1 on any failure or
  auto-fix):**
  1. `dotnet tool run csharpier format .` (verify `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  Use `/t:Rebuild`, never `/t:Build` — a warm `/t:Build` skips `CoreCompile` and the gate cannot
  fail. Run `dotnet tool restore` once in this worktree before the first CSharpier invocation.

---

## 7. Out-of-scope follow-up findings

Reported for separate triage. **None of these is in scope for issue #285** and none should be
touched by this change.

1. **`combinedToken` constructed but never used in four overloads.** Methods 6, 7, 9, and 10
   (impl lines 405, 475, 638, 720) build `combinedToken` (lines 419, 489, 653, 735) and then pass
   the **caller's** `cancel` token to the awaited task (lines 427, 496, 661, 742). The timeout is
   completely inert in those four overloads — a strictly worse variant of the #285 defect class.
2. **The two `TimeoutAfter` retry wrappers have dead catch clauses.** Methods 11 and 12
   (`TimeOutTask.cs:806-831` and `906-922`) wrap a **non-awaited** call to `task.TimeoutAfter(ms)`
   in `try/catch (TimeoutException)` at lines 818 and 914. `TimeoutAfter` returns a proxy `Task`
   that is *faulted* with `TimeoutException`; it does not throw synchronously. The catch clauses are
   therefore unreachable and `repeatAttempts` has no effect. The spec's third seeded test-strategy
   item ("re-audit the other two `catch (TimeoutException)` sites") maps to this finding — it should
   become its own issue, not a scope extension.
3. **Method 4's asymmetric handlers** (`TimeOutTask.cs:268` vs `:272`): its
   `TaskCanceledException` clause silently returns `default` without consuming `maxAttempts`, while
   its `TimeoutException` clause retries. Given that its awaited task *does* receive
   `combinedToken.Token` (line 266), `TaskCanceledException` is its real timeout signal and the
   non-retrying branch is likely inverted.
4. **`TimeOutTask_AdditionalTests.cs` is 527 lines**, exceeding the repository's 500-line ceiling
   (`.claude/rules/general-code-change.md`, "File Size Limit"). Pre-existing; not caused by this
   issue. It is a contributing reason not to add the new test there (§8).

---

## 8. Q6 — Home for the regression test: ONE file

**`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`**

Justification, from what each candidate currently covers:

| File | Lines | Covers | Verdict |
|---|---|---|---|
| `TimeOutTaskCoverageTests.cs` | 216 | Its own `[TestClass] TimeOutTaskCoverageTests`; `TimeoutAfter` short-circuits plus `Func<TResult>`, `Func<CancellationToken,Task<TResult>>`, `Func<T1,T2,TResult>` and the `<T1,T2>` void overload. **Does not test the `Func<T1,TResult>` overload at all.** | No |
| `TimeOutTask_Tests.cs` | ~150 | The `[TestClass] [DoNotParallelize]` root partial; `TimeoutAfter` basics + `Func<TResult>` / async `Func<TResult>`. **No `Func<T1,TResult>` tests.** | No |
| `TimeOutTask_InternalCoverageTests.cs` | ~90 | Only the `<T1,T2,T3>` void overload. | No |
| `TimeOutTask_AdditionalTests.cs` | **527** | Hosts three `FuncT1TResult` tests (171, 190, 219) including at-risk Test A. But it is **already 27 lines over the repo's 500-line ceiling**; adding to it deepens an existing policy breach. | No |
| **`TimeOutTask_OverloadCoverageTests.cs`** | **387** | Hosts at-risk Test B (`RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`, line 106) and `RunWithTimeout_FuncT1TResult_ShouldPropagateExceptions_WhenStrictModeIsEnabled` (line 125). | **YES** |

Decisive evidence: this file **owns the `_ShouldRetryAfterTaskCanceledException` naming family** for
cancellation-driven retries — `RunWithTimeout_Func_...` (line 12),
`RunWithTimeout_AsyncFunc_...` (58), `RunWithTimeout_FuncT1T2TResult_...` (186),
`RunWithTimeout_AsyncActionT1T2_...` (281), `RunWithTimeout_FuncT1T2T3TResult_...` (334). The
**`FuncT1TResult` member of that family is the only one missing** — which is precisely the defect.
Adding `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` here completes the
family in its established home.

Supporting facts:
- The file is `public partial class TimeOutTask_Tests` (line 9), a partial of the
  `[TestClass] [DoNotParallelize]` declaration at `TimeOutTask_Tests.cs:9-11`, so the new test
  inherits `[DoNotParallelize]`. That matters given the thread-pool-starvation flakiness history
  recorded for issue #253.
- Its usings (`System`, `System.Threading`, `System.Threading.Tasks`, `FluentAssertions`,
  `Microsoft.VisualStudio.TestTools.UnitTesting`, lines 1-5) already cover everything the sketch in
  §4.5 needs. **No new using directives.**
- At 387 lines it has room for the ~35-line test (and the optional second) while staying under the
  500-line ceiling.

---

## 9. Numeric Derivation Evidence

### N1-N4 — Catch-clause census of `UtilitiesCS/Threading/TimeOutTask.cs`

- **Complete Family:** every `catch` clause in `UtilitiesCS/Threading/TimeOutTask.cs`, of any
  exception type, in any of the file's 12 methods.
- **Exhaustive Search Scope:** the entire file, lines 1-993 (EOF at 993). No other file is in the
  family; the defect and all its siblings are declared in this single file.
- **Inclusion Rules:** any `catch` keyword introducing an exception-handler clause, whether typed,
  general, or filtered.
- **Exclusion Rules:** the string `catch` inside comments, identifiers, or string literals; `catch`
  clauses in any other file.
- **Primary Search Strategy:** full sequential human read of `TimeOutTask.cs` lines 1-993 (single
  `Read` of the whole file), recording each `catch` clause with its line number and enclosing
  method. This is structural, not pattern-matched, and therefore covers every overload and member
  in the family regardless of formatting.
- **Primary Member Set (line: type):**
  `65:TaskCanceled, 85:Exception, 130:TaskCanceled, 149:Exception, 200:Timeout, 220:Exception,
  268:TaskCanceled, 272:Timeout, 290:Exception, 351:TaskCanceled, 372:Exception, 429:TaskCanceled,
  450:Exception, 498:TaskCanceled, 519:Exception, 581:TaskCanceled, 603:Exception, 663:TaskCanceled,
  685:Exception, 744:TaskCanceled, 766:Exception, 818:Timeout, 914:Timeout`
- **Primary Count:** 23 total — **9** `TaskCanceledException`, **4** `TimeoutException`, **10**
  `System.Exception e`.
- **Cross-check Search Strategy or Query Expression:** an independent line-anchored regular
  expression over the raw file text, type-agnostic so it cannot be biased by the types I expected:
  `Grep(pattern="^\s*catch\s*\(", path="UtilitiesCS/Threading/TimeOutTask.cs", output_mode=content, -n=true)`.
  A **second, distinct** query then verified that the three named types account for the whole set:
  `Grep(pattern="catch \(TaskCanceledException\)|catch \(TimeoutException\)|catch \(System\.Exception e\)", path=<same>, output_mode=count)`.
- **Cross-check Member Set:** the anchored regex returned exactly the 23 lines
  `65, 85, 130, 149, 200, 220, 268, 272, 290, 351, 372, 429, 450, 498, 519, 581, 603, 663, 685, 744,
  766, 818, 914` with their types.
- **Cross-check Count:** 23 from the anchored regex; 23 from the type-union count query. Because the
  type-agnostic total (23) equals the three-type union total (23), no fourth catch type exists in
  the file — the family is proven exhaustively partitioned.
- **Member-set Comparison:** the normalized primary member set and the normalized cross-check member
  set are **identical** — same 23 line numbers, same type at every line, no member present in one
  and absent from the other. Counts agree. Assertions N1-N4 are released.

### N5 — Retrying `TaskCanceledException` clauses

- **Complete Family:** the 9 `TaskCanceledException` clauses established by N2.
- **Exhaustive Search Scope:** all 9 members, each read in full with its clause body.
- **Inclusion Rules:** a clause counts as "retrying" if its body contains an
  `if (attempt < maxAttempts) { … RunWithTimeout(…, attempt + 1 …) }` recursion.
- **Exclusion Rules:** clauses whose body only calls `ThrowIfCancellationRequested()`.
- **Primary Search Strategy:** direct read of each of the 9 clause bodies in the full-file read.
- **Primary Member Set (retrying):** `65, 130, 351, 429, 498, 581, 663, 744`. **Non-retrying:** `268`.
- **Primary Count:** 8 retrying, 1 non-retrying.
- **Cross-check Search Strategy or Query Expression:** an independent structural grep for the
  recursive-call sites, which must be one-per-retrying-clause:
  `Grep(pattern="Task\.Run\(|await task\(|await function\.|await copy\.", path=<file>, output_mode=content, -n=true)`,
  cross-referenced against the recursion lines it returns inside catch bodies:
  `71, 136, 206, 276, 357, 435, 504, 587, 669, 750`.
- **Cross-check Member Set:** of those 10 recursion sites, `206` and `276` sit inside
  `TimeoutException` clauses (200 and 272), leaving `71, 136, 357, 435, 504, 587, 669, 750` — the
  recursions belonging to `TaskCanceledException` clauses `65, 130, 351, 429, 498, 581, 663, 744`.
  Clause `268` has no recursion site between it and clause `272`.
- **Cross-check Count:** 8 retrying `TaskCanceledException` clauses; 1 non-retrying (`268`).
- **Member-set Comparison:** normalized sets are **identical** (`{65,130,351,429,498,581,663,744}`
  retrying; `{268}` not). Counts agree. Assertion N5 is released.

### N6 — Call sites of the `Func<T1, TResult>` overload

- **Complete Family:** every call expression in the repository that binds to
  `TimeOutTask.RunWithTimeout<T1, TResult>(this Func<T1, TResult>, T1, CancellationToken, int, int, bool[, …])`
  — the public wrapper at line 165 and its private impl at line 177.
- **Exhaustive Search Scope:** all `*.cs` files in the repository, both production and test, both
  extension-invocation syntax (`x.RunWithTimeout(...)`) and static-call syntax
  (`TimeOutTask.RunWithTimeout(...)`).
- **Inclusion Rules:** the first (receiver) argument must be a value or method group convertible to
  a **1-parameter, non-`Task`-returning** `Func<T1, TResult>`.
- **Exclusion Rules:** calls binding to any other overload; occurrences in `docs/**`,
  `test-output.txt`, or comments.
- **Primary Search Strategy or Query Expression:** a bare identifier grep,
  `Grep(pattern="RunWithTimeout", glob="!UtilitiesCS/Threading/TimeOutTask.cs", output_mode=content, -n=true)`,
  followed by reading the declaration of every receiver to determine its delegate arity and return
  type.
- **Primary Member Set:** production — `OneDriveDownloader.cs:139` (`Func<string, Stream>`);
  test — `TimeOutTask_AdditionalTests.cs:177, 205, 225`, `TimeOutTask_OverloadCoverageTests.cs:112, 132`.
- **Primary Count:** 1 production, 5 test. **This primary record was INCOMPLETE** — the bare
  identifier grep was truncated by pagination and omitted the `ConversationHelper` family.
- **Cross-check Search Strategy or Query Expression:** a distinct, invocation-anchored expression
  restricted to call syntax and run across all `*.cs` without exclusions,
  `Grep(pattern="\.RunWithTimeout\(", glob="*.cs", output_mode=content, -n=true)`. This form matches
  static-qualified calls (`TimeOutTask.RunWithTimeout(`) that the identifier grep's pagination
  dropped, and therefore covers the complete family including method-group receivers.
- **Cross-check Member Set:** production — `OneDriveDownloader.cs:139` (`Func<string, Stream>`) and
  **`ConversationHelper.Formatting.cs:80`** (method group `GetConversationTable`, declared
  `Table GetConversationTable(Conversation?)` at `ConversationHelper.Formatting.cs:111` — 1
  parameter, non-`Task` return, therefore this overload); test — `TimeOutTask_AdditionalTests.cs:177,
  205, 225`, `TimeOutTask_OverloadCoverageTests.cs:112, 132`. Receivers at
  `OneDriveDownloader.cs:48`, `StreamExtensions.cs:27`, `OlTableExtensions.TableAccess.cs:56`,
  `ConversationHelper.Formatting.cs:97`, `ConversationHelper.cs:252`, `ConversationHelper.cs:305`
  were each read and bind to other overloads.
- **Cross-check Count:** **2** production, 5 test.
- **Member-set Comparison:** the sets **DISAGREE**. The cross-check set is a strict superset of the
  primary set by exactly one member, `ConversationHelper.Formatting.cs:80`. Per the disagreement
  rule the primary count of 1 production site is **withdrawn**. The cross-check strategy is the
  exhaustive one (it is anchored on invocation syntax and was not truncated), and its extra member
  was independently confirmed by reading the callee declaration at
  `ConversationHelper.Formatting.cs:111`. **The released assertion is therefore the cross-check
  figure: 2 production call sites and 5 test call sites, enumerated in §5.2.** This correction is
  material — it adds the QuickFiler conversation-dataframe latency consequence documented in §5.4,
  and it introduces the method-group binding form analysed in §5.3.

---

## 10. Test strategy (no test code authored)

- **RED gate.** Add the single regression test of §4.5 to
  `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`. Confirm it **fails** against
  unmodified production code before the fix — the expected pre-fix failure is an escaping
  `TaskCanceledException` (because `strict: true`), not a wrong return value. Capture that failure
  output as the Bugfix-Workflow repro evidence.
- **Non-invalidation gate.** Re-run `TimeOutTask_AdditionalTests.cs:190` and
  `TimeOutTask_OverloadCoverageTests.cs:106` after the fix; both must pass **unmodified** (§3.4).
  Any edit to either test indicates the fix regressed to a replacement rather than an addition.
- **Regression scope.** Run the whole `UtilitiesCS.Test` assembly, plus `QuickFiler.Test` because
  `QfcItemControllerTests.cs:62` documents a dependency on the
  `TimeOutTask.RunWithTimeout -> GetConversationDfAsync` path affected by §5.4.
- **Policy conformance.** MSTest + FluentAssertions, no Moq needed. No `Task.Delay`,
  `Thread.Sleep`, wall-clock wait, temporary file, or external dependency. Deterministic by
  construction (already-cancelled token), independent, and isolated. Inherits `[DoNotParallelize]`
  from the partial class.
- **Coverage.** The changed lines are the catch filter and the factory plumbing; both are executed
  by the new test (filter via the `TaskCanceledException` path, factory via both attempts) and by
  the two existing tests (filter via the `TimeoutException` path). The optional exhaustion test
  additionally covers the `else { logger.Warn }` branch. Changed-line coverage does not decrease.

## 11. Intended footprint

Exactly three paths, per the scope constraint:

1. `UtilitiesCS/Threading/TimeOutTask.cs`
2. `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`
3. `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/**`

No sibling overload is modified. No opportunistic refactor is performed. The four out-of-scope
findings in §7 are reported for separate promotion, not fixed here.
