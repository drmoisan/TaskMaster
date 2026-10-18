# QuickFiler descendant selection and recursive expansion (Issue #997)

- Date captured: 2026-10-17
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/QuickFiler_descendant_selection_and_recursive_expansion/ (Issue #997)

- Issue: #997
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/997
- Last Updated: 2026-10-18
## Problem / Why

QuickFiler expands top-level folders, but revealed subfolders cannot be selected as filing destinations. Descendants with children do not show their own expansion controls, preventing navigation to deeper destinations.

## Proposed Behavior

Every displayed folder is selectable using the same interaction as a top-level destination, including intermediate folders with children. Plus/minus controls only expand or collapse their folder. Expansion reveals immediate children recursively at every depth; each folder with children has a control and leaves have none.

## Acceptance Criteria (early draft)

- [ ] Every displayed descendant folder, including intermediate folders, is selectable as a filing destination using the top-level selection behavior.
- [ ] Clicking plus/minus expands or collapses only and does not change the selected destination.
- [ ] Every folder with children has an expansion control at every depth; leaves have none.

## Constraints & Risks

Preserve archive-relative destination validation, existing confirmation behavior, duplicate folder identity handling, and asynchronous request ordering. Use existing dependencies and deterministic in-memory tests.

## Test Conditions to Consider

- [ ] Regression reproductions for descendant selection and missing nested controls.
- [ ] Three or more folder levels, intermediate and leaf selection, and expansion/collapse without selection.
- [ ] Existing bridge, state-model, and embedded browser UI contracts remain valid.

## Next Step

- [ ] Promote to a bug issue and create a full-bug active folder.
