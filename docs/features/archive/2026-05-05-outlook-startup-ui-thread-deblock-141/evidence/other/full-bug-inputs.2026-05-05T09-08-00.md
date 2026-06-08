# Full-Bug Inputs Evidence

Timestamp: 2026-05-05T09:08:00-04:00
Work Mode: full-bug
Exact Plan Path: c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-05-05-outlook-startup-ui-thread-deblock-141\plan.2026-05-05T08-43.md
Requirements Sources:
- c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-05-05-outlook-startup-ui-thread-deblock-141\issue.md
- c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-05-05-outlook-startup-ui-thread-deblock-141\spec.md
Supporting Context:
- c:\Users\DanMoisan\repos\TaskMaster\artifacts\research\20260504-outlook-startup-ui-thread-deblock-research.md
- c:\Users\DanMoisan\repos\TaskMaster\artifacts\orchestration\orchestrator-state.json
Branch: bug/outlook-startup-ui-thread-deblock-141
SearchScope: docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/
SearchPatterns: user-story.md
SearchResult: none
Scope Guardrails:
1. Outlook COM access must remain on the main Outlook STA/UI thread, including `Application`, `NamespaceMAPI`, `Store`, `Folder`, `Items`, reminder collections, inbox event hookup, and any COM-backed `MailItem` materialization.
2. Only computation, parsing, deserialization of non-COM objects, classifier/model loading that does not dereference COM, and disk I/O may move to background work.
3. The primary production-file budget for the fix is `TaskMaster/ThisAddIn.cs` only if coordinator entry behavior must change, `TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, and `TaskMaster/AppGlobals/AppToDoObjects.cs`.
4. `TaskMaster/AppGlobals/AppEvents.cs`, `TaskMaster/AppGlobals/AppAutoFileObjects.cs`, and `TaskMaster/AppGlobals/AppItemEngines.cs` remain out of scope unless the inspection artifact in [P1-T2] records direct evidence that one of those files must change to preserve COM affinity or startup responsiveness for issue `#141`.
5. No new runtime configuration keys, no persisted-data/schema changes, and no redesign of unrelated startup subsystems are permitted.
