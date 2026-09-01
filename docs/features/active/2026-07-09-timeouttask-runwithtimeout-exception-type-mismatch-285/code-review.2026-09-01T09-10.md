# Code Review — Issue #285 (`TimeOutTask.RunWithTimeout<T1, TResult>` exception-type mismatch)

- **Timestamp:** 2026-09-01T09-10
- **HEAD reviewed:** `46df4bf3779f7404bb4c91c7c400c19f5629bb4a`
- **Merge base:** `2b85134b42872e405602e6064e02dc9cda6c319b`
- **Production files changed:** 1 (`UtilitiesCS/Threading/TimeOutTask.cs`, +24/-6)
- **Test files changed:** 1 (`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, +40/-0)

## Verdict

**Approve. Blocking findings: 0.** Six non-blocking findings are recorded, of which one (CR-1) is
Major and merits a small follow-up edit.

The change is small, correctly targeted, and matches the shape of the nine structurally identical
siblings in the same file. The diagnosis in the spec is accurate and was independently confirmed
against the source. The regression test is genuinely deterministic and does not rely on any timing
assumption.

## Answers to the Four Explicit Review Questions

### Q1. Is `catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)` the right widening, and does excluding bare `OperationCanceledException` leave a hole?

**The narrowing is correct. There is no hole.** Analysis, from the post-change source at
`UtilitiesCS/Threading/TimeOutTask.cs` lines 197-245:

The awaited expression is `await Task.Run(() => function(arg1), combinedToken.Token)`. There are
exactly three ways a cancellation-family exception can leave that `await`:

1. **The combined token is already signalled when the work item would be dequeued.** `Task.Run` does not invoke the delegate; the task transitions directly to `TaskStatus.Canceled` with no stored `OperationCanceledException`, and the awaiter constructs and throws `new TaskCanceledException(task)`. This is the timer-driven timeout case, and it is exactly the case the widened filter now catches. It is also the mechanism the new test reproduces deterministically.
2. **The caller's own token is cancelled.** The combined token is linked to it, so the same path as (1) is taken and the awaiter again throws `TaskCanceledException`. The filter matches, and the first statement in the clause body at line 219 is `token.ThrowIfCancellationRequested();`, which immediately rethrows an `OperationCanceledException` carrying the caller's token. Caller cancellation therefore aborts rather than retries. That guard is what makes the widening safe, and it is why the clause body was correctly left byte-identical.
3. **`function` itself throws an `OperationCanceledException`.** The combined token is created inside the method and is never handed to `function` (the lambda closes over `arg1` only), so the thrown exception cannot carry the task's associated token. The task therefore ends `Faulted`, not `Canceled`, and the awaiter rethrows the same instance. A bare `OperationCanceledException` in this position is a caller-supplied cancellation from some unrelated token, and it must propagate, not be retried. It correctly misses the filter and reaches the general handler at line 238.

Widening to `OperationCanceledException` would change only case (3), and would change it for the
worse: it would route an unrelated cancellation into a retry ladder and would then be blocked at
line 219 only if the cancellation happened to be on the caller's own token. It would also diverge
from all nine siblings, which each catch `TaskCanceledException`. The spec's Root Cause Analysis
reaches the same conclusion and the executor implemented it faithfully.

One residual, recorded as CR-5: the filter does match a `TaskCanceledException` that `function`
itself throws for a foreign token, and will retry it. This is the identical exposure every sibling
clause already carries, so the change does not introduce it, and no production delegate in the two
call sites throws one.

Retaining `TimeoutException` in the filter is also correct, not defensive padding. Two existing
tests fake a timeout by throwing `TimeoutException` directly from the delegate and assert the retry
behaviour; a pure replacement would have broken both. The spec argues this explicitly and the
executor verified it (`evidence/regression-testing/p2-t5-at-risk-tests.md`, `Total tests: 2`,
`Passed: 2`).

The `System.Exception` spelling rather than bare `Exception` is required, not stylistic: line 9 of
the file is `using Microsoft.Office.Interop.Outlook;`, and that namespace declares a type named
`Exception`, so a bare `Exception` is CS0104-ambiguous. All ten pre-existing general handlers in the
file use the same spelling. The five-line comment explaining both decisions is proportionate and
comments the *why*, as the general policy requires.

### Q2. Is the `timeoutSourceFactory` seam an acceptable production API change?

**Acceptable, with one documentation gap (CR-1) and one design alternative worth recording (CR-2).**

On compatibility:

- **Source compatibility: preserved.** All seven existing call sites (2 production, 5 test) bind unchanged with the parameter defaulted to `null`. The seven were re-verified indirectly by the clean analyzer and type-check builds, both of which are `/t:Rebuild` over the whole solution and would have raised CS7036 or CS1501 on any unadapted call.
- **Overload resolution: unaffected.** The nearest neighbour is `RunWithTimeout<T1, TResult>(this Func<T1, CancellationToken, Task<TResult>>, ...)` at line 250. The two differ in both the arity and the return type of the `this` delegate, so no delegate instance or method group can satisfy both. The method-group receiver at `ConversationHelper.Formatting.cs` line 80 has one parameter and a non-`Task` return type, making the changed overload the only candidate.
- **Binary compatibility: not preserved.** Optional-parameter defaults are baked into the caller's IL at compile time, and the method's metadata signature changed, so a pre-compiled consumer built against the old signature would fail with `MissingMethodException`. This is correctly disclosed in the spec's Backward Compatibility section. It carries no practical risk here: `UtilitiesCS` and every consumer are projects of `TaskMaster.sln` and build in the same pass, and the identical change already shipped for the `Func<TResult>` overload with no caller edits.

On whether it should have been `internal`: `UtilitiesCS/Properties/AssemblyInfo.cs` line 19 grants
`[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`, so an `internal` seam overload was available
and would have kept the public surface unchanged. It would, however, have required a second
overload body or an `internal` forwarding method, duplicating the wrapper. Weighed against that, the
decisive argument is symmetry: the `Func<TResult>` sibling at lines 21-38 already exposes the
identical parameter publicly. Introducing a second, differently-scoped mechanism for the same seam in
the same file would be worse than the marginal surface widening. The chosen form is the right call.
Recorded as CR-2 for the record only.

### Q3. Is the new test deterministic and policy-compliant?

**Yes on both counts.** Checked line by line against `.claude/rules/general-unit-test.md`:

| Requirement | Observation |
| --- | --- |
| Banned APIs: `Thread.Sleep`, `Task.Delay`, real wall-clock waits, `setTimeout`, `Date.now()` | Zero occurrences anywhere in the 427-line file, confirmed by this reviewer's own search of the post-change source. `Thread.SpinWait` is also absent. |
| Determinism | The factory returns an already-cancelled `CancellationTokenSource` on attempt 0. `CreateLinkedTokenSource` therefore returns an already-cancelled source, and `Task.Run` with an already-cancelled token never dequeues the delegate. The outcome is fixed before any scheduling decision is made; there is no race window. |
| No reliance on the real timer | `milliseconds: 30_000` is passed but never used to arm a timer, because the injected factory ignores its argument. Even if the default path were somehow taken, 30 seconds exceeds the test's runtime by three orders of magnitude. Recorded runtime is 55 ms. |
| Arrange-Act-Assert | All three sections present with explicit comments, in order, with a single `await` in Act and three assertions in Assert. |
| Independence and isolation | Only local state. `[DoNotParallelize]` is inherited from the `[TestClass]` root partial at `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` lines 9-11, verified directly in this session. |
| No temporary files, no external services | Neither. |
| Clear failure messages | FluentAssertions throughout, per the C# unit test policy. |
| Test file location mirrors production | `UtilitiesCS.Test/Threading/` mirrors `UtilitiesCS/Threading/`. |
| Documented intent | Descriptive name plus a five-line comment that states the mechanism and why it is race-free. |

`Interlocked.Increment` on `factoryCalls` and `delegateCalls` is defensive rather than necessary —
the factory is invoked synchronously on the awaiting thread inside `RunWithTimeout`, and the
delegate runs on a pool thread but only once — but it is harmless and it makes the intent explicit.
No objection.

One interaction worth stating because it is not obvious and is nonetheless safe: the production code
takes ownership of the factory's return value via `using var timeoutSource = ...`, so `canceledSource`
is disposed at the end of attempt 0 and `liveSource` at the end of attempt 1, before the test's own
`using` declarations dispose them a second time at scope exit. `CancellationTokenSource.Dispose()`
is idempotent, and every `.Token` read happens before the corresponding disposal, so no
`ObjectDisposedException` is reachable. The test is correct as written. The underlying ownership
transfer is what CR-1 asks to be documented.

### Q4. Are the two at-risk pre-existing tests genuinely unmodified?

**Yes. Independently verified by this reviewer rather than accepted from `evidence/regression-testing/p2-t6-additive-only-diff.md`.**

- `git diff 2b85134b42872e405602e6064e02dc9cda6c319b...HEAD -- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` produced **no output**. The file, and therefore `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` at its line 190, is byte-identical to the merge base.
- `git diff 2b85134b...HEAD -- UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` piped through a count of lines beginning with a single `-` returned **0** deletion lines. The hunk header is `@@ -383,5 +383,45 @@`: a pure append of 40 lines immediately before the class-closing brace. `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries` at line 106 is untouched and retains its line number.
- `git status --porcelain` is empty, so no uncommitted edit to either file is pending.

The artifact's claims are accurate.

## Adjudication of the Behavioural Change at the Two Live Call Sites

**Assessment: safe at both sites. The documentation is substantively adequate but imprecise in one
respect, recorded as CR-3.**

### `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs` line 80

Read in full. The call is
`await TimeOutTask.RunWithTimeout(GetConversationTable, conversation, token, 1000, 3, false)`,
binding the method group `Table GetConversationTable(Conversation?)` declared at line 111 of the
same file. Line 88 then short-circuits on `conversationTableSnapshot is null`, which is the
`strict: false` degraded path and is unchanged by this fix.

- Retry arithmetic: `attempt < maxAttempts` with `maxAttempts: 3` and `attempt` starting at 0 permits attempts 0, 1, 2 and 3 — four invocations, so worst case rises from roughly 1 s to roughly 4 s. The commit message's "up to three more times" is arithmetically correct.
- Additional context the commit does not state: the immediately following call at line 97 uses the `Func<TResult>` overload, whose retry ladder was already live at the merge base with the same `1000`/`3` configuration. `GetDataFrameAsync`'s worst-case therefore moves from roughly 5 s to roughly 8 s, not from 1 s to 4 s. Both numbers are within what a user-initiated QuickFiler conversation load can absorb, and there is no UI-thread blocking involved (`GetDataFrameAsync` is awaited).
- Failure-rate direction: strictly better. The pre-fix behaviour returned `null` on the first timeout and logged it as a generic error, with `logger.Warn($"Task timed out after {attempt} attempts.")` never firing. Post-fix, a transient stall recovers and an exhausted ladder produces the intended warning.
- Regression evidence: `QuickFiler.Test` ran 1272 passed, 0 failed, unchanged from the baseline, and `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` line 62 documents a dependency on exactly this path.

### `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs` line 139

Read in full. `_writerTimeoutRunner` is
`(factory, destinationPath, cancel, timeoutMs) => factory.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)`,
with `factory` defaulting to `_getFileStreamWriter` at line 111, which constructs a `FileStream` in
`FileMode.Create`.

The obvious risk here is a leaked `FileStream` on a retried attempt. It is not reachable, for the
same reason the retry ladder fires at all: `Task.Run(delegate, token)` checks the token only before
invoking the delegate. Either the delegate is never invoked (task `Canceled`, retry fires, nothing
was allocated) or it is invoked and runs to completion (task `RanToCompletion`, no exception, no
retry, the stream is returned normally). There is no interleaving in which a `FileStream` is created
and then abandoned by the retry. A retry does re-enter `FileMode.Create` on the same path, which
truncates, but only in the case where no prior attempt produced a stream.

### CR-3 — one imprecision in the change documentation

Both the commit message body and the spec's Risks section describe the latency change as arising
from "a genuine timeout on the COM call `conversation.GetTable()`". That framing is inconsistent
with the spec's own Root Cause Analysis bullet 2, which correctly states that once the synchronous
delegate has started, the combined token is never handed to it, so cancelling produces no exception
at all and the task ends `RanToCompletion`.

The consequence is that a `GetTable()` call that has *already begun* and is stalling does not time
out and never has — before or after this fix. The only condition under which this overload's timeout
fires is that the work item was not dequeued from the thread pool within `timeoutMs`. The 1 s to 4 s
worst-case increase is therefore a thread-pool-starvation scenario, not a slow-COM-call scenario.

This does not change the verdict on the fix, which is correct for the case it addresses, and it does
not change the retry arithmetic. It does mean the PR description should not promise that a stalled
`GetTable()` will now be retried. It is also a useful observation for whoever picks up Non-Goals
item 2 (the four inert-timeout implementations), because it is a closely related class of inertness
that the current Non-Goals list does not name.

## Findings

| ID | Severity | Classification | Summary |
| --- | --- | --- | --- |
| CR-1 | Major | **Non-blocking** | The new public parameter transfers disposal ownership to the callee, undocumented |
| CR-2 | Minor | **Non-blocking** | The seam widens the public surface where an `internal` overload was available |
| CR-3 | Minor | **Non-blocking** | Commit message and spec Risks attribute the latency change to a slow COM call rather than to thread-pool starvation |
| CR-4 | Minor | **Non-blocking** | No test covers the retry-exhaustion arm reached via `TaskCanceledException` |
| CR-5 | Minor | **Non-blocking** | The filter retries a delegate-thrown `TaskCanceledException` carrying a foreign token |
| CR-6 | Minor | **Non-blocking** | The `RunWithTimeout` family now carries two divergent seam conventions across nine overloads |

### CR-1 — Undocumented disposal-ownership transfer on a public parameter

- **File / location:** `UtilitiesCS/Threading/TimeOutTask.cs` line 172 (public wrapper parameter) and lines 199-201 (`using var timeoutSource = (timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds);`).
- **Rule:** `CLAUDE.md` C#6.2 — "Public APIs should include XML documentation comments when behavior or contract is non-obvious"; and C#3.3 — "Document non-obvious side effects and failure modes."
- **Detail:** the `using var` declaration means the callee disposes whatever the caller's factory returns, once per attempt. A caller that supplies a shared, pooled, or long-lived `CancellationTokenSource` — a reasonable reading of "factory" that nothing in the signature contradicts — will have it disposed underneath them, and on the second attempt would then hit `ObjectDisposedException` from `.Token`. The `int` parameter's meaning (`milliseconds`) is likewise undocumented.
- **Verification:** read directly from the post-change source; there is no XML documentation comment on either the public wrapper or the private implementation.
- **Classification: Non-blocking.** The only caller today is the new test, where the behaviour is benign and correct. No production call site passes a factory. This is a latent trap for the next caller, not a present defect.
- **Recommended fix (one XML doc block, no behaviour change):** document on the public wrapper that `timeoutSourceFactory` receives the timeout in milliseconds, is invoked once per attempt, and that the returned `CancellationTokenSource` is disposed by this method. Worth doing on the `Func<TResult>` sibling at the same time, which carries the identical undocumented contract.

### CR-2 — Public surface widened where `internal` was available

- **File / location:** `UtilitiesCS/Threading/TimeOutTask.cs` line 172.
- **Rule:** `CLAUDE.md` C#5.2 — "Keep public surface area intentional and minimal. Prefer `internal` for non-public APIs."
- **Verification:** `UtilitiesCS/Properties/AssemblyInfo.cs` line 19 grants `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`, so an `internal` seam overload would have been consumable by the test.
- **Classification: Non-blocking, and this reviewer does not recommend changing it.** The `Func<TResult>` sibling already exposes the identical parameter publicly (line 27). Consistency across the family is worth more than the marginal reduction in public surface, and an `internal` variant would have required duplicating a wrapper body. Recorded so the trade-off is on the record rather than implicit.

### CR-3 — Latency-change attribution

Detailed above. **Non-blocking.** No source change required. Recommendation: adjust the PR
description so it does not claim a stalled `GetTable()` call is now retried, and consider adding the
already-started-delegate inertness to the Non-Goals follow-up set.

### CR-4 — Exhaustion arm for the `TaskCanceledException` path is untested

- **File / location:** `UtilitiesCS/Threading/TimeOutTask.cs` lines 233-236, the `else { logger.Warn($"Task timed out after {attempt} attempts."); }` arm, reached via `TaskCanceledException` for the first time as a result of this change.
- **Rule:** `.claude/rules/general-unit-test.md`, Scenario Completeness — negative flows and error-handling behaviour.
- **Detail:** the new test covers retry-then-succeed. The complementary case — a factory that always returns a cancelled source, so the ladder exhausts, the warning fires, and `strict: false` returns `default(TResult)` — has no test. The spec listed exactly this test as "Optional, not required for the failing-test gate", so the omission is declared rather than accidental.
- **Why the metric does not surface it:** the state machine `<RunWithTimeout>d__6<T1, TResult>` reads `branch-rate="1"` because the `else` arm is reached by the pre-existing `TimeoutException` test. Branch coverage is satisfied while the newly-live `TaskCanceledException` route into that arm is not exercised. This is the standard limitation of using a coverage percentage as a completeness proxy.
- **Classification: Non-blocking.** The uncovered composition is a two-line logging branch with no state change, its two constituent paths are each covered separately, and adding it is a five-line test. Recommended as a small follow-up, not as remediation.

### CR-5 — Filter retries a foreign-token `TaskCanceledException`

- **File / location:** `UtilitiesCS/Threading/TimeOutTask.cs` line 217.
- **Detail:** if `function` itself throws a `TaskCanceledException` for an unrelated token, the filter matches, `token.ThrowIfCancellationRequested()` does not fire (the caller's token is not cancelled), and the call is retried up to `maxAttempts`. Retrying a caller-originated cancellation is semantically wrong.
- **Classification: Non-blocking.** This exposure is identical in all nine pre-existing `catch (TaskCanceledException)` siblings, so the change neither introduces nor widens it, and neither of the two production delegates throws one. Distinguishing the two cases would require inspecting `TaskCanceledException.CancellationToken`, which is a family-wide change well outside this item.

### CR-6 — Two divergent seam conventions across the family

- **Detail:** after this change, two of the nine `RunWithTimeout` implementations carry a `timeoutSourceFactory` seam (`Func<TResult>` and `Func<T1, TResult>`) and seven do not. The seven remaining ones construct `new CancellationTokenSource(milliseconds)` inline and are correspondingly untestable without a wall-clock dependency.
- **Classification: Non-blocking, and correct for this change.** Adding the seam to seven untouched overloads would be exactly the scope creep the spec's census gate exists to detect. Recorded because it is the natural companion to the file-split follow-up: when `TimeOutTask.cs` is broken up to clear the 500-line breach, uniforming the seam across the family is the right time to do it.

## Positive Observations

These are recorded because they represent judgment calls that were made correctly and are worth
repeating, not as filler.

- **The additive filter rather than a replacement.** The spec identified two existing tests that a naive `catch (TaskCanceledException)` swap would break, explained the mechanism (`strict: true` plus the general handler's bare `throw;`), and the executor implemented the additive form and then proved both tests still pass with unchanged bodies. This is the single highest-value decision in the change.
- **The clause body was left byte-identical.** Including its leading `token.ThrowIfCancellationRequested()`, which is what keeps caller cancellation from being routed into a retry. A less careful implementation would have restructured the body while widening the filter.
- **The `e` versus `ex` variable name was chosen for a measured reason.** The plan records that the `e` spelling puts the clause at 97 columns and `ex` at exactly 100, CSharpier's default `printWidth`, so `ex` would have been split across two physical lines and defeated the single-line census assertions. This reviewer counted the line independently and confirms 97 characters including the 12-space indent.
- **The corrected `System.Exception` spelling.** The spec's own recommended edit used bare `Exception`, which does not compile in this file. The plan caught it before execution and documented the CS0104 reason in-code.
- **The evidence chain is unusually complete.** Every one of the twelve acceptance criteria is backed by a named artifact recording a command, an exit code, and a measured value against a Phase 0 baseline, and every figure this reviewer re-derived independently agreed.

## Recommendation

Approve and merge. Address CR-1 as a small documentation edit either in this PR or in the
`TimeOutTask.cs` split follow-up. File the file-size follow-up (PA-3 in the policy audit) as a GitHub
issue at PR time rather than post-merge.
