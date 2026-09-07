# File Sizes After Issue #480

Timestamp: 2026-08-26T08-55
Task: [P1-T7]

Command: `wc -l QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs <the five owned test files>`
EXIT_CODE: 0

| File | Baseline | Current | At most 500 |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | **325** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | **497** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | **374** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | **474** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | **214** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | **390** | yes |

Every recorded value is at most 500.

## Deltas

- `QfcItemController.FocusAndTheme.cs`: -1 line (the deleted unconditional toggle statement).
- `QfcItemController.MailActionsTests.cs`: +30 lines (the #480 `async: true` test plus its group header
  comment). The constraint C2 allowance for this group was 26; the 4-line excess is absorbed by the
  file's 316-line baseline headroom and leaves the file at 214 of a projected 478.
- `QfcItemController.TestSupport.cs`: +25 lines (the shared `BuildExecutingViewer()` arrange helper,
  appended after `ShutdownDispatcher`, the last existing member).

Output Summary: All six recorded files are at most 500 lines after the #480 change.
