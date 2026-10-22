# 2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch (Spec)

- **Issue:** #285
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T22-10
- **Status:** Ready for Planning
- **Version:** 1.0
- **Work Mode:** full-bug — this document is the sole acceptance-criteria source. No `user-story.md` exists for this item.

> Citation basis: every line number in this document was re-derived against the worktree at HEAD
> `2b85134b`. The line numbers in `issue.md` and in the earlier scaffold of this file are off by one
> because `#nullable enable` was added as line 1 of the production file; do not reuse them.

## Context

`TimeOutTask.RunWithTimeout<T1, TResult>` for the `Func<T1, TResult>` receiver is declared in
`UtilitiesCS/Threading/TimeOutTask.cs` as a public wrapper at lines 165-175 and a private
implementation at lines 177-230. The private implementation arms a timeout by constructing
`new CancellationTokenSource(milliseconds)` (line 189), links it to the caller token (lines 190-193),
and awaits `Task.Run(() => function(arg1), combinedToken.Token)` (line 198).

Timer-driven cancellation of that `Task.Run` surfaces as `TaskCanceledException`. The specific
handler guarding the retry ladder is `catch (TimeoutException)` at line 200. The two types share no
base other than `Exception`, so the retry ladder at lines 202-218 is unreachable for a genuine
timer-driven timeout. Every structurally identical sibling in the same file catches
`TaskCanceledException` instead; there are nine such clauses, at lines 65, 130, 268, 351, 429, 498,
581, 663 and 744.

Environment:

- Platform: not environment-specific. This is a control-flow defect in C# / .NET Framework 4.8.1 code.
- Affected projects: `UtilitiesCS` (`TargetFrameworkVersion` `v4.8.1`, verified at UtilitiesCS/UtilitiesCS.csproj line 16) and its test project (`v4.8.1`, verified at UtilitiesCS.Test/UtilitiesCS.Test.csproj line 17).
- Reproduction vehicle: a unit test using the injectable timeout-source seam described under Proposed Fix. No live Outlook process, network, or filesystem access is required.

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The `maxAttempts` retry contract for this one overload does not operate for the case it exists to
serve. Both production call sites configure `maxAttempts` values that currently have no effect.

## Repro & Evidence

Steps to reproduce:

1. Call the `Func<T1, TResult>` overload of `RunWithTimeout` (public wrapper at line 165 of `UtilitiesCS/Threading/TimeOutTask.cs`) such that the internal `CancellationTokenSource`-linked `Task.Run` at line 198 is cancelled by the timeout source before the delegate completes.
2. Observe that the awaited task completes in the `Canceled` state and the awaiter throws `System.Threading.Tasks.TaskCanceledException`, not `System.TimeoutException`.
3. Observe that `catch (TimeoutException)` at line 200 does not match, so the retry ladder at lines 202-218 does not execute.

Expected behavior:

A timer-driven timeout in this overload is handled by the same retry ladder as its structurally
identical siblings — the `Func<TResult>` implementation at line 40, the `Func<T1, T2, TResult>`
implementation at line 327, and the `Func<T1, T2, T3, TResult>` implementation at line 556 — all of
which await `Task.Run(<sync lambda>, combinedToken.Token)` and catch `TaskCanceledException` at
lines 65, 351 and 581 respectively. `maxAttempts` and `strict` should then behave consistently
across the family.

Actual behavior (corrected — the earlier claim that the exception "propagates unhandled to the
caller" is false):

The `TaskCanceledException` is not unhandled. It reaches the general handler at line 220 of
`UtilitiesCS/Threading/TimeOutTask.cs`:

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

- With `strict: true`, the exception is caught at line 220, logged at ERROR level by `logger.Error(e)`, and then rethrown by the bare `throw;` at line 225. It does reach the caller, but only after being handled and mislabelled as a generic error. The `maxAttempts` retry ladder never runs.
- With `strict: false`, the exception is caught at line 220, logged at ERROR level, and swallowed. Control falls through to `return result!;` at line 229, where `result` is still the `default!` assigned at line 195. The caller silently receives `default(TResult)` — `null` for reference types — with zero retries and no exception. The `logger.Warn($"Task timed out after {attempt} attempts.")` diagnostic at line 217 also never fires, so no timeout is recorded anywhere in the log.

Both production call sites pass `strict: false` (see Root Cause Analysis). Therefore, in shipped
behavior today, a genuine timeout on this overload never propagates at all: it is recorded as a
generic ERROR and silently degrades to `null`, and the configured `maxAttempts` value of 3 at both
sites has no effect.

Evidence:

- Full research record, read in full and used as the authoritative basis for this spec: `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/research/2026-08-31T21-30-timeouttask-taskcanceled-retry-research.md`.
- Catch-clause census of `UtilitiesCS/Threading/TimeOutTask.cs` at HEAD, independently re-derived for this spec with a line-anchored regular expression: 23 clauses total — 9 `catch (TaskCanceledException)` (lines 65, 130, 268, 351, 429, 498, 581, 663, 744), 4 `catch (TimeoutException)` (lines 200, 272, 818, 914), and 10 `catch (System.Exception e)` (lines 85, 149, 220, 290, 372, 450, 519, 603, 685, 766).
- The defect was first recorded, and explicitly deferred, during issue #253. No other open GitHub issue references `TimeOutTask`.

## Scope & Non-Goals

In scope — this change may create or modify these paths and nothing else:

- `UtilitiesCS/Threading/TimeOutTask.cs`
- `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`
- files under `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`

This item runs concurrently with other items against the same `main`. Any modification outside the
three paths above is a scope violation.

Non-goals. Each item below is a separate follow-up issue and is explicitly not part of issue #285.
The paths in this section are deliberately written without backticks so that they are not harvested
into this change's footprint; do not reformat them.

1. The two `catch (TimeoutException)` clauses at lines 818 and 914 of UtilitiesCS/Threading/TimeOutTask.cs. Research established that these are dead code for a different reason: the `TimeoutAfter` retry wrappers at lines 806-831 and 906-922 wrap a non-awaited call to `task.TimeoutAfter(ms)`, which returns a proxy task that is *faulted* with `TimeoutException` rather than throwing synchronously, so the clauses are unreachable and `repeatAttempts` has no effect. This supersedes the issue body's "re-audit the other two sites" suggestion; it is not a scope extension here.
2. The implementations at lines 405, 475, 638 and 720 of UtilitiesCS/Threading/TimeOutTask.cs, which each construct `combinedToken` and then pass the caller's `cancel` token to the awaited task (lines 427, 496, 661, 742), leaving their timeout inert.
3. The apparently inverted handlers in the implementation at line 244 of UtilitiesCS/Threading/TimeOutTask.cs, where the `TaskCanceledException` clause at line 268 does not retry while the `TimeoutException` clause at line 272 does.
4. The pre-existing 527-line UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs, which exceeds the repository's 500-line file ceiling. It is not caused by this issue and is not corrected here; it is also the reason the new test is not added there.
5. Any change to a sibling `RunWithTimeout` overload that already behaves correctly.

## Root Cause Analysis

The private implementation at line 177 selects a handler type that the cancellation mechanism it
uses cannot produce.

- `Task.Run(delegate, token)` with an already-cancelled token does not invoke the delegate. The task transitions directly to `TaskStatus.Canceled` with no stored `OperationCanceledException`, and the awaiter throws `new TaskCanceledException(task)`.
- If the token is cancelled after the synchronous delegate has already started, the delegate has no access to `combinedToken.Token` — the combined token is created inside the method and never handed to `function` — so it runs to completion and the task ends `RanToCompletion` with no exception at all. This is documented in-repo for the sibling overload by the comment at UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs lines 1276-1285.
- A bare `OperationCanceledException` is unreachable here. The awaiter rethrows a stored `OperationCanceledException` only when the delegate itself threw one carrying the task's own token, which would require `function` to hold the combined token instance.

The correct handler type is therefore `TaskCanceledException`, matching all nine sibling
cancellation clauses. The handler must not be widened to `OperationCanceledException`: that would
diverge from every sibling and would route unrelated caller-thrown cancellations into the retry
ladder.

Production call sites of this overload, verified by an invocation-anchored search across all `*.cs`
files and by reading each receiver declaration. There are two:

- UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs line 139 — `factory.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)`, receiver `Func<string, Stream>`, `maxAttempts: 3`, `strict: false`.
- UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs line 80 — `TimeOutTask.RunWithTimeout(GetConversationTable, conversation, token, 1000, 3, false)`, a method-group binding to `Table GetConversationTable(Conversation?)` declared at line 111 of the same file, `timeoutMs: 1000`, `maxAttempts: 3`, `strict: false`.

## Proposed Fix

### Design summary

The fix is an addition, not a replacement, plus a determinism seam that makes the defect testable
without a wall-clock wait.

1. Widen the specific handler at line 200 of `UtilitiesCS/Threading/TimeOutTask.cs` so that it matches `TaskCanceledException` **in addition to** the `TimeoutException` it already matches. Recommended single-line edit:

   ```csharp
   // A timer-driven cancellation of Task.Run surfaces as TaskCanceledException, not
   // TimeoutException (issue #285). TimeoutException is retained because a wrapped
   // delegate may raise it directly, and existing callers and tests depend on that retry.
   catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
   ```

   The clause body at lines 202-218 is unchanged, including its leading `token.ThrowIfCancellationRequested()`.

2. Add a trailing optional parameter `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` to both the public wrapper at line 165 and the private implementation at line 177, replace `new CancellationTokenSource(milliseconds)` at line 189 with `(timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds)`, and thread the factory through the recursive retry call at lines 206-213. This mirrors the existing seam on the `Func<TResult>` siblings exactly: parameter declarations at lines 27 and 47, construction at lines 52-54, recursion at line 77.

3. Add one regression test, `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException`, to `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`.

### Why a replacement is rejected

Replacing `catch (TimeoutException)` with `catch (TaskCanceledException)` breaks two existing
passing tests, both of which fake a timeout by throwing `TimeoutException` directly from the
delegate and both of which pass `strict: true`:

- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs line 190, `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`, asserts `result == "result-42"` and `attempts == 2`.
- `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` line 106, `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`, asserts the result is `null`.

Under a replacement, the directly-thrown `TimeoutException` in each test would miss the specific
clause, fall to the general handler at line 220, and be rethrown by `throw;` because `strict: true`.
Both would fail. Under the additive filter above, both pass unmodified: the `TimeoutException` still
matches the specific clause and both control paths are byte-for-byte the ones they exercise today.
Neither test may be modified by this change.

### Why `catch (Exception ex) when (...)` rather than two clauses

`TaskCanceledException` and `TimeoutException` share no base other than `Exception`, so a single
typed clause cannot cover both. Two sibling clauses would duplicate the roughly fifteen-line retry
body, which the General Code Change Policy discourages. CA1031 is not enforced in this repository —
`catch (System.Exception e)` already appears ten times in this file and builds clean under
`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — and `ex` is consumed by the filter,
so no unused-variable diagnostic arises.

### Determinism seam: rationale and constraints

- A `TimeProvider` seam is not viable for this overload. Both projects target `v4.8.1`, so the .NET 8+ `CancellationTokenSource(TimeSpan, TimeProvider)` constructor does not exist.
- Only an already-cancelled injected source is deterministic. Advancing a fake clock after `Task.Run` has been queued is thread-pool-scheduling-dependent, and if the delegate has already started, cancelling produces no exception at all. That is the flakiness class that produced issue #253.
- The `?` annotation on the new parameter is mandatory. `UtilitiesCS/Threading/TimeOutTask.cs` carries `#nullable enable` at line 1, so omitting it yields CS8625, which is a build error under `/p:TreatWarningsAsErrors=true`.

### Files and members changed

- `UtilitiesCS/Threading/TimeOutTask.cs` — public wrapper `RunWithTimeout<T1, TResult>(this Func<T1, TResult>, ...)` at line 165; private implementation of the same name at line 177 (signature, timeout-source construction at line 189, catch clause at line 200, recursive call at lines 206-213). No other member is touched.
- `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` — one new `[TestMethod]` appended. The file is currently 387 lines and already declares every using directive the new test needs (`System`, `System.Threading`, `System.Threading.Tasks`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`).

### Boundaries and invariants preserved

- Existing behavior for a delegate-thrown `TimeoutException` is unchanged in both `strict` modes.
- The general handler at line 220 is unchanged.
- The `token.ThrowIfCancellationRequested()` guards at lines 187 and 202 are unchanged, so caller-token cancellation continues to abort rather than retry.
- No sibling overload is modified.

### Backward compatibility

Adding a trailing optional parameter to the public wrapper is source-compatible: all seven existing
call sites (2 production, 5 test) bind unchanged with the parameter defaulted to `null`. It is not
binary-compatible, but `UtilitiesCS` and every consumer build from `TaskMaster.sln` in the same pass,
so no stale-binary risk exists. The identical change already shipped for the `Func<TResult>` overload
with no caller edits.

No overload-resolution ambiguity is introduced with the `Func<T1, CancellationToken, Task<TResult>>`
overload at line 232: the two differ in the arity and return type of the `this` delegate, so no
delegate instance or method group can satisfy both. The method-group receiver at
UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs line 80 has one parameter
and a non-`Task` return type, so only the affected overload is a candidate.

### Configuration, data, logging

- No user-facing API change beyond the new optional parameter. No configuration key, schema, migration, or CLI flag is affected.
- Logging changes only in effect, not in code: after the fix, an exhausted retry ladder emits the existing `logger.Warn($"Task timed out after {attempt} attempts.")` at line 217 instead of the current generic `logger.Error(e)` at line 222. No new log statement is added.

## Assumptions, Constraints, Dependencies

- Assumptions: the worktree is based on `origin/main` at `2b85134b`; `dotnet tool restore` has been run once in this worktree before the first CSharpier invocation.
- Constraints: .NET Framework 4.8.1 on both projects; MSTest with FluentAssertions; no `Task.Delay`, `Thread.Sleep`, real wall-clock wait, temporary file, or external dependency in tests, per `.claude/rules/general-unit-test.md`; no file may exceed 500 lines.
- External dependencies: none added. Moq is not required for this change.

## Test Strategy

The issue body's seeded suggestions are superseded as follows.

- "Exercise a genuinely short `milliseconds` value" is **rejected**. `.claude/rules/general-unit-test.md` bans real wall-clock waits and names `Thread.Sleep` and `Task.Delay` as prohibited APIs in test code, and a short `milliseconds` value on .NET Framework 4.8.1 arms a `System.Threading.Timer` that races `Task.Run` non-deterministically. The regression test uses the injectable `timeoutSourceFactory` seam instead.
- "Change the catch clause to `catch (TaskCanceledException)`" is **rejected as written**, because a pure replacement fails the two existing tests named under Proposed Fix. The additive filter is used instead.
- "Re-audit the other two `catch (TimeoutException)` sites" is **moved to non-goals** as a separate follow-up issue.

Regression test to add — `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` in
`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`:

- A factory closure returns a pre-cancelled `CancellationTokenSource` on its first invocation (attempt 0) and a live, never-cancelled source on its second (attempt 1).
- The caller token is `CancellationToken.None`, so neither `ThrowIfCancellationRequested()` guard can abort the retry.
- `milliseconds: 30_000`, deliberately large and never armed, so the test cannot depend on the real timer.
- `maxAttempts: 1`, `strict: true`.
- Assertions: the result is `"result-42"`; the delegate ran exactly once (attempt 0 never dequeued it); the factory was invoked exactly twice (one source per attempt).
- Contains no `Task.Delay`, no `Thread.Sleep`, no wall-clock wait, and no temporary file. It is deterministic by construction, since an already-cancelled linked token fixes the outcome before any scheduling decision is made.
- It inherits `[DoNotParallelize]` from the `[TestClass]` root partial declaration of `TimeOutTask_Tests`.

Order of work, per the Bugfix Workflow in CLAUDE.md: add the failing regression test first and
capture its failure, then apply the minimal production fix, then re-run.

- Expected pre-fix failure mode: an escaping `TaskCanceledException` from the `await` in the test, because `strict: true` causes the general handler at line 220 to rethrow. It is not a wrong return value.
- Non-invalidation check: re-run the two at-risk tests after the fix. Both must pass with no edit to either test. An edit to either one indicates the fix regressed into a replacement.
- Regression scope: the whole `UtilitiesCS.Test` assembly, plus `QuickFiler.Test`, because QuickFiler.Test/QfcItemControllerTests.cs line 62 documents a dependency on the `RunWithTimeout` to `GetConversationDfAsync` path affected by the behavioral change recorded under Risks.
- Coverage: the changed lines are the catch filter and the factory plumbing. Both are executed by the new test (the filter via the `TaskCanceledException` path, the factory on both attempts) and the filter is additionally executed by the two existing tests via the `TimeoutException` path. Changed-line coverage does not decrease.
- Optional, not required for the failing-test gate: a second test covering the exhaustion branch (`else { logger.Warn(...) }`) by returning an always-cancelled source with `maxAttempts: 1` and asserting a `null` result.

Toolchain, run in this exact order and restarted from step 1 on any failure or auto-fix:

```text
dotnet tool run csharpier format .          (verify: dotnet tool run csharpier check .)
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe <test-assembly-paths> /EnableCodeCoverage
```

Use `/t:Rebuild`, never `/t:Build`: a warm `/t:Build` skips `CoreCompile` and the gate cannot fail.
Never add `/p:Nullable=enable`; it is absent from `.github/workflows/ci.yml` and fails wholesale on
this repository.

## Acceptance Criteria

- [x] A new MSTest method named `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` exists in `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and its failure output against unmodified production code is captured as evidence showing an escaping `TaskCanceledException`.
- [x] After the production fix, `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTaskCanceledException` passes, asserting result `"result-42"`, exactly one delegate invocation, and exactly two timeout-source factory invocations.
- [x] A text search of `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` returns zero matches for `Task.Delay`, `Thread.Sleep`, and `Thread.SpinWait`, and the new test passes `milliseconds: 30_000` with `CancellationToken.None` as the caller token.
- [x] `RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException` (UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs line 190) and `RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries` (`UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs` line 106) both pass, and `git diff` shows no change to either test method body.
- [x] In `UtilitiesCS/Threading/TimeOutTask.cs` after the fix, a line-anchored search for `catch` clauses returns exactly 9 `catch (TaskCanceledException)`, exactly 3 `catch (TimeoutException)` (the clauses at former lines 272, 818 and 914), 10 `catch (System.Exception e)`, and exactly one filtered clause, which is the one inside the private `Func<T1, TResult>` implementation.
- [x] A text search of `UtilitiesCS/Threading/TimeOutTask.cs` returns zero matches for `OperationCanceledException`, confirming the handler was not widened beyond `TaskCanceledException`.
- [x] Both the public wrapper and the private implementation of `RunWithTimeout<T1, TResult>(this Func<T1, TResult>, ...)` in `UtilitiesCS/Threading/TimeOutTask.cs` declare a trailing `Func<int, CancellationTokenSource>? timeoutSourceFactory = null` parameter with the `?` annotation, the wrapper forwards it, and the retry recursion inside the widened catch clause forwards it.
- [x] `dotnet tool run csharpier check .` reports no unformatted files.
- [x] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` completes with 0 errors and 0 new analyzer warnings.
- [x] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` completes with 0 errors, with no `/p:Nullable=enable` added to the command.
- [x] `vstest.console.exe` with `/EnableCodeCoverage` runs the full `UtilitiesCS.Test` assembly and the `QuickFiler.Test` assembly with 0 failures, and the resulting coverage report shows the modified catch clause and the modified timeout-source construction expression in the private `Func<T1, TResult>` implementation as covered.
- [x] `git status --porcelain` and the branch diff against the merge base list only `UtilitiesCS/Threading/TimeOutTask.cs`, `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs`, and paths under `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/`.

## Risks & Mitigations

- **The fix makes currently-dead retry logic live.** This is the intended outcome, not a regression, but it is a real behavioral change at both production call sites, each of which passes `maxAttempts: 3` and `strict: false`.
  - At UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs line 80 (`timeoutMs: 1000`, `maxAttempts: 3`) the timeout currently causes `GetDataFrameAsync` to short-circuit at lines 88-91 after about 1 second. After the fix it retries up to three more times, so worst-case QuickFiler conversation-dataframe latency in that case rises from roughly 1 second to roughly 4 seconds, while the failure rate falls.
  - Scope of that timeout, stated precisely because it is narrower than it first appears. The token passed to `Task.Run(() => function(arg1), combinedToken.Token)` is only observed before the work item is dequeued, and `function` is a `Func<T1, TResult>` with no token parameter, so it cannot observe cancellation once it is running. A `conversation.GetTable()` call that has already begun is therefore never cancelled by the timeout and raises no exception at all, consistent with the delegate-already-started case recorded above under Test Design. The retry ladder this change makes live is reached when the work item is still queued at the deadline — thread-pool saturation — not when the COM call itself is slow. Making the timeout cover a running delegate is a separate defect, recorded under Non-Goals as the inert-timeout implementations.
  - At UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs line 139 the same shape applies to the file-writer factory.
  - Mitigation: state this consequence explicitly in the PR description; run `QuickFiler.Test` as part of the regression scope; no feature flag is proposed, since a flag would preserve the defect.
- **Risk that the fix is implemented as a replacement rather than an addition**, silently breaking the two at-risk tests. Mitigation: the acceptance criterion requiring both at-risk tests to pass with an unchanged body.
- **Risk of nullable build failure** from omitting the `?` on the new parameter (CS8625 under `/p:TreatWarningsAsErrors=true`). Mitigation: the explicit annotation criterion plus the type-check toolchain gate. Note that archived guidance in the issue #181 evidence recommending the non-`?` form is stale; it predates this file's nullable migration under issue #369.
- **Risk of scope creep** into the four out-of-scope defects recorded under non-goals, given that they live in the same file. Mitigation: the footprint criterion plus the catch-clause census criterion, which together detect any edit to a sibling clause.

## Rollout & Follow-up

- Rollout: ordinary branch, PR, and merge. No migration, no configuration change, no staged rollout.
- Post-merge follow-up: promote each of the five non-goals items to its own issue through the standard promotion lifecycle. Items 1 through 3 are additional defects in the same file and should be triaged together; item 4 is a file-size policy breach.
- Links: issue #285 (https://github.com/drmoisan/TaskMaster/issues/285); research record at `docs/features/active/2026-07-09-timeouttask-runwithtimeout-exception-type-mismatch-285/research/2026-08-31T21-30-timeouttask-taskcanceled-retry-research.md`; prior deferral recorded under issue #253.
