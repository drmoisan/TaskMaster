# Coverage-exemption audit (constraint C5)

Timestamp: 2026-08-26T14-08
Task: [P7-T11]

Command (run from the worktree root):

```
grep -c "ExcludeFromCodeCoverage" QuickFiler/Controllers/QfcItemController.EventWiring.cs QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs QuickFiler/Controllers/QfcItemController.MailActions.cs
```

EXIT_CODE: 0

| File | `[P0-T16]` baseline | Measured now | Match |
|---|---|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 1 | **1** | yes |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 2 | **2** | yes |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 0 | **0** | yes |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 0 | **0** | yes |
| **Total** | **3** | **3** | yes |

The per-file counts and the total match the `[P0-T16]` baseline exactly. No new
`[ExcludeFromCodeCoverage]` attribute was introduced anywhere by this feature, and none was removed.
The three retained occurrences are all pre-existing: the async-void WebView2 initialization shell in
`QfcItemController.EventWiring.cs`, and `InitializeWebViewAsync` plus `EnsureBreadcrumbPipeline` in
`QfcItemController.ViewerSetup.cs`.

Output Summary: EventWiring 1, ViewerSetup 2, FocusAndTheme 0, MailActions 0, total 3 — identical to
the `[P0-T16]` baseline.
