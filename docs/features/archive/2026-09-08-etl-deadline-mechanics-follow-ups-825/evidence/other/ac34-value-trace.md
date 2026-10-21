# AC34 — Reviewer Trace of One Accepted Timeout Value

Timestamp: 2026-09-09T17-28

Take `timeoutMs = 750` supplied by a direct caller, or equivalently a `FakeTimeProvider` supplied by
`DfDeedle`. Every line citation below is re-derived against the post-change tree.

## Step 1 — Accept point

UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs line 40 declares
`int timeoutMs = 2000,` on `GetTableInViewAsync`, and line 42 declares the new trailing optional
parameter `TimeProvider? timeProvider = null`, positioned immediately after `timeoutSourceFactory`.
The method validates nothing about either value: no range check, no null check, no clock check. Both
travel onward exactly as supplied.

## Step 2 — Resolution point

Line 63 of the same file opens the resolved local
`Func<int, CancellationTokenSource> resolvedTimeoutSourceFactory =`, and line 67 carries its
clock-derived arm, `(timeProvider ?? TimeProvider.System).CreateCancellationTokenSource(` with a
`TimeSpan.FromMilliseconds(ms)` argument. The resolution is a null-coalesce over the supplied
factory: an explicitly supplied `timeoutSourceFactory` still wins, and only when none is supplied is
the deadline source derived from the caller's clock. That is what keeps the existing direct-caller
seam working unchanged, and it is proven by test at
GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider.

Because `timeProvider` defaults to null and null resolves to `TimeProvider.System`, production timing
is unchanged when no clock is supplied.

## Step 3 — Throw point

Line 74 of TableAccess.cs is `table = await TimeOutTask.RunWithTimeout(`, and line 80 passes the
resolved local rather than the raw parameter. Inside
UtilitiesCS/Threading/TimeOutTask.cs, the factory is invoked at line 52, in the statement
`using var timeoutSource = (timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds);`
that spans lines 52 to 54. That statement sits **outside** the try opened at line 61, so an exception
originating in the factory is not caught by that try. `RunWithTimeout` is an async method, so such an
exception is captured into the task it returns and surfaces at the awaiting caller — that is, at
TableAccess.cs line 74, inside the try opened at line 72, where the
`catch (TimeoutException)` at line 113 receives it.

## Step 4 — Retry point

Line 113 of TableAccess.cs opens the `catch (TimeoutException)`, and the recursion inside it passes
`timeoutMs` at line 125 and `timeProvider` as its trailing argument. The pre-change file passed the
literal `2000` at that position; the file now contains zero lines matching `^\s+2000,$`, down from
one. The caller's 750 is therefore no longer discarded, and both attempts are governed by the same
caller-visible value on the same caller-supplied clock. A test can assert the value the second
attempt used, and
GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 does: it recorded 2000 against
the pre-change file and records 750 now.

The `catch (TaskCanceledException)` at line 88 already propagated `timeoutMs` and
`timeoutSourceFactory`, and now propagates `timeProvider` as its fifth argument as well, with
`timeoutMs` at line 102.

## What is unchanged

`TimeOutTask.RunWithTimeout`'s signatures are unchanged. Its `strict` semantics are unchanged: the
`catch (System.Exception e)` at TimeOutTask.cs lines 85 to 92 is neither narrowed nor widened, and
`strict` remains `false` at the GetTableInViewAsync call site. Its retry behaviour is unchanged: the
internal recursion at lines 71 to 78, guarded by `attempt < maxAttempts` at line 69, is untouched.

`GetTableInViewAsync` still passes one maximum attempt and non-strict mode to `RunWithTimeout`, so
the two-deep retry layering — `RunWithTimeout`'s own internal attempt and `GetTableInViewAsync`'s
recursion — and the total attempt count are unchanged. `GetTableInViewAsync`'s public non-null return
contract and its `return table!` at line 138 are also unchanged.

Both `Console.WriteLine` diagnostics, the `counter` variable and both catch blocks survive
byte-identical; see evidence/qa-gates/ac26-ac27-boundary.md.
