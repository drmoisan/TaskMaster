# Phase 6 Gate — File Sizes (P6-T13)

Timestamp: 2026-07-02T10-17
Command: wc -l on every production and test file created/edited in Phase 6

## Production (new seam files)

| File | Lines |
|---|---:|
| UtilitiesCS/Threading/IUiDispatcher.cs | 36 |
| UtilitiesCS/Threading/WpfUiDispatcher.cs | 40 |
| QuickFiler/Viewers/IWebViewCoreInitializer.cs | 34 |
| QuickFiler/Viewers/WebView2CoreInitializer.cs | 28 |
| QuickFiler/Interfaces/IMailItemActions.cs | 35 |
| QuickFiler/Interfaces/MailItemActionsAdapter.cs | 49 |

## Production (edited controller partials)

| File | Lines |
|---|---:|
| QfcItemController.cs | 311 |
| QfcItemController.Conversation.cs | 221 |
| QfcItemController.EventHandlers.cs | 213 |
| QfcItemController.EventWiring.cs | 395 |
| QfcItemController.FocusAndTheme.cs | 351 |
| QfcItemController.Initialization.cs | 427 |
| QfcItemController.MailActions.cs | 224 |
| QfcItemController.Navigation.cs | 270 |
| QfcItemController.ViewerSetup.cs | 282 |

## Test (new + edited)

| File | Lines |
|---|---:|
| QfcItemController.SeamDispatcherTests.cs | 199 |
| QfcItemController.SeamCoreTests.cs | 226 |
| QfcItemController.SeamFactoryTests.cs | 284 |
| MailItemActionsAdapterTests.cs | 96 |
| WpfUiDispatcherTests.cs | 25 |
| WebView2CoreInitializerTests.cs | 25 |
| QfcItemController.TestSupport.cs | 332 |
| QfcItemController.ConversationTests.cs | 284 |
| QfcItemController.MailActionsTests.cs | 184 |

## EventWiring split note

`WireEvents` was split into `WireControlTreeEvents` (exempt) and `WireIntentEvents` (testable) in
place; `QfcItemController.EventWiring.cs` is 395 lines, so no spill partial
(`QfcItemController.EventWiring.ControlTree.cs`) was required.

Output Summary: Every measured production and test file is `< 500` lines. Largest production file is
`QfcItemController.Initialization.cs` at 427; largest test file is `SeamFactoryTests.cs` at 284.
