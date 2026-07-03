# Baseline — 500-Line-Cap Inventory (Cycle-2 Remediation)

Timestamp: 2026-07-01T21-37
Command: wc -l on the redesign blast-radius files

## Production — QfcItemController partials (10)

| File | Lines | < 500 |
|---|---:|:--:|
| QfcItemController.cs | 294 | yes |
| QfcItemController.Initialization.cs | 398 | yes |
| QfcItemController.ViewerSetup.cs | 278 | yes |
| QfcItemController.Conversation.cs | 233 | yes |
| QfcItemController.FolderHandling.cs | 200 | yes |
| QfcItemController.EventWiring.cs | 372 | yes |
| QfcItemController.EventHandlers.cs | 212 | yes |
| QfcItemController.Navigation.cs | 275 | yes |
| QfcItemController.FocusAndTheme.cs | 344 | yes |
| QfcItemController.MailActions.cs | 233 | yes |

## Production — Viewer surface

| File | Lines | < 500 |
|---|---:|:--:|
| IItemViewer.cs | 120 | yes |
| ItemViewer.cs | 436 | yes |
| ItemViewer.Commands.cs | 109 | yes |
| ItemViewer.DisplayState.cs | 81 | yes |
| ItemViewer.FolderSearch.cs | 60 | yes |
| ItemViewer.WebViewThread.cs | 37 | yes |

## Test — existing QfcItemController*Tests

| File | Lines | < 500 |
|---|---:|:--:|
| QfcItemControllerTests.cs | 377 | yes |
| QfcItemController.ConversationTests.cs | 171 | yes |
| QfcItemController.EventWiringTests.cs | 117 | yes |
| QfcItemController.FolderHandlingTests.cs | 192 | yes |
| QfcItemController.MailActionsTests.cs | 87 | yes |
| QfcItemController.NavigationTests.cs | 129 | yes |
| QfcItemController.PropertiesTests.cs | 168 | yes |

Output Summary: Every blast-radius file is `< 500` lines at baseline. Highest-risk headroom:
Initialization.cs (398, will gain constructor parameters in Phase 6), EventWiring.cs (372, WireEvents
split candidate in P6-T10 — spill partial planned if it approaches the cap), ItemViewer.cs (436,
Designer-adjacent, not modified by this cycle). Acceptance: every count recorded; all `< 500`.
