# Human-Verification Runbook — Issue #680 (manual-verification kind)

Timestamp: 2026-08-28T16-12

## Status of the criteria this runbook covers

Spec **AC-1** and **AC-2** remain **UNCHECKED** in `spec.md` pending this manual step. They are not
dischargeable by any automated task in the #680 plan and are explicitly out of scope for the plan's
automated tasks.

The mechanism this fix addresses — WinForms `ModalMenuFilter` menu-mode engagement and live
keyboard-message retargeting — cannot be observed in a unit test: it requires a real message pump,
a real popup window, and a live WebView2 surface. This runbook is the manual counterpart to the
`TestCategory!=LiveOutlook` exclusion that every automated run in this plan applies (the full-suite
runs apply it through `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which always passes
`/TestCaseFilter:TestCategory!=LiveOutlook`). No automated run in this plan executes any
`LiveOutlook`-categorised test.

If any item below produces a negative outcome, do not amend this fix in place: trigger the Phase 7
fallback note in `rollout-notes` — promote the borderless-`Form` popup rewrite (replacing
`ToolStripDropDown` so no menu filter is involved) through the MCP promotion lifecycle as its own
issue.

## Preparation

1. Build the solution in `Debug|Any CPU` from this branch and register the VSTO add-in.
2. Launch Outlook with the TaskMaster add-in loaded.
3. Run QuickFiler against a mail item so at least one item viewer with a folder search box is shown.
4. Choose a target folder whose name is at least eight characters long and is reachable by the
   folder search.

## Checklist

### AC-1 — continuous typing (mirrors #438's HV-1)

- [ ] **HV-1.** Click into the QuickFiler folder search box and type the eight-plus-character folder
  name at normal speed, without pausing and without clicking anywhere between characters.
  Confirm: every character lands in the search box; the caret never leaves the textbox; the results
  drop-down auto-opens on the first character and then tracks (narrows/refreshes on) each subsequent
  keystroke rather than requiring a manual close-and-refocus.
- [ ] **HV-2.** Confirm the drop-down updates live as characters are added and as characters are
  removed with Backspace, and that the search box retains focus throughout.

### AC-2 — gesture paths unchanged (per #400/#438)

- [ ] **HV-3. Down-arrow handoff.** With results showing from a typed query, press Down. Confirm
  focus moves onto the drop-down, row navigation with Up/Down works, and Enter commits the
  highlighted folder.
- [ ] **HV-4. Mouse toggle.** Use the breadcrumb drop-down toggle to open and close the selector by
  mouse. Confirm the open focuses the popup and the close returns focus to the collapsed anchor.
- [ ] **HV-5. Row click on a gesture-opened popup.** Open by gesture, then click a result row.
  Confirm the selection commits.
- [ ] **HV-6. Outside-click dismissal, fresh gesture open.** Open by gesture, then click elsewhere in
  the form. Confirm the popup dismisses and the selection is not committed.
- [ ] **HV-7. Outside-click dismissal after a post-handoff state (DR-8 Risk 1).** Type to open the
  popup non-capturing, press Down to hand off focus (which restores `AutoClose = true` on a popup
  that is already visible), then click outside the popup. Confirm the popup still dismisses.
  Rationale: WinForms menu mode is entered inside `SetVisibleCore(true)`, so restoring `AutoClose`
  on an already-visible showing does not retroactively engage menu mode for that showing; outside-
  click dismissal on this specific post-handoff state may therefore differ from a fresh gesture
  open. This item exists to observe that difference if it is real.
- [ ] **HV-8. Escape.** With the popup open from typing, press Escape. Confirm the popup closes, the
  pending selection is cancelled (the committed folder is unchanged), and focus behaviour matches
  #400/#438. Repeat with the popup opened by gesture.

### DR-8 Risk 2 — row click on a search-driven (non-capturing) popup

- [ ] **HV-9.** Type a query so the popup opens non-capturing, then click a result row **directly**,
  without pressing Down first. Confirm the selection **commits** and is not cancelled.
  Rationale: with `AutoClose = false` the popup is shown non-activated, so a mouse click on a result
  row moves Win32 focus off `TxtboxSearch` and raises its `Leave` event. The controller's suppression
  latch is armed only by the `Keys.Down` gesture branch, so a row click's `Leave` routes through
  `SetFolderDroppedDown(false)` to `CancelSelector` and would cancel the very selection the click is
  trying to make. That is the same failure shape as the bug this plan fixes — a dismissal path firing
  on the gesture it is supposed to permit — and it is not reachable by any automated test in this
  plan, because it needs a live Win32 focus transition.

## Recording the outcome

Record the completed checklist, with a pass/fail note per item and the Outlook and Windows build
numbers, as a new artifact under
`docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/`.
Only after every AC-1 item and every AC-2 item passes may AC-1 and AC-2 be checked in `spec.md`.
