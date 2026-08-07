# quickfiler-post-show-duplicate-scoring (Issue #427)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-post-show-duplicate-scoring/ (Issue #427)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #427
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/427
- Last Updated: 2026-08-07
## Summary

In QuickFiler high-confidence mode, every accepted mail item is scored twice. The dequeue gate computes the item's top folder and discards it, then the item controller re-runs the identical `MailItemHelper` plus `FolderPredictor` sequence after the form is shown, to populate the folder combo. A carrier type that exists specifically to pass the predetermined folder forward is left unused on the live path.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8.1 VSTO add-in (no Python involvement)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox

## Steps to Reproduce

1. Enable High Confidence mode and launch QuickFiler.
2. Enable debug logging and inspect the `Probability debug` entries for a single accepted item.
3. Observe one entry from `QfcDatamodel.ScoreRemainingQueueMailItemAsync` during the pre-UI scan and a second, independent classification from `QfcItemController.LoadFolderHandlerAsync (FromField)` after the form is shown.

## Expected Behavior

An item accepted by the confidence gate carries its already-computed top-folder suggestion forward, so the item controller populates the folder combo from that result instead of recomputing it.

## Actual Behavior

The score is computed, the folder is discarded, and the full scoring sequence runs a second time per accepted item after `Show()`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: two `Probability debug` lines per accepted item — one tagged `[QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]`, one tagged `[QfcItemController.LoadFolderHandlerAsync (FromField)]`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low: this is wasted work, not incorrect behavior. It occurs after `Show()`, so it does not contribute to the startup stall fixed under issue #424. The user-visible effect is slower folder-combo population and redundant Outlook COM traffic proportional to the number of items on screen.

## Suspected Cause / Notes

Verified by reading the code at `fb32b923` (all citations checked against that commit):

- `QfcDatamodel.ScoreRemainingQueueMailItemAsync` (`QuickFiler/Controllers/QfcDatamodel.cs:346-360`) calls `FolderScoringService.ScoreAsync`, which returns `(Score, TopFolder)`, and returns only `score.Score`. The computed `TopFolder` is dropped.
- After the form is shown, `QfcItemController.LoadFolderHandlerAsync` (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-90`) constructs a `FolderPredictor` with `InitOptions.FromField` and awaits `InitAsync` — the same sequence `FolderScoringService.ScoreAsync` already ran.
- `QfcFormController` already has the machinery to avoid this: a `LoadItemsAsync(IList<QfcPreScoredItem>)` overload (`QuickFiler/Controllers/QfcFormController.Actions.cs:114-120`) alongside the plain `LoadItemsAsync(IList<MailItem>)` (`:62`). `QfcPreScoredItem` pairs a surviving mail item with its predetermined folder path for exactly this purpose. The live high-confidence path calls the plain overload, so the carrier overload is dormant.

Full analysis: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/research/2026-08-06T22-00-quickfiler-high-confidence-queue-init-stall-research.md` § 4. Recorded as an explicit non-goal of issue #424 and listed in the PR #425 follow-ups.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `QfcStreamingDequeueConfidenceGate` result shape, `QfcDatamodel` queue-processing return path, `QfcFormController.LoadItemsAsync` overload selection, `QfcItemController` folder-handler population
- [ ] Integration scenario to retest: high-confidence launch, confirming one scoring pass per accepted item and an unchanged folder-combo selection
- [ ] Manual verification notes: compare `Probability debug` log output before and after; confirm the preselected folder matches the previous behavior

Candidate directions (not a decision):

- Carry `TopFolder` through the gate's result so accepted items reach `LoadItemsAsync(IList<QfcPreScoredItem>)`, activating the existing dormant overload.
- Keep the plain overload as the normal-mode path so non-high-confidence behavior is untouched.

Scope note: the existing `QfcHomeControllerIssue218Tests` and `QfcFormControllerTests` pin the current overload-selection discipline and would need deliberate updating. Treat those pins as part of the specification, not as obstacles to route around. Tests must use MSTest with Moq and FluentAssertions, no live Outlook COM and no temporary files, per repository unit-test policy.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
