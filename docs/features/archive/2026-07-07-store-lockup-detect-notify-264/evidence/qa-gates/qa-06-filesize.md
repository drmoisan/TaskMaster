# QA Gate 06 — File-Size Cap (P9-T6)

Timestamp: 2026-07-08T08-40

Final line counts of all F4-touched and F4-new files (all <= 500):
- UtilitiesCS/OutlookObjects/Store/StoreLockupAttribution.cs — 37 (new)
- UtilitiesCS/Threading/LockupStallDecider.cs — 83 (new)
- UtilitiesCS/Threading/CurrentStoreContext.cs — 89 (new)
- UtilitiesCS/Dialogs/MyBoxModeless.cs — 127 (new)
- UtilitiesCS/Threading/StoreLockupResponder.cs — 132 (new)
- UtilitiesCS/Threading/UiThread.cs — 162 (modified)
- UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs — 209 (modified)
- UtilitiesCS/Threading/ThreadMonitor.cs — 240 (modified)
- TaskMaster/ThisAddIn.cs — 269 (modified)
- UtilitiesCS/Dialogs/MyBox.cs — 415 (unchanged; modeless path kept in a sibling file)
- UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs — 449 (modified)
- TaskMaster/AppGlobals/AppOlObjects.cs — 472 (modified; no partial extraction needed — see other/appolobjects-filesize.md)

No `AppOlObjects.StoreAttribution.cs` partial file was created (P7-T4 extraction not required).
Every listed file is <= 500 lines.
