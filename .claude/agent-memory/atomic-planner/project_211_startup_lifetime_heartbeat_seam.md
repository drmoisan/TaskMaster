---
name: project-211-startup-lifetime-heartbeat-seam
description: "#211 Phase 3.3: full add-in-startup-lifetime heartbeat lives as a DispatcherTimer seam in ThisAddIn.cs (lifecycle-exempt), delegating to pure StartupDiagnosticsProbe logic"
metadata:
  type: project
---

Issue #211 Phase 3.3 adds a full add-in-startup-lifetime UI heartbeat distinct from the prior `LoadSequentialAsync`-scoped `[ui-heartbeat]`. New log tag: `[startup-lifetime-heartbeat]`.

**Why:** The maintainer confirmed the UI locks ~2 min on every cold start, but the prior heartbeat only covered the ~3 s `LoadSequentialAsync` window. The freeze extends from before `Application_Startup` completes through external Outlook provider churn (GLookSyncer/GmailSyncImpl/WrappedMSProvider::Logon ~108 s). Evidence: `evidence/other/runtime-capture-allphase-uiheartbeat-gc-2026-06-24T10-24.md`.

**How to apply:** The `DispatcherTimer` (250 ms, on `UiThread.Dispatcher`) construction/start/stop belongs in `TaskMaster/ThisAddIn.cs` — that class is already `[ExcludeFromCodeCoverage]` (VSTO lifecycle entry point, not unit-tested). It must start as the FIRST statement of `Application_Startup`. The PURE logic (lifetime-heartbeat line formatting, stage-label set, and the max-cap-OR-sustained-responsiveness stop-condition state machine) goes in the coverable `TaskMaster/AppGlobals/StartupDiagnosticsProbe.cs` (NOT exempt), covered by deterministic MSTest in `StartupDiagnosticsProbeTests.cs`. `UiThread.Dispatcher` is a static `Dispatcher` (UtilitiesCS/Threading/UiThread.cs). Stage labels: PreGlobalsCtor, GlobalsCtor, AwaitingIdleQueue, Loading, PostLoad (PostLoad flipped at the existing `Finished loading globals` log line). Stopwatch only; banned: DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Highest AC was AC14; this plan introduces AC15. Related: [[plan-validator-phase-heading-constraint]].
