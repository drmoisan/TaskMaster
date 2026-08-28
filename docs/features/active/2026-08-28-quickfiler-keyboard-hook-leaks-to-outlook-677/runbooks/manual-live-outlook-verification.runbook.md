# Runbook — Manual Live-Outlook Verification of QuickFiler Keyboard-Focus Fix (Issue #677)

## Cue

Act on this runbook when the orchestrator has recorded an `exception` response for issue #677's
unautomatable requirement: verifying, in a live Outlook desktop session, that the keyboard-focus
fix for `BreadcrumbDropDownHost` / `QfcFormViewer` / `QfcFormController` (spec.md "Proposed Fix")
actually restores normal typing to native Outlook windows after QuickFiler is clicked away from.

This requirement cannot be automated because the failure exists only in the composition of three
things that a headless MSTest run cannot exercise without violating this repository's determinism
and no-external-process test policies:

1. Outlook's native windows and message pump;
2. the real WebView2 runtime's child windows and their focus-release behavior (a runtime defect,
   not reproducible with mocks — see Source and Citation);
3. real Win32 window-activation and focus transitions driven by an actual user click.

Running this runbook is required to close acceptance criteria **AC-1**, **AC-2**, and the
**manual half of AC-3** in `spec.md`. It also completes the secondary-contributor
reconfirmation recorded under spec.md's "Rollout & Follow-up" / "Post-implementation follow-up".
Do not check off AC-1, AC-2, or AC-3 in `spec.md` from unit-test evidence alone — only this manual
session closes them.

## Prerequisites

- A Windows machine with Outlook desktop installed and the TaskMaster VSTO add-in loaded and
  trusted, running the build that contains the fix described in `spec.md` "Proposed Fix" (the
  `BreadcrumbDropDownHost.MayTakeFocus` predicate and the `QfcFormController` deactivate-parking
  handler).
- An Outlook profile/mailbox with at least one folder containing several mail items, so that:
  - QuickFiler has items to display, page through, and file;
  - a mail item can be double-clicked open into a separate Inspector window (a "window in which
    an Outlook item is displayed" — see Source and Citation).
- Familiarity with QuickFiler's own keyboard-driven filing workflow: arrow-key row navigation,
  single-character filing/action keys, and the string-filter search box, all inside QuickFiler's
  own window.
- Familiarity with the breadcrumb selector control inside QuickFiler (the dropdown used to pick a
  destination folder), including how to open it and how it is dismissed by Escape versus by
  committing a selection.
- Read access to this feature folder's `spec.md` (Test Strategy and Acceptance Criteria sections)
  and `evidence/other/manual-verification-pending.md`, which this runbook operationalizes.
- A way to record the outcome: editing the three pending checkboxes in `spec.md`'s Acceptance
  Criteria section, and adding a dated result note to
  `evidence/other/manual-verification-pending.md` (or a new evidence file under
  `evidence/manual-verification/`) once the session completes. Recording results is outside this
  runbook's write scope (`runbooks/**` only); the person running the session performs that update
  directly in the feature folder.

## Step-by-step Instructions

Perform the walkthrough below as one continuous session. Do not close QuickFiler between steps
unless a step explicitly says to.

### Part A — Setup

1. Launch Outlook with the TaskMaster add-in loaded. Confirm the add-in is active (ribbon button
   or equivalent entry point visible).
2. Select a mail item and run QuickFiler on it so its filing window opens with keyboard
   navigation active (this is the QuickFiler "navigation on" state).
3. Open a second, unrelated mail item into its own Inspector window (double-click it from the
   Explorer list) and leave that Inspector window open in the background for the remainder of the
   session.
4. Confirm the Outlook Explorer window's search box is visible and empty.

### Part B — Native-window typing across QuickFiler's internal states (closes AC-1 and AC-2)

Repeat the following block for each of these three QuickFiler internal states, in order:

- **State 1 — keyboard navigation on, breadcrumb popup closed.** QuickFiler open, no breadcrumb
  dropdown showing.
- **State 2 — breadcrumb popup open.** In QuickFiler, open the breadcrumb selector dropdown for
  the current item and leave it open.
- **State 3 — mid-search.** In QuickFiler's string-filter box, begin typing a filter string and
  stop partway through, leaving the filter box in an active mid-typing state (this exercises the
  per-keystroke popup close/reopen churn noted in spec.md's Edge Cases).

For each of the three states above, perform this same three-target sub-sequence without closing
QuickFiler or resetting the state in between:

5. Click into the native Outlook **Explorer** window (the folder list/message list). Type several
   characters (for example, type letters to trigger Outlook's incremental find, or use arrow keys
   to move the list selection). Confirm Outlook responds exactly as it would if QuickFiler were
   not running (AC-1).
6. Click into the open **Inspector** window from Part A step 3. Click into its subject or body
   field and type a short string. Confirm the characters appear normally in the Inspector (AC-1).
7. Click into the Outlook Explorer's **search box**. Type a short search string. Confirm the
   characters appear in the search box and search behaves normally (AC-1).
8. Click back into the QuickFiler window. Exercise QuickFiler's own keyboard-driven workflow:
   press arrow keys to move the row/item selection, press a character key bound to a filing
   action, and type into QuickFiler's own string-filter box. Confirm all three behave exactly as
   they did before Part B began (AC-2).
9. Restore the QuickFiler internal state to the next state in the list (close the breadcrumb
   popup if it was opened for State 2; clear or complete the mid-search filter for State 3) before
   moving to the next state.

After completing the three-target sub-sequence for all three states (9 target/state
combinations total), Part B is complete.

### Part C — Escape/commit caret-return with the breadcrumb selector open (closes the manual half of AC-3)

10. In QuickFiler, open the breadcrumb selector dropdown.
11. Press **Escape**. Confirm the dropdown closes and the caret/keyboard focus returns to the
    breadcrumb anchor control (the control that had focus before the dropdown opened), not to a
    native Outlook window and not nowhere.
12. Re-open the breadcrumb selector dropdown.
13. This time, **commit a selection** (choose a destination folder from the dropdown rather than
    pressing Escape). Confirm the caret/keyboard focus again returns to the breadcrumb anchor
    control after the commit.

### Part D — Secondary-contributor reconfirmation (Rollout & Follow-up item, not a blocking AC)

This part is owned by the project maintainer per spec.md's "Rollout & Follow-up" and
"Post-implementation follow-up" sections. It reconfirms or rules out the secondary WinForms
modal-menu-mode contributor (`ToolStripManager.ModalMenuFilter` / hosted-message-hook behavior),
which was asserted from .NET Framework reference-source background knowledge and never verified
live.

14. Close QuickFiler and any open Inspector windows from the prior parts. Start a fresh QuickFiler
    session on a new mail item.
15. Without ever opening the breadcrumb selector dropdown during this pass, click into a native
    Outlook Explorer window and type. Confirm whether keyboard loss occurs.
16. Record the outcome: if keyboard loss never occurs in this pass (breadcrumb popup never
    opened), the secondary WinForms modal-menu-mode contributor is ruled out as a necessary
    condition. If keyboard loss does occur even though no popup was opened, the secondary
    contributor is confirmed as an independent condition.
17. If step 16 confirms the secondary contributor as an independent live defect, do not leave that
    finding as prose in this feature folder (feature-folder prose disappears at merge). Promote it
    through the MCP-lifecycle issue-promotion workflow into a new GitHub issue, per spec.md's
    "Post-implementation follow-up" item 2.

## Verification

- **AC-1 verified** when, across all nine target/state combinations in Part B, typing into the
  Explorer window, the Inspector window, and the search box produces normal Outlook behavior
  (visible characters, working incremental find/search, normal list-selection response) with no
  suppressed or missing keystrokes.
- **AC-2 verified** when, after every one of the nine Part B combinations, clicking back into
  QuickFiler and exercising arrow-key navigation, a character-key filing action, and the
  string-filter box all behave identically to QuickFiler's pre-click-out behavior.
- **AC-3 (manual half) verified** when both step 11 (Escape) and step 13 (commit) in Part C return
  keyboard focus to the breadcrumb anchor control, matching the existing automated coverage for
  this behavior (`FinishClose_PredicateTrue_FocusAnchorInvoked` and
  `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked` in
  `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`).
- If any Part B or Part C check fails, do not check off the corresponding acceptance criterion.
  Record the specific target/state combination that failed in
  `evidence/other/manual-verification-pending.md` (or a new dated evidence file) so it can be
  triaged against the three focus-path candidates in spec.md's Root Cause Analysis (the
  `FinishClose` steal, the late `_focusPending` completion, or the WinForms modal-menu-mode
  contributor).
- Part D's outcome (secondary-contributor confirmed or ruled out) is recorded per step 16 above; it
  does not gate AC-1/AC-2/AC-3 and is not itself a pass/fail condition for this runbook.
- Once AC-1, AC-2, and the manual half of AC-3 are verified, update the corresponding checkboxes in
  `spec.md`'s Acceptance Criteria section and reference the dated evidence recording this session's
  result.

## Source and Citation

- Primary source for the checklist and AC mapping (internal, first-party): `spec.md` Test Strategy
  ("Manual validation steps (required)" and "Integration scenario to retest (manual)") and
  Acceptance Criteria AC-1 through AC-3, this feature folder, Issue #677, Last Updated 2026-08-28.
- Source for the rationale that this requirement is unautomatable and for the verbatim checklist
  this runbook operationalizes: `evidence/other/manual-verification-pending.md`, this feature
  folder, dated 2026-08-28.
- **MCP-first/web-second sourcing note:** per this repository's documented limitation (no
  callable MCP documentation-retrieval tool is wired in this repository at this time; see the
  two-axis-model-selection spec, Out of Scope), the MCP-first clause could not be honored for the
  third-party terminology and defect citations below. `WebFetch` was used directly as the sole
  available web-second mechanism.
- Outlook window terminology ("Explorer window", "Inspector window") used in Parts A–C, sourced
  web-second via `WebFetch`: Microsoft Learn, "Explorer object (Outlook)". Source URL:
  https://learn.microsoft.com/en-us/office/vba/api/outlook.explorer — updated_at: 2024-03-15.
  Microsoft Learn, "Inspector object (Outlook)". Source URL:
  https://learn.microsoft.com/en-us/office/vba/api/outlook.inspector — updated_at: 2024-03-15.
- Underlying WebView2 runtime defect referenced as the reason live verification is mandatory (a
  WebView2 control hosted in a VSTO/WinForms host failing to release keyboard focus back to the
  host after the user clicks away), sourced web-second via `WebFetch`: GitHub,
  MicrosoftEdge/WebView2Feedback, issue #951 ("WebView2 WinForms control ... steals and holds on to
  keyboard focus"; status observed as in-progress/tracked at capture time). Source URL:
  https://github.com/MicrosoftEdge/WebView2Feedback/issues/951 — captured/updated_at: 2026-08-28.
  This is the same upstream defect cited by `spec.md` under "Assumptions, Constraints,
  Dependencies" and "Rollout & Follow-up".
- Underlying root-cause detail for the three focus-path candidates referenced in Verification:
  `research/2026-08-28T09-15-quickfiler-outlook-keyboard-suppression-677-research.md`, this
  feature folder, dated 2026-08-28.
