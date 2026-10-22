# Human-Exception Runbook — QuickFiler Folder-Search Focus/Keyboard-Retargeting Verification (Issue #680)

## Cue

Act on this runbook when the orchestrator records an `exception` response for issue #680's spec
acceptance criteria **AC-1** and **AC-2**. Both criteria depend on WinForms `ModalMenuFilter`
menu-mode engagement and live Win32 keyboard-message retargeting between QuickFiler's folder search
textbox and its results drop-down. That mechanism requires a real Windows message pump, a real
popup window, and a live WebView2 surface, none of which exist in the automated test environment.
It cannot be exercised by any unit test in the #680 plan, and every automated run in this plan
excludes it explicitly: full-suite runs pass `/TestCaseFilter:TestCategory!=LiveOutlook` via
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and no automated run in this plan executes a
`LiveOutlook`-categorised test. AC-1 and AC-2 are therefore recorded as unchecked in `spec.md`
pending this manual verification, and this runbook is the human follow-up that discharges them.

## Prerequisites

- A Windows workstation with Visual Studio (or the .NET Framework / VSTO build tooling) able to
  build the solution in `Debug|Any CPU` configuration from the #680 branch.
- Microsoft Outlook desktop installed, with the TaskMaster VSTO add-in registered and loadable.
- A mailbox reachable from that Outlook profile containing at least one mail item that QuickFiler
  can be run against, and at least one target folder whose name is at least eight characters long
  and is reachable by the folder search box.
- Familiarity with the QuickFiler item viewer's folder search box and results drop-down (breadcrumb
  toggle, textbox, popup list).

Before starting the checklist:

1. Build the solution in `Debug|Any CPU` from the #680 branch and register the VSTO add-in.
2. Launch Outlook with the TaskMaster add-in loaded.
3. Run QuickFiler against a mail item so at least one item viewer with a folder search box is shown.
4. Choose a target folder whose name is at least eight characters long and is reachable by the
   folder search.

## Step-by-step Instructions

Work through all nine items below in order. Do not skip an item and do not amend the fix in place
if an item fails — see Verification for the required response to a failure.

### AC-1 — continuous typing (mirrors #438's HV-1)

1. **HV-1.** Click into the QuickFiler folder search box and type the eight-plus-character folder
   name at normal speed, without pausing and without clicking anywhere between characters.
   Confirm: every character lands in the search box; the caret never leaves the textbox; the
   results drop-down auto-opens on the first character and then tracks (narrows/refreshes on) each
   subsequent keystroke rather than requiring a manual close-and-refocus.
2. **HV-2.** Confirm the drop-down updates live as characters are added and as characters are
   removed with Backspace, and that the search box retains focus throughout.

### AC-2 — gesture paths unchanged (per #400/#438)

3. **HV-3. Down-arrow handoff.** With results showing from a typed query, press Down. Confirm
   focus moves onto the drop-down, row navigation with Up/Down works, and Enter commits the
   highlighted folder.
4. **HV-4. Mouse toggle.** Use the breadcrumb drop-down toggle to open and close the selector by
   mouse. Confirm the open focuses the popup and the close returns focus to the collapsed anchor.
5. **HV-5. Row click on a gesture-opened popup.** Open by gesture, then click a result row.
   Confirm the selection commits.
6. **HV-6. Outside-click dismissal, fresh gesture open.** Open by gesture, then click elsewhere in
   the form. Confirm the popup dismisses and the selection is not committed.
7. **HV-7. Outside-click dismissal after a post-handoff state (DR-8 Risk 1).** Type to open the
   popup non-capturing, press Down to hand off focus (which restores `AutoClose = true` on a popup
   that is already visible), then click outside the popup. Confirm the popup still dismisses.
   Rationale: WinForms menu mode is entered inside `SetVisibleCore(true)`, so restoring `AutoClose`
   on an already-visible showing does not retroactively engage menu mode for that showing;
   outside-click dismissal on this specific post-handoff state may therefore differ from a fresh
   gesture open. This item exists to observe that difference if it is real.
8. **HV-8. Escape.** With the popup open from typing, press Escape. Confirm the popup closes, the
   pending selection is cancelled (the committed folder is unchanged), and focus behaviour matches
   #400/#438. Repeat with the popup opened by gesture.

### DR-8 Risk 2 — row click on a search-driven (non-capturing) popup

9. **HV-9.** Type a query so the popup opens non-capturing, then click a result row **directly**,
   without pressing Down first. Confirm the selection **commits** and is not cancelled.
   Rationale: with `AutoClose = false` the popup is shown non-activated, so a mouse click on a
   result row moves Win32 focus off `TxtboxSearch` and raises its `Leave` event. The controller's
   suppression latch is armed only by the `Keys.Down` gesture branch, so a row click's `Leave`
   routes through `SetFolderDroppedDown(false)` to `CancelSelector` and would cancel the very
   selection the click is trying to make. That is the same failure shape as the bug this plan
   fixes — a dismissal path firing on the gesture it is supposed to permit — and it is not
   reachable by any automated test in this plan, because it needs a live Win32 focus transition.

## Verification

- Record the completed checklist, with a pass/fail note per item (HV-1 through HV-9) and the
  Outlook and Windows build numbers, as a new artifact under
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/`.
- AC-1 may be checked in `spec.md` only after both HV-1 and HV-2 pass.
- AC-2 may be checked in `spec.md` only after HV-3 through HV-9 all pass.
- If any item produces a negative outcome — including either DR-8 case (HV-7 or HV-9) — do not
  amend the fix in place. Instead, promote the borderless-`Form` popup rewrite (replacing
  `ToolStripDropDown` entirely, so no menu filter is involved) through the MCP promotion lifecycle
  as its own issue, per the Phase 7 conditional-fallback note in `rollout-notes.2026-08-28T16-42.md`.
  That rewrite was evaluated during #680 research and rejected as non-minimal for this fix, but
  remains the viable long-term option if the manual verification surfaces `AutoClose`-toggling
  fragility.

## Source and Citation

The subject of this runbook is TaskMaster's own in-repo QuickFiler control (a custom WinForms
popup/textbox pair), not a third-party vendor UI, so the skill's MCP-first/web-second sourcing rule
for third-party UI navigation does not apply to the checklist content itself. Outlook desktop is
used only as the host process that loads the add-in; no Outlook-native UI is navigated as part of
this checklist. All checklist items, rationale, and the recording/rollback contract are sourced
from in-repo feature-folder artifacts:

- Checklist source (preparation steps and HV-1 through HV-9, verbatim basis for this runbook):
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/other/hv-runbook-680.2026-08-28T16-12.md` — updated_at: 2026-08-28T16-12.
- Acceptance-criteria definitions and traceability (AC-1, AC-2, and their mapping to HV-1/HV-2 and
  HV-3 through HV-9 respectively):
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/spec.md` — updated_at: 2026-08-28.
- Conditional-fallback contract (required response to a failed HV item, DR-8 Risk 1 / Risk 2
  definitions):
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/rollout-notes.2026-08-28T16-42.md` — updated_at: 2026-08-28T16-42.
