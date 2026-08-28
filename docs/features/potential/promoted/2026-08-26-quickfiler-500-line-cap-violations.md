# quickfiler-500-line-cap-violations (Issue #623)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-500-line-cap-violations/ (Issue #623)

- Issue: #623
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/623
- Last Updated: 2026-08-26
## Problem / Why


Three QuickFiler files exceed the repository's 500-line cap defined in `.claude/rules/general-code-change.md`:

| Path | Lines |
| --- | --- |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2349 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 827 |
| `QuickFiler/Controllers/QfcQueue.cs` | 610 |

All three predate issue #446 and none were in its owned file set, so #446 could not address them without widening its blast radius. The cap is not cosmetic here: during #446 the proximity of several files to the cap repeatedly constrained the shape of otherwise-straightforward changes and forced extraction work into unrelated tasks. `QfcCollectionController.cs` at 2349 lines is more than four times the cap.

## Proposed Behavior


Each file is decomposed into cohesive units under 500 lines, preserving public API and behavior. For the test file, split along fixture or scenario boundaries; note that `QuickFiler.Test.csproj` lists all Compile Include entries explicitly, so any new file must be registered there.

## Acceptance Criteria (early draft)


- [ ] `QuickFiler/Controllers/QfcCollectionController.cs` is at most 500 lines
- [ ] `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is at most 500 lines
- [ ] `QuickFiler/Controllers/QfcQueue.cs` is at most 500 lines
- [ ] No public API change and no test assertion weakened or removed
- [ ] The full QuickFiler.Test assembly remains green

## Constraints & Risks


- `QuickFiler.Test.csproj` enumerates every Compile Include explicitly; new files must be added there or they will not build.
- `QfcCollectionController.cs` at 2349 lines is large enough that decomposition should be staged rather than attempted in one change.
- Partial-class splits are the lowest-risk mechanism where a type must stay whole.

## Test Conditions to Consider


- [ ] Whole-assembly run green before and after each split
- [ ] Coverage does not regress on the moved lines
- [ ] No behavioral diff in the decomposed types

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-500-line-cap-violations/` folder from the template

