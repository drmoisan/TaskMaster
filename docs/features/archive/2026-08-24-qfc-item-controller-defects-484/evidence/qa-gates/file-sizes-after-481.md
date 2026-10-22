# File sizes after issue #481

Timestamp: 2026-08-26T10-58
Task: [P5-T13]

Command (run from the worktree root):

```
grep -c '' <each file below>
```

EXIT_CODE: 0

| File | Lines | <= 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 482 | yes |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 499 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 489 | yes |

All values are measured after a CSharpier format pass on every file edited in Phase 5, so they are
post-format counts and are not subject to later reflow.

## Constraint C2 relocation record for Phase 5

`QfcItemController.MailActionsTests.cs` was the planned home for the #481 intent-detach test but
closed Phase 4 at 498 lines. Under capacity rule 3 that test was relocated to
`QfcItemController.EventWiringTests.cs`, whose subject `UnwireIntentEvents()` lives in the matching
production partial; a header comment naming issue #481 accompanies it. Capacity rule 2 compaction was
applied throughout: the sixteen `VerifyRemove` assertions are routed through a local `Off` helper so
each fits one formatted line, and the shared reflection helper `RaiseProtected` was extracted into
`QfcItemController.TestSupport.cs`. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` was
compacted from 507 to 499 lines by shortening this feature's own comment blocks after `[P5-T10]`
pushed it over the ceiling. No `.csproj` was edited, no new file was created, and no forbidden file
was written.

Output Summary: All seven measured files are at most 500 lines. Maximum observed value is 499
(`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`).
