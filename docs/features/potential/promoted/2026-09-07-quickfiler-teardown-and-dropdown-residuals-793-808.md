# quickfiler-teardown-and-dropdown-residuals-793-808 (Issue #810)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-teardown-and-dropdown-residuals-793-808/ (Issue #810)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #810
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/810
- Last Updated: 2026-09-08
## Summary

Consolidates the QuickFiler review residuals filed as #793 (from the #791 Cancel-teardown review) and #808 (from the #796 drop-down review). They share the teardown path: #808's lead finding is that the new AC2 self-inflicted-deactivation guard in `QfcFormController.ParkFocusAndCancelSelectors` also gates the #791 Cancel teardown caller, where its predicate has no meaning, so the teardown's synchronous selector-cancel stage is skipped whenever a popup is open; #793 is two residual teardown defects in the same controllers, a disposed shared `CancellationTokenSource` that later sharers still `Cancel()`, and a ribbon-release callback invoked without a `finally`. The remaining #808 items (commit-pending latch never cleared on consumption, stale `FinishClose` comment, dead `SearchOwnsDropDownDismissal` accessor, untested AC2 producer) sit in the same files.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in; `main` at `04a54e68` (PR #807 merge commit)
- Command/flags used: QuickFiler ribbon launch; Cancel (#791 ordered teardown) with a breadcrumb popup open
- Data source or fixture: static review findings; the AC6 close-ordering observation (2026-09-07T12-19) in the #796 feature folder `evidence/other/`

## Steps to Reproduce

1. (#808 CR-3) Open the folder drop-down on any item, then trigger Cancel. The `park-focus` teardown stage (`QfcFormController.EventHandlers.cs` ~line 144) calls `ParkFocusAndCancelSelectors`, whose guard (`QfcFormController.Deactivate.cs` ~line 118) treats the open popup as a self-inflicted deactivation and skips every selector cancel; the popup is closed only later by `ItemViewer.ResetBreadcrumb` through a posted, fire-and-forget reset.
2. (#793 N1) `QfcHomeController.Cleanup()` disposes `_tokenSource` without nulling it; `QfcDatamodel.Cleanup()` and `QuiesceLoaderAsync()` call `_tokenSource?.Cancel()` on the shared source, which throws `ObjectDisposedException` once reached after disposal.
3. (#793 N2) `QfcFormController.Cleanup()` (`SetupDisposal.cs` ~line 259) invokes `_parentCleanup?.Invoke()` as its last statement with no `finally`; a throw from the viewer dispose at ~line 251 skips the ribbon release callback.
4. (#808 CR-2) Open the popup, commit a selection (sets `IsCommitPending`), then cause the next open to throw before `ShowPopup`; `RestoreAfterOpenFailure` (`BreadcrumbDropDownHost.cs` ~line 455) calls `FinishClose(Uncommitted)` and the stale latch suppresses the cancel, leaving a selector session open with no popup.

## Expected Behavior

- The AC2 guard applies only to the `Form.Deactivate` caller; the Cancel teardown's `park-focus` stage cancels every open selector synchronously, preserving the #791 ordering guarantee and the class-level invariant that no breadcrumb popup may survive teardown (WinForms modal menu mode would keep redirecting keyboard messages to it).
- Disposing the shared token source cannot make a later `Cancel()` throw; the ribbon release callback runs under `finally` exactly once regardless of which stage threw.
- The commit-pending latch lives for exactly one popup lifetime on every path.
- Comments match the gate placement; no dead accessor; the AC2 producer has an automated test.

## Actual Behavior

As in the reproduction steps. All are latent today: #808 CR-3 is mitigated later in teardown by the reset chain (weakened ordering, not lost responsibility); #793 N1 is unreachable only because `RibbonController` never calls `QfcHomeController.Cleanup()` directly; #793 N2 needs a throwing viewer dispose.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none; static findings. Sources: `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/code-review.2026-09-06T15-31.md` (N1, N2); `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/code-review.2026-09-07T17-05.md` (CR-1, CR-2, CR-3, CR-4, CR-7).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: #808 CR-3 was rated Major/latent by its reviewer because it touches the #677 keyboard-lock class, and #793 breaks the single-release-callback invariant #791 established once any caller reaches the unguarded paths.

## Suspected Cause / Notes

- Files: `QuickFiler/Controllers/QfcFormController.Deactivate.cs`, `QfcFormController.EventHandlers.cs`, `QfcFormController.SetupDisposal.cs`, `QfcHomeController.cs`, `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `BreadcrumbDropDownHost.Open.cs`, `QfcItemController.EventHandlers.cs`.
- Superseded issues: #793, #808 (close with a pointer to this issue).
- Separate manual item, not part of this issue: #796 AC3 clause 1 ("a mouse click on a row selects that row") was unchecked at merge and needs one post-fix observation against a `main` build; reopen #796 if it fails.

## Proposed Fix / Validation Ideas

Acceptance criteria:

- [ ] AC1: `ParkFocusAndCancelSelectors` takes a parameter such as `bool honourSelfInflictedGuard`, passed `true` from `FormViewer_Deactivated` and `false` from the `park-focus` teardown stage; a regression test pins "Cancel with a popup open still cancels every selector at the park-focus stage".
- [ ] AC2: `QfcHomeController.Cleanup()` nulls `_tokenSource` after disposal or ownership is made single with sharers holding only the token; a test proves a post-dispose `Cancel()` through a sharer does not throw.
- [ ] AC3: `QfcFormController.Cleanup()` invokes `_parentCleanup` under `finally`; a test with a throwing viewer dispose proves the release callback still runs exactly once.
- [ ] AC4: The commit-pending latch is cleared at the end of `FinishClose` or in `RestoreAfterOpenFailure`, with a test for the open-failure-after-committed-close sequence.
- [ ] AC5: The `FinishClose` comment states the two independent gates; `SearchOwnsDropDownDismissal` is removed or given a reader; the AC2 producer predicate has an automated test through a seam.

Validation:

- [ ] Unit coverage areas: both callers of `ParkFocusAndCancelSelectors`; teardown stages under throwing viewer dispose; latch lifetime across `ShowPopup`, `FinishClose`, `RestoreAfterOpenFailure`.
- [ ] Integration scenario to retest: Cancel with the drop-down open; open, commit, force an open failure, open again.
- [ ] Manual verification notes: after Cancel with a popup open, keyboard input in Outlook is not captured by a stale popup.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
