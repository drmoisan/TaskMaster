# `efcviewer-folder-tree-percentage` — User Story

- Issue: #327
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-07-15T17-30
- Epic: folder-tree-percentage-ui (child feature, wave 1)
- Work Mode: full-feature

## Story Statement

- As a QuickFiler/EfcViewer user reviewing suggested filing folders, I want folders that contain
  subfolders to appear as expandable tree nodes I can open and close, so that I can see and reach a
  nested target folder without reading long flat path strings.
- As a QuickFiler/EfcViewer user judging suggested filing folders, I want each suggestion's
  prediction probability shown as a right-aligned whole-number percentage, so that I can quickly
  compare how confident the model is in each suggestion and decide where to file.

## Problem / Why

In the EfcViewer matching-folders list, suggested filing targets are rendered in a flat
`ListBox` (`FolderListBox`) bound to a `string[]` of full folder paths. There is no hierarchy,
no expand/collapse affordance, and no visible indication of how confident the model is in each
suggestion. Users cannot judge or navigate suggested folders quickly, and folders that contain
subfolders are shown as flat path strings rather than as an expandable tree.

The prediction probability the model already computes for internal ranking is never shown, so the
user cannot see which suggestions the model is confident about. Nested folders appear only as long
path strings, which is slow to scan and offers no way to expand or collapse a branch.

## Personas & Scenarios

- Persona: QuickFiler/EfcViewer user (an Outlook user filing mail with the add-in).
  - Who they are: a person triaging and filing email into an archive folder hierarchy using the
    EfcViewer folder-suggestion list.
  - What they care about: filing each message into the correct folder quickly and with confidence.
  - Their constraints: many suggested folders, deep nested folder paths, and limited time per
    message; the display font is proportional, so alignment cues matter.
  - Their goals and frustrations: they want to compare suggestions at a glance and drill into a
    nested folder without parsing long path strings; today they see a flat list with no confidence
    signal and no way to expand or collapse a branch.
  - Their context and motivations: repeated, high-volume filing where small per-message friction
    accumulates.

- Scenario: judging and navigating suggestions.
  - Who is acting: the QuickFiler/EfcViewer user.
  - What triggered the action: the user opens EfcViewer for a message and sees the matching-folders
    list populated with suggestions.
  - Steps they take: they scan the list, reading each suggestion's right-aligned whole-number
    percentage to compare model confidence; they see a plus affordance next to a folder that
    contains subfolders; they click the plus (or highlight the node and press the right arrow) to
    expand it and reveal its children; they select the correct nested folder; if a branch is not
    relevant they click the minus (or press the left arrow on the highlighted node) to collapse it
    and reduce clutter.
  - Obstacles or decisions: they must distinguish high-confidence from low-confidence suggestions
    and decide which branch to open; non-selectable section/banner rows must not be mistaken for
    filing targets.
  - Outcome they expect: they file the message into the right folder faster and with more
    confidence, having compared percentages and navigated the hierarchy directly.

## Acceptance Criteria

These items are consistent with the authoritative Acceptance Criteria in `spec.md`. All items are
unchecked because nothing is delivered yet.

- [ ] Folders containing subfolders render with a plus/minus expand affordance in the EfcViewer folder list.
- [ ] Mouse click on the plus expands the node and reveals its children; click on the resulting minus collapses it and hides its descendants.
- [ ] With a node highlighted, the right arrow key expands it and the left arrow key collapses it.
- [ ] Each suggestion shows its prediction probability right-aligned in whole-number percent (no decimal places).
- [ ] The probability value is consumed from the upstream `folder-probability-plumbing` contract and is not recomputed; rows with no upstream probability render a blank percentage cell.
- [ ] The behavior is delivered in both `EfcViewer.cs` and `EfcViewer3.cs`.
- [ ] The shared, host-neutral tree-state, hierarchy-building, visible-row projection, and percentage-formatting logic meets the repository coverage thresholds.

## Non-Goals

- No change to the scoring or ranking algorithm, or to model output; the probability shown is the
  value already computed for internal ranking (upstream `folder-probability-plumbing`, epic
  placeholder issue 9001, delivers that plumbing).
- The QuickFiler folder dropdown tree + percentage behavior is a separate epic sibling feature
  (`quickfiler-folder-tree-percentage`, epic placeholder issue 9003) and is out of scope here.
- No change to which folders are suggested or their ordering.
- No unification of the two EfcViewer implementations into a shared base control beyond the
  reusable host-neutral helper.
