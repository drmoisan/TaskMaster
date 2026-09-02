---
name: 285-review-residuals
description: issue #285 TimeOutTask review closed PASS with 0 blocking; carries the Cobertura class-complexity-delta verification trick, the TimeOutTask family's already-started-delegate inertness fact, and the residual findings owed at PR time
metadata:
  type: project
---

Issue #285 (`TimeOutTask.RunWithTimeout<T1, TResult>` catch-clause widening) closed **PASS, 0
blocking, 12/12 AC** on 2026-09-01 at HEAD `46df4bf3`. Three reusable things came out of it.

**1. Cobertura class-level complexity delta proves new-branch coverage without line arithmetic.**
For an `async` member the compiler emits a state-machine `<class>` whose `name` ends `d__N<...>`.
Comparing baseline and post-change on that one element settles the modified-file tier in two greps:
`<RunWithTimeout>d__6<T1, TResult>` read `line-rate="1" branch-rate="1" complexity="4"` at the merge
base and `line-rate="1" branch-rate="1" complexity="10"` after. Complexity rose by 6 with both rates
pinned at 1.0, which proves every branch the change introduced is executed. This is faster and
stronger than the executor's per-line `hits` spot checks. Sibling of
[[project_package-counter-delta-corroborates-new-type-coverage]].

**2. The whole `TimeOutTask` family's timeout is inert once the delegate starts.** Every
`RunWithTimeout` overload awaits `Task.Run(<sync lambda>, combinedToken.Token)` and never hands the
combined token to the delegate. `Task.Run` checks its token only *before* invoking. So the retry
ladder fires only when the work item was not dequeued within `timeoutMs` (thread-pool starvation) —
never when an already-running COM call is stalling, which just runs to completion with no exception.
The #285 commit message and the spec's Risks section both say "a genuine timeout on the COM call
`conversation.GetTable()`", which contradicts the spec's own Root Cause Analysis bullet 2. Correct
any similar claim in the four Non-Goals follow-ups; this inertness class is not on the Non-Goals list
and should be added.

**Why the narrow `catch (... when (e is TaskCanceledException || e is TimeoutException))` is right
and `OperationCanceledException` must stay out:** the combined token is never given to `function`, so
a bare OCE can only originate from a caller's unrelated token and must propagate. Caller-token
cancellation already arrives as `TaskCanceledException` and is re-thrown by the clause body's leading
`token.ThrowIfCancellationRequested()`. Do not "fix" this by widening.

**3. Residuals owed at PR time, all non-blocking.**
- PA-3 / CR-6: `UtilitiesCS/Threading/TimeOutTask.cs` is 1011 lines (993 at merge base) against the 500 ceiling. File the split as a real GitHub issue **at PR time, not post-merge** — the spec already promises five post-merge Non-Goals promotions, and a sixth riding on prose is the leak point.
- CR-1: the seam's `using var timeoutSource = (factory ?? default)(ms)` means the callee **disposes** the caller's factory-returned CTS, undocumented on a public parameter. Same undocumented contract on the `Func<TResult>` sibling.
- CR-4: the retry-exhaustion arm reached via `TaskCanceledException` has no test; `branch-rate="1"` hides it because the `TimeoutException` path reaches the same `else`.
- PA-1: canonical `artifacts/csharp/coverage.xml` absent; figures came from the gitignored `coverage/p3-t7.cobertura.xml` and `coverage/p0-t10.cobertura.xml`, which will not survive into the PR and which **do** embed absolute host paths in every `filename` attribute — never commit them raw.

Raw merged repo-wide read 70.84% line / 46.98% branch; that denominator carries the test assembly at
97.85%, six third-party packages, and six first-party assemblies at 0-8% whose own test assemblies
were not in the run. The governing figure is the `UtilitiesCS` package at 89.21% / 83.04%. Same shape
as [[project_csharp-repowide-coverage-below-80]].

Related: [[project_powershell-coverage-mandatory-when-ps1-in-diff]] does not apply — this branch has
zero `.ps1` files. The agent is hard-isolated to its own worktree here, so the
[[project_review-worktree-differs-from-session-cwd-mirror-artifacts]] mirror was refused by the Write
tool and was not needed.
