# Full-Bug Input Snapshot

Timestamp: 2026-05-07T20:07:54.6108866-04:00
Work Mode: full-bug
Exact Plan Path: c:\Users\DanMoisan\repos\TaskMaster-wt-2026-05-07-13-34\docs\features\active\2026-05-07-outlook-startup-ui-lockup-followup-148\plan.2026-05-07T19-34.md
Requirements Sources:
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-05-07-13-34\docs\features\active\2026-05-07-outlook-startup-ui-lockup-followup-148\issue.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-05-07-13-34\docs\features\active\2026-05-07-outlook-startup-ui-lockup-followup-148\spec.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-05-07-13-34\artifacts\research\20260507-outlook-startup-ui-lockup-followup-148-research.md
Branch: bug/outlook-startup-ui-lockup-followup-148
Feature Folder: docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148
SearchScope: docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/
SearchPatterns: user-story.md, docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/v1/issue.md
SearchResult:
- user-story.md under SearchScope: not found
- docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/v1/issue.md: not found
- Fallback supporting issue path: docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/issue.md
Scope Guardrails:
- Outlook COM access must remain on the Outlook STA/UI thread, including selection reads, explorer and folder access, conversation acquisition, `Conversation.GetTable()`, `MAPIFolder.GetTable()`, `Outlook.Table` row/value extraction, inbox `Restrict`, and COM-backed `MailItem` property materialization.
- Background work may consume only immutable snapshots, arrays, DTOs, column maps, tokenization inputs, or dataframe-ready structures that no longer dereference Outlook or UI state.
- The approved primary production scope is limited to `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, and `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs` remain contingent-only unless the instrumentation verdict artifact created in Phase 1 explicitly promotes exactly one of them.
- No new feature flag, configuration key, persisted-data schema change, or user-facing command/control is permitted in this fix.
- Coverage is a gating outcome for this bug fix. Baseline and final MSTest-with-coverage artifacts must record numeric values, and final manual Outlook validation remains blocked until the latest coverage summary records `Coverage Conclusion: PASS`.
