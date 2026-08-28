# reset-detaches-collapsed-and-popup-surfaces-with-different-synchrony (Potential Bug)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`BreadcrumbItemViewerLifecycleCoordinator.Reset()` detaches the **collapsed** surface synchronously but
the **popup** surface only through a posted lambda. The two surfaces therefore come apart at different
times, and a caller that treats `Reset()` as complete on return is correct about one surface and wrong
about the other. This is the same class of defect as #488's D2, and it spans two files owned by
different features, which is why #488 could not fix it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1, VSTO / WinForms)
- Command/flags used: n/a — identified by source reading during the #488 orchestrator comment cross-check
- Data source or fixture: `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` and
  `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`

## Steps to Reproduce

1. Attach both a collapsed messenger and a popup messenger to a coordinator, using a queued, non-inline
   synchronization context.
2. Call `Reset()`.
3. Before draining the queue, observe that the collapsed surface is already detached while the popup
   surface is not.

## Expected Behavior

`Reset()` should apply one ordering rule to both surfaces, so that on return either both are detached or
neither is, and a caller can reason about the post-reset state without knowing which dispatcher path
each surface takes.

## Actual Behavior

`Reset()` calls `DetachCollapsedMessenger()` inline. It never calls `DetachPopupMessenger()` directly;
the popup detach reaches `_detachPopupMessenger()` inside the `PostAsync` lambda opened by
`BreadcrumbDropDownOpenCoordinator.Reset()`. On any non-inline context the two detaches land at
different times.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no captured log; verified against source in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-20-orchestrator-comment-crosscheck.md`
  under "Claim 3".

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

On the production UI thread every post runs inline, so the two detaches coincide and no user is
affected today. The severity is in the contract rather than the current symptom: the asymmetry is
invisible at the call site and will surface the first time a caller reaches `Reset()` from off the
boundary.

## Suspected Cause / Notes

**This entry names two files, and both halves must be changed together.**

- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` holds the synchronous collapsed-side
  detach inside `Reset()`.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` holds the asynchronous popup-side detach
  inside its own `Reset()`'s posted lambda.

**Why #488 could not fix it.** The #488 correction comment proposed folding this into D2's fix so the
ordering rule is applied once rather than per call site. That was unavailable: the asynchronous half
lives in `BreadcrumbDropDownOpenCoordinator.cs`, owned by sibling feature
`breadcrumb-coordinator-hub-defects-501` for issue #462, whose spec cedes #488's four production files
to #488 and correspondingly retains the open-coordinator file. Changing only the collapsed half does not
fix a synchrony mismatch and would have been an opportunistic refactor of a path neither #488 nor #475
filed.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a test that resets under a queued context and asserts both surfaces detach on the same side of the drain
- [ ] Integration scenario to retest: `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks`, which must stay green
- [ ] Manual verification notes: confirm pooled viewer reuse still reattaches both surfaces correctly

## Next Step

Promote to a GitHub issue naming **both** `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`
and `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`, so whoever owns it can change both halves
together.
