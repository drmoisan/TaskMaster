# efcviewer-folder-tree-percentage — Spec

- **Issue:** #327
- **Parent (optional):** Epic `folder-tree-percentage-ui` (child feature, wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T17-30
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature

## Overview

In the EfcViewer matching-folders list, suggested filing targets are rendered in a flat
`ListBox` (`FolderListBox`) bound to a `string[]` of full folder paths. There is no hierarchy,
no expand/collapse affordance, and no visible indication of how confident the model is in each
suggestion. Users cannot judge or navigate suggested folders quickly, and folders that contain
subfolders are shown as flat path strings rather than as an expandable tree.

This feature adds two presentation capabilities to the EfcViewer matching-folders list:

1. Folders that contain subfolders render as expandable tree nodes with a plus/minus affordance,
   expandable and collapsible by mouse and by keyboard.
2. Each suggestion's prediction probability is printed right-aligned, in whole-number percentage
   format (no decimal places). The probability is consumed from the upstream
   `folder-probability-plumbing` contract and is never recomputed here.

The feature is scoped to presentation only. It changes how existing suggestions are displayed;
it does not change which folders are suggested, their ordering, or the scores that rank them.

## Scope

### In Scope

- Rendering folders that contain subfolders as expandable tree nodes in the EfcViewer
  matching-folders list.
- Plus/minus expand/collapse affordance driven by mouse click.
- Keyboard expand/collapse on the highlighted node (right arrow expands, left arrow collapses).
- Right-aligned, whole-number percentage display of each suggestion's prediction probability,
  consumed from the upstream `folder-probability-plumbing` contract.
- A blank percentage cell for any row that has no probability available (banner/section rows,
  recents, and unscored search matches).
- Delivery of the behavior in BOTH parallel viewers: `QuickFiler/Viewers/EfcViewer.cs` and
  `QuickFiler/Viewers/EfcViewer3.cs`.
- A reusable, host-neutral, testable helper that carries the shared logic (tree state, hierarchy
  building, percentage formatting, visible-row projection, banner classification, and the
  path-to-probability adapter), meeting repository coverage thresholds.

### Out of Scope

- Any change to the scoring or ranking algorithm, or to model output. The probability surfaced is
  the value already computed for internal ranking; this feature consumes it and does not alter it.
  Scoring plumbing itself is delivered by the upstream sibling feature `folder-probability-plumbing`
  (epic placeholder issue 9001).
- The QuickFiler folder dropdown tree + percentage behavior, which is a separate epic sibling
  feature (`quickfiler-folder-tree-percentage`, epic placeholder issue 9003).
- No unification of the two EfcViewer implementations into a shared base control beyond factoring
  the reusable host-neutral helper.
- No change to folder suggestion selection, ordering, or the set of suggested folders.

## Behavior / Functional Requirements

In the EfcViewer matching-folders list:

1. **Tree rendering.** Folders that contain subfolders (as derived from the presented suggestion
   paths) render as expandable tree nodes. A folder with subfolders shows a plus affordance to its
   left.
2. **Mouse expand/collapse.** Clicking the plus affordance expands the node and reveals its
   children; clicking the resulting minus affordance collapses it and hides its descendants.
3. **Keyboard expand/collapse.** When a node is highlighted, the right arrow key expands it and the
   left arrow key collapses it. On a highlighted leaf node, an already-expanded node (right arrow),
   or an already-collapsed node (left arrow), the corresponding key is a no-op for expand/collapse
   state.
4. **Percentage display.** Each suggestion prints its prediction probability right-aligned, in
   whole-number percentage format with no decimal places (for example, a probability of `0.732`
   renders as `73%`). The value is consumed from the upstream `folder-probability-plumbing`
   contract and is joined to each row; it is not recomputed.
5. **Blank percentage where unavailable.** Rows that carry no probability — section/banner rows,
   recents, and search matches with no upstream score — render a blank percentage cell.
6. **Banner / section rows.** Existing non-selectable section/banner rows (those beginning with
   `====`) remain non-selectable, non-expandable, and are never valid filing targets.
7. **Dual delivery.** The behavior is delivered identically in both `EfcViewer.cs` and
   `EfcViewer3.cs`. There is no shared base class between the two viewers.

## Upstream Contract Dependency

This feature depends on the `folder-probability-plumbing` upstream contract (epic sibling,
placeholder issue 9001).

- **Assumed shape.** The contract surfaces a mapping of folder identity (full folder-path string)
  to prediction probability, where the probability is a `double` in the range `[0, 1]`. It is most
  plausibly exposed as either a keyed lookup by full folder path or an ordered
  `IReadOnlyList<(string FolderPath, double Probability)>`.
- **Consumed, not recomputed.** This feature joins the upstream probability to each presented row
  by full-path string equality against the existing suggestion output. It does not compute,
  normalize, or re-rank scores. If upstream exposes an already-scaled percentage rather than a
  `[0, 1]` probability, only the percentage formatter changes (drop the `× 100` step).
- **Sequencing.** At epic execution time, `folder-probability-plumbing` (9001) merges into the
  epic integration branch before this feature (#327) runs. The assumed contract shape must be
  re-confirmed against the merged 9001 surface before implementation proceeds, and the plan must
  record any deviation from the assumed shape.

## Design Constraints

- **No native hierarchy in ListBox/ComboBox.** Neither the `ListBox` used here nor the `ComboBox`
  used elsewhere natively supports hierarchy; tree expand/collapse and right-alignment must be
  built on top of a hierarchy-capable control. The proportional display font rules out
  column alignment by string padding.
- **Reuse the existing tested tree pattern where practical.** The repository already contains a
  proven, tested folder-tree pattern built on BrightIdeasSoftware `TreeListView`
  (`FilterOlFoldersController` with the host-neutral `FolderTreeCompatibilityView`), and the
  `ObjectListView 2.9.1` library is already referenced by QuickFiler. The design should reuse this
  established pattern rather than hand-rolling tree glyph, hit-testing, keyboard, and owner-draw
  logic, consistent with the repository "simplicity first" and "match existing style" principles.
  This spec does not mandate a specific control API; it constrains the design to reuse the existing
  tested tree pattern where practical.
- **Shared host-neutral helper.** The two viewers share no base class. Shared logic — tree state,
  hierarchy building from the presented paths, percentage formatting, visible-row projection,
  banner classification, and the path-to-probability adapter — must be factored into a reusable,
  host-neutral, testable helper (no WinForms or COM dependency) so both viewers deliver identical
  behavior with only thin, per-viewer wiring differing.

## Testability & Coverage Requirements

- The host-neutral helper (tree state and state transitions, hierarchy building, visible-row
  projection, percentage formatting, banner classification, and the path-to-probability adapter)
  is the coverage-bearing deliverable and must meet the repository coverage thresholds for new
  code.
- Tests use MSTest with Moq and FluentAssertions, are deterministic, and use no temporary files,
  network, filesystem, or COM. The host-neutral helper is pure in-memory logic and is tested
  directly, not through the COM-bound controller.
- WinForms form-derived classes and Designer-generated code are coverage-exempt per CLAUDE.md and
  are excluded via `[ExcludeFromCodeCoverage]`. Research finding: `EfcViewer.cs` already carries
  `[ExcludeFromCodeCoverage]`, but `EfcViewer3.cs` currently does not; if UI/tree wiring is added
  to `EfcViewer3`, the attribute must be added so the Form-derived code stays exempt and does not
  enter the testable denominator.
- Test scenarios to cover include: hierarchy building from representative sectioned path input
  (roots, nested children, a deep path without its parent present, banners); expand/collapse
  transitions and visible-row projection for all edge cases (leaf, already-expanded,
  already-collapsed, root, highlighted banner, empty list, single node); percentage formatting
  (0, 1, rounding at the `.5` boundary away from zero, null probability rendering blank); and the
  path-to-probability join by full-path equality including unmatched rows.
- The full C# toolchain must be green: `csharpier` formatting, .NET analyzer diagnostics, nullable
  reference-type checks with warnings treated as errors, and MSTest unit tests. WinForms/COM wiring
  in the two Forms and the controller is coverage-exempt and verified by build plus manual QA.

## Acceptance Criteria

This section is an authoritative acceptance-criteria source for this feature, alongside
`user-story.md`. All items are unchecked because nothing is delivered yet.

- [x] Folders containing subfolders render with a plus/minus expand affordance in the EfcViewer folder list.
- [x] Mouse click on the plus expands the node and reveals its children; click on the resulting minus collapses it and hides its descendants.
- [x] With a node highlighted, the right arrow key expands it and the left arrow key collapses it.
- [x] Each suggestion shows its prediction probability right-aligned in whole-number percent (no decimal places).
- [x] The probability value is consumed from the upstream `folder-probability-plumbing` contract (folder path to `double` in `[0, 1]`) and is not recomputed; rows with no upstream probability render a blank percentage cell.
- [x] The behavior is delivered in BOTH viewers: `QuickFiler/Viewers/EfcViewer.cs` and `QuickFiler/Viewers/EfcViewer3.cs`.
- [x] The shared, host-neutral logic (tree state and transitions, hierarchy building, visible-row projection, percentage formatting, banner classification, path-to-probability adapter) is factored into a reusable testable helper that meets the repository coverage thresholds.
- [x] The full C# toolchain (csharpier, .NET analyzers, nullable, MSTest with Moq + FluentAssertions) is green.

## Definition of Done

- [x] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in both viewers
- [x] Tests updated/added (unit for host-neutral helper; build + manual QA for UI wiring)
- [x] Edge cases and error handling covered by tests
- [x] Docs updated (this spec and `user-story.md` cross-referenced)
- [x] Upstream `folder-probability-plumbing` contract shape re-confirmed after 9001 merges
- [x] Toolchain pass completed (format → lint → type-check → test)
