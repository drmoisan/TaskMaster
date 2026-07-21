# File-Size Baseline of Touched Files (P0-T10)

Timestamp: 2026-07-08T07-54

Command: `wc -l <files>` on the execution base (HEAD 872eafb4).

Line counts (all <= 500 on the execution base):
- UtilitiesCS/Threading/ThreadMonitor.cs — 121
- UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs — 200
- UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs — 441
- TaskMaster/AppGlobals/AppOlObjects.cs — 463
- UtilitiesCS/Dialogs/MyBox.cs — 415
- TaskMaster/ThisAddIn.cs — 237
- UtilitiesCS/Threading/UiThread.cs — 137 (referenced by P8 wiring)

Files at or over 500 lines: NONE.

Note on AppOlObjects.cs: the plan (P0-T10, P7-T4) anticipated ~525 lines (pre-F2). On this
execution base F2/F3 already extracted partial files
(`AppOlObjects.JunkFolders.cs` 186, `AppOlObjects.StoreLoading.cs` 75,
`AppOlObjects.StoreRehook.cs` 113), so `AppOlObjects.cs` is 463 lines. The additive P7-T3
`CurrentStoreContext.Begin` wrap adds ~2-3 lines, so the file is expected to remain under 500
and the P7-T4 partial-class extraction is likely unnecessary (to be re-measured after P7-T3).
