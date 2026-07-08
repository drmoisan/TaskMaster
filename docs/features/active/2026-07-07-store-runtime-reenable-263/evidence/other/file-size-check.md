# File-Size Ceiling Check (P6-T6)

Timestamp: 2026-07-08T01-27

Line counts for every new and modified production file in the scope lock (ceiling: 500 lines):

| File | Lines | <= 500 |
|---|---|---|
| UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs (new) | 100 | yes |
| TaskMaster/AppGlobals/StoreRehookCoordinator.cs (new) | 273 | yes |
| TaskMaster/AppGlobals/AppEvents.StoreRehook.cs (new) | 86 | yes |
| TaskMaster/AppGlobals/AppOlObjects.StoreRehook.cs (new) | 113 | yes |
| TaskMaster/AppGlobals/ApplicationGlobals.StoreRehook.cs (new; further partial split to keep ApplicationGlobals.cs under 500) | 104 | yes |
| UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs (modified) | 50 | yes |
| UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs (modified) | 113 | yes |
| UtilitiesCS/OutlookObjects/Folder/IOutlookFolderNotificationSink.cs (modified) | 43 | yes |
| UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs (modified) | 498 | yes |
| UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs (modified) | 441 | yes |
| TaskMaster/AppGlobals/AppEvents.cs (modified) | 477 | yes |
| TaskMaster/AppGlobals/AppOlObjects.cs (modified) | 463 | yes |
| TaskMaster/AppGlobals/ApplicationGlobals.cs (modified) | 475 | yes |

Notes:
- `IStoreRehookService.cs` is F1-owned and not touched.
- `IApplicationGlobals.cs` was not modified (no `StoreRehook` accessor added).
- `ApplicationGlobals.cs` was 471 lines pre-change; the DI construction change (line 118) plus the `partial` keyword left it at 475. The coordinator composition-root helpers were placed in the new `ApplicationGlobals.StoreRehook.cs` partial (104 lines) to keep both files under the ceiling, as authorized ("further partial split if needed").

Result: every listed file is <= 500 lines. PASS.
