# Phase 1 — Partial-Split File Sizes (P1-T12)

Timestamp: 2026-06-29T11-00

Line counts (wc -l) after the 10-file partial split:
- QfcItemController.cs = 291 (main: usings, fields, properties, INotifyPropertyChanged; research estimate ~310)
- QfcItemController.Initialization.cs = 374
- QfcItemController.ViewerSetup.cs = 268
- QfcItemController.Conversation.cs = 226
- QfcItemController.FolderHandling.cs = 184
- QfcItemController.EventWiring.cs = 361
- QfcItemController.EventHandlers.cs = 195
- QfcItemController.Navigation.cs = 259
- QfcItemController.FocusAndTheme.cs = 326
- QfcItemController.MailActions.cs = 230

Output Summary: All ten files are < 500 lines (max = Initialization.cs at 374). Pure structural split: methods moved verbatim by research §1 cluster, class declared `partial`, SetTopicThread relocated from the INotifyPropertyChanged region into Conversation.cs. All nine new files wired into QuickFiler.csproj.
