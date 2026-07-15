# quickfiler-folder-tree-percentage (Issue #325)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-folder-tree-percentage/ (Issue #325)
- Epic: folder-tree-percentage-ui (child feature 9003, wave 1)
- Depends on: folder-probability-plumbing (epic placeholder issue 9001)

- Issue: #325
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/325
- Last Updated: 2026-07-15
- Work Mode: full-feature

## Problem / Why

In the QuickFiler folder dropdowns, filing-target suggestions are presented as a flat list of
folder names in a plain `ComboBox` (`CboFolders`), independently declared across multiple viewer
Designer files (up to nine variants such as `QfcItemViewerV1`, `QfcItemViewerExpanded`,
`QFCItemViewerDarkNew`) with no shared base control. Two gaps result:

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

Expand/collapse behavior required:

- A folder with subfolders shows a plus affordance to its left; clicking the plus expands it and
  clicking the resulting minus collapses it.
- When a node is highlighted, the right arrow key expands it and the left arrow key collapses it.

## Proposed Approach (high level)

- Build tree expand/collapse behavior (mouse plus keyboard left/right) on the `ComboBox`, or
  substitute/augment with an appropriate control consistent with the existing QuickFiler UI,
  delivered across the viewer variants. Factor shared logic (tree state, hierarchy building,
  percentage formatting) into a reusable, testable, host-neutral helper rather than copy-pasting
  across all variants.
- The percentage display consumes the upstream per-folder probability contract introduced by
  9001 (folder identity + its probability). This feature does not recompute scores and does not
  implement 9001's scoring changes.

## Acceptance Criteria (early draft)

- [ ] Folders with subfolders render with a plus/minus expand affordance in the QuickFiler folder dropdown across the enumerated viewer variants.
- [ ] Clicking the plus expands a node; clicking the resulting minus collapses it.
- [ ] With a node highlighted, right arrow expands it and left arrow collapses it.
- [ ] Each suggestion shows its prediction probability right-aligned in whole-number percentage format (no decimal places).
- [ ] The percentage value consumes the upstream 9001 probability contract; scores are not recomputed here.
- [ ] Shared tree/format/hierarchy logic lives in a reusable, testable, host-neutral seam meeting repository coverage thresholds.

## Constraints & Risks

- Depends on upstream contract from 9001 (per-folder probability exposed to the presentation
  layer). At epic execution time 9001 merges into the integration branch before this feature runs.
- `ComboBox` does not natively support hierarchy; expand/collapse must be built on top of it or a
  substitute/augmenting control consistent with the QuickFiler UI.
- Cross-cutting change repeated across up to nine viewer variants with no shared base control.
- WinForms/COM coverage exemption applies to Designer-generated classes; testable logic must stay
  in host-neutral seams meeting coverage thresholds.
- Shares NO files with the QuickFiler inline-image `cid:` bugfix sibling (9004). The body-render
  path must not be touched.

## Test Conditions to Consider

- [ ] Unit coverage of tree state transitions (expand/collapse invariants)
- [ ] Unit coverage of percentage formatting (whole-number percent, no decimals, right-aligned)
- [ ] Unit coverage of hierarchy building from folder identities
- [ ] Consumption of the 9001 probability contract shape

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-folder-tree-percentage/` folder from the template
