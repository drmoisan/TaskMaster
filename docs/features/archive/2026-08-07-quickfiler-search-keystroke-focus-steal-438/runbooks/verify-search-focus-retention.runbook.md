# Human-Exception Runbook — Verify QuickFiler Folder-Search Focus Retention (Issue #438)

This runbook is a contract-conformant human-exception runbook per
`.claude/skills/human-exception-runbook/SKILL.md`, authored for the single manual-verification
requirement recorded against issue #438 that automated MSTest coverage cannot discharge.

## Cue

Execute this runbook after the #438 fix has been implemented and the automated MSTest suite for
the fix is green (unit-level and integration-harness assertions over the `IItemViewer`,
`BreadcrumbDropDownOpenCoordinator`, `BreadcrumbDropDownHost`, and `BreadcrumbBridgeCoordinator`
seams, per `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/research/2026-08-08T10-30-quickfiler-search-keystroke-focus-steal-research.md`
§8). Run it before or shortly after merge, as post-fix verification of behavior that sits outside
every managed seam. This runbook is **not** a merge gate: the automated seam-level assertions are
the merge gate for issue #438. This check confirms two native behaviors that no unit test can
observe:

1. whether CoreWebView2 popup-surface creation grabs Win32 focus on its own during the first
   search-driven open, and
2. whether `ToolStripDropDown.AutoClose` keeps the non-activated popup open while the user
   continues typing in the same window.

## Prerequisites

- A build of the add-in containing the #438 fix, registered as a VSTO add-in and loaded in a
  desktop Outlook session (Outlook process running with the add-in visible in the ribbon/COM
  add-ins list).
- A mailbox connected to that Outlook profile with enough folders that a wildcard search on an
  eight-character folder-name fragment returns **two or more matches** in the QuickFiler folder
  selector. Confirm this before starting: if the mailbox does not have such folders, create or
  identify a folder-name fragment (for example a shared eight-character substring across two or
  more existing folders) that satisfies this, or add temporary test folders to the mailbox for the
  duration of the check.
- QuickFiler opened on a mail item, so the `ItemViewer` folder-search textbox (`TxtboxSearch`) is
  visible and interactive.
- Note the name of the folder that is selected/highlighted in the breadcrumb before search text is
  typed (the "starting folder"). This is required for the Escape-restore check below.

## Step-by-step Instructions

### A. Caret-retention and drop-down-tracking check (the primary #438 manual note)

1. Click into `TxtboxSearch` so it has keyboard focus and the caret is visible in the textbox.
2. Type the eight-character folder-name fragment identified in Prerequisites at normal typing
   speed (continuous keystrokes, not one character followed by a pause). Do not click the mouse or
   press any other key during this step.
3. While typing, watch the caret continuously. After each of the eight characters, confirm the
   caret is still visible inside `TxtboxSearch` and the character just typed appears in the
   textbox's text.
4. While typing, watch the drop-down surface continuously. Confirm it opens no later than the
   first character and, once open, does not visibly close and reopen (flicker) on any subsequent
   keystroke.
5. After the eighth character, confirm the drop-down's row list reflects the full eight-character
   fragment (that is, it does not still show the row set that matched only the first one or two
   characters).

### B. Down-arrow explicit-gesture regression check

6. With the drop-down open from step 4-5 and keyboard focus still in `TxtboxSearch`, press the
   Down arrow key once.
7. Confirm keyboard focus moves from `TxtboxSearch` into the drop-down surface (the highlighted row
   in the drop-down becomes visibly focused/active).

### C. Mouse-toggle explicit-gesture regression check

8. Close the drop-down (press Escape or click elsewhere to return to a closed state), then clear
   `TxtboxSearch`.
9. Click the drop-down toggle control (the arrow/button that opens the folder selector) with the
   mouse, without typing anything first.
10. Confirm the drop-down opens and keyboard focus moves into the drop-down surface.

### D. Escape-restore regression check

11. Click into `TxtboxSearch` again and repeat step 2 (type the eight-character fragment at normal
    speed) to reopen the drop-down with a partial search in progress.
12. Press Escape.
13. Confirm the breadcrumb/selector display reverts to showing the "starting folder" noted in
    Prerequisites — the folder that was selected before the search began — and not the row that was
    highlighted mid-search.

## Verification

Record the outcome of each check explicitly. Do not proceed past a failing check as if it passed.

| Check | Pass condition | Fail condition | On failure |
|---|---|---|---|
| A: caret retention (steps 1-3) | The caret remains visible inside `TxtboxSearch` after every one of the eight keystrokes; all eight characters appear in the textbox text. | The caret leaves `TxtboxSearch` at any point during typing, or fewer than eight characters appear in the textbox text because a keystroke was redirected elsewhere. | Record which keystroke (by position, 1-8) the caret left on. Report against issue #438: this indicates CoreWebView2 popup-surface creation is grabbing Win32 focus outside the managed pipeline (research §8, §6 risk "WebView2 native focus grab"). |
| A: drop-down stability (steps 4-5) | The drop-down opens by the first character, stays open with no visible close/reopen flicker through all eight keystrokes, and its final row set reflects the full eight-character fragment. | The drop-down visibly closes and reopens (flickers) on any keystroke, or its final row set reflects fewer than eight characters of the fragment. | Record which keystroke triggered the flicker or stale row set. Report against issue #438: this indicates `ToolStripDropDown.AutoClose` is not behaving as expected while typing continues in the same window (research §6 risk on `AutoClose`), or the search-preserving row-replacement path is not reached. |
| B: Down arrow (steps 6-7) | Focus visibly moves from `TxtboxSearch` into the drop-down surface after one Down-arrow press. | Focus does not move, or moves somewhere other than the drop-down surface. | This is a regression against the existing #400 contract for the Down-arrow gesture, not a new #438 behavior. Report as a regression against issue #438 (the fix must not have altered `TextBoxSearch_KeyDown`). |
| C: mouse toggle (steps 8-10) | Clicking the drop-down toggle opens the drop-down and moves focus into it. | The drop-down does not open, or opens without moving focus into it. | This is a regression against the existing #400 contract for the mouse-toggle gesture (AC-13). Report as a regression against issue #438. |
| D: Escape restore (steps 11-13) | After Escape, the breadcrumb/selector shows the starting folder noted in Prerequisites, not the mid-search highlighted row. | After Escape, the breadcrumb/selector shows the mid-search highlighted row, or any folder other than the starting folder. | Report against issue #438: this indicates the per-keystroke highlight is committing to the model instead of remaining pending-only (research §2 stale-cache defect), or the cancel path is not restoring `OriginalIdentity`. |

If any check fails, do not silently accept the observation. Record it in writing (which check,
which step, what was observed) and open a report against issue #438. If the observed behavior is
clearly a distinct native-focus defect not caused by the managed pipeline (for example, a
CoreWebView2 focus grab that is reproducible even with the #438 fix's managed focus calls fully
suppressed), file it as a new issue instead, per the possibility flagged in the research artifact's
§6 risk list, and cross-reference issue #438 from the new issue.

## Source and Citation

- Requirement origin: `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/issue.md`,
  "Proposed Fix / Validation Ideas" → "Manual verification notes" (repository file, captured
  2026-08-07, last updated 2026-08-08) — updated_at: 2026-08-08.
- Automation-feasibility analysis and the two named native-behavior gaps: `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/research/2026-08-08T10-30-quickfiler-search-keystroke-focus-steal-research.md`
  §8 "Automation Feasibility" and §6 "Recommended Approach" (risks and mitigations subsection) —
  updated_at: 2026-08-08.
- Down-arrow and mouse-toggle explicit-gesture contract this runbook regression-checks:
  `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`, acceptance
  criteria AC-5 through AC-9 and AC-13 (repository file) — updated_at: 2026-07-21.
- Sourcing-rule note: this runbook's steps describe interaction with the repository's own QuickFiler
  add-in UI inside a locally loaded Outlook desktop session, not a vendor-documented third-party UI
  workflow, so there is no applicable Microsoft Learn or vendor navigation page to cite MCP-first or
  web-second for the procedure itself. No MCP documentation-retrieval tool is wired in this
  repository at this time (see the two-axis-model-selection spec, Out of Scope), so the MCP-first
  clause is not exercised here; the repository artifacts above are the authoritative sources for
  this requirement and its steps.
