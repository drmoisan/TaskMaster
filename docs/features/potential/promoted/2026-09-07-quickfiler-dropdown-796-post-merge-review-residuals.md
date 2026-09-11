# quickfiler-dropdown-796-post-merge-review-residuals (Issue #808)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-dropdown-796-post-merge-review-residuals/ (Issue #808)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #808
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/808
- Last Updated: 2026-09-07
## Summary

PR #807 (issue #796, folder drop-down closes on open and click does not select) merged with four non-blocking review findings and one untested seam in the same three files. The most consequential is CR-3: the new AC2 self-inflicted-deactivation guard in `QfcFormController.ParkFocusAndCancelSelectors` also gates the #791 Cancel teardown caller, where its predicate has no meaning, so the teardown's synchronous selector-cancel stage is skipped whenever a breadcrumb popup is open and the popup is only closed later by a fire-and-forget reset. The others are a commit-pending latch that is never cleared on consumption (CR-2), a stale comment that now contradicts the guard placement (CR-1), a dead internal accessor (CR-4), and no automated test on the AC2 producer side (CR-7). Source: `code-review.2026-09-07T17-05.md` in the #796 feature folder and the Follow-ups section of PR #807.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in; `main` at `04a54e68` (PR #807 merge commit)
- Command/flags used: QuickFiler ribbon launch; Cancel (#791 ordered teardown) with a breadcrumb popup open
- Data source or fixture: live mailbox; code-read findings, no runtime capture yet

## Steps to Reproduce

1. Launch QuickFiler and open the folder drop-down on any item so a `ToolStripDropDown` popup is open.
2. Trigger Cancel (the #791 ordered teardown) while the popup is open.
3. Observe that the `park-focus` teardown stage skips every selector cancel because the AC2 guard treats the open popup as a self-inflicted deactivation, and that the popup is closed only later by `ResetBreadcrumb` via a posted, fire-and-forget reset.
4. For CR-2: open the popup, commit a selection (sets the commit-pending latch), then cause the next open to throw before `ShowPopup` runs; `RestoreAfterOpenFailure` calls `FinishClose(Uncommitted)` and the stale latch suppresses the cancel, leaving a selector session open with no popup.

## Expected Behavior

- The AC2 guard applies only to the `Form.Deactivate` caller; the Cancel teardown's `park-focus` stage cancels every open selector synchronously as the #791 ordering requires.
- The commit-pending latch lives for exactly one popup lifetime on every path, including open failure.
- Comments in `BreadcrumbDropDownHost.FinishClose` describe the two independent gates accurately.
- No dead accessor remains, and the AC2 producer side has an automated test.

## Actual Behavior

- `QfcFormController.Deactivate.cs` line 118 gates both callers of `ParkFocusAndCancelSelectors`; from `ActionCancelAsync` stage `park-focus` (`QfcFormController.EventHandlers.cs` line 144) the predicate degenerates to "is any popup open?" and the stage is skipped. Mitigation exists later via `QfcCollectionController.Cleanup` -> `QfcItemController.Cleanup` -> `ItemViewer.ResetBreadcrumb` -> `BreadcrumbDropDownOpenCoordinator.Reset`, but through `PostAsync`, so the ordering guarantee the teardown was written to provide is weakened.
- `IsCommitPending` (`BreadcrumbDropDownHost.Open.cs` 102-107) is cleared only by `ShowPopup`; `RestoreAfterOpenFailure` (`BreadcrumbDropDownHost.cs` line 455) calls `FinishClose(Uncommitted)` unconditionally and can consume a previous lifetime's latch.
- `BreadcrumbDropDownHost.cs` line 450 still says "only the focus step is gated; the cancel step above always runs", contradicted by the gate at line 447.
- `QfcItemController.EventHandlers.cs` line 209 `SearchOwnsDropDownDismissal` has no reader (the underlying field is the live AC4 latch).

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; findings are from the merged code review. The AC6 close-ordering observation that motivated the guard is in `evidence/other/` of the #796 feature folder (2026-09-07T12-19).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

CR-3 is rated Major/latent by the reviewer: the class documentation states that no breadcrumb popup may stay open after teardown or WinForms modal menu mode keeps redirecting keyboard messages to the popup, which is the #677 keyboard-lock class. The remaining items are minor but sit in the same files and should ship together.

## Suspected Cause / Notes

- Review artifact: `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/code-review.2026-09-07T17-05.md`, findings CR-1 through CR-4 and CR-7; policy audit § 8 F1 for CR-4.
- Not included here: CR-5 (`_breadcrumbPopupOwners` entries never removed, bounded by the viewer pool) and CR-6 (re-pin by string field name), both informational.
- Separate manual item, not part of this issue: AC3 clause 1 of #796 ("a mouse click on a row selects that row") was unchecked at merge and needs one post-fix observation against a `main` build; if the click still does not select, reopen #796.

## Proposed Fix / Validation Ideas

Acceptance criteria:

- [ ] AC1: `ParkFocusAndCancelSelectors` takes a parameter (for example `bool honourSelfInflictedGuard`) passed `true` from `FormViewer_Deactivated` and `false` from the `park-focus` teardown stage; a regression test pins "Cancel with a popup open still cancels every selector at the park-focus stage".
- [ ] AC2: The commit-pending latch is cleared at the end of `FinishClose` or in `RestoreAfterOpenFailure`, with a test covering the open-failure-after-committed-close sequence.
- [ ] AC3: The `FinishClose` comment at `BreadcrumbDropDownHost.cs` ~line 450 states the two independent gates.
- [ ] AC4: `SearchOwnsDropDownDismissal` is removed or given a real reader.
- [ ] AC5: The AC2 producer side (the self-inflicted-deactivation predicate) has an automated test, using a seam if the producing type is coverage-exempt.

Validation:

- [ ] Unit coverage areas: both callers of `ParkFocusAndCancelSelectors`; latch lifetime across `ShowPopup`, `FinishClose`, `RestoreAfterOpenFailure`; predicate producer.
- [ ] Integration scenario to retest: Cancel with the drop-down open; open, commit, force an open failure, open again.
- [ ] Manual verification notes: after Cancel with a popup open, keyboard input in Outlook is not captured by a stale popup.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
