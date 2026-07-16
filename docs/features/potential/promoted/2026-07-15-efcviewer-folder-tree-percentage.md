# efcviewer-folder-tree-percentage (Potential)

- Date captured: 2026-07-15
- Author: Dan Moisan
- Status: Promoted -> Issue #327 (https://github.com/drmoisan/TaskMaster/issues/327)
- Epic: folder-tree-percentage-ui (child feature, wave 1)
- Depends on: folder-probability-plumbing (upstream probability contract)

## Problem / Why

In the EfcViewer matching-folders list, suggested filing targets are rendered in a flat
`ListBox` (`FolderListBox`) bound to a `string[]` of full folder paths. There is no hierarchy,
no expand/collapse affordance, and no visible indication of how confident the model is in each
suggestion. Users cannot judge or navigate suggested folders quickly, and folders that contain
subfolders are shown as flat path strings rather than as an expandable tree.

## Proposed Behavior

In the EfcViewer matching-folders list:

1. Render folders that contain subfolders as expandable tree nodes.
   - A folder with subfolders shows a plus affordance to its left.
   - Clicking the plus expands the node; clicking the resulting minus collapses it.
   - When a node is highlighted, the right arrow key expands it and the left arrow key collapses it.
2. Print each suggestion's prediction probability right-aligned, in whole-number percentage
   format (no decimal places). The probability value is consumed from the upstream
   `folder-probability-plumbing` contract; it is not recomputed here.

Both parallel EfcViewer implementations (`EfcViewer.cs` and `EfcViewer3.cs`) must deliver the
behavior. There is no shared base class; shared logic (tree state, hierarchy building,
percentage formatting) is factored into reusable, host-neutral, testable helpers where practical.

## Acceptance Criteria (early draft)

- [ ] Folders containing subfolders render with a plus/minus expand affordance in the EfcViewer folder list.
- [ ] Mouse click on the plus expands the node; click on the minus collapses it.
- [ ] With a node highlighted, right arrow expands and left arrow collapses.
- [ ] Each suggestion shows its prediction probability right-aligned in whole-number percent (no decimals).
- [ ] Probability value is consumed from the upstream folder-probability-plumbing contract, not recomputed.
- [ ] Behavior delivered in both EfcViewer.cs and EfcViewer3.cs.
- [ ] Testable tree-state, hierarchy-building, and formatting logic meets repository coverage thresholds.

## Constraints & Risks

- Neither the `ListBox` nor the `ComboBox` used elsewhere natively supports hierarchy; tree
  expand/collapse must be built on top of the chosen control.
- Two non-shared EfcViewer implementations increase duplication risk; factor shared logic into a
  reusable helper.
- WinForms form-derived and Designer-generated classes are coverage-exempt; keep testable logic
  in host-neutral seams.
- Depends on the upstream `folder-probability-plumbing` public contract (folder identity + its
  probability). At epic execution time the upstream merges into the integration branch first.

## Test Conditions to Consider

- [ ] Unit coverage: tree state transitions (expand/collapse) via mouse and keyboard.
- [ ] Unit coverage: hierarchy building from flat folder-path input.
- [ ] Unit coverage: whole-number percentage formatting and right-alignment.
- [ ] Integration scenarios: both EfcViewer implementations bind and render correctly.

## Next Step

- [x] Promote to GitHub issue (feature request template) -> Issue #327
- [x] Create `docs/features/active/2026-07-15-efcviewer-folder-tree-percentage-327/` folder from the template
