# quickfiler-folder-tree-percentage (Potential — Promoted)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> Issue #325 -> docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/
- Issue: #325 (https://github.com/drmoisan/TaskMaster/issues/325)
- Epic: folder-tree-percentage-ui (child feature 9003, wave 1)
- Depends on: folder-probability-plumbing (epic placeholder issue 9001)

## Problem / Why

In the QuickFiler folder dropdowns, filing-target suggestions are presented as a flat list of
folder names in a plain `ComboBox` (`CboFolders`), independently declared across multiple viewer
Designer files with no shared base control. Two gaps result:

1. Folders that contain subfolders are not navigable as a hierarchy; the user cannot expand or
   collapse parent folders.
2. The prediction probability that the scoring layer already computes for internal ranking is not
   surfaced to the dropdown; the arrays handed to the control contain folder names only.

The EfcViewer folder-list feature (sibling child 9002) delivers the equivalent tree +
percentage behavior on its `ListBox`. This feature brings the same behavior to the QuickFiler
dropdowns.

## Proposed Behavior

In the QuickFiler folder dropdowns:

(a) Render folders that contain subfolders as expandable tree nodes.
(b) Print each suggestion's prediction probability right-aligned in whole-number percentage
    format (no decimal places).

Expand/collapse behavior:

- A folder with subfolders shows a plus affordance to its left; clicking the plus expands it and
  clicking the resulting minus collapses it.
- When a node is highlighted, the right arrow key expands it and the left arrow key collapses it.

## Promotion Outcome

- Promoted as a `feature` workflow in `full-feature` work mode.
- GitHub issue #325 created.
- Active feature folder: `docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/`
  (issue.md, spec.md, user-story.md, research/, plan.2026-07-15T16-43.md).

Full requirement detail, acceptance criteria, constraints, and the consumed upstream 9001
contract are captured in the active feature folder's `issue.md`, `spec.md`, and `user-story.md`.
