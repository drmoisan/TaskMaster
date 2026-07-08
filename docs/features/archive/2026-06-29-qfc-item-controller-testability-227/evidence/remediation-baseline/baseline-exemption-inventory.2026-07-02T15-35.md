# Baseline — Exemption Inventory (Cycle 4, Issue #227)

Timestamp: 2026-07-02T15-35
Command: `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs`
EXIT_CODE: 0
Output Summary: 24 matches. Confirmed count equals 24, unchanged from cycle-3 (this cycle is test-only and does not touch any exemption boundary).

Full grep output:
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
