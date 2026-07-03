# Final QA — 500-Line-Cap Audit (P8-T7, AC6)

Timestamp: 2026-07-02T10-45
Command: wc -l on every production and test file modified/created across Phases 5-7

## Production — QfcItemController partials (all 10, modified)

| File | Lines |
|---|---:|
| QfcItemController.cs | 311 |
| QfcItemController.Conversation.cs | 221 |
| QfcItemController.EventHandlers.cs | 226 |
| QfcItemController.EventWiring.cs | 401 |
| QfcItemController.FocusAndTheme.cs | 351 |
| QfcItemController.FolderHandling.cs | 208 |
| QfcItemController.Initialization.cs | 446 |
| QfcItemController.MailActions.cs | 224 |
| QfcItemController.Navigation.cs | 240 |
| QfcItemController.ViewerSetup.cs | 292 |

## Production — new seam files

| File | Lines |
|---|---:|
| UtilitiesCS/Threading/IUiDispatcher.cs | 36 |
| UtilitiesCS/Threading/WpfUiDispatcher.cs | 40 |
| QuickFiler/Viewers/IWebViewCoreInitializer.cs | 34 |
| QuickFiler/Viewers/WebView2CoreInitializer.cs | 28 |
| QuickFiler/Interfaces/IMailItemActions.cs | 35 |
| QuickFiler/Interfaces/MailItemActionsAdapter.cs | 49 |
| QuickFiler/Viewers/IItemViewer.cs (unchanged, no new members) | 121 |

## Test files (new + edited)

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
| QfcItemController.InitializationTests.cs | 193 |
| QfcItemController.EventHandlersTests.cs | 340 |

## Pre-existing debt (NOT modified this cycle)

- `QuickFiler/Controllers/QfcCollectionController.cs` = 2296 lines. It was **not modified or split** this
  cycle (per the plan's Scope Boundaries and Invariants); it is `<=` its baseline and is recorded here as
  pre-existing debt outside the cycle-2 scope (AC6 note).

Output Summary: Every production and test file modified/created across Phases 5-7 is `< 500` lines
(largest: `QfcItemController.Initialization.cs` at 446). No spill partial was required for the
`WireEvents` split (`EventWiring.cs` = 401). `QfcCollectionController.cs` is unchanged pre-existing debt.
AC6 satisfied.
