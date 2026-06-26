# outlook-startup-intelconfig-continuation-stall (Issue #211)

- Date captured: 2026-06-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-intelconfig-continuation-stall/ (Issue #211)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #211
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/211
- Last Updated: 2026-06-22
- Work Mode: full-bug

## Summary

After the #207 readiness-gate fix removed the Outlook hookup-path STA block, a distinct, pre-existing stall remains: the startup `[Startup timing]` table attributes ~60–115 s to the `IntelConfig` phase even though `IntelligenceConfig.ReadConfigurationAsync` itself runs in ~130 ms. The cost is the `Task.Run` continuation in `ApplicationGlobals.LoadSequentialAsync` being unable to resume on the STA for an extended period, leaving the Outlook UI locked. This issue is a detailed-diagnostics effort to localize exactly what occupies/blocks the STA during that window and to determine whether the add-in causes it.

## Environment

- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework (net48) Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread (thread 1)
- Command/flags used: Normal add-in startup; `[Startup timing]` and `[IntelConfig timing]` blocks emit on the console/Debug output path
- Data source or fixture: Live Outlook/Exchange profile; the Microsoft-published Teams Meeting Add-in is present and must remain installed (it is a professionally released add-in; the workaround must be on the TaskMaster side, not by removing Teams)

## Steps to Reproduce

1. Launch Outlook with the TaskMaster add-in built from `main` (post-#207 merge) loaded.
2. Allow `ApplicationGlobals.LoadAsync(false)` to run the sequential startup phases.
3. Observe the `[Startup timing]` table: `IntelConfig` phase shows ~60–115 s wall-clock while the `[IntelConfig timing]` per-resource block shows the deserialize completed in ~130 ms, and the UI is unresponsive during the gap.

## Expected Behavior

Startup completes well within the 60 s COM-apartment threshold with the STA continuously pumping; no phase records a multi-minute wall-clock that is not attributable to its own work; the UI stays responsive.

## Actual Behavior

The `IntelConfig` phase wall-clock is dominated by a `Task.Run` continuation that cannot resume on the STA. In the 2026-06-22 post-fix capture: `[IntelConfig timing]` emitted at 16:41:55 (read 3.92 ms, deserialize ~130 ms), then the STA was unavailable ~16:41:57–16:43:57 (~120 s) before the next phase ran; the phase table recorded `IntelConfig 1:54.99`, `TOTAL 2:02.02`. During the gap the log shows the Teams add-in throwing many first-chance exceptions and TaskMaster's own assemblies (Swordfish, ToDoModel, the WPF stack, TaskVisualization) loading.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/diagnostics/ac12-postfix-capture-2026-06-22.md` and `startup-timing-increment3-2026-06-21.md` (the stall measured 60–115 s across multiple captures, independent of and predating the #207 fix).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Multi-minute startup unresponsiveness on affected profiles and a `ContextSwitchDeadlock` MDA risk, even after the #207 hookup-path fix.

## Suspected Cause / Notes

The dominant cost is the STA being unavailable to resume the `LoadIntelConfigAsync` `Task.Run` continuation in `ApplicationGlobals.LoadSequentialAsync`, not TaskMaster deserialization (proven fast). Attribution is open and must be settled by instrumentation, per the maintainer rule "in scope iff this add-in causes it." Candidate occupants of the shared STA during the window:

- The Microsoft Teams Meeting Add-in's load (many first-chance exceptions in `TeamsMeetingAddinDomain`) making long synchronous calls on the shared Outlook STA. Teams cannot be removed (required, professionally released); any fix must be a TaskMaster-side workaround.
- TaskMaster's own JIT/loading of heavy assemblies (the WPF stack via `TaskVisualization`) on the startup continuation path.
- Visual Studio debugger overhead in the captured runs (`WpfTap`, symbol loading, per-exception first-chance handling) — must be ruled out with a non-debugger capture.

Files to inspect:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`LoadSequentialAsync`, `LoadIntelConfigAsync`, `YieldBetweenStartupPhasesAsync`, `StartupTimingRecorder` phase boundaries)
- `TaskMaster/ThisAddIn.cs` (`Application_Startup`, `IdleAsyncQueue` enqueue), `UtilitiesCS/Threading/IdleAsyncQueue.cs`, `ApplicationIdleTimer.cs`, `UiThread.cs`
- `TaskVisualization` assembly load / WPF initialization on the startup path

## Proposed Fix / Validation Ideas

- [x] Diagnostics first: instrument the inter-phase continuation in `LoadSequentialAsync` to record, at the moment the continuation finally resumes, what the STA was doing and how long each continuation waited (a continuation-latency probe distinct from the phase wall-clock), plus a non-debugger and a Teams-enabled-vs-disabled comparison to attribute the occupant.
- [x] Unit coverage areas: any pure scheduling/continuation-affinity decision logic extracted for testability (no live COM).
- [x] Integration scenario to retest: non-debugger cold start; capture whether the IntelConfig-phase wall-clock tracks a real STA occupant vs debugger overhead.
- [x] Manual verification notes: confirm whether moving heavy assembly loads (WPF/TaskVisualization) off the critical startup continuation, and/or restructuring the phase awaits so non-COM continuations do not require the STA, reduces the stall while Teams remains installed.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch

## Status Update — Scope Expansion (2026-06-23)

Issue #211 remains **OPEN and unresolved**. The Phase 1 attribution instrumentation
succeeded as a diagnostic and disproved the narrow *IntelConfig continuation*
sub-hypothesis: in a non-debugger cold start the IntelConfig `Task.Run`
continuation resumes on the STA in ~0.6 ms, and the earlier 60–115 s IntelConfig
attribution was Visual Studio debugger overhead.

However, the multi-minute startup latency that motivated this issue is real and
persists. The same non-debugger capture relocates the dominant cost to the
`Engines` phase (`1:52.59` of a `1:58.79` total). The actual goal of #211 —
eliminate the startup latency — is unmet.

Scope is therefore expanded (see `spec.md` -> `## Scope Expansion (2026-06-23)`
and AC7–AC10): instrument `AppItemEngines.InitAsync` for per-engine attribution,
re-capture to localize the dominant engine/resource, then apply the minimal
TaskMaster-side fix. Work continues on branch `bug/outlook-startup-latency-211`.

The IntelConfig-phase Phase 1 work was relocated off `main` (which had been
advanced without a PR) onto this branch; `main` was reset to the pre-#211 state.