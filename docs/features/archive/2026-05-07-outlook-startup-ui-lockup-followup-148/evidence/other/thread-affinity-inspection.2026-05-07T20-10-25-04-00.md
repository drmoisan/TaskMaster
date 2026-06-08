# Thread-Affinity Inspection Evidence

Timestamp: 2026-05-07T20:10:25.3876574-04:00
Inspected Methods:
- AppEvents.LoadAsync()
- AppEvents.Hook()
- AppEvents.ProcessNewInboxItemsAsync()
- EfcHomeController.CreateAsync(...) / LoadToList(...) as the active selection entry path
- EfcDataModel..ctor(IApplicationGlobals, MailItem, CancellationTokenSource, CancellationToken)
- EfcDataModel.CreateAsync(...)
- ConversationResolver.LoadDfAsync(...)
- ConversationResolver.BackgroundInitInfoItemsAsync(...)
- DfDeedle.GetEmailDataInViewAsync(...)
- ConvHelper.GetConversationDfAsync(...)
- MailItemHelper.FromMailItemAsync(...)
- MailItemHelper.MaterializeTokenizationDependencies()
- OlTableExtensions.GetTableInViewAsync(...)
- OlTableExtensions.GetTableAsync(...)
- OlTableExtensions.EtlAsync(...)
Decision:
- Outlook-STA-only: `AppEvents.Hook()` touches `Globals.Ol.ToDoFolder.Items`, reminders, inbox items, and event hookup; `AppEvents.ProcessNewInboxItemsAsync()` performs inbox `Restrict`, item enumeration, and `MailItem` acquisition; `EfcHomeController.LoadToList(...)` reads `ActiveExplorer().Selection`; `EfcDataModel` constructor and `TryGetFirstInSelection()` read the current selection; `ConvHelper.GetConversationDfAsync(...)` still acquires the conversation and conversation table from a live `MailItem`; `MailItemHelper.FromMailItemAsync(...)` eagerly materializes COM-backed fields through `MaterializeTokenizationDependencies()`; `OlTableExtensions.GetTableInViewAsync(...)`, `GetTableAsync(...)`, and row extraction paths still acquire or enumerate live Outlook tables.
- Snapshot-based background transforms allowed after capture: Deedle/DataFrame construction in `DfDeedle.Email2dArrayToDf(...)`, resolver filtering/ranking once `ConversationResolver.Df` has been populated from immutable table data, `MailItemHelper.TokenizeAsync()` after COM-backed sender/body/recipient/attachment fields have been materialized, and ETL or projection work that consumes copied row arrays, column maps, and DTO-like mail snapshots instead of live COM objects.
- Mixed/needs refactor boundary: `EfcDataModel.CreateAsync(...)`, `ConversationResolver.LoadDfAsync(...)`, `DfDeedle.GetEmailDataInViewAsync(...)`, `ConvHelper.GetConversationDfAsync(...)`, and `OlTableExtensions.EtlAsync(...)` currently blur COM acquisition with asynchronous/background execution by calling `Task.Run` around COM-backed acquisition or by continuing to dereference live Outlook state after an `await`.
- Contingent promotion decision: no contingent-only file is required based on current inspection; the dominant unresolved startup and first-selection stalls remain attributable to primary-scope files, so contingent promotion is not justified before instrumentation.
