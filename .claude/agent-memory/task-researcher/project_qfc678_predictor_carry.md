---
name: qfc678-predictor-carry
description: "Issue #678 (QuickFiler carry folder predictor): live producer is the #233 dequeue gate NOT QfcHighConfidencePreFilter; carrier path fully dormant; second scoring pass happens on TWO legs (LoadSecondaryAsync and QfcQueue); FolderPredictor is safely reusable"
metadata:
  type: project
---

Issue #678 research completed 2026-08-31 against `prep-678` (base `origin/main` @ `2b85134b`).

**Facts that were surprising and are not obvious from the code layout:**

- The class the issue names as the producer (`QfcHighConfidencePreFilter.FilterAsync`) is **dormant**.
  The live scoring producer is `QfcDatamodel.ScoreRemainingQueueMailItemAsync` ->
  `IFolderScoringService.ScoreAsync` driven by `QfcStreamingDequeueConfidenceGate` (#233). Both emit
  `Probability debug` lines, so log-based repro descriptions attribute the first pass to the wrong class.
- `QfcDequeueBatch.PreScored` is produced but **never read** by any production member. The whole
  `LoadItemsAsync(IList<QfcPreScoredItem>)` / `LoadControlsAndHandlers_01Async(IList<QfcPreScoredItem>, ...)`
  chain has no production call site.
- The second `InitAsync(FromField)` happens on **two** legs: first page via
  `QfcCollectionController.LoadSecondaryAsync`, and every subsequent page via
  `QfcQueue.LoadControllersViewersAsync` -> `QfcItemController.InitializeAsync` ->
  `PopulateFolderComboBoxAsync(default, null)`. Fixing only the first leg leaves the symptom present
  after page one. The `QfcQueue` leg is not mentioned in the issue.
- **`LoadFolderHandler` (sync) never initialises anything.** The default factory calls
  `new FolderPredictor(globals, objItem, options)`, and that 3-arg ctor discards both `objItem` and
  `options`. So the sync path yields a recents-only combo on both its branches, and the
  conversation-expansion `FromArrayOrString` replication is silently a no-op. Separate latent defect.
- `FolderPredictor` after `InitAsync(FromField)` holds **only** an in-memory `FolderScorer` dictionary
  plus app-scoped `_globals`/`_olApp`. No MailItem, no MAPIFolder, no MailItemHelper, no
  CancellationToken, nothing disposable, no thread affinity — and the current code already moves the
  instance from a `Task.Run` pool thread to the UI thread. It is safely reusable.
- Latent trap on the dormant carrier path: `PredeterminedFolder` is the **raw** suggestion path but
  `FolderArray` holds the **archive-prefix-stripped** projection (`ProjectSuggestionPath`), so
  `_itemViewer.FolderContains(_predeterminedFolder)` fails for archive-rooted folders and silently
  falls back to index 1. Activating the carrier path without normalising one side changes the
  preselected folder.

**Why:** the issue body's analysis and line numbers were taken at `988e819b` and several are moved or
mischaracterised — notably `QfcHomeController.cs:310` "sole overload-selection call site" (actually
`:307`, and it is not a selection point at all) and the claim that
`QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`/`:277` need rewriting (they are
disabled-mode `Times.Never` assertions that must stay; the enabled-mode test needing rewrite is
`RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` at `:111-210`, unnamed in the issue).

**How to apply:** when planning or reviewing #678, do not treat `QfcHighConfidencePreFilter` as live,
do not accept a leg-A-only fix as closing the issue without saying so, and require a normalisation
test for the raw-vs-projected folder path before the carrier path goes live. File-size constraints:
`QfcCollectionController.cs` 2446 lines (and `[ExcludeFromCodeCoverage]`), `QfcQueue.cs` 610,
`QfcItemController.ViewerSetup.cs` 499, `QfcItemController.Initialization.cs` 489 — new members need
new partial parts.

Related: [[qfc-high-confidence-dual-pipeline]], [[qfc424-high-confidence-startup-stall]],
[[qfc-collection-controller-defects-468]].

Research artifact:
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/research/2026-08-31T21-15-quickfiler-carry-folder-predictor-research.md`
