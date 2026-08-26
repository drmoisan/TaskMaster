# qfcformcontroller-actions-testability-seams (Issue #624)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfcformcontroller-actions-testability-seams/ (Issue #624)

- Issue: #624
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/624
- Last Updated: 2026-08-26
## Problem / Why


`QuickFiler/Controllers/QfcFormController.Actions.cs` sits at 47.89% line and 45.24% branch coverage because three blocks of it cannot be exercised without injectable seams. Measured during issue #446, the uncovered set is:

| Lines | Block | Why untestable today |
| --- | --- | --- |
| 29-160 | `LoadItems` / `LoadItemsAsync` overloads | bound directly to `TableLayoutPanel` and Outlook COM types with no seam |
| 241-258 | `ProcessUndoItemAsync` | COM-and-dispatcher take branch |
| 267-306 | `UndoDialog` | three modal `MessageBox.Show` calls at :225, :238 and :248 |

This was accepted as a documented carve-out for #446 because the Bugfix Workflow requires a minimal targeted fix and directs that deeper design problems be opened as a new issue rather than widening scope. The #446 feature review confirmed the carve-out was legitimate but recorded that no promoted document routed the resulting debt, which is what this entry fixes.

One correction to the original #446 rationale, established by the review: the carve-out note named only the `MessageBox.Show` calls, which understates the problem. A `MessageBox` seam alone would lift the file to roughly 67%, not past 90%, because the COM-bound loader overloads at 29-160 dominate the uncovered set. Any plan that seams only the dialog will miss the target.

## Proposed Behavior


Introduce injectable seams so the three blocks become testable without a live Outlook or a real WinForms message pump:

- a dialog-service seam replacing direct `MessageBox.Show` calls;
- loader seams isolating `TableLayoutPanel` and COM interaction from the `LoadItems*` control flow;
- a seam or fake for the `ProcessUndoItemAsync` take branch's COM and dispatcher dependencies.

Pure logic moves behind the seams and is covered by unit tests; the residual host-bound wiring stays as thin as possible.

## Acceptance Criteria (early draft)


- [ ] `QuickFiler/Controllers/QfcFormController.Actions.cs` line coverage is at least 90%
- [ ] Branch coverage on that file is at least 75%
- [ ] No `[ExcludeFromCodeCoverage]` is added to reach the target
- [ ] No test starts a real dialog, message pump, or live COM object
- [ ] Existing behavior is unchanged and the full QuickFiler.Test assembly remains green

## Constraints & Risks


- The repository's policy direction disfavors coverage exemptions where the real answer is a testability seam, so `[ExcludeFromCodeCoverage]` is not an acceptable route here.
- `QfcFormController` is a partial type; sibling declarations are owned by other epic children (features 442, 484, 444, 489). Coordinate to avoid annexing their scope.
- The file is currently 496 of the 500-line cap, so seam work must extract rather than add.
- UT4 prohibits exercising modal dialogs and live COM in unit tests; the seams exist precisely to keep tests off them.

## Test Conditions to Consider


- [ ] Dialog seam: each of the three call sites drives its branch through an inert fake
- [ ] Loader seams: positive, empty, and error paths for both `LoadItems*` overloads
- [ ] `ProcessUndoItemAsync`: take branch, exception branch, and cancellation
- [ ] Coverage re-measured on the changed-file scope, with the denominator stated

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/qfcformcontroller-actions-testability-seams/` folder from the template

