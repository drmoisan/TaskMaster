# Code Review — Issue #798

- Timestamp: 2026-09-07T19-05
- Issue: #798
- Branch: `bug/quickfiler-crash-column-add-timeout-swallowed-keynotfound-798`
- Head commit: `39433790e27585df3c4c2fab73fdd3227763e402`
- Base commit: `c431dc3297e864041d829e8d79b348960b8d8019`
- Blocking findings: 0
- Non-blocking findings: 11

## Scope and Method

The review covers the full branch diff against the base commit: sixteen code paths across three
production assemblies and three test assemblies, plus the committed feature folder.

Method. The supplied diff was read in full. Claims that were load-bearing for a verdict were
re-derived against the worktree rather than accepted from the executor's artifacts:

- The repository-wide branch-coverage figure was parsed from the `<coverage>` root element of both
  committed Cobertura documents. No executor artifact reports it.
- The `RibbonCommandBoundary.cs` per-file coverage figure was re-derived from the two class-level
  counter elements and reconciled to 55 of 61, and the identity of each uncovered line was read from
  the per-line `hits` attributes.
- The line counts of three files were confirmed by reading the file tail.
- The `TimeoutAfter(Task, int, TimeProvider?)` overload the fix re-applies was read in full to
  establish that repeated application to a single task instance is sound.
- The propagation path of the new exception was traced through `GetEmailDataInViewAsync` to confirm
  no enclosing catch absorbs it.
- The presence and provenance of every `[ExcludeFromCodeCoverage]` attribute in the write set was
  established by search rather than inferred from the coverage document.

## Assessment of the Change

The fix is well-targeted and the design reasoning is sound. Four observations support that judgement.

**The timeout redesign is correct and is the right shape.** The previous implementation recursed on
`TimeoutException`, and because `TimeoutAfter` neither cancels nor awaits the wrapped task, each
recursion started a fresh `Task.Run` against the same COM `Table`. Up to three column-add calls could
therefore run concurrently, and after the third the `catch` body did nothing and the method completed
successfully with no failure channel at all. The replacement starts the work exactly once and
re-applies the deadline to that same instance:

```csharp
var work = Task.Run(() => adder(table, folder), token);

for (var attempt = counter; attempt < AttemptLimit; attempt++)
{
    try
    {
        await work.TimeoutAfter(3000, timeProvider);
        return;
    }
    ...
}
```

This satisfies the second alternative of AC1's disjunction. A blocking synchronous COM call cannot be
interrupted on .NET Framework, so cancellation was not available; holding one task and re-deadlining
it is the only shape that removes the overlap while preserving the 9000 ms total budget. It is also
the only shape that is both analyzer-clean, since `BannedSymbols.txt` forbids `Task.Delay`, and
deterministically testable through the existing `TimeProvider` parameter.

Re-applying `TimeoutAfter` to one task was verified as sound rather than assumed. Each call allocates
its own `TaskCompletionSource`, arms its own timer, and attaches an independent `ContinueWith` to the
source task; multiple continuations on one task are independent. The `task.IsCompleted` short-circuit
at the top of the overload returns the source task directly when the work has already finished, which
is why the positive-path tests observe no spurious timer. When the work eventually completes, each
attached continuation disposes its own timer and marshals the result, so no timer is orphaned and the
source task's exception is observed rather than left unobserved.

**The failure now has somewhere to go.** The new `TimeoutException` was traced to confirm it actually
reaches the user. `GetEmailDataInViewAsync` awaits the column add at `UtilitiesCS/Extensions/DfDeedle.cs`
line 176 with no enclosing try block, so the exception propagates to
`QfcDatamodel.GetEmailsInViewDfAsync`, which now rethrows with `throw;`, then to
`QfcHomeController.LaunchAsync`, whose `catch (OperationCanceledException)` does not match a
`TimeoutException`, then to the ribbon handler, which is now wrapped. The chain is unbroken.

**Two independent guards, not one.** AC1 alone would leave every other route to a short column
dictionary unguarded, and AC3 alone would report the symptom without the cause. Both landed. The
validator is called on both the asynchronous path at `DfDeedle.cs` line 195 and the synchronous path
in `GetEmailDataInView`, which closes the duplicate unchecked indexing in `GetEmailDataFromTable`
that the asynchronous guard can never reach.

**The coverage-visible boundary is the right decomposition.** Placing the decision logic in
`RibbonCommandBoundary`, which carries no coverage-exemption attribute, and leaving only the
`MessageBox.Show` call and three handler one-liners in the exempt `RibbonViewer` shim, is exactly the
remedy `.claude/rules/general-unit-test.md` prescribes for host-bound code. The result is a
host-neutral type with 90.16 percent measured line coverage where previously the equivalent logic
would have been unmeasurable.

## Findings

### NB-1 — Repository-wide branch coverage is below the uniform floor (Non-blocking)

`.claude/rules/quality-tiers.md` sets a uniform branch-coverage floor of 75 percent for
branch-capable languages, which includes C#. The committed Cobertura documents record:

- Baseline: `branch-rate="0.6605220330495744"`, 21,105 of 31,952 branches.
- Post-change: `branch-rate="0.6611926319075866"`, 21,178 of 32,030 branches.

Both are below the floor. The condition is pre-existing and the direction of movement is upward.

The finding is recorded chiefly because `evidence/qa-gates/coverage-delta.md` does not report a
branch figure on either side. Its four acceptance clauses and its repository-wide reporting section
are line-coverage only, so the omission is not visible from the executor's evidence. The argument
that document-level coverage is a weak gate is accepted on its merits, but it does not extend to
omitting a measurement that the artifact already contains.

Recommendation: report branch coverage alongside line coverage in future coverage-delta artifacts.
No action on this branch.

### NB-2 — Four untested scenarios hold `RibbonCommandBoundary` at its coverage margins (Non-blocking)

`TaskMaster/Ribbon/RibbonCommandBoundary.cs` clears the 90 percent new-module obligation at 55 of 61
lines, or 90.16 percent. Fifty-four of 61 would read 88.52 percent and fail. The margin is one line
wide. Class-level branch coverage is 9 of 12 conditions, exactly 75 percent, which is at the uniform
floor rather than above it.

The uncovered lines and conditions were identified individually rather than left as an aggregate:

| Location | Uncovered element | Missing scenario |
|---|---|---|
| Lines 109, 110, 112 | The `catch` body inside `SafeLog` | The log sink itself throws |
| Lines 155, 156, 157 | The zero-inner-exception branch of `CollectDetail` | An `AggregateException` wrapping nothing |
| Line 46 | `?? throw new ArgumentNullException(nameof(logFailure))` | Construction with a null log sink |
| Line 47 | `?? throw new ArgumentNullException(nameof(presentFailure))` | Construction with a null presentation sink |

All four are scenarios the General Unit Test Policy names explicitly: two are error-handling
behaviour and two are negative flows for invalid input. The existing
`RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate` exercises a throwing presentation
sink but not a throwing log sink, so the `SafeLog` catch is never entered even though the failing
presentation path routes through `SafeLog` a second time.

Adding four small tests takes the file to 61 of 61 lines and 11 of 12 branches, which removes the
margin exposure entirely and closes two documented behaviours that are currently asserted only by the
source comments. The tests belong in `TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs`, which is
already in the write set at 249 lines with ample headroom.

This is Non-blocking because every applicable threshold is met as measured.

### NB-3 — The command boundary is not injectable into `RibbonViewer` (Non-blocking)

`TaskMaster/Ribbon/RibbonViewer.cs` assigns `_commandBoundary = CreateCommandBoundary()` in both
constructors, including the internal test constructor that otherwise exists specifically to inject
the `_loadFolderFilterAsync` and `_reportFolderFilterInitializationFailure` seams. The boundary's
presentation sink is therefore hard-wired to `MessageBox.Show` even under test.

No current test drives a handler — `NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`
inspects the handlers reflectively and does not invoke them — so there is no hazard today. A future
test that constructs a `RibbonViewer` and invokes `QuickFiler_Click` on a failing controller would
raise a modal dialog and block the test run with no diagnostic.

Recommendation: add an optional `RibbonCommandBoundary` parameter to the internal constructor,
defaulting to `CreateCommandBoundary()`. This is consistent with the seam pattern the same
constructor already uses and costs one parameter.

### NB-4 — The exhaustion message hardcodes a budget the loop does not always spend (Non-blocking)

The descriptive throw in `AddQfcColumnsAsync` reads:

```csharp
throw new TimeoutException(
    $"The column add step timed out after 9000 ms for folder '{folderName}'. "
        + "The folder did not return from the column add within its budget."
);
```

The loop runs `AttemptLimit - counter` deadlines, so the elapsed budget is
`(AttemptLimit - counter) * 3000` milliseconds, not invariably 9000. The sole production call site
passes `counter = 0`, so the message is accurate today. A caller passing a non-zero `counter` would
receive a message overstating the elapsed time, which is the opposite of what an instrumentation-led
fix should produce.

Related: the attempt count is a named constant `AttemptLimit`, but the per-attempt deadline `3000`
appears as a bare literal at the `TimeoutAfter` call and again, multiplied, inside the message
string. The two can drift independently.

Recommendation: introduce `private const int DeadlineMilliseconds = 3000;` beside `AttemptLimit`, use
it at the `TimeoutAfter` call, and compose the message from `(AttemptLimit - counter) * DeadlineMilliseconds`.

### NB-5 — A degenerate cancellation shape produces three immediate re-awaits (Non-blocking, latent)

If `work` faults with `TaskCanceledException` while `token.IsCancellationRequested` is false, the
`catch (TaskCanceledException)` guard does not return, the loop advances, and the next
`await work.TimeoutAfter(...)` re-observes the already-faulted task immediately. This repeats for the
remaining attempts and then throws the column-add `TimeoutException` with no time having elapsed.

The path is not reachable from the production call site: `Task.Run(action, token)` produces a
cancelled task only when that same token is signalled, in which case
`token.IsCancellationRequested` is true and the guard returns. It is recorded as a latent shape issue
rather than a live defect. The behaviour is bounded at three iterations and cannot loop indefinitely.

Recommendation, if touched later: rethrow rather than continue when the token is not signalled, since
a cancellation the caller did not request is not a timeout and should not be reported as one.

### NB-6 — The timeout tests use unbounded awaits (Non-blocking)

`FireOneDeadlineAsync` awaits `probe.Entered` and `barrier.Armed` with no timeout, and the tests
await `call` directly. The arrangement is deterministic in outcome — the analysis is recorded in the
policy audit — but a future regression that stops arming the next deadline would present as a suite
hang absorbed by the vstest blame hang timeout rather than as a named test failure.

This failure mode was anticipated during planning and did not occur: the Phase 2 record states the
blame collector reported all tests finished in every run and the four-minute hang timeout never fired.
The design trade is reasonable, because a bounded wall-clock wait in the test would reintroduce
exactly the nondeterminism the `FakeTimeProvider` exists to remove. Recorded so a future maintainer
reading a hung suite knows where to look first.

### NB-7 — Pre-existing 500-line-cap violation in the modified COM test class (Non-blocking)

`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` stands at 869 lines against the 500-line cap in
`.claude/rules/general-code-change.md`. It stood at 882 at the base commit, so the violation is
pre-existing and this change strictly reduces it by removing the `GetAddQfcColumnsAsyncMethod`
reflection helper and its two local bindings.

The reasoning for not bringing the file under the cap was evaluated in the policy audit and accepted.
The decisive legs are `CLAUDE.md`'s Bugfix Workflow prohibition on opportunistic refactors and the
external scheduling cost of adding a seventeenth path while three sibling items are prepared against
the same footprint. The residual is recorded for follow-up promotion.

### NB-8 — Unresolved conflict between two governing policy documents (Non-blocking)

`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement and directs reviewers to treat such an exclusion as Blocking. `CLAUDE.md`, which ranks
first in the policy compliance order, ratifies `[ExcludeFromCodeCoverage]` for VSTO ribbon classes and
Outlook Interop event-handler classes in `QuickFiler` and `TaskMaster`.

Two such attributes exist in the write set: `TaskMaster/Ribbon/RibbonViewer.cs` line 32 and
`QuickFiler/Controllers/QfcDatamodel.cs` line 25, the latter class-level and therefore covering both
partials. Both pre-date the change; the branch diff adds and removes none.

The conflict is a repository-level documentation defect, not a defect in this change. This change in
fact complies with the stricter document's stated remedy by extracting the new decision logic into a
non-exempt type. Resolving the conflict is outside a targeted bugfix.

### NB-9 — Follow-up promotions are recorded but not created (Non-blocking, obligation transfers)

`evidence/other/followup-promotions.md` records three findings in full, with locations, and states
explicitly that the potential-to-issue lifecycle was not run on this branch because creating a
promotion file would add a seventeenth path and falsify the write-set gate. That reasoning is sound
and the artifact is unusually complete: each finding can be promoted from it without re-derivation.

The obligation nonetheless transfers to the caller. The three findings are the unreachable
`catch (TimeoutException)` in the two `repeatAttempts` `TimeoutAfter` overloads, the unguarded
shared-static message-box seam mutation in the COM test class, and the pre-existing 500-line-cap
violation in that same class. None should be lost when the feature folder is archived.

### NB-10 — Zero headroom on the timeout test file (Non-blocking)

`UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` sits at exactly 500 lines. This is
compliant, since 500 does not exceed 500, but any future edit to that class breaches the cap. The
file reached 547 lines during Phase 2 and was compacted by extracting a shared call helper and
shortening documentation. A maintainer adding one test there will be forced into a split. If a test
must be added to that area, `DfDeedleRequiredColumnValidationTests.cs` at 218 lines is the correct
destination, which is the routing the plan already established.

### NB-11 — The log capture mutates process-wide log4net state (Non-blocking)

`CaptureDfDeedleLog` sets `Hierarchy.Configured` to true and raises the `DfDeedle` logger's `Level`
for the duration of the capture, restoring both in a `finally`. This is necessary rather than
optional: the repository is unconfigured because neither `UtilitiesCS` nor `UtilitiesCS.Test` carries
a log4net configurator attribute, so without it the production `Debug` call is a no-op and the AC2
assertions would be vacuous. That is precisely the defect Phase 3 discovered and repaired.

`[DoNotParallelize]` on the class bounds the mutation against other classes in the same assembly, and
the `finally` bounds it in time. It is not bounded against another assembly sharing the test host
process during the capture window. The exposure is small and the alternative is a non-discriminating
gate, so the trade is accepted. It is recorded because it belongs to the same shared-static hazard
class that this change's own Finding 2 promotes as a follow-up.

## Test Quality

The tests are of high quality and several choices deserve specific credit.

- **Shape-agnostic exception assertions.** Both `QfcDatamodelRethrowTests` and the timeout tests walk
  the full exception chain with an `Unwrap` helper and assert over the flattened set. Because
  wrapping happens upstream in the timeout helper's result marshalling, `throw;` restores the stack
  without unwrapping the `AggregateException`; an assertion that assumed either shape would have been
  fragile. The doc comment states the reasoning.
- **Existence rather than count in the log assertions.** log4net binds one logger per type for the
  whole process, so a concurrently running class can add events but never remove them. An existence
  assertion is deterministic under that constraint; a count assertion would be order-dependent.
- **The fail-before evidence was re-derived, not patched over.** When Phase 3 found the log capture
  inert, the correct conclusion was drawn: the original fail-before observation was non-discriminating
  and had to be re-derived against a working capture rather than retained. The executor also caught a
  false result inside that re-derivation, where a preserved `LastWriteTime` caused MSBuild to skip
  `CoreCompile` and re-test an uninstrumented binary while still exiting zero, and recorded both the
  discarded run and the mechanism rather than dropping them.
- **The invocation-count assertion is the real AC7 evidence.** `probe.Invocations.Should().Be(1)`
  across three deadlines is a direct assertion of the non-overlap invariant, not a proxy for it.

One test-design note, not a finding: `RunAbsorbingAsync` exists so that a sink-behaviour assertion is
not masked by the propagation defect that a sibling test pins on its own. That is a deliberate and
correct separation of concerns between tests, and it is documented in the helper's summary.

## Adjudication of the Phase 6 Documented Deviation

The plan's P6-T3 described a single `private static void ReportRibbonCommandFailure(string, System.Exception)`
that both logs through `logger.Error` and presents through `MessageBox.Show`. The implementation
declares `ReportRibbonCommandFailure` with exactly that signature and wires it as the log sink, and
wires a `MessageBox.Show` lambda as a separate presentation sink through a `CreateCommandBoundary()`
factory called from both constructors.

The deviation is accepted for three reasons:

1. It is not implementable as described. `RibbonCommandBoundary` takes an `Action<string, System.Exception>`
   and an `Action<string>`. One method cannot satisfy both delegate types.
2. Wiring one method as both sinks would show two dialogs per failure, since the boundary invokes
   both sinks on the same failure.
3. The presentation sink must receive the already-rendered message so that the inner-exception detail
   reaches the dialog. This is an explicit AC11 requirement and the reason the wrapper's own message,
   "One or more errors occurred.", is not acceptable on its own.

Every literal the plan names is present: the method exists with the specified signature, it calls
`logger.Error`, and `MessageBox.Show` is the presentation mechanism. Only their composition differs,
and the split is documented in the source `<remarks>` on the factory. No acceptance condition is
weakened and no criterion is affected.

## Adjudication of the Superseded Phase 0 Verdict

`evidence/baseline/log4net-capture-probe.md` records `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED`,
which Phase 3 established is not reproducible. The addendum was assessed for adequacy and is
adequate. It:

- marks the original verdict SUPERSEDED in place with a forward pointer, and retains the original
  text unaltered with an explicit statement that retention is not endorsement;
- states the mechanism precisely: neither assembly carries a configurator attribute, so the default
  repository is unconfigured, `Hierarchy.IsDisabled` reports every level disabled, and that
  repository-level disable is evaluated before the per-logger level, making the production `Debug`
  call a no-op however the appender is attached;
- states honestly that the probe was removed by P0-T14 and cannot be re-examined, and that whether
  the two assemblies share a default repository remains unestablished by execution rather than
  claiming the question was settled;
- identifies the implemented strategy as a third strategy and distinguishes it from both of the
  plan's documented branches, explaining why neither branch addresses a repository-level disable;
- records that the AC2 fail-before evidence was non-discriminating and names its replacement.

AC14's check-off does not rest on the superseded statement. AC14 requires that the Phase 0
log-capture check be recorded with its outcome and that the strategy actually used be documented in
the plan. The recorded outcome is the addendum's outcome, and the strategy is documented both in the
addendum and in the plan by correction A3. The addendum's description of the third strategy was
verified against the source: `CaptureDfDeedleLog` sets `repository.Configured = true`, calls
`appender.ActivateOptions()` before attaching, and restores the previous `Configured` value and
logger `Level` in a `finally`. The description and the implementation agree.

## Recommendations

Ordered by value, none blocking:

1. Add the four `RibbonCommandBoundary` tests identified in NB-2. They close two documented
   error-handling behaviours and two null-guard contracts, and they remove the one-line coverage
   margin and the exactly-at-floor branch figure.
2. Create the three follow-up promotions from `evidence/other/followup-promotions.md` after merge
   (NB-9).
3. Extract a `DeadlineMilliseconds` constant and compose the exhaustion message from the actual
   attempt count (NB-4).
4. Make the command boundary injectable through the internal `RibbonViewer` constructor (NB-3).
5. Report branch coverage alongside line coverage in future coverage-delta artifacts (NB-1).
6. Raise the conflict between the Coverage Exclusion Policy and the ratified VSTO exemption as a
   repository-level documentation issue (NB-8).

## Verdict

**PASS.** Zero Blocking findings. The change is focused, the design reasoning is sound and recorded
in the source where it is non-obvious, the tests are discriminating, and the toolchain is clean in a
single pass. The eleven Non-blocking findings are improvements and carried-forward obligations, not
defects requiring a remediation cycle.
