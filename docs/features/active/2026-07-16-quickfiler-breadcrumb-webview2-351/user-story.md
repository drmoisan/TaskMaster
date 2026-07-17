# `quickfiler-breadcrumb-webview2` — User Story

- Issue: #351
- Epic: folder-tree-breadcrumb-redesign
- Owner: drmoisan
- Status: Ready for planning
- Last Updated: 2026-07-16
- Work Mode: full-feature
- Companion spec: `spec.md`

## Story Statement

- As a QuickFiler user triaging mail, I want each suggested filing folder to appear as a
  single-line breadcrumb (`Folder -> SubFolder -> Leaf`) ending at the predicted folder, so
  that I can read the full context of a suggestion at a glance instead of decoding an indented
  tree built from only the top-ranked suggestions.
- As a QuickFiler user, I want to expand a breadcrumb segment and see every real subfolder that
  actually exists in my Outlook mailbox, so that I can file into a nearby folder the predictor
  did not rank, without leaving QuickFiler.
- As a QuickFiler user, I want the prediction percentage next to each suggestion to always be
  fully readable, so that I can compare suggestion confidence without guessing at clipped or
  invisible numbers.
- As a keyboard-centric QuickFiler user, I want left/right arrow keys and double-click to
  collapse and expand breadcrumb segments, so that my existing keyboard-driven filing flow keeps
  working with the new control.

## Problem / Why

The current QuickFiler folder dropdown is a stock WinForms `ComboBox`. Its hierarchy is
synthesized by splitting the top-ranked suggestion paths on `\`, so it can only ever show
folders that already appear among the suggestions — never the real neighboring subfolders in the
mailbox. The prediction percentage is not reliably visible at runtime. This feature replaces the
dropdown in the live QuickFiler item viewer with a WebView2-hosted breadcrumb control backed by
a live Outlook folder-hierarchy provider (issue 9101), matching the design delivered for the
EfcViewer surface (issue 9102) while keeping the filing workflow — selection, search, the
"Trash to Delete" shortcut, and keyboard navigation — exactly as it behaves today.

## Personas & Scenarios

- **Persona: high-volume filer.** Processes dozens of messages per QuickFiler session, relies on
  the top suggestion most of the time, and needs the percentage to decide quickly whether to
  trust it. Frustration today: the percentage is sometimes unreadable, and the folder list shows
  hierarchy fragments that do not match the real mailbox structure.
- **Persona: keyboard-centric user.** Drives QuickFiler almost entirely by keyboard, including
  the existing Right-arrow (expand / pop-out dialog) and Left-arrow (collapse/close) behaviors.
  Needs those semantics preserved on the new control.

- **Scenario: filing with confidence.** A user opens QuickFiler on an inbox message. Each
  suggestion appears as a one-line breadcrumb ending at the predicted folder with its percentage
  fully visible at the right edge. The user compares two suggestions by their percentages,
  selects the higher-confidence breadcrumb, and files the message; it lands in the full folder
  path shown by the breadcrumb.
- **Scenario: filing next to a suggestion.** The predicted folder is close but wrong — the right
  target is a sibling subfolder the predictor did not rank. The user expands the parent segment
  of the breadcrumb; the control lists every real immediate subfolder of that parent from
  Outlook (not just suggested ones). The user picks the correct subfolder and files there.
- **Scenario: trimming a long path.** A deeply nested suggestion crowds the row. The user
  double-clicks an early segment; everything after it collapses behind a plus. The row is now
  short and readable. Later, the user clicks the plus and the full breadcrumb returns.
- **Scenario: searching for a folder.** The user types in the folder search box. Matching
  folders (plain search results, no percentage) appear in the breadcrumb control; selecting one
  yields exactly that folder path, and the message files there — identical to today's search
  behavior, including the "Trash to Delete" entry.

## Acceptance Criteria

- [ ] US-1: When I open a message in QuickFiler, each folder suggestion appears as a single-line
  breadcrumb `Folder -> SubFolder -> Leaf` anchored at the predicted leaf folder, in the live
  item viewer.
- [ ] US-2: A plus/minus expand affordance appears on the leaf only when that folder actually
  has subfolders; leaves without subfolders show no affordance.
- [ ] US-3: When I double-click a non-leaf segment, everything after that segment collapses and
  a plus appears beside the now-terminal segment; clicking the plus restores the full
  breadcrumb.
- [ ] US-4: When I expand a segment, I see every real immediate subfolder of that folder from my
  live Outlook mailbox (via the shared 9101 folder-hierarchy provider), not only subfolders that
  happen to appear among the top-ranked suggestions.
- [ ] US-5: The prediction percentage for each suggestion is always fully visible and
  unobstructed — in dark and light themes, with long folder paths, with many rows, and at
  higher display scaling. (Long paths truncate in the middle segments; the percentage never
  clips or disappears.)
- [ ] US-6: Left and right arrow keys work on the breadcrumb: Right expands where expansion is
  possible and otherwise falls back to today's behavior (Pop Out / Enumerate Conversation
  dialog); Left collapses and otherwise falls back to closing the folder control — my existing
  keyboard filing flow is unchanged.
- [ ] US-7: Selecting a breadcrumb (or a listed subfolder, or a search result) files the message
  to exactly the full folder path shown; typing in the search box still lists matching folders
  and the "Trash to Delete" entry still behaves exactly as it does today.
- [ ] US-8: The suggestion percentages themselves are unchanged — the same scores the predictor
  already computes are displayed; only the presentation changes.

## Non-Goals

- No change to how suggestions are scored or ranked, or to the model output.
- No redesign of the EfcViewer surface (delivered separately by issue 9102).
- No changes to the nine inactive QuickFiler viewer variants; only the live item viewer changes.
- No new third-party UI controls; the control is WebView2-hosted HTML/CSS/JS.
