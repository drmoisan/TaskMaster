# Bug: quickfiler-teardown-and-dropdown-residuals-793-808

- Issue: #810
- Work Mode: full-bug
- Type: bug
- Owner: drmoisan
- Last Updated: 2026-09-07
- Source issue: https://github.com/drmoisan/TaskMaster/issues/810

> Reconstruction note: the promoted potential record named in the issue body
> (`docs/features/potential/2026-09-07-quickfiler-teardown-and-dropdown-residuals-793-808.md`)
> does not exist on disk or on `main`. The GitHub issue was created from it, but the
> file did not survive promotion (known promoted-record-loss defect, issue #487 family).
> The body below is reconstructed verbatim from the GitHub issue body for issue #810.
> No new potential entry was created and no promotion was re-run, because issue #810
> already exists and re-running promotion would mint a duplicate issue.

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

## Source
From: docs/features/potential/2026-09-07-quickfiler-teardown-and-dropdown-residuals-793-808.md (record not present on disk; see reconstruction note above)

## Predecessor Issues
- #793 — closed NOT_PLANNED at 2026-09-08T00:06Z so that #810 is the single live tracker. Its CLOSED state is not evidence the work shipped.
- #808 — closed NOT_PLANNED at 2026-09-08T00:06Z for the same reason.

## Acceptance Criteria
The authoritative acceptance-criteria source for this `full-bug` work mode is
`spec.md` in this folder. The list below mirrors the spec's acceptance-criteria
table for convenience and must be kept consistent with it.

- [ ] AC1: The AC2 self-inflicted-deactivation guard is scoped to the `Form.Deactivate` caller only, so the Cancel teardown's `park-focus` stage cancels every open selector synchronously.
- [ ] AC2: The issue-677 keyboard-lock contract is preserved for a genuine deactivation; no change weakens it.
- [ ] AC3: `QfcHomeController` no longer leaves a disposed shared `CancellationTokenSource` reachable by later `Cancel()` callers.
- [ ] AC4: `QfcFormController.Cleanup()` invokes the ribbon-release callback under `finally`, exactly once, regardless of which earlier stage threw.
- [ ] AC5: The commit-pending latch is cleared on consumption in `BreadcrumbDropDownHost.RestoreAfterOpenFailure`, so it lives for exactly one popup lifetime on every path.
- [ ] AC6: The stale `FinishClose` comment is corrected and the dead `SearchOwnsDropDownDismissal` accessor is removed.
- [ ] AC7: The AC2 self-inflicted-deactivation producer has automated test coverage.
- [ ] AC8: Full C# toolchain pass completed in order (CSharpier, msbuild analyzers, msbuild nullable, vstest with coverage) with no regression.
