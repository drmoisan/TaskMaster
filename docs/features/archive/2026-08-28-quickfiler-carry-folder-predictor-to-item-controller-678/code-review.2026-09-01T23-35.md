# Code Review — issue #678, carry the folder predictor to the item controller

- Timestamp: 2026-09-01T23-35
- Head: `d1f51e3a99cc5a98f622663df27abac7c8043f11`
- Base: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Reviewed surface: all 35 changed source paths (16 under `QuickFiler/`, 19 under `QuickFiler.Test/`)

## Summary of the change as delivered

The producer (`FolderScoringService.ScoreAsync`) now publishes the `FolderPredictor` it already
initialised as a third tuple element instead of letting it fall out of scope. That handler is
threaded through `QfcStreamingDequeueConfidenceGate`, `QfcDatamodel.ScoreRemainingQueueMailItemAsync`
and `QfcPreScoredItem` to both display legs, and `QfcItemController.LoadFolderHandlerAsync` adopts it
in place of a second `FolderPredictor.InitAsync(FromField)` pass.

The design is sound. The carried type is the narrow `IFolderSearchHandler` seam rather than the
concrete predictor, so the consuming surface stays minimal. The adoption is confined to the exact
branch where a per-item scoring pass would otherwise run, and the negative case is pinned by a test.
Two members were relocated into new partial parts rather than growing files already past the 500-line
limit, and both relocations left a pointer comment at the original site. The one behavioural delta
the design forces — freezing conversation-derived suggestions at scan time — is stated in the change
description with its per-leg severity analysis rather than discovered later.

## Findings

Blocking: **0**. Non-blocking: **8**.

### NB-1 — Major, non-blocking. Leg A now displays the pre-unhook carrier list, which can diverge from the dequeued item list

- File: `QuickFiler/Controllers/QfcHomeController.cs:299-320`
- Supporting: `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:193` and `:31-66`

`DequeueWithHighConfidenceGateWithOutcomeAsync` returns

```csharp
return new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop);
```

`Items` is the list *after* `UnhookDequeuedNodes` has run over it; `PreScored` is `accepted`, the
list built *before* it. `TryUnhookOrReplace` at `QfcDatamodel.QueueProcessing.cs:31-66` is not a
read-only pass: when `_moveMonitor.UnhookItem(node)` throws, it executes `nodes.Remove(node)`, pulls
a fresh item from `_masterQueue.TryTakeFirst()` and inserts it at the same index. The two collections
can therefore differ in membership.

Before this change `RunAsync` displayed `batch.Items`. After it, `RunAsync` displays `preScored`, so
on the unhook-failure path the first page will:

1. display the item whose `UnhookItem` threw — an item deliberately removed from `Items` and still
   hooked to the move monitor, which `LoadControlsAndHandlers_01Async` then hooks a second time; and
2. omit the substituted replacement item, which has already been taken out of `_masterQueue` by
   `TryTakeFirst()` and is therefore lost rather than deferred.

The executor identified this exact hazard for leg B and mitigated it there — `QfcQueue.Enqueue.cs`
matches carriers to items by `EntryID` precisely "because `UnhookDequeuedNodes` can replace an
element of the item list in place" — but leg A has no equivalent reconciliation. `listEmail` is
assigned from `batch.Items` at `QfcHomeController.cs:311` and then goes unused in the
high-confidence branch.

Non-blocking because the divergence requires `UnhookItem` to throw, which is an already-logged error
path; because AC4 mandates the switch to the carrier overload; and because the pre-existing
`TryUnhookOrReplace` already injects an unscored item into a high-confidence batch, so the path was
not clean before either.

Recommendation: in `RunAsync`, project `preScored` against `batch.Items` by `EntryID` before handing
it to `LoadItemsAsync`, reusing the shape of `QfcQueue.ResolveCarriedHandler`. Alternatively, have
`DequeueWithHighConfidenceGateWithOutcomeAsync` rebuild `PreScored` from the post-unhook `Items` so
the two collections are guaranteed to describe one dequeue, which is what the member's own
documentation comment at `QfcDatamodel.QueueProcessing.cs:165-169` already claims.

### NB-2 — Minor, non-blocking. The projection helper does not mirror `ProjectSuggestionPath` in the case its documentation and test name claim

- File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:243-271`
- Supporting: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-857`
- Test: `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:212-239`

The doc comment states the projection "mirrors `FolderPredictor.ProjectSuggestionPath` exactly". The
two guards differ. `ProjectSuggestionPath` returns early only when `_globals is null`:

```csharp
if (_globals is null) { return folderPath; }
var archivePrefix = _globals.Ol.ArchiveRootPath + "\\";
```

`ProjectPredeterminedFolder` returns early when the archive root is null **or empty**. With non-null
globals and an empty or null `ArchiveRootPath`, `ProjectSuggestionPath` forms the one-character
prefix `\` and strips a leading backslash from any path that starts with one, while
`ProjectPredeterminedFolder` returns the input unchanged. In that state `FolderArray` entries are
stripped and the probed value is not, which reopens the raw-versus-projected mismatch AC12 exists to
close.

The boundary test compounds this: `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`
asserts that "an empty archive root is the identity", which is the opposite of what the method named
in the test's own title does under that input. The test pins the new helper correctly; the claim of
parity in its name and in the doc comment is what is unsupported.

Non-blocking: an empty `ArchiveRootPath` with non-null globals is not a state production is expected
to reach, and the pre-change code missed the probe in that state as well, so this is an incompletely
closed edge case rather than a regression.

Recommendation: change the guard to `archiveRootPath is null` and keep the null-path guard separate,
or soften the doc comment and rename the test to describe the helper's own contract.

### NB-3 — Minor, non-blocking. The adoption path no longer observes the cancellation token

- File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:68-77`

Every pre-change route through the `varList is null` branch went through
`await Task.Run(..., cancel).ConfigureAwait(false)`, which throws `OperationCanceledException` for an
already-cancelled token. The adoption path assigns `_folderHandler` and returns without consulting
`cancel`, so a row whose load is cancelled mid-flight now completes normally on the carried path.

Recommendation: add `cancel.ThrowIfCancellationRequested();` immediately before the adoption, which
restores the prior cancellation semantics at negligible cost.

### NB-4 — Minor, non-blocking. AC20's per-member clause fails for two relocated members

- Files: `QuickFiler/Controllers/QfcQueue.Enqueue.cs:67-139` (`EnqueueAsync`, 0/46) and `:169-212`
  (`LoadControllersViewersAsync`, 0/24)

Reproduced independently from `coverage/coverage.cobertura.xml`. Full analysis, including the
verification that both members were at zero at the base ref, is in
`policy-audit.2026-09-01T23-35.md` under "Disposition of the sub-floor new-file row". No repository
policy floor is breached and there is no regression on changed lines.

### NB-5 — Minor, non-blocking. Declared evidence timestamps do not match the artifacts' actual creation times

- Files: all thirteen artifacts under `evidence/qa-gates/`

Each artifact declares a `Timestamp:` between `2026-09-02T00-02` and `2026-09-02T00-34`. The actual
file modification times run from `2026-09-01 22:42` to `2026-09-01 23:25` local, and the commit that
contains them, `d1f51e3a`, is dated `2026-09-01 23:24:02 -0400`. Every declared value is in the
future relative to the file it labels, by an inconsistent margin of roughly 45 to 85 minutes, and
on the following calendar date. The values are neither local time nor UTC, which would be
`02:42` to `03:25` on 09-02.

The ordering of the declared timestamps is internally consistent and matches the ordering of the
modification times, and every substantive figure in the artifacts was reproduced by this reviewer
against the on-disk Cobertura document. This is therefore a provenance-labelling defect, not a
fabricated result. It matters because the timestamp is the only thing tying an artifact to the tree
state it describes.

Recommendation: derive evidence timestamps from a single clock read at artifact-write time.

### NB-6 — Minor, pre-existing, not introduced here. Three files remain over the 500-line limit

| File | Base | Head | Over by |
|---|---:|---:|---:|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2446 | 2336 | 1836 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 827 | 792 | 292 |
| `QuickFiler/Controllers/QfcQueue.cs` | 610 | 505 | 5 |

All three were over the limit at the base ref and all three are smaller after this change. AC21 is
satisfied on its own terms and no file crossed the limit. `QfcQueue.cs` at 505 is five lines over and
could be brought under by relocating one more member, which is the cheapest of the three to close.
Already registered by the executor in `evidence/other/out-of-scope-register.md` item 3 for the
consolidated follow-up issue.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` moved 499 -> 500. It sits exactly at the
cap and does not exceed it, but it has no headroom left; the next edit to it must relocate rather
than extend.

### NB-7 — Informational. Leg B's end-to-end carry is proved in two halves with the joining statement unproven

- `QfcQueue.ResolveCarriedHandler` is pinned at 14/14 by two direct tests.
- The `ItemControllerFactory` production default is pinned at 11/11 by
  `ItemControllerFactory_DefaultInvocation_BuildsControllerCarryingTheHandler`, which invokes the
  default and reads `_carriedFolderHandler` off the constructed controller.
- The two statements in `LoadControllersViewersAsync` that join them —
  `x.grp.CarriedFolderHandler = ResolveCarriedHandler(preScored, x.grp.MailItem);` and the factory
  invocation at `QfcQueue.Enqueue.cs:190-199` — are themselves uncovered.

The composition is therefore inferred from the two halves rather than executed. This is an honest
consequence of the host binding and the executor recorded it; it is noted here so a later reader does
not read "leg B is covered" as end-to-end proof.

### NB-8 — Minor. AC11 and AC12 are in tension as authored

AC11 requires the preselected folder entry to be "identical to the entry the pre-change code
preselects". AC12 requires the archive-prefix normalisation that, for an archive-rooted suggestion,
deliberately changes the preselected entry from the index-1 fallback to the named folder. Both cannot
hold literally for the archive-rooted case.

Read together, AC11 governs the cases AC12 does not touch and AC12 is the more specific criterion for
the archive-rooted case. The delivered code implements exactly that reading, and the change
description states the resolution as AC12 requires. The defect is in the criteria text, not in the
code. Recorded so a later audit does not read AC11 as unmet.

## Positive observations

These are recorded because they are the kind of choice that is easy to get wrong and worth
preserving.

1. **The rewritten pinning assertion was checked for pinning power rather than assumed.**
   `QfcHomeControllerRunAsyncHighConfidenceTests.cs:231-256` replaces a reference-equality constraint
   with a shape constraint and carries a comment explaining that the naive rewrite would have been
   satisfied trivially after the change. The reviewer confirmed the delivered predicate still
   discriminates: `carriers.Count == unfilteredInitialBatch.Count` is true for both lists, and the
   `ReferenceEquals(carriers[0].MailItem, unfilteredInitialBatch[0])` clause is what does the work.
2. **The disabled-mode assertions AC13 protects are genuinely untouched.** Baseline lines 246 and 277
   of `QfcHomeControllerRunAsyncHighConfidenceTests.cs` fall between diff hunks; the added
   `DequeueNextItemGroupWithOutcomeAsync` setup was placed in the shared arrange helper so both
   overloads stay configured and the disabled-mode tests keep exercising their own path.
3. **The seam was narrowed in response to its own coverage measurement, not to pass a gate.**
   `ItemControllerFactory` originally took a concrete `QfcItemGroup`, which made its production
   default unreachable without a live window (1/12). It was narrowed to `IItemViewer` so the default
   could be invoked with a double, taking it to 11/11. The whole toolchain loop was then restarted.
4. **The gate that fired on a documentation mention was satisfied rather than dismissed.** The
   attribute-invariant check flagged an added line that merely quoted the exclusion attribute's name
   in a comment. The comment was reworded and a second, prose-immune measurement was added. Declaring
   it a false positive would have been defensible and would have cost the gate its discriminating
   power.
5. **The `#pragma warning disable CS0618` was relocated verbatim.** A relocation is a common place to
   quietly widen or drop a suppression; this one carries its original justification comment intact.

## Test quality assessment

| Dimension | Verdict |
|---|---|
| Framework, mocking and assertion libraries | MSTest, Moq, FluentAssertions throughout. Compliant. |
| Determinism | No wall-clock read, no sleep, no retry, no ordering dependency in any added test. |
| External dependencies | None. `MailItem` is always a Moq double; the one concrete `QfcQueue` is built with a null home controller and mocked globals. |
| Temporary files | None. |
| Documented intent | Every added test carries an XML summary naming the criterion it serves and, where relevant, what the pre-change code did. |
| Negative and boundary coverage | Strong. `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull` covers five distinct negative inputs; `ProjectPredeterminedFolder_BoundaryCases_...` covers six; the AC9 guard proves the carried handler is ignored on the `FromArrayOrString` path. |
| RED-first evidence | `evidence/regression-testing/ac16-red.md` records a scoped single-test run at exit 1 with `Total tests: 1, Failed: 1`, the sentinel exception identified by type and message, and a preceding exit-0 build to rule out a stale assembly. This satisfies the RED-first standard. |
| Seam used by the AC16 test | The carried handler is injected by reflection into `_carriedFolderHandler` rather than through the constructor. Constructor storage is pinned separately by `QfcItemController.InitializationTests` and by the leg-B factory-default test, so the invariant is covered from both directions. |

## Verdict

The change is well-constructed, thoroughly evidenced and does what its acceptance criteria describe.
No finding blocks the merge. NB-1 is the one finding with real behavioural weight and is the item
this reviewer would put first in the consolidated follow-up issue.
