# outlook-startup-intelconfig-continuation-stall (Spec)

- **Issue:** #211
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-22T17-48
- **Status:** Draft
- **Version:** 0.1

## Context
After the #207 readiness-gate fix removed the Outlook hookup-path STA block, a distinct, pre-existing stall remains: the startup `[Startup timing]` table attributes ~60–115 s to the `IntelConfig` phase even though `IntelligenceConfig.ReadConfigurationAsync` itself runs in ~130 ms. The cost is the `Task.Run` continuation in `ApplicationGlobals.LoadSequentialAsync` being unable to resume on the STA for an extended period, leaving the Outlook UI locked. This issue is a detailed-diagnostics effort to localize exactly what occupies/blocks the STA during that window and to determine whether the add-in causes it.

Environment:
- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework (net48) Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread (thread 1)
- Command/flags used: Normal add-in startup; `[Startup timing]` and `[IntelConfig timing]` blocks emit on the console/Debug output path
- Data source or fixture: Live Outlook/Exchange profile; the Microsoft-published Teams Meeting Add-in is present and must remain installed (it is a professionally released add-in; the workaround must be on the TaskMaster side, not by removing Teams)

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Multi-minute startup unresponsiveness on affected profiles and a `ContextSwitchDeadlock` MDA risk, even after the #207 hookup-path fix.


## Repro & Evidence
Steps to Reproduce:
1. Launch Outlook with the TaskMaster add-in built from `main` (post-#207 merge) loaded.
2. Allow `ApplicationGlobals.LoadAsync(false)` to run the sequential startup phases.
3. Observe the `[Startup timing]` table: `IntelConfig` phase shows ~60–115 s wall-clock while the `[IntelConfig timing]` per-resource block shows the deserialize completed in ~130 ms, and the UI is unresponsive during the gap.

Expected:
Startup completes well within the 60 s COM-apartment threshold with the STA continuously pumping; no phase records a multi-minute wall-clock that is not attributable to its own work; the UI stays responsive.

Actual:
The `IntelConfig` phase wall-clock is dominated by a `Task.Run` continuation that cannot resume on the STA. In the 2026-06-22 post-fix capture: `[IntelConfig timing]` emitted at 16:41:55 (read 3.92 ms, deserialize ~130 ms), then the STA was unavailable ~16:41:57–16:43:57 (~120 s) before the next phase ran; the phase table recorded `IntelConfig 1:54.99`, `TOTAL 2:02.02`. During the gap the log shows the Teams add-in throwing many first-chance exceptions and TaskMaster's own assemblies (Swordfish, ToDoModel, the WPF stack, TaskVisualization) loading.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: see `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/diagnostics/ac12-postfix-capture-2026-06-22.md` and `startup-timing-increment3-2026-06-21.md` (the stall measured 60–115 s across multiple captures, independent of and predating the #207 fix).


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
The dominant cost is the STA being unavailable to resume the `LoadIntelConfigAsync` `Task.Run` continuation in `ApplicationGlobals.LoadSequentialAsync`, not TaskMaster deserialization (proven fast). Attribution is open and must be settled by instrumentation, per the maintainer rule "in scope iff this add-in causes it." Candidate occupants of the shared STA during the window:

- The Microsoft Teams Meeting Add-in's load (many first-chance exceptions in `TeamsMeetingAddinDomain`) making long synchronous calls on the shared Outlook STA. Teams cannot be removed (required, professionally released); any fix must be a TaskMaster-side workaround.
- TaskMaster's own JIT/loading of heavy assemblies (the WPF stack via `TaskVisualization`) on the startup continuation path.
- Visual Studio debugger overhead in the captured runs (`WpfTap`, symbol loading, per-exception first-chance handling) — must be ruled out with a non-debugger capture.

Files to inspect:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`LoadSequentialAsync`, `LoadIntelConfigAsync`, `YieldBetweenStartupPhasesAsync`, `StartupTimingRecorder` phase boundaries)
- `TaskMaster/ThisAddIn.cs` (`Application_Startup`, `IdleAsyncQueue` enqueue), `UtilitiesCS/Threading/IdleAsyncQueue.cs`, `ApplicationIdleTimer.cs`, `UiThread.cs`
- `TaskVisualization` assembly load / WPF initialization on the startup path


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [ ] Diagnostics first: instrument the inter-phase continuation in `LoadSequentialAsync` to record, at the moment the continuation finally resumes, what the STA was doing and how long each continuation waited (a continuation-latency probe distinct from the phase wall-clock), plus a non-debugger and a Teams-enabled-vs-disabled comparison to attribute the occupant.
- [ ] Unit coverage areas: any pure scheduling/continuation-affinity decision logic extracted for testability (no live COM).
- [ ] Integration scenario to retest: non-debugger cold start; capture whether the IntelConfig-phase wall-clock tracks a real STA occupant vs debugger overhead.
- [ ] Manual verification notes: confirm whether moving heavy assembly loads (WPF/TaskVisualization) off the critical startup continuation, and/or restructuring the phase awaits so non-COM continuations do not require the STA, reduces the stall while Teams remains installed.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
