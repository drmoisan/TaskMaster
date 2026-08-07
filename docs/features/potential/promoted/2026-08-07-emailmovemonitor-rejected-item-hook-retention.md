# emailmovemonitor-rejected-item-hook-retention (Issue #426)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/emailmovemonitor-rejected-item-hook-retention/ (Issue #426)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #426
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/426
- Last Updated: 2026-08-07
## Summary

Mail items that the QuickFiler high-confidence dequeue gate scores and rejects are removed from the master queue but are never unhooked from `EmailMoveMonitor`. Each retained entry holds a live `MailItem` COM reference and participates in a `Folder.BeforeItemMove` subscription until session cleanup, so COM-reference retention grows in proportion to the number of rejected candidates.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8.1 VSTO add-in (no Python involvement)
- Command/flags used: QuickFiler launched from the TaskMaster ribbon with `QfSettings.HighConfidenceModeEnabled = true`
- Data source or fixture: Live Outlook mailbox with a realistic message volume and a low fraction of items above `HighConfidenceThreshold`

## Steps to Reproduce

1. Enable High Confidence mode and launch QuickFiler against a folder where most messages score below the threshold.
2. Let the dequeue gate scan and reject a large number of candidates while assembling the first batch.
3. Inspect `EmailMoveMonitor._hookedItems` (or observe process COM-reference growth) while the QuickFiler session remains open.

## Expected Behavior

An item removed from the master queue is unhooked from the move monitor regardless of whether the gate accepted or rejected it, so `_hookedItems` tracks only items still under management.

## Actual Behavior

Only accepted items are unhooked. `_hookedItems` accumulates one `EmailMoveAction` per rejected candidate for the life of the session, each holding a live `MailItem` COM reference. If such a mail is moved while QuickFiler is open, the retained hook action fires `_masterQueue.Remove(x)` for an item no longer in the queue — a no-op removal, but evidence that the stale hook is still live.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not captured. The defect was found by static analysis during the issue #424 investigation, not from a runtime report.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no data loss or incorrect filing. The cost is session-scoped unmanaged-resource retention and a set of live event subscriptions that outlive their purpose. Everything is released at `Cleanup()`. Severity rises with the volume of rejected candidates, so the issue #424 fix — which makes high-confidence scanning practical and therefore more heavily used — increases exposure rather than reducing it.

## Suspected Cause / Notes

Verified by reading the code at `fb32b923` (all citations checked against that commit):

- Accepted items go out through `UnhookDequeuedNodes` (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:107-128`), which calls `TryUnhookOrReplace` (`:18`) and reaches `_moveMonitor.UnhookItem(node)` (`:33`).
- The high-confidence gate, however, takes candidates through the bare delegate `() => _masterQueue.TryTakeFirst()` (`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:82`). Rejected candidates are simply not added to `accepted` in `QfcStreamingDequeueConfidenceGate.DequeueAsync`; they never pass through `UnhookDequeuedNodes`.
- `EmailMoveMonitor.HookItem` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:46-58`) adds an `EmailMoveAction(mail, folder, moveAction)` to `_hookedItems` (`:44`) and subscribes `folder.BeforeItemMove` (`:57`). Nothing removes the entry except `UnhookItem` or `UnhookAll` (`:185`), and `UnhookAll` runs only from `QfcDatamodel` cleanup (`QuickFiler/Controllers/QfcDatamodel.cs:80`).

Dropping rejected items from the session is the intended mode contract, pinned by `DequeueAsync_BelowThresholdItemsAreDiscarded` (`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:226-237`). This issue is not a request to change that contract — only to release the monitor hook when the item is dropped.

Full analysis: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/research/2026-08-06T22-00-quickfiler-high-confidence-queue-init-stall-research.md` § 5.2. Recorded as an explicit non-goal of issue #424 and listed in the PR #425 follow-ups.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `QfcDatamodel.QueueProcessing` dequeue paths, `QfcStreamingDequeueConfidenceGate` rejection path, `EmailMoveMonitor` hook lifecycle
- [ ] Integration scenario to retest: high-confidence launch against a low-yield folder, asserting `_hookedItems` does not grow with rejected candidates
- [ ] Manual verification notes: confirm no regression in the existing move-monitor behavior for accepted items

Candidate directions (not a decision):

- Route the gate's take through a delegate that unhooks on rejection, rather than the bare `TryTakeFirst`.
- Give the gate an explicit rejection callback that the datamodel wires to `_moveMonitor.UnhookItem`.
- Unhook at the point the item is taken and re-hook only for accepted items.

Testability note: `EmailMoveMonitor.HookItem` marshals COM work (`mail.Parent`, `folder.EntryID`, event wiring) onto the captured STA thread. Any fix must preserve the thread-affinity contract established by issues #214 and #420. Tests must use the existing injectable seams with Moq — no live Outlook COM, no temporary files, per repository unit-test policy.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
