# File sizes after issue #484

Timestamp: 2026-08-26T10-09
Task: [P4-T9]

Command (run from the worktree root):

```
grep -c '' <each file below>
```

EXIT_CODE: 0

| File | Lines | <= 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 487 | yes |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 338 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 483 | yes |

All values are measured after a CSharpier format pass on every file edited in Phase 4, so they are
post-format counts and are not subject to later reflow.

## Constraint C2 relocation record

`QfcItemController.MailActionsTests.cs` was the planned home for all three #484 tests. It entered
Phase 4 at 459 lines (275 lines above its 184-line C2 baseline, spent by Phases 1 to 3), leaving 41
lines of headroom against the 500-line ceiling. The `[P4-T1]` and `[P4-T2]` tests consumed 39 of them.
Under capacity rule 3, `Cleanup_NullsMailActions_AndSaveParametersRebindsIt` was therefore relocated to
`QfcItemController.ViewerSetupTests.cs`, whose subject `Cleanup()` lives in the matching production
partial, with a header comment naming issue #484 as rule 3 requires. Its shared arrange helper
`DriveSaveParameters` was extracted into `QfcItemController.TestSupport.cs` per capacity rule 2. No
`.csproj` was edited, no new file was created, and no forbidden file was written.

Output Summary: All seven measured files are at most 500 lines. Maximum observed value is 498
(`QfcItemController.ViewerSetupTests.cs` and `QfcItemController.MailActionsTests.cs`).
