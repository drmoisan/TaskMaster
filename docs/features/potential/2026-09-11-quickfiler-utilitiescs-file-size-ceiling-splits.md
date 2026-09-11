# quickfiler-utilitiescs-file-size-ceiling-splits (Potential)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

## Problem / Why

Four files exceed the 500-line ceiling in the General Code Change Policy. All are pre-existing; each was flagged by a review that did not cause the excess. Deferred out of the 2026-09-11 consolidated bug run as refactor work (issue #727 sub-finding 3). Measured on `main` at 3cb974422:

| File | Lines |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2028 |
| `QuickFiler/Controllers/EfcFormController.cs` | 1181 |
| `UtilitiesCS/Threading/TimeOutTask.cs` | 878 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 649 |

`QuickFiler/Controllers/QfcQueue.cs`, flagged in #727 at 505 lines, is now 439 and no longer over the ceiling.

## Proposed Behavior

Split each file into partial-class parts by responsibility, with no behavior change, following the precedents already in the tree: `QfcQueue.Enqueue.cs` (a member group moved to a named part with a header explaining why) and item #817 (a test class split into five partial-class files). Each resulting part is under 500 lines and named `<Type>.<Responsibility>.cs`. `<Compile Include>` items are added for each new part and none is dropped.

## Acceptance Criteria (early draft)

- [ ] No file among the four exceeds 500 lines, and no new part exceeds 500 lines.
- [ ] `git diff` shows moves only: no member body changes, verified by a before/after symbol inventory.
- [ ] Coverage on every moved line is unchanged (moved lines are neither newly covered nor newly uncovered).
- [ ] Each new part's header states which members it holds and why it exists.
- [ ] Full C# toolchain passes.

## Constraints & Risks

- Must be scheduled after the 2026-09-11 bug run merges: items #792 and #742 edit `QfcCollectionController.cs`, item #792 edits `EfcFormController.cs`, and item #743 edits neighbouring `QfcItemController` parts. A split landing concurrently would force every one of them to re-resolve.
- `QfcCollectionController.cs` at 2028 lines will need at least four parts; choose the responsibility boundaries from the existing region and comment structure rather than by line count.
- One item per file, or one item for all four, is a planning choice for whoever picks this up; the blast radius is disjoint across the four files.

## Test Conditions to Consider

- [ ] Unit coverage areas: none new; existing suites must pass unchanged.
- [ ] Integration scenarios: none.
- [ ] CLI/API examples: not applicable.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-utilitiescs-file-size-ceiling-splits/` folder from the template
