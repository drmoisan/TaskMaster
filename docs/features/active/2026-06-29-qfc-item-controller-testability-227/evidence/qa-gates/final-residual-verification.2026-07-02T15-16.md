Timestamp: 2026-07-02T15:16
Command: `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs`
EXIT_CODE: 0 (grep found matches)
Output Summary: 24 matches, matching the P10-T36 boundary artifact (`evidence/other/exemption-boundary.2026-07-02T15-05.md`) exactly.

## Itemized 24

**12 — no-leaf-interface/`(ItemViewer)`-cast invariant:**
1. `Initialize` (9-arg private) — `QfcItemController.Initialization.cs`
2. `Initialize(bool async)` — `QfcItemController.Initialization.cs`
3. `InitializeAsync` — `QfcItemController.Initialization.cs`
4. `InitializeGraphicsAsync` — `QfcItemController.Initialization.cs`
5. `InitializeSequentialAsync` — `QfcItemController.Initialization.cs`
6. `CreateAsync` — `QfcItemController.Initialization.cs`
7. `CreateSequentialAsync` — `QfcItemController.Initialization.cs`
8. `InitializeWebViewAsync` — `QfcItemController.ViewerSetup.cs`
9. `ResolveControlGroups(ItemViewer)` — `QfcItemController.ViewerSetup.cs`
10. `ResolveControlGroupsAsync(ItemViewer)` — `QfcItemController.ViewerSetup.cs`
11. `WireEvents` — `QfcItemController.EventWiring.cs`
12. `WireControlTreeEvents` — `QfcItemController.EventWiring.cs`

**2 — `TlpCellSnapShot`-bound (P7-T5 named follow-up):**
13. `ToggleExpansionOff` — `QfcItemController.Navigation.cs`
14. `ToggleExpansionOn` — `QfcItemController.Navigation.cs`

**3 — deliberate virtual test seams:**
15. `DoLoadConversationResolverCoreAsync` — `QfcItemController.Conversation.cs`
16. `ToggleExpansion(Enums.ToggleState)` — `QfcItemController.Navigation.cs`
17. `ToggleExpansionAsync(Enums.ToggleState)` — `QfcItemController.Navigation.cs`

**6 — `async void` WinForms-event-signature shells:**
18. `BtnPopOut_Click` — `QfcItemController.EventHandlers.cs`
19. `BtnReply_Click` — `QfcItemController.EventHandlers.cs`
20. `BtnReplyAll_Click` — `QfcItemController.EventHandlers.cs`
21. `BtnForward_Click` — `QfcItemController.EventHandlers.cs`
22. `TxtboxBody_DoubleClick` — `QfcItemController.EventHandlers.cs`
23. `WebView2Control_CoreWebView2InitializationCompleted` — `QfcItemController.EventWiring.cs`

**1 — genuine external-runtime dependency:**
24. `WebView2CoreInitializer` — `QuickFiler/Viewers/WebView2CoreInitializer.cs`

Total: 12 + 2 + 3 + 6 + 1 = **24**. Matches the P10-T36 boundary artifact exactly (verified by direct
member-by-member comparison against `evidence/other/exemption-boundary.2026-07-02T15-05.md`).
