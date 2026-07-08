# quickfiler-high-confidence-prefilter (Issue #171)

- Date captured: 2026-06-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-high-confidence-prefilter/ (Issue #171)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #171
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/171
- Last Updated: 2026-06-02
- Work Mode: full-bug

## Summary

QuickFiler high-confidence mode (Issue #169) applies its confidence filter only after every email has been fully materialized and loaded into the UI item controllers. The filter must instead run before the emails are loaded into UI objects: the email list must be scored and filtered first, and only emails whose top suggested folder meets or exceeds the configured confidence threshold (default 90%) may ever be passed to the UI.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: N/A (C# / VSTO Outlook add-in)
- Command/flags used: Ribbon entry point "QuickFiler — High Confidence"
- Data source or fixture: Live Outlook mailbox with a Bayesian folder classifier trained

## Steps to Reproduce

1. Launch QuickFiler via the "QuickFiler — High Confidence" ribbon entry point.
2. Observe the initial batch load.
3. All emails in the batch are materialized and rendered into UI item controllers first.
4. Per-item Bayesian scoring (`LoadSecondaryAsync`) then runs, and only afterward are below-threshold groups removed from the already-populated view (`ApplyHighConfidenceFilterAsync` -> `RemoveBelowThresholdAsync`).

## Expected Behavior

- The email list is filtered before being loaded into the UI item controllers.
- Folder scoring runs on the candidate email list first; any email that cannot be resolved to a suggested folder at or above the threshold (default 90%) is eliminated from the list entirely.
- The UI receives only emails at or above the threshold, each fed in with its predetermined high-confidence folder choice already selected (because all surviving items are above the threshold).
- The UI never receives an email below the threshold.

## Actual Behavior

- The full batch is loaded into UI item controllers and the window is shown before any filtering occurs.
- Scoring and below-threshold removal happen after the UI objects exist, so the UI transiently receives and renders emails below the threshold, then removes them.
- The folder choice is not pre-selected from the high-confidence prediction.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: N/A

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

- `QuickFiler/Controllers/QfcFormController.cs` `LoadItemsAsync` builds UI controllers (`LoadControlsAndHandlers_01Async`), shows the window, then calls `LoadSecondaryAsync` and `ApplyHighConfidenceFilterAsync`.
- `QuickFiler/Controllers/QfcCollectionController.cs` `LoadSecondaryAsync` performs per-item scoring against `_itemGroups` (UI controllers), and `RemoveBelowThresholdAsync` removes below-threshold groups post-hoc.
- `QuickFiler/Controllers/QfcHomeController.cs` `RunAsync` builds the email list via `InitEmailQueueAsync` then hands it to `LoadItemsAsync`.
- The redesign needs a scoring pass over the raw `IList<MailItem>` (or an equivalent pre-UI candidate list) so filtering and folder pre-selection occur before UI object creation.
- Standard (non-high-confidence) QuickFiler behavior must remain unchanged.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage: pre-UI scoring/filter selects only >= threshold items; below-threshold and no-suggestion items are excluded; predetermined folder choice is applied; mode disabled leaves standard flow unchanged.
- [ ] Integration scenario: high-confidence launch shows only above-threshold items with a folder pre-selected; standard launch unaffected.
- [ ] Manual verification notes: confirm UI never renders a below-threshold email during high-confidence launch.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch