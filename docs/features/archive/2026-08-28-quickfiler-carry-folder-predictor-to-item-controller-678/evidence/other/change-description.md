# P1-T11 — Change description (issue #678)

Timestamp: 2026-09-01T23-34

## Summary

QuickFiler scored every accepted mail item twice in high-confidence mode: once by the dequeue-time
confidence gate, and again by the item controller after `Show()`. The first pass built and
initialised a `FolderPredictor`, read two scalars off it, and let it fall out of scope; the item
controller then built and initialised a second predictor for the same item.

This change carries the already-initialised handler forward from the gate to the item controller, on
both display legs, and adopts it in place of the second initialisation.

## The AC12 normalisation decision, and which side was normalised

**The consumer side was normalised.**

`FolderScoringService.ScoreAsync` returns the RAW top-suggestion path, read straight from
`predictor.Suggestions.ToArray(1)`. `FolderPredictor.FolderArray` stores the **projected** form:
`AddSuggestions` maps every suggestion through `ProjectSuggestionPath`, which strips
`_globals.Ol.ArchiveRootPath` plus a separator from the front of an archive-rooted path,
case-insensitively, when the remainder is non-empty.

For an archive-rooted suggestion the two forms differ, so `_itemViewer.FolderContains` was probed
with a value that could not be present in the combo box. The probe missed, the code fell through to
`SetFolderSelectedIndex`, and the carried predetermined folder had no effect at all — silently, for
exactly the suggestions the archive root is most likely to produce.

`QfcItemController.AssignFolderComboBox` now projects `_predeterminedFolder` through a new
`internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath)` before
the containment probe and before `SetFolderSelectedItem`, so both sides of the comparison are in the
same form.

Two properties of that choice are deliberate:

- **The projection is duplicated rather than reused.** `FolderPredictor.ProjectSuggestionPath` is
  `private` and lives under `UtilitiesCS/`, which AC23 forbids this change from touching. The
  duplicate mirrors the original statement for statement and carries a comment saying why it is a
  duplicate, so a later reader does not remove it as redundant.
- **The projection is the identity when the archive root is null or empty.** That preserves the
  pre-change selection behaviour exactly for the standard path and for every existing test that
  supplies no globals (AC11).

**Why the consumer side rather than the producer side.** Normalising `FolderScoringService.ScoreAsync`
would also close the mismatch, but that class carries `[ExcludeFromCodeCoverage]` and is COM-bound,
so the corrected behaviour could not be pinned by any headless test, and the mismatch would return
the moment a future producer published a raw path. Normalising at the point of comparison closes it
for every producer and is directly testable. `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder`
fails against the unnormalised form and passes after; the failing run's invocation log is recorded in
`evidence/regression-testing/ac12-path-normalisation.md`.

## The AC15 accepted behavioural delta

**Reusing the scan-time suggestion set freezes conversation-derived (`CtfMap`) suggestions at scan
time rather than re-deriving them at display time, for both legs.**

Before this change the item controller ran its own `FolderPredictor.InitAsync(FromField)` pass
immediately before the row was displayed, so any conversation-derived suggestion that became
available between the scan and the display was picked up. After this change the row displays the
suggestion set the gate computed when it accepted the item.

**The scan-to-display interval is longer for leg B.** Leg A is the first page: the gate scores it
during startup and `RunAsync` displays it moments later, so the interval is short and bounded by
`QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`, which is 12 seconds. Leg B is every
subsequent page: those items are scored by the background dequeue as the queue drains, then wait in
`QfcQueue` until the user pages forward to them. That wait is unbounded and is a function of how
fast the user files, so a leg-B row can display a suggestion set computed a long time before it is
seen. This is accepted deliberately as the cost of removing the duplicate scoring pass, and it is
recorded here rather than discovered later.

**Bayesian suggestions and the recents list are unaffected**, because the folder array is still built
lazily at display time. `FolderPredictor.FolderArray` is a property whose getter populates
`_folderList` on first access, drawing the top-five scored suggestions from `Suggestions` and then
appending `_globals.AF.RecentsList`. The carried handler is the same object either way, so the array
is still materialised when `AssignFolderComboBox` reads it, and the recents portion still reflects
the recents list as it stands at display time. Only the *scores* are frozen, because they were
computed during the scan; the array's construction, ordering and recents section are not.

## What changed, by concern

### Producer

- `QfcPreScoredItem` gained an `IFolderSearchHandler FolderHandler` member and a third, optional
  constructor parameter. `MailItem` and `PredeterminedFolder` keep their names, types and non-null
  contracts.
- `IFolderScoringService.ScoreAsync` widened to
  `Task<(long Score, string TopFolder, IFolderSearchHandler Handler)>`.
  `FolderScoringService.ScoreAsync` publishes the predictor its own `InitAsync` call produced
  instead of discarding it. It keeps its `[ExcludeFromCodeCoverage]` attribute and its justification
  remark block.

### Datamodel boundary

- `QfcStreamingDequeueConfidenceGate`'s `scoreLoader` delegate widened to the same tuple, and its
  acceptance projection constructs the carrier with the handler. The handler is therefore present on
  `QfcGateBatch.Accepted`.
- `QfcDatamodel.QueueProcessing.ScoreRemainingQueueMailItemAsync` forwards the third element, so it
  reaches `QfcDequeueBatch.PreScored`.

### Leg A, the first page

- `QfcHomeController.RunAsync` in enabled mode calls `DequeueNextItemGroupWithOutcomeAsync`, the only
  member that surfaces the carriers, and selects the `IList<QfcPreScoredItem>` overload of
  `LoadItemsAsync`. Disabled mode is unchanged and still selects the `IList<MailItem>` overload.
- `QfcItemGroup` carries the handler alongside `PredeterminedFolder`.
- `QfcCollectionController.EncapsulateItemGroup` and the carrier overload of
  `LoadControlsAndHandlers_01Async` thread it into the `QfcItemController` constructor. Both were
  relocated into a new partial part, because the base file is already far over the 500-line limit.

### Leg B, every subsequent page

- `QfcHomeController.IterateQueueAsync` forwards `batch.PreScored` into `IQfcQueue.EnqueueAsync`.
- `QfcQueue` carries it through to the rows it constructs, matching carrier to item by `EntryID`
  rather than by position, because `UnhookDequeuedNodes` can replace an element of the item list in
  place.
- An injectable-delegate seam, `QfcQueue.ItemControllerFactory`, was introduced (form 2 of
  `.claude/rules/csharp.md`, mirroring `QfcDatamodel.ScoringServiceFactory`). No new interface. Its
  production default reproduces the previous construction expression exactly.

### Consumer

- `QfcItemController` stores the carried handler and, inside the `varList is null` branch of
  `LoadFolderHandlerAsync` only, adopts it and returns without touching `_folderPredictorFactory` or
  `FolderPredictor.InitAsync`.
- The `FromArrayOrString` branches of both `LoadFolderHandler` and `LoadFolderHandlerAsync` are
  unchanged; a carried handler is never adopted there, because a caller-supplied folder search is not
  a per-item scoring pass.
- `Cleanup` releases the carried reference alongside `_folderHandler`.

## Options considered and not implemented

- **Carrying only the top-folder string**, which the original issue #427 document proposed. Rejected
  on the evidence in the research: the item controller still needs `FolderArray`, `Suggestions` and
  `FolderRowArray`, all of which come from `_folderHandler`, so it would still have run the second
  `InitAsync` pass. Carrying the string alone changes which entry is preselected — behaviour the
  code already implements — and saves no scoring work.
- **Activating `QfcHighConfidencePreFilter.FilterAsync`.** Rejected: AC13 requires it to stay
  dormant, and issue #233 moved high-confidence enforcement from post-display filtering to
  dequeue-time gating. Its `QfcPreScoredItem` construction site was updated only so it compiles and
  populates the new member.
- **Adding `InitAsync` to `IFolderSearchHandler`** so the carried handler could be re-initialised
  through the narrow seam. Out of scope (AC22) and unnecessary: the carried handler is already
  initialised, which is the whole point.
- **Making `FolderPredictor.ProjectSuggestionPath` accessible** and calling it from QuickFiler.
  Rejected because it is a change under `UtilitiesCS/`, which AC23 forbids.
- **Positional matching of carriers to items in leg B.** Rejected because `UnhookDequeuedNodes` can
  replace an element in place, which would pair a row with another row's handler silently.
