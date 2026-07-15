# `quickfiler-folder-tree-percentage` — User Story

- Issue: #325
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-15T16-43
- Epic: `folder-tree-percentage-ui` (child 9003, wave 1, complexity C3)
- Depends on: `folder-probability-plumbing` (epic placeholder issue 9001)

## Story Statement

- As a QuickFiler user reviewing filing-target suggestions in the folder dropdown, I want folders
  that contain subfolders to appear as expandable tree nodes I can open and close, so that I can
  navigate into a parent folder's children without losing the surrounding list of suggestions.
- As a QuickFiler user judging which suggested folder to file into, I want each suggestion's
  prediction probability shown right-aligned as a whole-number percentage, so that I can compare the
  confidence of competing suggestions at a glance and file with more confidence.

## Problem / Why

In the QuickFiler folder dropdown, filing-target suggestions are presented as a flat list of folder
names in a plain `ComboBox` (`CboFolders`). Two gaps result:

1. Folders that contain subfolders are not navigable as a hierarchy; the user cannot expand or
   collapse parent folders.
2. The prediction probability that the scoring layer already computes for internal ranking is not
   surfaced to the dropdown; the arrays handed to the control contain folder names only.

The EfcViewer folder-list feature (sibling child 9002) delivers the equivalent tree + percentage
behavior on its `ListBox`. This feature brings the same behavior to the QuickFiler dropdown. The
per-folder probability is consumed from the upstream 9001 contract; this feature does not recompute
scores.

## Personas & Scenarios

- **Persona: QuickFiler user (email triager).**
  - Who: a person filing incoming email into a deep Outlook folder hierarchy using QuickFiler.
  - What they care about: filing quickly and correctly, and confirming a suggested folder is the
    right target before committing.
  - Constraints: the folder hierarchy is deep; several suggestions may look similar by name; the
    dropdown is the primary interaction surface for choosing a target.
  - Goals and frustrations: they want to trust the top suggestion, but today they cannot see how
    confident the system is, and they cannot drill into a parent folder's children from the dropdown.
  - Context and motivations: they process many messages in a session and rely on the ranked
    suggestions to move fast without misfiling.

- **Scenario: navigating and judging suggestions.** The user opens the QuickFiler folder dropdown for
  a message. They see ranked suggestions, each showing a right-aligned whole-number percentage. A
  top parent folder shows a plus affordance because it contains subfolders. The user expands it to
  inspect its children, compares the displayed percentages, highlights the correct child with the
  keyboard, and selects it. If a parent is not the target, they collapse it to restore the compact
  list and continue.

## Acceptance-Oriented Scenarios (Given / When / Then)

### Scenario 1 — Expand a folder that has subfolders (mouse)

- Given the QuickFiler folder dropdown is open and a suggestion is a folder that contains subfolders,
- And that node shows a plus (`+`) affordance to its left,
- When the user clicks the plus affordance,
- Then the node expands, its direct child folders become visible indented beneath it,
- And the affordance changes to a minus (`-`).

### Scenario 2 — Expand a folder that has subfolders (keyboard)

- Given the QuickFiler folder dropdown is open and a folder that contains subfolders is highlighted,
- And that node is currently collapsed,
- When the user presses the Right arrow key,
- Then the highlighted node expands and its child folders become visible.
- And given a leaf node (no subfolders) is highlighted, when the user presses the Right arrow key,
  then nothing changes (no-op).

### Scenario 3 — Collapse an expanded folder (mouse and keyboard)

- Given the QuickFiler folder dropdown is open and a folder that contains subfolders is expanded,
- And that node shows a minus (`-`) affordance,
- When the user clicks the minus affordance, then the node collapses and its child folders are hidden,
  and the affordance changes back to a plus (`+`).
- And given that expanded node is highlighted, when the user presses the Left arrow key, then the node
  collapses and its child folders are hidden.
- And given a leaf node or an already-collapsed node is highlighted, when the user presses the Left
  arrow key, then nothing changes (no-op).

### Scenario 4 — Read the prediction percentage on each suggestion

- Given the QuickFiler folder dropdown is open and suggestions carry a prediction probability from the
  upstream 9001 contract,
- When the user views the list,
- Then each suggestion displays its probability right-aligned as a whole-number percentage with no
  decimal places (for example a probability of `0.4267` displays as `43%`, `1.0` displays as `100%`,
  and `0.0` displays as `0%`),
- And synthesized ancestor rows, sentinel header rows, recents, and the "Trash to Delete" row display
  an empty percentage field.

### Scenario 5 — Collapse and re-expand preserves inner state

- Given a folder that has subfolders is expanded and one of its child folders is itself expanded,
- When the user collapses the parent and then re-expands it,
- Then the previously expanded child is shown expanded again (inner expansion state is preserved).

## Non-Goals

- No change to the scoring or ranking algorithm; the probability shown is the score already computed
  for internal ranking, consumed from the 9001 contract without recomputation.
- No change to the nine dead, design-time-only viewer variants that declare `CboFolders`; only the
  runtime-live `ItemViewer` is changed.
- No change to the EfcViewer folder-list path (owned by sibling 9002).
- No change to the QuickFiler body-render / WebView2 path or any file owned by the 9004 inline-image
  `cid:` bugfix.

## Acceptance Criteria

- [ ] Folders with subfolders render with a plus/minus expand affordance in the QuickFiler folder
      dropdown on the runtime-live `ItemViewer`; leaf folders render with no glyph.
- [ ] Clicking the plus expands a node; clicking the resulting minus collapses it.
- [ ] With a node highlighted, the Right arrow key expands it and the Left arrow key collapses it,
      with no-op behavior for leaves and already-expanded/collapsed nodes.
- [ ] Each suggestion shows its prediction probability right-aligned in whole-number percentage format
      (no decimal places); rows with no probability render an empty percentage field.
- [ ] The percentage value consumes the upstream 9001 probability contract; scores are not recomputed
      here.
- [ ] Shared tree/format/hierarchy logic lives in reusable, testable, host-neutral seams meeting the
      repository coverage thresholds (target: line `>= 85%`, branch `>= 75%`, new-module `>= 90%`).
