# Instrumentation Verdict Evidence

Timestamp: 2026-05-07T20:10:44.4930200-04:00
Dominant Stall Owner: AppEvents
Primary Startup Findings:
- `AppEvents.LoadAsync()` immediately calls `ProcessNewInboxItemsAsync()` after event hookup, so startup still performs inbox restriction, unprocessed-item enumeration, `MailItem` acquisition, engine applicability checks, and mail-item materialization inside the startup window.
- `ProcessNewInboxItemsAsync()` holds the Outlook-owned inbox pipeline in a long loop and uses `syncContext?.Post(... Application.DoEvents())` rather than an explicit snapshot-or-batch boundary, which is consistent with a remaining startup responsiveness stall.
Primary FirstSelection Findings:
- The first-selection path still begins with live selection reads in `EfcHomeController.LoadToList(...)` and `EfcDataModel` selection fallback, then flows into `ConversationResolver.LoadDfAsync(...)`, `ConvHelper.GetConversationDfAsync(...)`, `DfDeedle.GetEmailDataInViewAsync(...)`, and `OlTableExtensions.GetTableInViewAsync(...)` / `EtlAsync(...)`, where COM acquisition and row extraction are still mixed with `Task.Run`-wrapped asynchronous code.
- `MailItemHelper.FromMailItemAsync(...)` remains effectively synchronous for COM-backed field materialization, so the first-selection path still risks one contiguous Outlook-owned segment before any pure background transform begins.
Promoted Contingent File Count: 0
Promoted Contingent Files: none
Plan Revision Required: false
