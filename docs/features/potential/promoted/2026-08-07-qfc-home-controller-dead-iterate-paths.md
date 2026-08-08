# qfc-home-controller-dead-iterate-paths (Issue #447)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-home-controller-dead-iterate-paths/ (Issue #447)

- Issue: #447
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/447
- Last Updated: 2026-08-08
## Problem / Why

`QfcHomeController.Iterate()` and `QfcHomeController.Iterate2()` are unreachable in production but are still compiled, so they sit in the coverage denominator and must be tested by epic child F7 (`quickfiler-qfc-home-controller-coverage`, issue #433) despite delivering no behavior.

Evidence is recorded in
`docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/research/QfcHomeController.Iteration.cs.research.2026-08-07T20-50.md` (finding LD1):

- The two methods account for 23 of the 86 lines of `QuickFiler/Controllers/QfcHomeController.Iteration.cs`.
- `Iterate` is bound into `QfcFormController`'s private `IterateDelegate Iterate` field (`QfcFormController.cs:48`, declared at `:85`, nulled at `QfcFormController.SetupDisposal.cs:225`). That field is never invoked.
- `Iterate2` has no caller anywhere in the repository.
- Both are declared on `IQfcHomeController` (`QuickFiler/Controllers/IQfcHomeController.cs`), so removal is an interface change.

## Proposed Behavior

Remove `Iterate()` and `Iterate2()` and their `IQfcHomeController` declarations, together with the unused `IterateDelegate` binding in `QfcFormController`, after confirming no reflection-based or late-bound caller exists. The observable behavior of QuickFiler should be unchanged because neither method executes today.

## Acceptance Criteria (early draft)

- [ ] A repository-wide search confirms no caller of either method, including reflection and delegate indirection.
- [ ] Both methods and their interface declarations are removed.
- [ ] The now-unused `IterateDelegate` field and its assignment and disposal in `QfcFormController` are removed.
- [ ] Full C# toolchain passes and no observable QuickFiler behavior changes.

## Constraints & Risks

- This spans files owned by two concurrent epic children: `QfcHomeController.Iteration.cs` and `IQfcHomeController.cs` (F7) and `QfcFormController*` (F6). It must not be executed unilaterally inside either child.
- Sequencing: doing this before F7 executes would shrink F7's target surface by 23 lines and remove test work; doing it after means F7 writes tests that are then deleted. Deferring to the epic capstone (F16) or scheduling it after the epic completes both avoid mid-epic churn.

## Decision recorded for F7

F7 will COVER these methods rather than remove them. Rationale: removing production code inside a coverage child would breach the epic NFR of no behavior change through a testability refactor, and the removal necessarily touches F6-owned files, which the epic's disjoint-file-set design prohibits. The removal is recorded here instead so it is not lost.

## Test Conditions to Consider

- [ ] Unit coverage areas: confirm no remaining references after removal.
- [ ] Integration scenarios: a full QuickFiler session in both normal and High Confidence mode.
- [ ] CLI/API examples: n/a

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Schedule after the `quickfiler-per-file-coverage` epic completes, or fold into its capstone
