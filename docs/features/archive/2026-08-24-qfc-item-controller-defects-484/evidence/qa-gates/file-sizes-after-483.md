# File Sizes After Issue #483

Timestamp: 2026-08-26T09-37
Task: [P3-T8]

Command: `wc -l QuickFiler/Controllers/QfcItemController.MailActions.cs <the five owned test files>`
EXIT_CODE: 0

| File | Baseline | Current | At most 500 |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 224 | **257** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | **497** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | **374** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | **474** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | **452** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | **453** | yes |

Every recorded value is at most 500.

## Capacity note

The #483 test group was first written at 170 lines against a constraint C2 allowance of 96, which would
have left `QfcItemController.MailActionsTests.cs` without room for the remaining #481 and #484 groups.
Constraint C2 capacity rule 2 was applied rather than relocation:

- its arrange block was extracted into the shared helper
  `QfcItemControllerTestSupport.InjectFilingCollaborators(controller, filerFactory)`, and the
  pre-cancelled-token arrange into `QfcItemControllerTestSupport.CancelledToken()`, both appended after
  the last existing member of `QfcItemControllerTestSupport`;
- a three-line `Filing(...)` wrapper in the test class binds the shared helper to the local
  `MailController` subclass and pre-replaces the notifier seam, so no test can reach a modal dialog;
- the per-test XML doc comments were reduced to a single line each.

The delivered group is 130 lines. `QfcItemController.MailActionsTests.cs` is at **452** with 48 lines of
headroom, and `QfcItemController.TestSupport.cs` at **453** with 47.

## Remaining capacity plan for Phase 5 (issue #481) and Phase 4 (issue #484)

`QfcItemController.EventWiringTests.cs` retains the largest headroom (126 lines) and is the natural home
for both #481 unwire tests. Under constraint C2 capacity rule 3 the #481 intent-detach group is
relocated there from its planned `QfcItemController.MailActionsTests.cs` home, carrying a header comment
naming issue #481; the #481 control-tree unwire test is already mandated to live there by `[P5-T2]`. The
#481 teardown-robustness test is relocated to `QfcItemController.ViewerSetupTests.cs`, which already
hosts `Cleanup_NullsTrackedPrivateFields` and has 26 lines of headroom. The #484 group stays in
`QfcItemController.MailActionsTests.cs` with its arrange extracted to `QfcItemController.TestSupport.cs`.

No `.csproj` was edited, no file was created, and no forbidden file was written.

Output Summary: All six recorded files are at most 500 lines after the #483 change.
