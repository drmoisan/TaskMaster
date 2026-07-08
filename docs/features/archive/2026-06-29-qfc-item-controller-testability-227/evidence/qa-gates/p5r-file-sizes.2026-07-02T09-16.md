# Phase 5 Gate — File Size Cap (P5-T11)

Timestamp: 2026-07-02T09-16
Command: wc -l on every QfcItemController production partial and every Phase-5 edited/created test file

## Production partials (QuickFiler/Controllers)

| File | Lines |
|---|---:|
| QfcItemController.cs | 294 |
| QfcItemController.Conversation.cs | 230 |
| QfcItemController.EventHandlers.cs | 201 |
| QfcItemController.EventWiring.cs | 369 |
| QfcItemController.FocusAndTheme.cs | 346 |
| QfcItemController.FolderHandling.cs | 200 |
| QfcItemController.Initialization.cs | 393 |
| QfcItemController.MailActions.cs | 229 |
| QfcItemController.Navigation.cs | 275 |
| QfcItemController.ViewerSetup.cs | 274 |

## Test files (QuickFiler.Test/Controllers)

| File | Lines |
|---|---:|
| QfcItemController.TestSupport.cs (new) | 286 |
| QfcItemController.InitializationTests.cs (new) | 193 |
| QfcItemController.ViewerSetupTests.cs (new) | 213 |
| QfcItemController.EventHandlersTests.cs (new) | 339 |
| QfcItemController.FocusAndThemeTests.cs (new) | 347 |
| QfcItemController.MailActionsTests.cs (edited) | 184 |
| QfcItemController.ConversationTests.cs (edited) | 279 |
| QfcItemController.EventWiringTests.cs (edited) | 194 |
| QfcItemController.NavigationTests.cs (edited) | 255 |

Output Summary: Every measured production and test file is `< 500` lines. Largest production file is
Initialization.cs at 393; largest test file is FocusAndThemeTests.cs at 347. Acceptance met.
