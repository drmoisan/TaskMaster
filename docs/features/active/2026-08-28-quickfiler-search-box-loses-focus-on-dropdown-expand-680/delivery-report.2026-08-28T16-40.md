# Delivery Report — Issue #680: QuickFiler search box loses focus on drop-down expand

- Timestamp: 2026-08-28T16-40
- Issue: https://github.com/drmoisan/TaskMaster/issues/680
- Branch: `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680`
- Baseline commit: `c2d683d51d907d5591e313a550099fc267c10da6`
- Plan: `plan.2026-08-28T12-56.md` (v1.4, 8 phases, 55 tasks)

## Root cause and fix, in one paragraph

The breadcrumb results popup is a `ToolStripDropDown` constructed with `AutoClose = true`. WinForms'
`ToolStripDropDown.SetVisibleCore(true)` unconditionally enters `ModalMenuFilter` menu mode for a
top-level drop-down unless `AutoClose` is `false`. Menu mode installs a message filter whose keyboard
handling retargets messages to the drop-down's handle whenever the drop-down does not contain focus —
which is precisely the state issue #438's fix produces, because a search-driven open deliberately
leaves Win32 focus in the search textbox. Every keystroke after the first therefore went to the popup
instead of the textbox. The fix uses the framework's own opt-out: a `takeFocus: false` open now sets
`DropDown.AutoClose = false` before `Show`, and the default is restored at the two transitions where
standard popup semantics must resume (close completion, and a `takeFocus: true` reopen on an
already-open popup). Because `AutoClose = true` also provided automatic dismissal, the controller
takes ownership of the two dismissal paths it replaced: search-textbox `Leave` and Escape, both routed
to the existing, already-tested `CancelSelector` path.

## Changed and created files (12)

Production (7, all modified):

- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — `ShowPopup(Point, bool takeFocus)` sets
  `DropDown.AutoClose = takeFocus;` before the show delegate; `FinishClose` restores the default first
  in its `CompleteAll` chain.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — the already-open `takeFocus` branch schedules
  a restore of `AutoClose = true` before `_focusPending()`.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` — threads the existing `takeFocus` value from
  `OpenCoreAsync` through `ShowCurrentSurface` into `_host.ShowPopup`.
- `QuickFiler/Viewers/IItemViewer.cs` — two additive members: `event System.EventHandler SearchLeave;`
  and `bool IsFolderDropDownOpen { get; }`.
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` — forwarding implementations of both.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — the `_searchLeaveHandoffPending`
  one-shot latch, the Escape branch, and the `TextBoxSearch_Leave` body.
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` — subscribe/detach for `SearchLeave`.

Tests (4):

- `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` (modified)
- `QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` (**new**)
- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` (modified)
- `QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` (**new**)

Build (1):

- `QuickFiler.Test/QuickFiler.Test.csproj` — two `Compile Include` entries for the new test files
  (the project is legacy non-SDK, so an omitted entry silently drops the file from the build).

## Test-name-to-file map (AC-3 / AC-4) — eighteen new tests

`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` (6, AC-3):

1. `ShowPopup_NonFocusingOpen_RunsTheShowDelegateWithAutoCloseFalse` — fail-before
2. `ShowPopup_GestureOpen_RunsTheShowDelegateWithAutoCloseTrue`
3. `Close_AfterANonFocusingOpen_RestoresAutoCloseTrue`
4. `OpenAsync_TakeFocusReopenOnANonFocusingOpen_RestoresAutoCloseTrue`
5. `ShowPopup_GestureOpenAfterANonFocusingCycle_RunsTheShowDelegateWithAutoCloseTrue`
6. `ShowPopup_TwoConsecutiveNonFocusingOpens_ShowOnceWithAutoCloseFalse` — fail-before

`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs` (6, AC-4):

7. `TextBoxSearchKeyDown_EscapeWhileDropDownOpen_RoutesExactlyOneCloseIntent` — fail-before
8. `TextBoxSearchKeyDown_EscapeWhileDropDownClosed_RoutesNoIntentAndLeavesKeyUnhandled`
9. `TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent` — fail-before
10. `TextBoxSearchLeave_WhileDropDownClosed_RoutesNoIntent`
11. `TextBoxSearchLeave_AfterDownArrowHandoff_SuppressesExactlyOneClose` — fail-before
12. `TextBoxSearchKeyDown_DownArrow_StillOpensAndFocusesTheDropDown`

`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` (2, AC-4):

13. `WireIntentEvents_SubscribesSearchLeave`
14. `UnwireIntentEvents_DetachesSearchLeave`

`QuickFiler.Test/Viewers/ItemViewerSearchDismissalContractTests.cs` (4, AC-4):

15. `IItemViewer_DeclaresSearchLeaveAsPlainEventHandler`
16. `IItemViewer_DeclaresIsFolderDropDownOpenAsReadOnlyBool`
17. `IItemViewer_ExistingSearchAndDropDownMemberShapes_AreUnchanged`
18. `ItemViewer_ImplementsSearchLeaveAndIsFolderDropDownOpen`

Fail-before evidence: `evidence/regression-testing/p2-t3-red-run-host.2026-08-28T15-30.md` (2 of 2
predicted failures) and `evidence/regression-testing/p2-t10-red-run-dismissal.2026-08-28T15-47.md`
(3 of 3 predicted failures). Pass-after: `p3-t6-green-run-host.2026-08-28T15-57.md` and
`p3-t9-green-run-dismissal.2026-08-28T16-02.md`.

## Toolchain — all four steps passed in the final pass

Run in the CLAUDE.md order. The first loop pass restarted because the formatter rewrote three files
this plan had just authored; the second pass was clean end to end.

1. **Format** — `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`.
   `PRE_FORMAT_CHECK_EXIT: 0`, post-check `EXIT_CODE: 0`, porcelain identical before and after.
   Artifact: `evidence/qa-gates/p6-t1-format.2026-08-28T16-20.md`.
2. **Analyzers** — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
   `EXIT_CODE: 0`, 0 errors, 5 pre-existing non-code warnings (unchanged from baseline).
   Artifact: `evidence/qa-gates/p6-t2-analyzers.2026-08-28T16-20.md`.
3. **Nullable / type-check** — `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
   `EXIT_CODE: 0`, 0 errors, no `CS86xx`.
   Artifact: `evidence/qa-gates/p6-t3-nullable.2026-08-28T16-20.md`.
4. **Test with coverage** — `pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput coverage\coverage-final-680.cobertura.xml`
   (vstest with the Cobertura collector, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`).
   `EXIT_CODE: 0`, 6839 total, 6839 passed, 0 failed.
   Artifact: `evidence/qa-gates/p6-t4-coverage-final.2026-08-28T16-20.md`.

All four steps passed without errors in the final pass.

## Coverage

| Figure | Baseline | Final |
|---|---|---|
| Repo-wide `line-rate` | 0.85269 | **0.85279** |
| Repo-wide `branch-rate` | 0.792133 | **0.792235** |
| Total tests | 6821 | **6839** (+18) |
| Failing tests | 0 | **0** |

New/changed-code coverage: all six changed members at **1.0000** line coverage against the 0.90 floor
(`ShowPopup`, `FinishClose`, `OpenWithFocusIntentAsync` including its scheduled lambda,
`ShowCurrentSurface`, `TextBoxSearch_KeyDown`, `TextBoxSearch_Leave`). All five changed measured
production files show a final covered-line count greater than or equal to baseline. Full analysis:
`evidence/qa-gates/p6-t5-coverage-delta.2026-08-28T16-20.md`.

## Discharge of issue #677's follow-up item

Issue #677's spec listed a "WinForms modal-menu-mode contributor" under Rollout & Follow-up as
**asserted, not verified**. The #680 research verified it at `dotnet/winforms` framework-source level —
`SetVisibleCore(true)` calls `ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this)` unless
`AutoClose` is `false`, and the filter's keyboard branch reads
`if (!activeToolStrip.ContainsFocus) { m.HWnd = activeToolStrip.Handle; }` — and this change fixes it.
That follow-up item is therefore **discharged by #680**. No `docs/features/**/*677*` tracking folder
exists in this worktree, so the record is carried by `spec.md`'s Rollout & Follow-up section, the
rollout notes, and the pull-request body. #677's own `MayTakeFocus` machinery has not merged into this
branch's base (verified in `evidence/other/base-state-677.2026-08-28T15-20.md`), so this change was
authored against, and composes with, the pre-#677 shape.

## Human-verification status

Spec **AC-1** and **AC-2** remain **unchecked**, pending a live-Outlook session. Menu-mode engagement
and live keyboard-message retargeting cannot be unit-tested: they need a real message pump, a real
popup window, and a live WebView2. Every automated run in this delivery excludes `LiveOutlook`-
categorised tests. Runbook: `evidence/other/hv-runbook-680.2026-08-28T16-12.md` — HV-1/HV-2 cover AC-1,
HV-3 through HV-9 cover AC-2 including the two DR-8 composition risks (post-handoff outside-click,
and a row click on a non-capturing popup).

## Rollback

No feature flag. `AutoClose` retains its current `true` behaviour everywhere outside the
`takeFocus: false` branch, so a revert is a **single-commit revert** with no data, config, or schema
consequences.

## Post-Rebase Addendum — 2026-08-28T19-30

This addendum corrects two statements above that were accurate when this report was written but have
since been overtaken by a rebase of this branch onto `main`. The existing text above is left unedited;
these are corrections layered on top of it.

- Correction 1: the scheduled action calls FocusPending(), not the raw _focusPending delegate.
- Correction 2: issue #677 has since merged into this branch's base and the shipped code composes with its MayTakeFocus machinery.

**Correction 1 detail.** The "Changed and created files" bullet for `BreadcrumbDropDownHost.Open.cs`
states that the already-open `takeFocus` branch schedules "a restore of `AutoClose = true` before
`_focusPending()`." At current `HEAD`, the shipped code instead calls the guarded wrapper
`FocusPending()`, which itself checks `MayTakeFocus()` before invoking the raw `_focusPending` delegate
field. The scheduled lambda reads `DropDown.AutoClose = true; FocusPending();`, not a call to the raw
delegate field directly.

**Correction 2 detail.** The "Discharge of issue #677's follow-up item" section states that "#677's own
`MayTakeFocus` machinery has not merged into this branch's base ... this change was authored against,
and composes with, the pre-#677 shape." That was accurate when written. This branch was later rebased
onto `main`, which has since merged issue #677, and the shipped code in
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` at current `HEAD` composes with its `MayTakeFocus`,
`FocusPending`, and `FocusAnchorIfPermitted` machinery. A dedicated composition test,
`OpenAsync_TakeFocusReopenAfterNonCapturingOpenWithPredicateFalse_RestoresAutoCloseButSuppressesFocus`
in `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`, now pins issue #680's unconditional
`AutoClose` restore composing correctly with issue #677's `MayTakeFocus` guard.
