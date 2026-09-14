# gettableinviewasync-returns-null-on-timeout (Spec)

- **Issue:** #838
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug

> **Acceptance-criteria authority.** Work mode for this item is `full-bug`. Under
> the acceptance-criteria-tracking skill that makes this document the sole
> authoritative acceptance-criteria source. The companion user story in this folder is narrative
> context only and carries no criteria.

> **Formatting notice — do not "fix" this.** An automated extractor derives this item's blast radius
> by harvesting backtick-delimited tokens from this document. The `## Write Set` section below is the
> only place in this document where a repository path appears inside backticks. Every other file
> reference — including comparison files, files deliberately left untouched, and line citations — is
> written in plain prose without backticks, on purpose. Backticks around method names, type names and
> exception type names are intentional and are not path claims. Do not add backticks to any other
> path reference.

## Context

`GetTableInViewAsync` returns null to its caller on the ordinary timeout path instead of throwing, so
a timed-out table read is indistinguishable from a successful empty read. The method is declared as
returning a non-null table type in a file that has opted into nullable analysis, and a null-forgiving
suppression on the return statement hides the contradiction from the compiler.

The single production consumer, GetEmailDataInViewAsync in DfDeedle.cs, declares its local as a
non-nullable table, passes it directly into AddQfcColumnsAsync and then dereferences it. A null
therefore travels one frame past the point of failure and surfaces as a NullReferenceException at the
frame-building boundary in QfcDatamodel.FrameBuilding.cs, with no folder name, no attempt count, and
a type that does not describe what happened. The cost is diagnostic: an operator seeing that failure
cannot tell a timed-out acquisition from a genuine null-reference defect, and a mail folder that
silently produces no rows looks identical to a mail folder that legitimately has none.

Environment:

- OS/version: Windows 11, Outlook VSTO host
- Target framework: net48 (C#). No Python component.
- Command/flags used: not applicable; this is a runtime path.
- Primary site: OlTableExtensions.TableAccess.cs, the `GetTableInViewAsync` method.

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Repro & Evidence

Steps to reproduce:

1. Call `GetTableInViewAsync` against a folder whose table acquisition cannot start inside the
   2000 ms deadline.
2. Observe that `TimeOutTask.RunWithTimeout` is invoked with a maximum-attempts argument of 1 and a
   strict argument of false.
3. Observe the value the method hands back to its caller.

Expected: a timeout surfaces as an exception, or as an explicit sentinel the caller is contractually
required to handle. Failure is not silently indistinguishable from success.

Actual: on the ordinary timeout path `RunWithTimeout` absorbs the `TaskCanceledException`, exhausts
its internal retry, and returns the default value of the result type without throwing. Neither catch
block in `GetTableInViewAsync` is entered, neither retry recursion runs, and the method returns a
null table through a null-forgiving suppression.

Supporting evidence, from the research record in this feature folder:

- The binding overload is the single-generic `RunWithTimeout` form in TimeOutTask.cs. The other
  six-parameter candidate cannot bind, because it would have to take the cancellation token as its
  first generic argument and then bind an int to a `CancellationToken` parameter.
- That overload forwards to a private overload with a zero-based attempt index and compares attempt
  against maximum-attempts with a strict less-than. A maximum-attempts argument of 1 therefore yields
  two attempts, indices 0 and 1. At index 1 the comparison fails, the helper logs a warning and
  returns the default value.
- The `strict` argument of false affects only the generic catch in that helper. The
  `TaskCanceledException` catch has no strict check at all and always absorbs once the retry budget
  is spent, so flipping `strict` to true would not fix the defect.
- In production the resolved deadline-source factory is a wrapper over the time provider's
  cancellation-token-source creation, which throws neither `TaskCanceledException` nor
  `TimeoutException`. Both catch clauses in `GetTableInViewAsync` and both of its recursive retries
  are therefore unreachable in production. The absorbed default is the only production route to a
  null return.

Logs / screenshots: none. The defect is the absence of any signal. That absence is itself the
evidence, and it is why the regression test named in the acceptance criteria has to construct the
condition through the deadline-source factory seam rather than wait for it.

## Scope & Non-Goals

In scope:

- Converting every null-producing exit of `GetTableInViewAsync` into an exception, and removing the
  null-forgiving suppression on its return statement.
- Adding a dedicated failure-contract test class for the converted paths, and registering it in the
  owning test project.
- Correcting the stale prose comment inside `GetTableInViewAsync` and the stale prose comment in
  GetTableInViewAsyncClockTests.cs that asserts the old null-on-timeout behaviour.

Out of scope / non-goals (paths below are deliberately written without backticks so they do not enter
the change footprint; do not add backticks to them):

- TimeOutTask.cs. Whether `RunWithTimeout` should itself stop returning a default value on exhaustion
  affects every caller of that helper across the solution and is an uncontained behaviour change. It
  is a legitimate follow-up and is explicitly not done here.
- DfDeedle.cs. Its consumer at the call site needs no change: it already declares a non-nullable local
  and dereferences it, which is correct once the producer honours its contract. No compensating null
  guard is to be added there.
- QfcDatamodel.FrameBuilding.cs. Its existing log-and-rethrow boundary already handles a
  `TimeoutException` correctly and its quiet `TaskCanceledException` path is the intended behaviour.
  Its catch list must not be widened.
- DfDeedle.QfcColumns.cs. It is the precedent this fix matches, not a file this fix edits.
- OlTableExtensions_Tests.cs, OlTableExtensionsTimeoutDiagnosticsTests.cs and DfDeedleEtlTimeoutTests.cs.
  These must pass unmodified. No assertion in them is to be weakened, deleted or relaxed.
- BannedSymbols.txt, .editorconfig and every policy document. This change complies with them; it does
  not amend them.
- The pre-existing divergence between the repository policy documents on the coverage floor. It is
  recorded below as a known constraint and is not resolved here.

## Root Cause Analysis

`GetTableInViewAsync` declares a non-null return type but has exits that produce null, and a
null-forgiving suppression on the return statement prevents nullable analysis from reporting the
contradiction. This is the same null-through-suppression class as issue 825 item 3, at a different
site. It was surfaced during preparation of feature 825 in the review-residuals-2026-09-08 epic,
recorded there as spec AC35, and deliberately left out of that feature's blast radius.

Four exits produce null, and all four converge on the single return statement:

1. The absorbed default. `RunWithTimeout` exhausts attempt 0 and attempt 1 and returns the default
   value without throwing, so the local is never assigned a table and no catch clause runs. This is
   the production-reachable path and is the defect.
2. The cancellation branch of the `TaskCanceledException` catch, taken when the outer token is
   already cancelled. Unreachable in production; reachable in test through the factory seam.
3. The retry-ceiling branch of the `TaskCanceledException` catch, taken when the counter has reached
   2. Unreachable in production; reachable in test through the factory seam.
4. The retry-ceiling branch of the `TimeoutException` catch, taken when the counter has reached 2.
   Unreachable in production; reachable in test through the factory seam.

A fifth exit is not a null path and is already correct: an `OperationCanceledException` raised by the
outer token's cancellation check inside `TimeOutTask` propagates out of `GetTableInViewAsync`
uncaught, because `OperationCanceledException` is the base of `TaskCanceledException` and therefore
does not match the narrower catch. That behaviour is pinned by an existing test and must survive
unchanged.

## Proposed Fix

### The invariant this fix establishes

`GetTableInViewAsync` returns a non-null table or it throws: a deadline that expires without
producing a table raises a `TimeoutException` naming the retry counter and the millisecond budget,
caller-initiated cancellation of the supplied token raises an `OperationCanceledException`, a current
view that is not a table view continues to raise an `InvalidOperationException`, and no null value
and no null-forgiving suppression can leave the method by any route.

### Trace of one accepted value from entry to the boundary that can report

The path chosen below is the production-reachable one and has no guard anywhere between the point the
call is accepted and the point the null is absorbed. That is what makes the fix load-bearing rather
than defensive.

1. **Accept point.** `GetTableInViewAsync` casts the explorer's current view to a table view and
   throws `InvalidOperationException` when the cast fails. That is the only validation performed
   before the deadline-governed work begins. It validates the view, and it does not validate — and
   cannot validate — that the acquisition will produce a table.
2. **Throw point.** Inside `TimeOutTask`, the awaited `Task.Run` for the acquisition delegate is
   cancelled before the work item is dequeued, so the await raises `TaskCanceledException`. This
   happens on attempt 0 and again on attempt 1.
3. **Current absorption point.** The `TaskCanceledException` catch inside `TimeOutTask` re-checks the
   outer token, finds it uncancelled, then finds the retry budget spent, logs a warning and falls
   through to a return of the default value. That location cannot report the failure to the caller:
   it holds no folder identity, its contract with every other caller of the shared helper is to
   absorb, and changing it would alter behaviour for callers outside this issue. The absorbed default
   flows back into the local, no catch clause in `GetTableInViewAsync` is entered, and the
   null-forgiving suppression on the return statement erases the null from analysis.
4. **Where the fix puts the report.** A guard immediately before the return statement in
   `GetTableInViewAsync`. That boundary can report: it holds the retry counter, the millisecond
   budget and the caller's token, and it is the last point at which the method still owns the
   failure. The guard calls `token.ThrowIfCancellationRequested()` first, so a cancelled outer token
   is reported as cancellation rather than mislabelled as a timeout, and otherwise throws a
   `TimeoutException`.

Why neither half suffices alone: the invariant without the trace is a statement an implementer can
satisfy by widening a consumer-side catch until the two ends merely agree, which reports nothing new;
the trace without the invariant documents one path and leaves the other three exits free to keep
returning null through the same suppression. Both are required, and acceptance criterion 12 pins the
delivered implementation to this trace.

### Design summary (what changes where)

The remedy is to throw. The signature of `GetTableInViewAsync` is unchanged: the parameter list and
the return type both stay byte-identical.

That byte-identity is load-bearing. The research record for this issue enumerates the complete family
of source locations that bind `GetTableInViewAsync` for execution and derives a total of nine under
two independent search strategies with identical member sets: three direct C# invocations and six
reflective bindings. The reflective bindings resolve through a method lookup that matches on parameter
types, so any change to the parameter list would break them at run time rather than at compile time.
Reflection is required rather than preferred in those tests: the return type involves an embedded
interop type, so a direct await from the test assembly is rejected with CS1769. Keeping the signature
identical means none of the nine binding sites needs any edit.

Changes, by path:

1. The absorbed-default path gains a guard before the return statement. The guard calls
   `token.ThrowIfCancellationRequested()` first, then throws a `TimeoutException`.
2. The cancellation branch of the `TaskCanceledException` catch rethrows the caught exception rather
   than assigning null. Because `TaskCanceledException` derives from `OperationCanceledException`,
   this preserves the existing cancellation contract and keeps the quiet consumer path intact. It
   requires no catch-variable binding.
3. The retry-ceiling branch of the `TaskCanceledException` catch throws a `TimeoutException` carrying
   the caught exception as `InnerException`. This requires binding the catch variable.
4. The retry-ceiling branch of the `TimeoutException` catch throws a `TimeoutException` carrying the
   caught exception as `InnerException`. This requires binding the catch variable.
5. The return statement returns the local directly, with the null-forgiving suppression deleted.
6. An outer-token cancellation continues to propagate as `OperationCanceledException`, uncaught. No
   catch for `OperationCanceledException` is to be added.

The three throw sites share a private failure-construction helper that returns the exception rather
than throwing it, which keeps definite-assignment and nullable flow analysis correct without a
does-not-return attribute; that attribute is not available in the net48 base class library.

The exception type matches the sibling step on the same pipeline, AddQfcColumnsAsync in
DfDeedle.QfcColumns.cs, which already returns quietly on outer-token cancellation and throws a
`TimeoutException` naming the folder and the budget on exhausted retry. That method is production
code on the main branch, is pinned by an existing test, and is invoked one statement after the call
site being fixed. Matching it makes two adjacent steps of the same pipeline report the same failure
mode the same way.

### Boundaries and invariants to preserve

- The parameter list and return type of `GetTableInViewAsync` stay byte-identical.
- `OperationCanceledException` continues to escape uncaught on outer-token cancellation.
- The `InvalidOperationException` raised when the current view is not a table view is unchanged.
- The consumer-side catch list in QfcDatamodel.FrameBuilding.cs must not be widened. Its
  `TaskCanceledException` clause is the intended quiet path and its generic clause is the intended
  loud path; a `TimeoutException` correctly falls to the latter, so the exception shape at that
  boundary is unchanged and no caller-side control flow changes.
- No null guard is to be added in DfDeedle.cs. Adding one would replace a single suppression in the
  producer with two in the consumer.

### Dependencies or blocked work

None. No other issue blocks this one, and this change does not block others. The question of whether
`RunWithTimeout` should stop returning a default value on exhaustion is recorded as a follow-up.

### Implementation strategy (what changes, not sequencing)

Files and modules to change: see the `## Write Set` section, which is authoritative.

Functions and classes impacted: `GetTableInViewAsync` and a new private failure-construction helper
in the same partial type. No public surface other than the documented exception contract changes.

The failure-construction helper goes in a new partial-class file rather than in the existing file.
The existing production file stands at 452 lines against a 500-line ceiling, leaving a 48-line budget
that the guard, the two catch-variable bindings, the rethrow and the new XML documentation are
expected to consume. Splitting the helper out removes the risk of breaching the ceiling after
formatting. The C# projects in this repository are not SDK-style, so adding a .cs file requires an
explicit Compile item in the owning project file; that is why both project-file edits appear in the
Write Set.

Data flow and validation changes: the local table variable is no longer allowed to reach the return
statement holding null. Every route that previously assigned null now raises.

Error handling and logging updates: existing warning logs on the timeout paths are retained. The
`TimeoutException` message names the retry counter and the millisecond budget. XML documentation is
added to `GetTableInViewAsync` stating the three-part failure contract. The comment inside the method
that describes the null-on-timeout condition as a pre-existing latent condition is corrected, because
it will no longer be true. The prose comment in GetTableInViewAsyncClockTests.cs that states
advancing past the timeout would make the returned table null is corrected for the same reason; no
assertion in that file depends on it.

Rollback and feature-flag considerations: none. The change is a small, self-contained behaviour
correction with no configuration surface. Rollback is a revert of the commit.

### Technical specifications (interfaces and contracts)

Inputs and outputs: unchanged. The method takes an explorer, a cancellation token, a retry counter, an
optional millisecond budget defaulting to 2000, an optional deadline-source factory and an optional
time provider, and returns a task of table.

Failure contract after the fix:

- `TimeoutException` — the acquisition deadline was exhausted. Message names the retry counter and
  the millisecond budget. Carries the originating exception as `InnerException` when one was caught.
- `OperationCanceledException` — the caller's token was cancelled. Includes its `TaskCanceledException`
  subtype when that is what was caught.
- `InvalidOperationException` — the explorer's current view is not a table view.

Required configuration keys and defaults: none added.

Backward-compatibility expectations: this is a deliberate, documented behaviour change at the failure
boundary only. The success path is untouched. The sole production consumer already treats a null
result as a defect by dereferencing it, so no working caller behaviour regresses; the change converts
a NullReferenceException at a later frame into a `TimeoutException` at the correct frame.

Performance constraints: none introduced. The added guard is a null comparison and a token check on a
path that is already terminal. No additional attempt, delay or allocation is added to the success
path.

## Assumptions, Constraints, Dependencies

Assumptions:

- The research record in this feature folder is authoritative for the control-flow analysis, the
  enumeration of binding sites and the determinism technique, and its line citations were re-derived
  against the current worktree.
- The deadline-source factory seam remains the supported injection point for tests.

Constraints:

- Target framework is net48. There is no IsExternalInit polyfill, so init accessors, `record` and
  `record struct` fail with CS0518. This is the reason a result-struct sentinel was rejected rather
  than merely disfavoured.
- The C# projects are not SDK-style. Adding a .cs file requires an explicit Compile item in the
  owning project file.
- Nullable enforcement is per-file opt-in through a nullable directive plus warnings-as-errors. The
  production file being modified already carries the directive. A solution-wide nullable property must
  never be proposed; CI deliberately omits it and forcing it conscripts files that never opted in.
- Tests use MSTest, Moq and FluentAssertions. No temporary files, no wall-clock waits, no sleeps, and
  no retries to mask timing.
- The banned-symbol list forbids both timed cancellation-token-source constructors and both
  cancel-after overloads. The deterministic technique the research settled on uses the parameterless
  constructor followed by an explicit cancel, returning a fresh source on every factory invocation.
  A fresh source per invocation is mandatory: the helper holds each source in a using declaration and
  disposes it at the end of each attempt, so reusing one instance would make the retry read a token on
  a disposed source and raise `ObjectDisposedException`, which is a different path entirely.
- Evidence convention, effective now: commit projections only. The change must not add any test-result
  or raw coverage XML file to the repository. Numeric figures are recorded inside Markdown evidence
  artifacts and the raw tool output is discarded.
- Evidence artifacts belong only in the feature folder's evidence subdirectories named baseline,
  regression-testing and qa-gates, whose full paths appear in the Write Set. No other location is
  permitted.
- **Known policy divergence, not resolved here.** CLAUDE.md states a repository floor of 80 percent
  and 90 percent for new code. The general unit-test rule file states 85 percent line and 75 percent
  branch. The divergence is pre-existing. The CLAUDE.md figures apply to this issue because the
  policy-compliance order places CLAUDE.md first. Resolving the divergence is out of scope.

External dependencies: none added. The tests use packages already referenced by the test project.

## Data / API / Config Impact

- User-facing or API changes: the documented failure contract of `GetTableInViewAsync` changes from
  "may return null" to "throws". No signature, no configuration and no user-visible setting changes.
- Data or migration considerations: none.
- Logging and telemetry updates: existing timeout warnings retained; the new `TimeoutException`
  message carries the retry counter and the millisecond budget, which the previous silent path did not
  surface anywhere.
- Compatibility notes: no CLI flags, no config schema, no versioning impact.

## Test Strategy

Unit coverage areas: the `RunWithTimeout` absorbed-default behaviour under a strict argument of false
with a maximum-attempts argument of 1, and each converted exit of `GetTableInViewAsync`.

Regression tests to add, all in the new failure-contract test class named in the Write Set:

1. `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException` — the regression test
   for the defect. It supplies a deadline-source factory that returns a fresh, already-cancelled
   source on every invocation, and a counting table-read delegate. The awaited invocation must throw
   `TimeoutException`. The same test asserts the factory invocation count is exactly two and the
   table-read delegate invocation count is exactly zero. Those two counts are the empirical proof that
   both attempts were internal to `RunWithTimeout` and that neither of the method's own recursive
   retries ran. Before the fix this test fails by returning null; after the fix it passes.
2. `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation`
   — the factory cancels the test-owned outer source and then throws `TaskCanceledException`. The
   awaited invocation must throw `OperationCanceledException`. Ordering matters: the helper's own
   token check runs before the factory is invoked, so the token must still be uncancelled at method
   entry and be cancelled by the factory body.
3. `GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException` — counter
   argument of 2, factory always throws `TaskCanceledException`. The awaited invocation must throw
   `TimeoutException` whose message names the retry counter and the millisecond budget.
4. `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner` —
   counter argument of 2, factory always throws `TimeoutException`. The awaited invocation must throw
   `TimeoutException` whose `InnerException` is the same instance the factory threw, which
   distinguishes the new wrapper from a bare rethrow.

All four invoke through a reflective helper of the same shape already used in
GetTableInViewAsyncClockTests.cs, because the CS1769 embedded-interop constraint applies equally.

Edge cases and negative scenarios covered: deadline exhaustion with no delegate execution; outer-token
cancellation racing the factory; retry ceiling reached under each of the two caught exception types;
inner-exception preservation.

Error handling and logging verification: the exception type and message content are asserted directly
by the tests above. Log output is not asserted, because the existing warning calls are unchanged.

Tests that must pass unmodified: the existing cancellation-contract test
`GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` and every other existing test
in OlTableExtensions_Tests.cs, OlTableExtensionsTimeoutDiagnosticsTests.cs,
GetTableInViewAsyncClockTests.cs and DfDeedleEtlTimeoutTests.cs. No existing test reaches any of the
four null exits, which is precisely why the defect survived, so no existing assertion requires a
change. The only permitted edit to an existing test file is the stale prose comment correction in
GetTableInViewAsyncClockTests.cs.

Determinism: no wall-clock wait, no timer advance, no gate and no thread-pool dependency is used by
the new tests. The already-cancelled-source technique makes the work item cancel before dispatch, so
the outcome does not depend on host scheduling. The new class does not require the do-not-parallelize
attribute, because it never dispatches a work item and never blocks a gate.

Coverage impact and targets: the four new tests exercise every converted exit plus the preserved
cancellation path.

Toolchain commands to run, in this order:

1. `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. vstest.console.exe over the built test assemblies with the /EnableCodeCoverage flag, per the
   CLAUDE.md C# toolchain step 4. The assembly paths are supplied by the executing agent and are
   written here as prose rather than as a bracketed placeholder.

Manual validation steps: none required. Every criterion below is decided by a named test or a named
command.

## Write Set

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.Failures.cs`
`UtilitiesCS/UtilitiesCS.csproj`
`UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs`
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`
`UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/issue.md`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/user-story.md`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/research/2026-09-12T15-30-gettableinviewasync-null-contract-research.md`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/plan.2026-09-12T16-09.md`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/baseline/`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/regression-testing/`
`docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/evidence/qa-gates/`

## Acceptance Criteria

- [x] 1. The absorbed-default path throws `TimeoutException` instead of returning null. Verified by
  the new test `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException` passing in
  the vstest run, where it asserts the awaited invocation throws `TimeoutException`.
- [x] 2. That same test proves the failure originated in the shared helper's internal retry and not
  in the method's own recursion, by asserting that the deadline-source factory was invoked exactly
  twice and that the table-read delegate was invoked exactly zero times. Verified by both assertions
  passing inside `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException`.
- [x] 3. The retry-ceiling branch of the `TaskCanceledException` catch throws `TimeoutException`.
  Verified by the new test
  `GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException` passing, where it
  asserts the thrown `TimeoutException` message names the retry counter and the millisecond budget.
- [x] 4. The retry-ceiling branch of the `TimeoutException` catch throws `TimeoutException` carrying
  the caught exception as `InnerException`. Verified by the new test
  `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner`
  passing, where it asserts `InnerException` is the same instance the factory threw.
- [x] 5. Outer-token cancellation still surfaces as `OperationCanceledException`, and the cancellation
  branch of the `TaskCanceledException` catch rethrows rather than returning null. Verified by two
  passing tests: the existing
  `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException`, which must pass with its
  containing file absent from the change diff, and the new
  `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation`.
- [x] 6. No catch clause for `OperationCanceledException` was added to `GetTableInViewAsync`, and no
  consumer-side catch list was widened. Verified by a grep of the two production files in the Write
  Set returning zero occurrences of a catch clause naming `OperationCanceledException`, together with
  the absence of QfcDatamodel.FrameBuilding.cs and DfDeedle.cs from the change diff produced by
  `git diff --name-only` against the merge base.
- [x] 7. The null-forgiving operator is gone from the return statement of `GetTableInViewAsync`, and
  the method compiles clean under the per-file nullable opt-in with warnings treated as errors.
  Verified by a grep of OlTableExtensions.TableAccess.cs returning zero occurrences of a
  null-forgiving suppression on a return statement, and by
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  completing with exit code 0 and zero CS86 nullable diagnostics.
- [x] 8. No existing test assertion was weakened, deleted or relaxed. Verified by
  `git diff --name-only` against the merge base showing OlTableExtensions_Tests.cs,
  OlTableExtensionsTimeoutDiagnosticsTests.cs and DfDeedleEtlTimeoutTests.cs absent from the diff
  entirely, and by the diff for GetTableInViewAsyncClockTests.cs touching comment lines only, with
  zero changed lines containing an assertion call.
- [x] 9. Both stale prose comments are corrected: the comment inside `GetTableInViewAsync` that
  describes the null-on-timeout condition as a pre-existing latent condition, and the comment in
  GetTableInViewAsyncClockTests.cs stating that advancing past the timeout would make the returned
  table null. Verified by a grep of those two files returning zero occurrences of the phrase
  "pre-existing latent" and zero occurrences of the phrase "making the returned table null".
- [x] 10. Each C# file in the Write Set is at or under 500 lines. Verified by a line count of
  OlTableExtensions.TableAccess.cs, the new failures partial file, the new failure-contract test file
  and GetTableInViewAsyncClockTests.cs, each returning a value no greater than 500. This criterion
  does not apply to the project files or to the Markdown documents in the Write Set, which the
  general code-change policy exempts.
- [x] 11. Both new .cs files are registered for compilation in their owning non-SDK-style project
  files. Verified by a grep of UtilitiesCS.csproj returning a Compile item for the new failures
  partial file, a grep of UtilitiesCS.Test.csproj returning a Compile item for the new
  failure-contract test file, and by the analyzer build completing with exit code 0.
- [x] 12. The delivered implementation matches the four-step trace in the Proposed Fix section: the
  view-cast guard is the only pre-deadline validation and is unchanged; the deadline expiry is raised
  inside the shared helper; the absorbed default no longer reaches the return statement; and the new
  report is emitted by a guard immediately before the return statement that calls
  `token.ThrowIfCancellationRequested()` before it throws `TimeoutException`. Verified by the new
  test `GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout`
  passing, which reaches that guard with a null local and an already-cancelled outer token and so
  observes an `OperationCanceledException` only when the cancellation check precedes the timeout
  throw, together with criterion 1's test proving the absorbed default now raises.

  **Amended 2026-09-12 by the atomic planner. Reason recorded here rather than in a separate
  document.** This criterion previously named
  `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation`
  as the verifier of the ordering claim. That test drives the cancellation branch of the
  `TaskCanceledException` catch, which under the remedy rethrows, so control returns before the
  final guard is ever reached and the test passes under either ordering inside that guard. It could
  not decide the property this criterion asserts. The replacement test reaches the guard through the
  shared helper's generic absorb path: the table-read delegate cancels the test-owned outer source
  and then throws an exception that is neither `TaskCanceledException` nor `TimeoutException`, which
  the shared helper logs and absorbs under its strict argument of false, so the helper returns its
  default value while the outer token is cancelled. The same construction also makes the guard's
  `token.ThrowIfCancellationRequested()` an executed line, so criterion 15's changed-and-added-line
  figure contains no unreachable statement. For this criterion only, this amendment supersedes the
  four-item enumeration under Test Strategy: the replacement test is a fifth test in the same new
  failure-contract test class, and criteria 10 and 13 apply to it unchanged.
- [x] 13. The new tests introduce no banned-symbol call site and no non-deterministic timing.
  Verified by a grep of the new failure-contract test file returning zero occurrences of
  `CancelAfter`, zero occurrences of a `CancellationTokenSource` constructor invoked with an argument,
  zero occurrences of `Thread.Sleep`, and zero occurrences of `Task.Delay`.
- [x] 14. The full C# toolchain passes in order in one clean pass with no step failing and no step
  auto-modifying a file: `dotnet tool run csharpier check .` reporting zero files needing formatting,
  then the analyzer msbuild command, then the nullable msbuild command, then the vstest run, each
  returning exit code 0.
- [x] 15. Coverage: no regression on changed lines, and the changed and added code in
  OlTableExtensions.TableAccess.cs and the new failures partial file reaches at least 90 percent line
  coverage, per the CLAUDE.md figure selected by the policy-compliance order. Verified by the vstest
  run with the code-coverage flag, with the resulting figures transcribed into a Markdown artifact in
  the feature folder's qa-gates evidence subdirectory. The repository-wide figure is recorded in the
  same artifact as a report-only observation against the testable denominator, and this criterion does
  not gate on it; no merge-base coverage baseline for this feature existed at authoring time, so a
  repository-wide blocking threshold cannot be shown satisfiable here.
- [x] 16. No raw test-result or raw coverage XML artifact was committed. Verified by
  `git diff --name-only` against the merge base returning zero added paths with a .trx extension and
  zero added paths with a .cobertura.xml extension, and by every evidence artifact added by this
  change residing in one of the three evidence subdirectories listed in the Write Set.

## Risks & Mitigations

Technical and operational risks:

- **A caller somewhere relied on the null return.** Mitigated by the enumeration in the research
  record, which derived the complete family of binding sites under two independent search strategies
  with identical member sets. The single production consumer already dereferences the result without a
  guard, so it cannot have relied on null.
- **The production file breaches its 500-line ceiling after formatting.** Mitigated by moving the
  failure-construction helper into a separate partial file up front rather than reactively, and by
  criterion 10.
- **An implementer "makes it safe" by widening a consumer-side catch.** Mitigated by the explicit
  non-goals for QfcDatamodel.FrameBuilding.cs and DfDeedle.cs and by criterion 6, which fails if
  either file appears in the diff.
- **A flaky new test from thread-pool timing.** Mitigated by the already-cancelled-source technique,
  which never dispatches a work item, and by criterion 13.

Mitigations and rollbacks: the change is confined to one method plus a new partial file and a new
test class. Rollback is a revert of the commit; nothing persists state and no configuration changes.

## Rollout & Follow-up

Release and rollout steps: standard branch, toolchain pass, PR and merge. No staged rollout, no flag,
no migration.

Post-fix monitoring and clean-up: after merge, a table-acquisition failure appears at the
frame-building boundary as a `TimeoutException` naming the retry counter and the budget rather than as
a NullReferenceException. That is the signal to look for when confirming the fix in the host.

Follow-up candidates, recorded and not actioned here:

- Whether `TimeOutTask.RunWithTimeout` should stop returning a default value on exhaustion, which
  affects every caller of that shared helper.
- The pre-existing divergence between CLAUDE.md and the general unit-test rule file on the coverage
  floor.

Links: issue #838 at https://github.com/drmoisan/TaskMaster/issues/838; the research record and the
plan in this feature folder; the user story in this feature folder, which is context only.
