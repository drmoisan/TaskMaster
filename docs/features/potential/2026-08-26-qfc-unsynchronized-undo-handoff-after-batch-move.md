# qfc-unsynchronized-undo-handoff-after-batch-move (Potential Bug)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Draft
- Captures: **follow-up candidate 7** of `## Follow-up Candidates` in
  `docs/features/active/qfc-collection-controller-defects-468/spec.md`
- Origin: issue **#468** defect family, task `[P14-T5]`

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

The batch-move path treats the undo stack as populated by the time the move completes, but the push is
performed asynchronously on a queue worker and may not have happened yet.
`MoveMailAsync` only *enqueues* the filer
(`QuickFiler/Controllers/QfcItemController.MailActions.cs:111`) and returns `Task.CompletedTask`
(`:112`). The push onto the global undo stack happens later, on the queue's worker. So when
`BackGroundMoveAsync` proceeds to `WriteMetrics`
(`QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231`) and then `CleanupBackground()`
(`:233`), the undo entries for that batch may not yet exist.

This does not break undo in the observed configuration — the entries land eventually and are
serialized — but the handoff is unsynchronized, and nothing in the code expresses the ordering it
relies on.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Framework: .NET Framework 4.8.1, VSTO Outlook add-in
- Command/flags used: not reproducible from a command line; requires a live Outlook session and a batch move
- Data source or fixture: any QuickFiler batch move of two or more emails

## Steps to Reproduce

1. Select two or more emails in a QuickFiler session and assign destination folders.
2. Confirm the move, so `BackGroundMoveAsync` runs.
3. Observe the contents of the global undo stack at the moment `CleanupBackground()` is reached.

## Expected Behavior

Either the batch-move completion awaits the undo pushes for that batch, or the ordering dependency is
made explicit so that a future change to `WriteMetrics` or `CleanupBackground` cannot start depending
on entries that are not yet present.

## Actual Behavior

`CleanupBackground()` may run while some or all of the batch's undo entries are still queued. Nothing
observes the gap today, so the defect is latent rather than active.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; identified by triage during issue #468, recorded at
  `docs/features/active/qfc-collection-controller-defects-468/spec.md:1040-1049`
  ("Deferred observation — unsynchronized undo handoff").

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Latent. The entries land eventually and are serialized, so undo works in the observed configuration.
The severity comes from the absence of any expressed ordering constraint: a future caller that reads
the stack immediately after a batch move would see an incomplete stack with no diagnostic.

## Suspected Cause / Notes

`MoveMailAsync` returning `Task.CompletedTask` after an enqueue makes the operation look synchronous
to its awaiter while the real work is still pending. That is the same shape as an `async void`
boundary: the caller has no handle on the work it started.

Files to inspect: `QuickFiler/Controllers/QfcItemController.MailActions.cs`,
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs`,
`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs`.

This observation is explicitly out of scope for all seven issues in the #468 family, because it lives
entirely outside `QuickFiler/Controllers/QfcCollectionController.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that enqueues two moves through a fake filer queue and asserts that
      the completion signal the caller awaits does not complete before both pushes have run
- [ ] Integration scenario to retest: a batch move of several emails followed immediately by an undo
- [ ] Manual verification notes: confirm the metrics written by `WriteMetrics` are unaffected by any
      added synchronization

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Source: triage during issue #468, deliberately not absorbed because it touches no file in that
feature's owned set.
