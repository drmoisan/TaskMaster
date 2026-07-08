# Startup Wiring File-Size Check (P8-T3)

Timestamp: 2026-07-08T08-30

Line counts after the P8 wiring edits:
- TaskMaster/ThisAddIn.cs — 269 (<= 500)
- UtilitiesCS/Threading/UiThread.cs — 162 (<= 500)

Both files remain within the 500-line cap.

No new COM property reads on the UI thread were added by the wiring. The watchdog's attribution
path reads only the already-cached `CurrentStoreContext.Current` string and, on a confirmed lockup,
resolves a `StoreIdentity` via the pure `StoreIdentity.Resolve(displayName)` (no COM). The
`StoreLockupResponder` is constructed lazily from `_globals.StoreDisable` (an in-memory F1 service)
and a `WpfUiDispatcher`; neither introduces a blocking COM read on the STA.
