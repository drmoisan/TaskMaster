Timestamp: 2026-07-02T14:26
Command: grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs
EXIT_CODE: 0
Output Summary: 41 matches (38 QfcItemController*.cs members + 3 DI-adapter shims: WpfUiDispatcher, WebView2CoreInitializer, MailItemActionsAdapter). Count confirmed via `wc -l` = 41, matching evidence/qa-gates/p7r-residual-verification.2026-07-02T10-30.md. Full grep output:

QuickFiler/Controllers/QfcItemController.Conversation.cs:79
QuickFiler/Controllers/QfcItemController.EventHandlers.cs:51,63,86,100,114,128,170
QuickFiler/Controllers/QfcItemController.EventWiring.cs:32,42,105,315
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:32,79,97,142,342
QuickFiler/Controllers/QfcItemController.FolderHandling.cs:29,52,121,140
QuickFiler/Controllers/QfcItemController.Initialization.cs:129,159,190,249,275,383,416
QuickFiler/Controllers/QfcItemController.Navigation.cs:60,177,195,214,228
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:35,78,127,179,195
UtilitiesCS/Threading/WpfUiDispatcher.cs:16
QuickFiler/Viewers/WebView2CoreInitializer.cs:15
QuickFiler/Interfaces/MailItemActionsAdapter.cs:13

Total: 41 (matches expected baseline exactly).
