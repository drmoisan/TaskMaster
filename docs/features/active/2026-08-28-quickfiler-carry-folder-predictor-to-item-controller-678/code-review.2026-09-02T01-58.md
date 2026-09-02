# Code Review — issue #678, carry the folder predictor to the item controller (closing review, post remediation cycle 1)

- Timestamp: 2026-09-02T01-58
- Head: `bd57dc9d400ac269317d2397c1ad649deac426de`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Reviewed surface: all 36 changed source paths (16 under `QuickFiler/`, 20 under `QuickFiler.Test/`)
- Supersedes: `code-review.2026-09-01T23-35.md` (round 1, head `d1f51e3a`)

## What the remediation cycle changed

The cycle is small and well-bounded: 34 added executable production lines across five files, plus
three new tests and their evidence. Its whole production footprint is commits `be1e0b97` (the fix)
and the CSharpier reflow carried in `bd57dc9d`.

The most consequential change is R1. Leg A previously handed `batch.PreScored` to
`LoadItemsAsync`; it now hands `QfcPreScoredItem.ReconcileCarriersToItems(batch.Items, batch.PreScored)`,
making `batch.Items` — the post-unhook set — the spine of the displayed list, exactly as leg B
already did. The matcher was hoisted onto the carrier type as `QfcPreScoredItem.ResolveCarrier`
and `QfcQueue.ResolveCarriedHandler` reduced to a one-line delegation to it, so the tree now holds
one carrier-matching implementation instead of two that could drift.

## Verification of the four remediation items

Each item was verified against the source at head, not against the executor's report.

### R1 — leg A now displays the post-unhook set. **Fixed.**

The spine is genuinely swapped. `QuickFiler/Controllers/QfcHomeController.cs:309-313` builds
`preScored` from `batch.Items`, and `ReconcileCarriersToItems`
(`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:210-222`) iterates `items` and emits one
carrier per surviving item in item order. An item with no matching carrier receives
`new QfcPreScoredItem(item, null)`, whose constructor coerces `PredeterminedFolder` to
`string.Empty` and leaves `FolderHandler` null, so that row falls back to its own scoring pass —
the pre-#678 behaviour.

This reviewer traced the value to the boundary that consumes it rather than stopping at the
assignment, which is what the remediation input asked for. `QfcFormController.LoadItemsAsync`
(`QuickFiler/Controllers/QfcFormController.Actions.cs:114-135`) forwards the list to
`QfcCollectionController`, whose body derives the displayed spine as `preScored.Select(x => x.MailItem)`.
The displayed set is therefore the reconciled list, and the invariant holds at the row that is
actually rendered.

The regression test is the strongest artifact in the cycle.
`RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary`
(`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`) drives the
real `TryUnhookOrReplace` throw branch through a move monitor that throws once, and asserts four
things at the load boundary: the carrier overload was invoked, the list contains exactly one item,
that item is the substitute, and no carrier references the failed item. It also asserts
`loaded[0].FolderHandler` is null, pinning the fallback for the substitute.

The red run at `evidence/regression-testing/r1-red.md` is what makes this convincing. It failed at a
**stage-two** assertion with all four stage-one assertions passing, which is the evidence that the
production `TryUnhookOrReplace` throw branch actually produced the divergence rather than the test
hand-building it. The artifact also rules out the standard false-red causes by name: a pre-run build
at exit 0, a discovery control of exactly 1 test, a 475 ms duration rather than a sub-millisecond
assembly-load failure, and a named FluentAssertions failure type. The `Mock<MailItem:1>` versus
`Mock<MailItem:2>` message states the defect exactly.

The doc block at `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:166-175` no longer claims
the two collections "describe one dequeue rather than two"; it now states the throw-path divergence
in both directions and names where each leg reconciles.

### R2 — the projection now mirrors its parity target. **Fixed.**

The executor chose alignment over narrowing the claim, which is the stronger of the two options R2
offered. `ProjectPredeterminedFolder` (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:272-286`)
now guards on `archiveRootPath is null` instead of `string.IsNullOrEmpty(archiveRootPath)`, and the
call site at `:230-234` emits null only for a null `_globals`:

```csharp
_globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)
```

This reviewer compared the bodies directly against
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` and they are now identical modulo the
parameter name: same `archivePrefix` construction, same `StartsWith` with `OrdinalIgnoreCase`, same
`Length > archivePrefix.Length` condition, same `Substring`. The null guard now stands in exact
correspondence with that member's `_globals is null` guard. `FolderPredictor.cs` is unmodified,
confirmed against the diff — the parity target was not moved to meet the claim.

The doc comment was rewritten to match. It no longer says "exactly"; it states parity for non-null
inputs and then names the two remaining divergences (a null or empty `folderPath` is returned rather
than dereferenced; a non-null globals with a null `Ol` is treated as an empty archive root rather
than reproducing a null dereference) and explains that both are null-safety differences rather than
projection differences. That is an accurate description of the delivered code.

The behaviour is pinned at the boundary R2 named, not at helper equality:
`AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder` sets up
`FolderContains` for the projected form only and verifies `SetFolderSelectedItem` is called with it.
The pre-existing boundary test's empty-archive-root assertion was corrected from "identity" to
"strips a single leading separator", which R2 clause 1 authorises and which is now true.

### R3 — the adoption path observes cancellation. **Fixed, with one residual recorded as NB-9.**

`cancel.ThrowIfCancellationRequested()` is the first statement of the adoption branch at
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:78`. The line is covered — this reviewer
confirmed it is absent from that file's uncovered set. The test asserts all three observable
consequences: the exception propagates, `_folderHandler` is not assigned, and the predictor factory
is invoked `Times.Never`.

### R4 — evidence timestamps are real clock values. **Fixed.**

The 17 declarations across 13 round-1 artifacts were corrected in `be1e0b97`, and this reviewer
confirmed from the diff that only `Timestamp:` lines changed — no `Command:`, `EXIT_CODE:` or
`Output Summary:` value was rewritten. The forward-looking half is stronger than required: P2-T13
audited this cycle's own 35 artifacts, found 22 of them carrying the same defect, and corrected them
to their own pre-correction write times.

The artifact then records that its own plan clause is unsatisfiable — correcting a timestamp
rewrites the mtime, so a re-measurement band and a correction instruction form a fixpoint that no
number of passes converges on — and declines to claim a pass for that sub-clause. This reviewer
checked the reasoning and it is correct. Recording a plan defect against oneself instead of
dispositioning it into a pass is the right call and is noted as a positive observation below.

## Status of all eight round-1 findings

| # | Round-1 finding | Severity | Current status |
|---|---|---|---|
| NB-1 | Leg A displayed the pre-unhook carrier list | Major | **FIXED** — verified at the load boundary; red-then-green regression test |
| NB-2 | Projection did not mirror `ProjectSuggestionPath` | Minor | **FIXED** — guards aligned, bodies now identical, doc corrected, boundary test added |
| NB-3 | Adoption path did not observe the cancellation token | Minor | **FIXED** — throw added and covered; one residual raised as NB-9 |
| NB-4 | AC20 per-member clause fails for two relocated members | Minor | **STILL OPEN**, deferred by agreement. Figures re-measured; see below |
| NB-5 | Declared evidence timestamps were not real clock values | Minor | **FIXED** — 17 declarations corrected, plus 22 more in this cycle's own artifacts |
| NB-6 | Three files remain over the 500-line limit | Minor, pre-existing | **STILL OPEN**, deferred by agreement. Re-measured, unchanged |
| NB-7 | Leg B's end-to-end carry is proved in two halves | Informational | **STILL OPEN**, deferred by agreement. Re-measured, unchanged |
| NB-8 | AC11 and AC12 are in tension as authored | Minor, criteria text | **STILL OPEN**, deferred by agreement. `issue.md` is byte-identical to its preimage |

Four closed, four open by explicit agreement. None regressed.

## Findings

Blocking: **0**. Non-blocking: **7**.

### NB-4 — Minor, non-blocking, still open. AC20's per-member clause fails for two relocated members

- Files: `QuickFiler/Controllers/QfcQueue.Enqueue.cs:76-138` (`EnqueueAsync`, 0/46) and `:163-198`
  (`LoadControllersViewersAsync`, 0/24)

Re-measured at head. Both members remain at zero. The file's measured rate moved from 28.00 percent
(28/100) to 15.29 percent (13/85), which looks like a regression and is not one: this reviewer
enumerated the uncovered line numbers and counted exactly **72**, the same count and the same two
member bodies as round 1. The ratio fell only because R1 collapsed the 26-line `ResolveCarriedHandler`
body into a one-line delegation, removing 15 lines that were all covered; the same logic now lives
in `QfcHighConfidencePreFilter.cs` at 73/73 = 100 percent. Covered and total each fell by exactly 15,
so no line became uncovered.

Full disposition, with the five grounds on which it is non-blocking, is in
`policy-audit.2026-09-02T01-58.md` under "Disposition of the two sub-floor rows".

### NB-6 — Minor, pre-existing, not introduced here, still open. Three files remain over the 500-line limit

| File | Base | Head | Over by |
|---|---:|---:|---:|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2446 | 2336 | 1836 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 827 | 792 | 292 |
| `QuickFiler/Controllers/QfcQueue.cs` | 610 | 505 | 5 |

Re-measured at head; unchanged by the remediation cycle. All three were over at the base ref and all
three are smaller after this change. AC21 is satisfied on its own terms and no file crossed the
limit. The cycle's own new file, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`,
is 247 lines with ample headroom.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` remains at exactly 500. It is at the cap
and does not exceed it, but it has no headroom; the next edit to it must relocate rather than extend.
Already registered in `evidence/other/out-of-scope-register.md` item 3.

### NB-7 — Informational, still open. Leg B's end-to-end carry is proved in two halves with the joining statement unproven

- `QfcQueue.ResolveCarriedHandler` is now a one-line delegation and is covered, and the matcher body
  it delegates to is covered at 20/20.
- The `ItemControllerFactory` production default is pinned by
  `ItemControllerFactory_DefaultInvocation_BuildsControllerCarryingTheHandler`.
- The two statements in `LoadControllersViewersAsync` that join them remain uncovered
  (`QuickFiler/Controllers/QfcQueue.Enqueue.cs:163-198`, 0/9 measured).

Unchanged by the remediation. The composition is still inferred from the two halves rather than
executed. This is an honest consequence of the host binding; it is repeated here so a later reader
does not read "leg B is covered" as end-to-end proof.

### NB-8 — Minor, criteria text, still open. AC11 and AC12 are in tension as authored

AC11 requires the preselected entry to be "identical to the entry the pre-change code preselects";
AC12 requires the archive-prefix normalisation that, for an archive-rooted suggestion, deliberately
changes the preselected entry from the index-1 fallback to the named folder. Both cannot hold
literally for the archive-rooted case.

Read together, AC11 governs the cases AC12 does not touch and AC12 is the more specific criterion
for the archive-rooted case. The delivered code implements exactly that reading. R2 has now widened
the set of inputs where AC12 governs — the (non-null globals, empty archive root, leading-separator)
state moved from AC11's reading to AC12's — which makes the tension slightly broader in scope but
does not change its character or its resolution. `issue.md` is byte-identical to its Phase 0
preimage, so no criterion text was edited to paper over this. The defect is in the criteria text,
not in the code.

### NB-9 — Minor, non-blocking, new. The adoption path's cancellation does not reproduce the pre-change logging side effect, and the in-code rationale does not record that

- File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:70-78`
- Supporting: the `try` at `:88`, the `catch (System.Exception e)` at `:127-131`

R3's stated invariant is that an already-cancelled token "produces the same observable outcome on the
adoption path as it did on the pre-change path". The delivered fix restores the propagation and the
non-assignment of `_folderHandler`, both of which the test asserts. Two smaller observable
differences remain.

First, the throw sits at `:78`, **before** the `try` that opens at `:88`. On the pre-change route an
already-cancelled token surfaced from `await Task.Run(..., cancel)` inside that try, was caught by
`catch (System.Exception e)` at `:127`, and was logged through `logger.Error(e.Message, e)` before
being rethrown. On the adoption path the exception now bypasses that catch, so the `logger.Error`
entry is not emitted.

Second, the exception type differs. `Task.Run(func, token)` with an already-cancelled token yields a
cancelled task, so the await threw `TaskCanceledException`; `cancel.ThrowIfCancellationRequested()`
throws `OperationCanceledException`. The former derives from the latter, so the test's
`ThrowAsync<OperationCanceledException>` is satisfied by both and every `catch (OperationCanceledException)`
in the call chain still matches. The sole caller
(`QuickFiler/Controllers/QfcCollectionController.cs:520-526`) wraps the call in `Task.Run(..., Token)`
and awaits it, which re-normalises a token-matched cancellation back to `TaskCanceledException` at
that boundary, so no downstream `catch` clause changes behaviour.

The in-code comment at `:70-77` reasons carefully about placement, but only about the alternative it
rejected: it explains that hoisting the throw to the top of the member would remove the `logger.Error`
for the **FromField** route. It does not record that the chosen placement removes it for the
**adoption** route. The reasoning is sound as far as it goes and the conclusion is defensible; the
comment simply understates one consequence of its own choice.

Non-blocking, and this reviewer would not recommend "fixing" it by wrapping the adoption in a
logging catch. The repository already made the opposite decision explicitly: at
`QuickFiler/Controllers/QfcCollectionController.cs:2208-2214`, issue #473 defect 2 established that
"a cancellation is a control-flow signal, not a move failure ... it must not be recorded as an
error." The delivered behaviour is more consistent with that landed decision than the pre-change
behaviour was. The recommendation is therefore to correct the comment to state both consequences and
cite #473 as the reason the missing `logger.Error` is acceptable, not to add the logging back.

### NB-10 — Minor, non-blocking, new. A leg-B test's documented contract is now stale after the matcher was widened

- File: `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:312-316` (XML summary), `:335`
  (assertion reason), `:276-277` (a second summary)
- Supporting: `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:162-190`

R1 widened the matcher from EntryID-only to reference-identity-first. The old body returned null
immediately when `mailItem.EntryID` was null or empty; the new body checks
`ReferenceEquals(carrier.MailItem, mailItem)` at `:175` before consulting the identifier at all.

`QfcQueuePurePathsTests.cs` was not revisited in the cycle and still documents the old contract. Its
summary at `:312-316` lists "a mail item with no EntryID" among the inputs that "all resolve to
null", and the assertion reason at `:335` repeats it. That is no longer the contract: a mail item
with no EntryID that is reference-identical to a carrier's `MailItem` now resolves to that carrier.
The test still passes only because its helper `MailWithEntryId(null)` constructs a fresh mock, which
is a distinct instance from any carrier's item. The summary at `:276-277` similarly still says the
resolver "matches a carrier to its mail item by `EntryID`" without mentioning identity.

This is the same class of defect as NB-2 — a documented parity or contract claim that the code no
longer satisfies — which is why it is worth recording rather than waiving. It is narrower: NB-2's
stale claim sat on a production member and masked a real behavioural gap, whereas this one sits on
test documentation and the widening it fails to describe is deliberate, safe and strictly more
permissive. This reviewer confirmed the widening is intentional and correctly motivated: the
production comments at `QfcHighConfidencePreFilter.cs:151-157` and `QfcQueue.Enqueue.cs:63-68` both
state that identity is tried first because the happy path builds the item list from the carriers'
own mail items, so an item whose `EntryID` is null or empty is still matchable.

Recommendation: update the two summaries and the one reason string to describe identity-then-EntryID,
and consider adding a positive case asserting that a null-EntryID item matches by identity, which is
currently the one branch of the new matcher with no direct negative-space test.

### NB-11 — Informational, new. `ReconcileCarriersToItems` does not consume carriers, so a duplicate identifier would map two items to one carrier

- File: `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:210-222`

`ResolveCarrier` is called once per item and performs a fresh scan each time; nothing removes a
matched carrier from the lookup. If two distinct surviving items shared an `EntryID`, both would
resolve to the same carrier and both rows would adopt the same `IFolderSearchHandler` instance.

This is recorded as informational rather than as a defect for three reasons. Outlook `EntryID` values
are unique per item in a store, so the input is not reachable in practice. Reference identity is
tried first, so the happy path — where the item list is built from the carriers' own mail items — is
an exact one-to-one mapping regardless. And the release path is a null assignment rather than a
dispose (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:466`), so two rows sharing one
handler would not produce a double-dispose; each simply drops its own reference.

It is noted so a later reader does not assume a one-to-one mapping is enforced by construction. A
secondary observation on the same member: `ResolveCarrier` reads `mailItem.EntryID` at `:172` before
the identity scan, so on a live `MailItem` it costs one COM read per item even when the first
carrier matches by identity. Moving that read below the `ReferenceEquals` check would remove it on
the common path. Neither point warrants a change on its own.

## Did the remediation introduce anything new?

The three behaviour-changing edits were each examined for consequences beyond their stated purpose.

**The reference-identity-first matcher** widens the set of inputs that match; it never narrows it.
Every input that matched before still matches, because the EntryID comparison is retained unchanged
as the second test. The only new matches are reference-identical pairs whose `EntryID` is null or
empty, which the old code rejected. This reviewer checked the existing negative test for exactly the
stranding hazard the caller raised: `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull` at
`:318-340` passes `MailWithEntryId(null)`, a freshly constructed mock that is not reference-identical
to the carrier's item, so it still resolves to null and the test retains its pinning power. The
carrier is not stranded and the test was not weakened to accommodate the change. Its documentation is
now stale, which is NB-10.

**The projection alignment** changes behaviour only in the (non-null globals, empty archive root,
leading-separator path) state, where a single leading separator is now stripped. This reviewer
checked the two states that reach this code in practice. Outside high-confidence mode
`_predeterminedFolder` is empty, so the `string.IsNullOrEmpty(folderPath)` guard returns the identity
before the archive root is consulted at all; the non-high-confidence path is untouched. A null
`_globals` still yields null and therefore the identity, preserving the behaviour for every test that
supplies no globals. `AssignFolderComboBox` measures 29/32 with its only uncovered lines being the
pre-existing `InvokeRequired` marshalling guard, and all four `AssignFolderComboBox` tests pass.

**The cancellation observation** is confined to the `_carriedFolderHandler is not null` branch, so no
un-carried row can reach it. Its consequences are analysed as NB-9 above.

**One structural check on the null-versus-empty contract.** AC14 requires the carrier overload of
`LoadItemsAsync` to return early on null and not on empty, and `ReconcileCarriersToItems` never
returns null — it returns an empty list when `items` is null. This reviewer checked whether that
could suppress an early return that previously fired, and it cannot:
`DequeueWithHighConfidenceGateWithOutcomeAsync` builds `nodes` by projecting `accepted`
(`QfcDatamodel.QueueProcessing.cs:197`), which would throw on a null `accepted` before the batch is
constructed, so `batch.PreScored` could never be null on this path either. An empty accepted set
produced an empty list before the change and produces an empty list after it. The early-return
condition at `QfcFormController.Actions.cs:116-125` is unaffected.

## Positive observations

Recorded because they are the kind of choice that is easy to get wrong and worth preserving.

1. **The remediation removed a duplicated implementation rather than adding a second one.** R1 could
   have been closed by copying the leg B matcher into `QfcHomeController`. Instead the matcher was
   hoisted onto `QfcPreScoredItem` and leg B rewired to delegate to it, so the tree now holds one
   implementation where it held one-and-a-bit. That is the harder change and the correct one: the
   two legs can no longer drift apart, which was the underlying condition that let NB-1 exist.
2. **R2 was closed by aligning the code, not by narrowing the claim.** The remediation input offered
   both options. Softening the doc comment and renaming the test would have been cheaper and would
   have satisfied the letter of the item. Aligning the guard actually closes the AC12 mismatch in the
   state that reopened it, and the parity target was left unmodified so the alignment is real rather
   than arranged.
3. **The red run was constructed so that it could distinguish two failure modes.** The R1 test is
   split into labelled stages precisely so a failure can be attributed. Its red run failing at stage
   two with stage one green is what proves the production throw branch produced the divergence; a
   test that simply asserted the final list would have produced an identical red for a defect and for
   a badly built fixture.
4. **A plan defect was reported rather than dispositioned into a pass.**
   `evidence/qa-gates/remediation-timestamp-fidelity.md` identifies that its own clause is a fixpoint
   — correcting a timestamp rewrites the mtime it is measured against — and states plainly that the
   band "is **not** satisfied ... and cannot be", while showing the substantive property R4 wanted is
   met. Claiming a pass would have been easy and unfalsifiable at a glance.
5. **The R3 comment reasons about the alternative it rejected.** Even though it understates one
   consequence (NB-9), recording *why* the throw sits inside the branch rather than at the top of the
   member is the kind of note that prevents a later contributor from "tidying" it upward and silently
   dropping the FromField route's error logging.
6. **The C# 7.3 constraint was handled in-line and explained.** The R3 test uses a `using` statement
   with a comment recording that a `using` declaration would be CS8370 in this project. That is a
   real trap in this repository's test projects and the note will save the next author a build cycle.

## Test quality assessment

| Dimension | Verdict |
|---|---|
| Framework, mocking and assertion libraries | MSTest, Moq, FluentAssertions throughout the three added tests. Compliant. |
| Determinism | No wall-clock read, no sleep, no retry, no ordering dependency. The R1 test's throw-once monitor is driven by a call counter, not by timing. |
| External dependencies | None. `MailItem` is always a Moq double; the `TryUnhookOrReplace` throw branch is driven entirely through a mocked move monitor. |
| Temporary files | None. |
| Documented intent | Each added test names its remediation item and states what the pre-change code did. The R1 test additionally labels its two assertion stages, which is what makes its red run interpretable. |
| Negative and boundary coverage | Strong. The R2 boundary test covers six inputs including the newly aligned empty-archive-root case; the R3 test asserts three separate consequences of cancellation rather than only the exception. One gap is noted as NB-10: the identity-match branch for a null-EntryID item has no direct positive test. |
| RED-first evidence | Met for all three tests. R1's is the strongest and is analysed above. R2 and R3 share `evidence/regression-testing/r2-r3-red.md`, which records the R3 failure as "no exception was thrown" — a discriminating red rather than a generic one. |
| Existing tests preserved | Verified. AC13's `Times.Never` and `preFilterInvoked` assertions are present and unmodified in both named files. Exactly one existing assertion changed, and it is the one R2 explicitly authorises. |
| Independent confirmation at head | The retained TRX at `TestResults/p2-t5/` records 12 discovered, 12 passed, 0 failed, covering all three remediation regression tests plus the AC7, AC9, AC12 and AC16 pinning tests and both pre-existing `ResolveCarriedHandler` tests. |

## Verdict

The remediation did what it was asked to do. All four items are fixed in the source, not merely
claimed, and the two that carried real behavioural weight — R1 and R2 — were each closed by the more
expensive and more correct of the two available options. The three new findings are documentation and
informational; none describes a behaviour the code gets wrong.

Blocking findings: **0**. The change is ready to merge.
