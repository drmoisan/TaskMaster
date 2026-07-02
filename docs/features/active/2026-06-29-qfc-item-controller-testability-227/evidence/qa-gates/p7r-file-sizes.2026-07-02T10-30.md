# Phase 7 Gate — File Sizes (P7-T6)

Timestamp: 2026-07-02T10-30
Command: wc -l on every QfcItemController partial edited in Phase 7 (per-member justification comments added; 4 dead overloads removed)

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

Output Summary: Every edited file is `< 500` lines. Largest is QfcItemController.Initialization.cs at 446.
Navigation.cs shrank to 240 after removing the four dead raw-parameter overloads (P7-T1).
