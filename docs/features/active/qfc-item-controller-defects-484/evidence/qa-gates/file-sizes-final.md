# File sizes after the final formatting pass

Timestamp: 2026-08-26T14-08
Task: [P7-T8]

Command (run from the worktree root, after the `[P7-T1]` CSharpier pass):

```
grep -c '' <each of the nine owned files>
```

EXIT_CODE: 0

| File | Lines | <= 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | yes |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 482 | yes |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 257 | yes |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 338 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 499 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 498 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 489 | yes |

All nine owned files — four production partials and five test files — are named explicitly above with
their measured values. Every value is at most 500. The maximum observed value is 499
(`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`).

These counts are post-format: `[P7-T1]` confirmed by SHA-256 comparison that CSharpier rewrote none
of the nine, so no later reflow can change them.

Output Summary: Nine of nine owned files at most 500 lines; maximum 499.
