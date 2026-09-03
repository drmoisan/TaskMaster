# Code Review — ribbon-engine-toggle-defects (Issue #735)

- Timestamp: 2026-09-03T06-19 (UTC)
- Branch: `bug/ribbon-engine-toggle-defects-735`
- Head: `30e66833e73267327a18e58228f493e8c8e3a4dd`
- Verdict: **PASS — 0 blocking findings, 13 non-blocking**

## Diff anchor used (stated verbatim for later readers)

```
git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a3324f355df219b0e diff b13d5b7b HEAD
```

Two-dot, against `b13d5b7b1a6dd0aa79d51d48a7156ee67377f9d0`. That SHA is simultaneously the current
`origin/main` tip and `git merge-base HEAD origin/main`, both verified in this session, so the
two-dot and three-dot forms are identical here and the range is exactly what the pull request will
show.

**Why this anchor and not the one originally supplied.** The delegating prompt first specified
`git diff a679cd082819af6788cd0fb35f4366786fab87e3...HEAD` and then superseded it. `a679cd08` is an
ancestor of `HEAD` following the second `origin/main` merge (`30e66833`, 2026-09-03 02:06 -0400).
When the left operand is an ancestor of the right, `merge-base(a679cd08, HEAD)` is `a679cd08`
itself, so the three-dot form silently degenerates to two-dot and sweeps in everything `HEAD` gained
from `main` in the interim.

Measured, not assumed:

| Anchor | Changed paths | Paths under `.github/`, `Directory.Build.props`, `scripts/vscode/`, `tests/scripts/vscode/` |
|---|---|---|
| `a679cd08` (superseded) | 184 | 18 |
| `b13d5b7b` (correct) | 78 | **0** |

The caller's assertion that those paths arrived from `main` rather than from this item was verified
rather than accepted: `git diff --name-only b13d5b7b HEAD` returns zero matches for
`^(\.github/|Directory\.Build\.props|scripts/vscode/|tests/scripts/vscode/|artifacts/pr_context|docs/features/active/2026-09-02-(ci-build-infra-debt-730|coverage-cobertura))`.
No scope-boundary or write-set finding is raised against any of them.

Independently, the executor's own footprint gate
(`evidence/qa-gates/footprint-scope.2026-09-02T12-04.md`) re-derived its merge base at run time as
`a679cd08` — correct at the moment it ran, before the second merge — and reported the same twelve
source paths. The two derivations converge on identical source scope.

## Review surface

Twelve source and project paths, the feature folder, and four `.claude/agent-memory/` paths. The
agent-memory paths are genuine branch content from the preparation commit `044551f0` and were
audited, not excluded.

## What the change does

**Finding 1 — dead CustomUI bindings.** Four `onAction` values renamed from the `_Clicked` spelling
to `_Click`; the `BtnMigrateIDs` button element deleted. Two reflection-based regression tests added
to the existing XML-consistency fixture.

**Finding 2 — unguarded globals dereference.** The decidable part of `ClearSpamManagerAsync` is
extracted into a new host-neutral `internal sealed class SpamManagerResetGate`; the engine-touching
remainder is deferred into a lambda that receives the already-resolved manager and engines.

**Finding 3 — toggle-state last-writer race.** The pressed-state dictionary is replaced by a new
`EngineTogglePressedStateCache` carrying a monotonic ticket per observation and a compare-and-apply
store. CR-2 additionally repairs prime completion so a canceled prime is treated as a failure; CR-3
adds a test for the pre-existing engines-unavailable guard.

## Correctness analysis

### Finding 1 — verified by independent reproduction

This review parsed both revisions of `TaskMaster/Ribbon/RibbonExplorer.xml` with an XML DOM walker
that visits element nodes only and collects attributes whose local name is `onAction`, `onChange`,
`onLoad`, or begins with `get`, then compared each distinct value against the public instance method
names declared across `RibbonViewer.cs` and `RibbonViewer.EngineCommands.cs`:

```
PRE-FIX   (b13d5b7b): 84 distinct callback names, 5 unresolved
                      BtnMigrateIDs_Click, MoveEntireConversation_Clicked,
                      SaveAttachments_Clicked, SaveEmailCopy_Clicked, SavePictures_Clicked
POST-FIX  (HEAD):     83 distinct callback names, 0 unresolved
```

The rename targets exist with the correct shape: `RibbonViewer.cs:180, 186, 192, 198` each declare
`public void <Name>_Click(Office.IRibbonControl control, bool pressed)`. `BtnMigrateIDs` appears
nowhere in the solution after the deletion. The XML diff is exactly one element deletion plus four
attribute-value renames, with no other semantic change — confirmed line by line.

### Finding 2 — the gate is a correct decision boundary

`SpamManagerResetGate.RunAsync` checks the reset delegate for null before invoking either accessor,
so a caller defect can never be reported as "not ready". It resolves `autoFile?.Manager` and the
engines facade, notifies exactly once and returns `Task.CompletedTask` when either is null, and
otherwise returns `reset(manager, engines)` directly. There is no `await` and no `catch`, so a
fault from the deferred work propagates with its original instance intact — pinned by
`RunAsync_WhenResetFaults_PropagatesUnchangedAndDoesNotNotify`, which asserts `BeSameAs(failure)`.

The call-site rewrite preserves behavior. The old shape was
`if (response == DialogResult.Yes) { ...body... }`; the new shape is
`if (response != DialogResult.Yes) { return; }` followed by the gated call. The
synchronization-context preamble and the confirmation dialog are byte-identical and in their
original order. Inside the lambda, `Globals.AF.Manager` and `Globals.Engines` are replaced by the
captured `manager` and `engines` parameters; these are the same object references the old code would
have dereferenced, so the only behavioral difference is that they are now resolved once up front
instead of three times mid-flight.

### Finding 3 — the freshness invariant holds

The design rests on ordering observation *start* times, not write *completion* times. Both writers
take a ticket immediately before their activation read:

- `ApplyPrimeAsync` (`EngineToggleStateCoordinator.cs:310-334`) takes the ticket, then reads.
- `ExecuteToggleAsync` (`:222-234`) takes the ticket **after** `ToggleEngineAsync` completes and
  before `EngineActiveAsync`, which is the moment its observation window actually opens.

The invariant this buys was checked rather than assumed. If a prime's ticket is greater than a
toggle's ticket, then the prime's ticket was issued after the toggle's, which was issued after
`ToggleEngineAsync` had already completed; therefore the prime's read begins after the toggle landed
and observes the post-toggle state. So a higher ticket always carries an at-least-as-recent
observation, and `TryApplyState`'s `existing.Sequence >= sequence` rejection can never discard a
fresher value.

`TryApplyState`'s loop terminates: each iteration either returns, or observes a strictly newer stored
ticket, or retries after losing a `TryAdd`/`TryUpdate` race in which some other writer made progress.
The choice of a reference-type `PressedState` as the `TryUpdate` comparand is correct and the XML
documentation explains why — a value tuple would compare structurally, so an unrelated writer that
happened to store an equal value would satisfy the comparand check and silently weaken the guard to
"the value looked the same".

Conditional invalidation does not lose a needed refresh. A rejected write means some other writer
already stored a strictly newer observation, and that writer's own `TryApplyState` returned true and
invalidated. `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` guards
the opposite failure mode, that the new conditional degenerates into "never invalidate".

CR-2 is correct. The pre-fix `CompletePrime` keyed on `completed.Exception`, which is null for a
`Canceled` task, so a cancellation returned early: nothing logged, cache unset, and the in-flight
marker left registered, permanently blocking any re-prime for that key. Switching the test to
`completed.Status == TaskStatus.RanToCompletion` is the right predicate, and the synthesized
`TaskCanceledException` gives the sink a non-null exception. The faulted path is unchanged and still
reports `GetBaseException()`, so the pre-existing test at
`EngineToggleStateCoordinatorTests.cs:233`, which asserts `BeSameAs(failure)` by reference, keeps
passing — confirmed by the 24/24 coordinator regression run and the 134/134 ribbon run.

The continuation is attached with `TaskContinuationOptions.None` and `TaskScheduler.Default`, so it
runs on all three terminal outcomes, which is what CR-2 requires.

### Test determinism

The race tests are deterministic rather than timing-dependent, which was verified by tracing the
state machine rather than by trusting the comment. In
`ExecuteToggleAsync_WhenOlderObservationCompletesLast_...`, `ToggleEngineAsync` returns
`Task.CompletedTask`, so the first `ExecuteToggleAsync` call runs synchronously through the toggle,
takes ticket 1, and only then suspends on the held `olderRead.Task`. The second call therefore takes
ticket 2. Ticket assignment is fixed by the synchronous prefix of the state machine, not by a race.
The same reasoning applies to `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_...`, where
`SetupSequence` binds the prime to the first (held) read and the toggle to the second (immediate)
read.

`GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` documents its own load-bearing
assertion order in a `<remarks>` block: because the harness mock is strict and only one setup is
supplied, the re-prime triggered by the second read re-enters the same canceled task and logs a
second error, so the single-error assertion must precede it. That reasoning is correct, and the
marker-cleared conclusion is drawn from prime-handle identity rather than from an error count, which
is the deterministic signal.

## Findings

All findings are non-blocking. None changes the PASS verdict.

### NB-1 — `string.Format` with no format placeholders (Non-blocking)

`TaskMaster/Ribbon/SpamManagerResetGate.cs:132-140`

```csharp
private static string BuildNotReadyMessage()
{
    return string.Format(
        CultureInfo.CurrentCulture,
        "The Spam Manager cannot be cleared yet because the classifier manager is still "
            + "loading. Please try again once initialization completes."
    );
}
```

There is no `{0}`. This parses a format string and allocates for nothing. Every other
`string.Format(CultureInfo.CurrentCulture, ...)` in the ribbon layer has placeholders —
`EngineGatedCommandRunner.cs:130-136` and `EngineToggleStateCoordinator.cs:370, 383, 395, 408` were
each checked — so this is the shape copied without the substance, and the `using
System.Globalization` it forces is otherwise unused in the file. Recommendation: make it a `private
const string` field, or return the literal directly and drop the `System.Globalization` import.

### NB-2 — prime-marker removal happens outside the gate that guards registration (Non-blocking)

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:269-277` and `:341-353`

```csharp
lock (_primeGate)
{
    if (_primeTasks.ContainsKey(engineName)) { return; }
    _primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);   // :276
}
...
private void CompletePrime(Task completed, string engineName)
{
    if (completed.Status == TaskStatus.RanToCompletion) { return; }
    _primeTasks.TryRemove(engineName, out _);                                        // :348
    ...
}
```

`StartObservedPrime(...)` is fully evaluated — including scheduling the continuation — before its
result is assigned into `_primeTasks`. When `ApplyPrimeAsync` completes without ever suspending
(for example when `EngineActiveAsync` returns an already-completed or already-canceled task), the
continuation can run on the thread pool and execute the `TryRemove` at line 348 before the
assignment at line 276 lands. The assignment then re-registers a marker for a prime that has already
failed, and no later read can re-prime that key for the rest of the session — the same class of
defect CR-2 exists to fix.

This is a pre-existing ordering hazard: the same shape was present before this change on the faulted
path. CR-2 widens the set of outcomes that reach `TryRemove` to include cancellation, so it becomes
reachable on one additional path. It does not make any test in this change flaky — both
cancellation tests draw their conclusions from prime-handle identity, which holds either way.

Recommendation for a follow-up issue: either take `_primeGate` around the `TryRemove`, or register a
placeholder marker before attaching the continuation and swap it afterwards.

### NB-3 — evidence arithmetic error in the coverage-run artifact (Non-blocking)

`evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md`

The artifact writes "2 XML-consistency tests (Finding 1), 9 gate tests (Finding 2), 6 race tests
(Finding 3) and 9 cache tests (P4-T3 branch B). 2 + 9 + 6 + 9 = 26", then explains the missing
twenty-seventh test as a pre-existing fixture whose name did not match the P0-T8 baseline filter.

The cache fixture has **10** `[TestMethod]` declarations, not 9 — counted from source and confirmed
against `evidence/qa-gates/p4-t3/p4-t3.trx`, which contains 10 results whose names begin with
`TryApplyState`, `NextSequence` or `TryGetActive`. The correct arithmetic is 2 + 9 + 6 + 10 = 27,
which matches the observed delta exactly, so no residual needs explaining.

Cross-checked independently: `[TestMethod]` count across `TaskMaster.Test/Ribbon/` is 85 at
`b13d5b7b` and 112 at `HEAD`, a delta of 27.

The substantive conclusions of the artifact — 6982/6982 passing, no test removed or skipped — are
unaffected. The defect is that a reconciliation was constructed for an arithmetic error instead of
the count being rechecked. It is worth correcting because the mis-attribution names a real test and
would mislead a later reader into believing the baseline filter is unreliable.

### NB-4 — constant added but the duplicate literal left in place (Non-blocking)

`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:316` and `:332`

Line 332 introduces
`private const string RibbonControlTypeName = "Microsoft.Office.Core.IRibbonControl";`, but the
pre-existing inline occurrence at line 316 was not replaced, so the file now carries both. The
spec's Test Strategy described this as "one private constant hoisting the ribbon-control type-name
literal that already appears inline in that file". A third independent copy exists at
`RibbonViewerEngineCallbackShapeTests.cs:43`; consolidating all three into one shared test constant
would be the cleaner end state.

### NB-5 — test fixture is four lines under the 500-line ceiling (Non-blocking)

`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` is 496 lines (verified with
`awk 'END{print NR}'`, which does not undercount the way `Measure-Object -Line` does). The next
addition to this fixture will breach the limit. The repository already has the partial-split
precedent in this directory (`RibbonControllerTests.cs` / `.Engines.cs`, and now
`EngineToggleStateCoordinatorTests.cs` / `.Race.cs`), so the remedy is known; this is a heads-up,
not a violation.

### NB-6 — spec Write Set not amended after the branch-B contingency (Non-blocking)

`spec.md` `## Write Set` lists ten paths and the cross-cutting criterion says "All three new source
files are registered as compile items". The delivered footprint is twelve paths and five new source
files, because the P4-T3 branch-B contingency extracted `EngineTogglePressedStateCache.cs` and added
`EngineTogglePressedStateCacheTests.cs`.

The extraction itself is properly authorized: the spec's own Risks table pre-authorizes it
("The coordinator source may exceed the 500-line ceiling after formatting … If exceeded, extract the
versioned cache into its own class rather than trimming documentation"), and the decision is recorded
in `evidence/qa-gates/coordinator-size-contingency.2026-09-02T12-04.md` with the measured trigger
(515 lines after formatting, 415 after extraction). The gap is only that the Write Set and the
cross-cutting criterion text were left describing the pre-contingency plan.

### NB-7 — callback resolution admits `System.Object` members (Non-blocking)

`TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs:338-339`

```csharp
private static MethodInfo[] GetViewerCallbackSurface() =>
    typeof(RibbonViewer).GetMethods(BindingFlags.Public | BindingFlags.Instance);
```

This includes the inherited `ToString`, `Equals`, `GetHashCode` and `GetType`, so a CustomUI
attribute bound to one of those four names would be reported as resolving. Practical risk is low —
none is a plausible callback name — but adding `BindingFlags.DeclaredOnly`, or filtering
`method.DeclaringType != typeof(object)`, would make the assertion exact. Note that
`DeclaredOnly` is safe here because `RibbonViewer` is not part of an inheritance chain that
contributes callbacks.

### NB-8 — lazy gate construction is not thread-safe and the invariant is undocumented (Non-blocking)

`TaskMaster/Ribbon/RibbonController.Intelligence.cs:206 and :220-230`

```csharp
private SpamManagerResetGate SpamManagerReset =>
    _spamManagerResetGate ??= new SpamManagerResetGate(...);
```

Two concurrent readers could each construct a gate. The consequence is benign — the gate is
stateless apart from its three captured delegates — and in practice ribbon callbacks are serialized
on the Outlook STA, so the race cannot occur. The `<remarks>` block explains why this gate is
separate from `EngineGatedCommandRunner` but does not state the single-threaded-callback invariant
the lazy assignment depends on. One sentence would close it.

Related, and correct as written: the two null-forgiving operators (`() => Globals?.AF!`,
`() => Globals?.Engines!`) are annotations only. This file carries no `#nullable enable`, so they
have no compile-time effect; the accompanying comment correctly records that a null result is a
supported value the gate treats as "not ready", rather than a suppressed defect.

### NB-9 — large Cobertura documents committed to the feature evidence tree (Non-blocking, repo hygiene)

`evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml` (10.8 MB, 194,456 lines) and
`evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml` (10.8 MB, 194,753 lines).

Committing them is defensible — they are the coverage evidence, and this review depended on both to
recompute per-file figures independently. The caution is that once merged with full history they
cannot be removed later without leaving reachable blobs. Recommend squash-merging this branch.

### NB-10 — the coverage document predates the final merge (Non-blocking)

`coverage-final...cobertura.xml` has an mtime of 01:55 local; the last implementation commit
(`3e45428e`) is 02:04 and the final `origin/main` merge (`30e66833`) is 02:06. The document was
therefore produced against the final working tree of this item's own work, but before `main`'s
#730/#733 content was merged in. Its provenance is otherwise sound — it contains
`TaskMaster.EngineTogglePressedStateCache`, a type that exists only after the branch-B extraction,
which proves it postdates that edit rather than being a stale copy.

Consequence: the 85.41% repository-wide figure describes the pre-merge tree. Every per-file figure
for this item's own files is unaffected, and the repository-wide gate of record is the CI run on the
pull request.

### NB-11 — acceptance criterion F2-AC8 remains open (Non-blocking, operator action)

`spec.md`, Finding 2, criterion 8: "The change description records the manual verification."

`evidence/other/manual-verification-clear-spam-manager.2026-09-02T12-04.md` records
`ManualVerificationStatus: OPERATOR-ACTION-REQUIRED`, leaves both observation fields explicitly
unfilled, and states the reason: the procedure requires launching Outlook with add-in user-interface
errors shown and interacting with the ribbon during the pre-initialization window, which no automated
session can do and which the unit-test policy independently forbids automating. The criterion is
correctly left unchecked rather than asserted.

This is the right handling. The gap is real but cannot be closed by any code change; it needs an
operator to run the two-step procedure and record the outcome before the issue is closed.

### NB-12 — PR context artifacts belong to a different item (Non-blocking, process)

`artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt` in the review worktree
resolve `Head ref … bug/ci-build-infra-debt-730 @ ff2106fe` against base `8be5a6aa`. They describe
issue #730, not this branch, and their autoclose candidate list names #395 and #561.

These are tracked files inherited from `main` (last touched by `96bc3512`), so regenerating them in
place would overwrite another item's committed content. Scope for this review was derived from
`git merge-base` and `git diff` instead, and the deviation is recorded in the policy audit's Scope
Resolution section. This is a repository-wide workflow issue rather than a defect in this change.

### NB-13 — coverage-exclusion policy conflict (context, not a finding)

`TaskMaster/Ribbon/RibbonController.cs:36` carries `[ExcludeFromCodeCoverage]`.
`.claude/rules/general-unit-test.md` states that no production file may be excluded from coverage
measurement and that a feature-review agent must treat such an exclusion as blocking, while
`CLAUDE.md` ratifies exactly this exemption for VSTO/COM-bound classes. The attribute predates this
branch and is unchanged by it — verified with
`git show b13d5b7b:TaskMaster/Ribbon/RibbonController.cs`, which has it at the same line — so no
finding is raised against this change. Recorded here because the two policy documents remain
unreconciled and the next reviewer will hit the same question.

## What this change does well

- The extraction is justified by measurement, not preference: the coordinator hit 515 lines after
  formatting, the spec's own risk row pre-authorized the extraction remedy, and the resulting file
  is 415 lines with the cache at 157.
- The evidence is unusually falsifiable. `fail-before-exception` records why a pre-fix failing run
  is structurally impossible for Finding 2 and substitutes a named one-to-one mapping from the three
  gate tests onto the three null states, instead of asserting a run that did not happen. The manual
  verification dossier reports `OPERATOR-ACTION-REQUIRED` rather than claiming a result.
- Host-token sanitization was run before every commit rather than only at the end. This review
  verified the outcome independently: `git grep -i` for the account token across all six branch
  commits returns zero matching files in the feature folder, and both TRX and Cobertura documents
  carry `REDACTED-MACHINE` / `REDACTED-ACCOUNT` placeholders in `computerName=` and `runUser=`.
- The uncovered residual is honest. The two uncovered lines in the new cache
  (`EngineTogglePressedStateCache.cs:109` and `:127`) are precisely the CAS retry paths, which
  require a real thread race to reach; the file still clears both coverage floors, and no exemption
  was sought to hide them.

## Recommended follow-ups (none blocking this pull request)

1. Promote NB-2 (prime-marker removal outside `_primeGate`) to its own issue. It is a latent
   pre-existing race in the same class this change already touches.
2. Correct the cache-test count in `evidence/qa-gates/vstest-coverage-run.2026-09-02T12-04.md` and
   withdraw the twenty-seventh-test attribution (NB-3).
3. Amend `spec.md`'s Write Set and cross-cutting criterion to the delivered twelve paths and five new
   files (NB-6).
4. Fold `BuildNotReadyMessage` into a constant and drop the unused `System.Globalization` import
   (NB-1).
5. Squash-merge, given the two 10.8 MB Cobertura blobs (NB-9).
6. Route F2-AC8 to an operator with a live Outlook host before closing issue #735 (NB-11).
