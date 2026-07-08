# Baseline — Exemption Inventory (Cycle 5)

- **Timestamp:** 2026-07-02T17-00
- **Command:** `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs`
- **EXIT_CODE:** 0
- **Output Summary:** Count = 24 (matches the ratified-pending boundary in `evidence/other/exemption-boundary.2026-07-02T15-05.md` exactly). 23 matches across `QfcItemController*.cs` (`ViewerSetup.cs` x3, `EventWiring.cs` x3, `Navigation.cs` x4, `Initialization.cs` x7, `EventHandlers.cs` x5, `Conversation.cs` x1) + 0 (`WpfUiDispatcher.cs`) + 1 (`WebView2CoreInitializer.cs`) + 0 (`MailItemActionsAdapter.cs`) = 24.

## Full grep output

```
QuickFiler/Controllers/QfcItemController.Conversation.cs:79:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:60:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:83:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:97:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:111:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:125:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventWiring.cs:32:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventWiring.cs:42:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.EventWiring.cs:105:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:138:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:168:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:200:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:260:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:291:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:403:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Initialization.cs:436:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Navigation.cs:173:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Navigation.cs:191:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Navigation.cs:210:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.Navigation.cs:224:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:35:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:78:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:127:        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
QuickFiler/Viewers/WebView2CoreInitializer.cs:15:    [ExcludeFromCodeCoverage]
```
