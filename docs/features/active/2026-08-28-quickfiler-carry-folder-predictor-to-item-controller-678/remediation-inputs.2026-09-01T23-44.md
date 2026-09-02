# Remediation Inputs — Issue #678, Cycle 1

- Timestamp: 2026-09-01T23-44
- Branch: `bug/quickfiler-carry-folder-predictor-to-item-controller-678`
- Base ref (literal SHA, use in every git command): `807fb0bb6e5e49f43efa6b256b05960bf078ca19`
- Source audits: `code-review.2026-09-01T23-35.md`, `feature-audit.2026-09-01T23-35.md`, `policy-audit.2026-09-01T23-35.md`
- Cycle entry reason: the reviewer recorded 0 Blocking findings. The orchestrator is nonetheless
  opening this cycle, because three of the Non-blocking findings are defects **introduced by this
  change** rather than pre-existing conditions, and the deferral route agreed for this run covers
  pre-existing and out-of-scope items only.

## Scope of this cycle

Four items: R1, R2, R3, R4. Every fix stays inside the existing AC23 footprint
(`QuickFiler/`, `QuickFiler.Test/`, and this feature folder). No acceptance criterion text is
edited. No new acceptance criterion is added.

Explicitly NOT in this cycle, and NOT to be fixed: NB-4 (AC20 per-member coverage), NB-6
(pre-existing oversized files), NB-7 (informational), NB-8 (AC11/AC12 criterion-text tension).
These are deferred to a single consolidated follow-up issue filed from a separate branch after
merge. Do not promote them, do not create a potential entry, and do not open a GitHub issue.

---

## R1 — Leg A displays the pre-unhook carrier set (from NB-1, Major)

**State the invariant, not the symptom.** The invariant this change must preserve is:

> The set of mail items displayed on leg A is exactly the set that survived
> `UnhookDequeuedNodes`. No item whose `UnhookItem` call failed may be displayed, and no item that
> `TryUnhookOrReplace` pulled out of the master queue may go undisplayed.

Do not satisfy this by making `PreScored` and `Items` textually agree, and do not satisfy it by
relaxing the assertion. Trace an accepted value through to the boundary that consumes it
(`QfcFormController.LoadItemsAsync` and onward to the row that is actually rendered) and show that
the invariant holds there.

Verified mechanism, re-derive it yourself rather than trusting this summary:

- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:193` returns
  `new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop)`. `Items` is the
  post-unhook list; `PreScored` is `accepted`, captured before the unhook pass.
- `TryUnhookOrReplace` (`:31-65`) is not read-only. On an `UnhookItem` throw it performs
  `nodes.Remove(node)`, then `node = _masterQueue.TryTakeFirst()`, then `nodes.Insert(i, node)`.
- Therefore on that path the two collections diverge in both directions: the failed item is in
  `PreScored` but not in `Items`, and the substitute is in `Items` but not in `PreScored`.
- `QuickFiler/Controllers/QfcHomeController.cs:307-320` now passes `preScored` to
  `LoadItemsAsync` in high-confidence-enabled mode. Before this change leg A passed `listEmail`
  (`batch.Items`).

Consequences, both live on the `UnhookItem` throw path: an item that is still hooked to the
`EmailMoveMonitor` is displayed, and a substitute that has already left the master queue is never
displayed and is lost for the session.

The same hazard on leg B was already mitigated in this changeset by `EntryID` matching. Leg A was
not. Mirror the leg B mitigation, or reconcile `PreScored` against `Items` at the leg A boundary.
Prefer reusing the existing leg B helper over writing a second one.

Additionally: the XML documentation block at
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:165-170` currently asserts that `Items` and
`PreScored` "describe one dequeue rather than two". That is true only on the happy path. Correct
the comment so it states the throw-path divergence.

**Acceptance for R1**
1. A new MSTest test drives `TryUnhookOrReplace` down its throw branch (the `UnhookItem` mock
   throws once, and `_masterQueue.TryTakeFirst()` yields a distinct substitute) and asserts that
   the item set reaching the leg A load boundary contains the substitute and does not contain the
   failed item. The test must fail against the current code; record that red run.
2. The analyzer build and the nullable build both exit 0.
3. The doc block at `QfcDatamodel.QueueProcessing.cs:165-170` no longer claims an unconditional
   correspondence.

---

## R2 — `ProjectPredeterminedFolder` does not mirror `ProjectSuggestionPath` (from NB-2, Minor)

**Invariant:** the carried `PredeterminedFolder` and the `FolderArray` entries must be the same
projection of the same input, so that `_itemViewer.FolderContains` matches for every archive-rooted
suggestion the predictor can produce. The test must pin that boundary behaviour, not the internal
equality of two helper bodies.

Verified divergence:

- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` guards on `_globals is null`, then
  unconditionally builds `archivePrefix = _globals.Ol.ArchiveRootPath + "\\"`. With non-null globals
  and an EMPTY `ArchiveRootPath`, `archivePrefix` is `"\"`, so a `folderPath` beginning with a
  separator and longer than one character has that separator stripped.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:253-256` guards instead on
  `string.IsNullOrEmpty(archiveRootPath)` and returns the input unchanged in that same state.

So the two disagree for (non-null globals, empty archive root, leading-separator path), which
reopens exactly the AC12 mismatch the change set out to close.

The doc comment at `QfcItemController.FolderHandling.cs:246-247` states the projection "mirrors
`FolderPredictor.ProjectSuggestionPath` exactly". As written that is false. The test named
`...MatchFolderPredictorProjection` in
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:212-239` asserts a
parity that does not hold.

**Acceptance for R2**
1. Either align the projection so the two agree on the (non-null globals, empty archive root) state,
   or narrow the documented claim and the test name so neither asserts unconditional parity. State
   which option was chosen and why.
2. A test covers the (non-null globals, empty archive root, leading-separator path) case explicitly
   and asserts the chosen behaviour at the `FolderContains` boundary.
3. AC12's existing archive-rooted test continues to pass unmodified.

---

## R3 — Adoption path does not observe the cancellation token (from NB-3, Minor)

At `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:68-77` the carried-handler adoption
branch returns without observing `cancel`. Every pre-change route reached the predictor through
`Task.Run(..., cancel)`, which throws for an already-cancelled token.

**Invariant:** an already-cancelled token produces the same observable outcome on the adoption path
as it did on the pre-change path.

**Acceptance for R3**
1. The adoption branch observes `cancel` in a way that reproduces the pre-change behaviour for an
   already-cancelled token.
2. A test passes an already-cancelled token down the adoption path and asserts that outcome.
3. AC7's single-initialisation test and AC9's negative guard both still pass unmodified.

---

## R4 — Evidence timestamps are not real clock values (from NB-5, Minor)

All 13 artifacts under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/`
declare `Timestamp:` values running 45-85 minutes ahead of the files' own mtimes, landing on the
following calendar date. Relative ordering is correct; the absolute values are neither local time
nor UTC.

**Acceptance for R4**
1. Each of the 13 `Timestamp:` values is corrected to a real clock value consistent with that
   artifact's mtime, retaining the existing `yyyy-MM-ddTHH-mm` format and the existing relative
   ordering.
2. No other field in any of those artifacts is altered. The recorded `EXIT_CODE:`, `Command:` and
   `Output Summary:` values are factual records of runs that already happened and must not be
   rewritten.
3. State the method used to derive the corrected values.

---

## Constraints for the whole cycle

- Do not modify `artifacts/orchestration/orchestrator-state.json`.
- Do not write under `.claude/agent-memory/`.
- Do not edit `.git/info/exclude` or any git configuration; it is shared across worktrees.
- Do not add or remove any `[ExcludeFromCodeCoverage]` attribute.
- Do not weaken, delete, or rename any existing passing test to accommodate a fix.
- Re-run the full four-gate C# toolchain in order after the changes and record fresh evidence.
- Never embed absolute host paths in committed artifacts.
