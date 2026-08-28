# quickfiler-carry-folder-predictor-to-item-controller (Issue #678)

- Date captured: 2026-08-28 (originally identified 2026-08-24; promoted from a stranded worktree during cleanup)
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-carry-folder-predictor-to-item-controller/ (Issue #678)
- Found during: preparation of epic child `quickfiler-queue-datamodel-defects` (primary issue #446)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #678
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/678
- Last Updated: 2026-08-28
## Summary

Issue #427 reports that every accepted QuickFiler mail item is scored twice in high-confidence mode.
Preparation research for the `quickfiler-queue-datamodel-defects` feature established that the fix
proposed in the original #427 potential document does not actually remove the second scoring pass,
so #427 cannot be fully resolved by carrying the top-folder string alone. This entry records the
remaining consumer-side work.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8.1 VSTO add-in
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox

## Steps to Reproduce

1. Enable High Confidence mode and launch QuickFiler.
2. Enable debug logging and inspect the `Probability debug` entries for a single accepted item.
3. Observe one entry from the pre-UI scan and a second, independent classification after the form is shown.

## Expected Behavior

An item accepted by the confidence gate carries its already-initialised folder predictor forward, so
the item controller populates the folder combo, the suggestion list and the folder array from that
result instead of recomputing them.

## Actual Behavior

The initialised predictor is discarded and the full `FolderPredictor.InitAsync(InitOptions.FromField)`
sequence runs a second time per accepted item after `Show()`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: two `Probability debug` lines per accepted item, as recorded in the original #427 potential document.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: wasted work, not incorrect behavior. It occurs after `Show()`. The user-visible effect is slower
folder-combo population and redundant Outlook COM traffic proportional to the number of items on screen.

## Suspected Cause / Notes

Verified at `988e819b` during preparation research for issue #446. Full analysis was recorded at
`docs/features/active/quickfiler-queue-datamodel-defects-446/research/2026-08-24T09-50-quickfiler-queue-datamodel-defects-research.md`
§ 4.5 in the worktree that captured it; that worktree's copy of the feature folder is a superseded
pre-execution draft and was not carried into the merged feature (the merged version does not include
this consumer-side follow-up).

The original #427 potential document proposed activating the dormant
`QfcFormController.LoadItemsAsync(IList<QfcPreScoredItem>)` overload so the predetermined folder is
carried forward. That premise is incorrect:

- `_predeterminedFolder` is consumed only for combo-box *selection* inside `AssignFolderComboBox`
  (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:193-199`).
- The surrounding code still requires a fully-initialised predictor: `FolderArray`, `Suggestions`
  and `FolderRowArray` all come from `_folderHandler` (`IFolderSearchHandler`, declared
  `QuickFiler/Controllers/QfcItemController.cs:41`), which is produced only by
  `LoadFolderHandler`/`LoadFolderHandlerAsync`.
- So even on the carrier path the item controller must still run
  `FolderPredictor.InitAsync(FromField)`. Carrying only the top-folder string changes which entry is
  preselected, a behavior the code already implements, and saves no scoring work.

Removing the second scoring pass requires carrying the initialised `FolderPredictor` /
`IFolderSearchHandler` from `FolderScoringService.ScoreAsync`
(`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:184`, where it is discarded) through to
`_folderHandler`.

Line numbers above were verified against commit `988e819b` (2026-08-24) and should be re-checked
against current `main` before planning, since the referenced files may have moved since.

## Proposed Fix / Validation Ideas

Files that must change, none of which were owned by the `quickfiler-queue-datamodel-defects` feature:

- `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` — widen `IFolderScoringService.ScoreAsync` to surface the predictor
- `QuickFiler/Controllers/QfcItemGroup.cs:50` — new carried member
- `QuickFiler/Controllers/QfcCollectionController.cs:428-471`, `:616`
- `QuickFiler/Controllers/QfcItemController.cs:41`, `:83-89`
- `QuickFiler/Controllers/QfcItemController.Initialization.cs:63-64`, `:108`, `:398-400`
- `QuickFiler/Controllers/QfcHomeController.cs:310` — the sole overload-selection call site

Prerequisite already landed by the `quickfiler-queue-datamodel-defects` feature (Scope 427-A): the
producer side no longer discards the scoring result, and the datamodel boundary exposes
`QfcPreScoredItem` carriers on its dequeue batch. Nothing consumes them yet; this entry is that
consumer work.

Pinned tests that must be deliberately rewritten, not deleted, because they encode the landed
decision of issue #233 that high-confidence enforcement moved from post-display filtering to
dequeue-time gating:

- `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs:137-259`
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246`, `:277`

The `Times.Never` assertion on `HighConfidencePreFilterLoader` should stay: the pre-filter class
remains dormant, and only the carrier overload would become live.

- [ ] Unit coverage areas: predictor carry-through, `QfcItemController` folder-handler population, overload selection
- [ ] Integration scenario to retest: high-confidence launch, confirming one scoring pass per accepted item and an unchanged folder-combo selection
- [ ] Manual verification notes: compare `Probability debug` log output before and after; confirm the preselected folder matches the previous behavior

Tests must use MSTest with Moq and FluentAssertions, no live Outlook COM and no temporary files, per
repository unit-test policy.

## Next Step

- [ ] Promote to GitHub issue (bug-report template), or attach as a scoped follow-up to issue #427
- [ ] Coordinate with the epic children that own the six files listed above
