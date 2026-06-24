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
  - Phase 1 (immediate): a behavior-preserving continuation-latency attribution probe in `ApplicationGlobals.LoadSequentialAsync` that measures and logs how long each inter-phase continuation waits to resume on the STA, plus cheap STA-occupancy signals, to attribute the stall (TaskMaster vs external Teams/Outlook vs debugger overhead).
  - Phase 2 (evidence-gated): if the non-debugger capture confirms a large real continuation wait, move the IntelConfig post-`Task.Run` continuation off the STA (`ConfigureAwait(false)`) and explicitly re-marshal to the STA before the next COM-bound phase, so a momentarily-busy STA does not serialize startup.
- Out of scope / non-goals:
  - The Microsoft Teams Meeting Add-in (cannot be modified or removed; its shared-STA occupation is external — any remedy is TaskMaster-side scheduling).
  - Moving COM-bound phase bodies/continuations (OlObjects, ToDo, AutoFile, Engines, Events) off the STA.
  - Issues #208 (log4net) and #209 (Tesseract); the #207 hookup-path fix (already merged).
- Explicitly excluded systems, integrations, or datasets: none beyond the above.

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

Source: `artifacts/research/2026-06-22-intelconfig-continuation-stall-211-research.md`.

### Design summary (what changes where):
- **Phase 1 — attribution probe (this deliverable).** Replace `ApplicationGlobals.YieldBetweenStartupPhasesAsync()` with `YieldWithContinuationProbeAsync(string priorPhaseName)` (`protected internal virtual`). It does `Stopwatch.StartNew()` -> `await Task.Yield()` -> `sw.Stop()` -> emits one `[continuation-resume]` log line with: `priorPhase`, `waitMs` (F1, Stopwatch — the attribution number), `resumeThreadId` (vs `UiThread.UiThreadId`), `resumeSyncContext`, `staIsIdle` (`ApplicationIdleTimer.IsIdle`), `staCpuUsage` (`CurrentCPUUsage`), `staGuiActivity` (`CurrentGUIActivity`). The five inter-phase call sites pass their preceding phase name. Stopwatch only; no banned APIs. Behavior is preserved (still a single `Task.Yield` back to the Dispatcher).
- **Phase 2 — off-STA IntelConfig continuation (evidence-gated).** Only if a non-debugger capture shows the IntelConfig continuation `waitMs` is materially large (> 5000 ms): add `.ConfigureAwait(false)` to the `Task.Run` await in `LoadIntelConfigAsync`, and in `LoadSequentialAsync` insert `await UiThread.UiSyncContext;` (existing `SynchronizationContextAwaiter`, UiThread.cs:82-85) after the IntelConfig phase and before `LoadOlObjectsPhaseAsync`, so the IntelConfig continuation completes on the thread pool and re-marshals to the STA only when OlObjects needs it. The intervening `StopAndRestart`/`RecordPhase` calls are pure and thread-safe.

### Boundaries and invariants to preserve:
- Phase order and outcomes unchanged; all COM-bound phase bodies run on the STA; `OlObjects` and later phases resume on the STA (verified by the re-marshal in Phase 2).
- No banned APIs (`DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`); `Stopwatch` for timing. net48 (no positional `record struct`). All touched files <= 500 lines.

### Dependencies or blocked work:
- Phase 2 is blocked on the maintainer's non-debugger (and, if needed, Teams-disabled) runtime capture from the Phase 1 instrumentation; if that capture shows the stall is debugger-only/not reproduced, Phase 2 is not implemented and the issue closes with that finding.

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

- [x] Diagnostics first: instrument the inter-phase continuation in `LoadSequentialAsync` to record, at the moment the continuation finally resumes, what the STA was doing and how long each continuation waited (a continuation-latency probe distinct from the phase wall-clock), plus a non-debugger and a Teams-enabled-vs-disabled comparison to attribute the occupant. — Probe delivered (commit 72520363); debugger-attached and non-debugger captures recorded under `evidence/other/`.
- [x] Unit coverage areas: any pure scheduling/continuation-affinity decision logic extracted for testability (no live COM). — Deterministic MSTest covers the `protected internal virtual` probe seam; no further scheduling logic extracted because Phase 2 is not warranted.
- [x] Integration scenario to retest: non-debugger cold start; capture whether the IntelConfig-phase wall-clock tracks a real STA occupant vs debugger overhead. — Non-debugger capture shows IntelConfig continuation `waitMs=0.6`; the multi-minute IntelConfig stall does not reproduce outside the debugger.
- [x] Manual verification notes: confirm whether moving heavy assembly loads (WPF/TaskVisualization) off the critical startup continuation, and/or restructuring the phase awaits so non-COM continuations do not require the STA, reduces the stall while Teams remains installed. — Not warranted for IntelConfig: the continuation already resumes on the STA in ~0.6 ms. The residual cost is in the `Engines` phase and is tracked as a separate follow-up.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria

Phase 1 (attribution instrumentation) is the immediate deliverable; Phase 2 is evidence-gated.

- [x] AC1: `LoadSequentialAsync` emits one `[continuation-resume]` log line per inter-phase boundary via the existing `log4net` logger, each with `priorPhase`, `waitMs` (Stopwatch, F1), `resumeThreadId`, `resumeSyncContext`, `staIsIdle`, `staCpuUsage`, `staGuiActivity`. — Verified: implementation commit 72520363; the non-debugger capture (`evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md`) shows all five boundaries (IntelConfig, OlObjects, ToDo, AutoFile, Engines) emitting the full field set.
- [x] AC2: behavior-preserving — the probe replaces the existing `Task.Yield()` inter-phase yields without changing phase order, count, or outcomes; `Stopwatch` only; no banned API introduced; net48 (no positional `record struct`). — Verified: final-QC (`evidence/qa-gates/final-qc-2026-06-22T18-05.md`) and feature-review PASS (`code-review.2026-06-22T22-45.md`).
- [x] AC3: a deterministic MSTest (Moq + FluentAssertions) using a `TestApplicationGlobals` subclass overriding the `protected internal virtual` probe verifies it is invoked once per phase boundary in the correct order with the correct phase names; no live COM, no live timer, no network/filesystem, no temporary files. — Verified: feature-review PASS confirms the deterministic test.
- [x] AC4: full C# toolchain passes in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage, gated `/TestCaseFilter:"TestCategory!=LiveOutlook"`); the new testable seam meets the coverage policy; no repository-wide regression; all touched files <= 500 lines. — Verified: `evidence/qa-gates/final-qc-2026-06-22T18-05.md`. Note: the `UtilitiesCS TimedAsyncTask_Tests.RequestTask_WithProvidedTask_InvokesTaskAfterInterval` test is a recorded pre-existing real-interval timer flake, not a regression from this change.
- [x] AC5 (runtime, maintainer): a non-debugger cold-start capture (DebugView / OutputDebugString) produces the `[continuation-resume]` fields; this is the gating evidence for Phase 2 and is recorded under `evidence/`. (Not CI-automatable.) — Verified: `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md` (maintainer-provided non-debugger capture).
- [x] AC6 (Phase 2, evidence-gated): IF the non-debugger capture shows the IntelConfig continuation `waitMs` > 5000 ms with the STA externally occupied, apply the off-STA IntelConfig continuation (`ConfigureAwait(false)` + `await UiThread.UiSyncContext` before `OlObjects`), with a unit test asserting phase ordering is preserved and the `OlObjects` phase resumes on the STA, and a re-capture confirming the reduction. IF the capture shows the stall is debugger-only / not reproduced outside the debugger, Phase 2 is not implemented and the issue closes documenting that finding. — Resolved via the second branch: the non-debugger capture shows IntelConfig continuation `waitMs=0.6` (far below the 5000 ms threshold), with `resumeThreadId=1` (STA) and `staIsIdle=True`. The originally-reported 60–115 s IntelConfig stall does not reproduce outside the Visual Studio debugger. Under the maintainer rule "in scope iff this add-in causes it," no TaskMaster-side Phase 2 fix for IntelConfig is warranted. Phase 2 is intentionally not implemented; finding documented in `evidence/other/runtime-capture-nondebugger-2026-06-23T13-51.md`.

### Acceptance Criteria Status Summary (2026-06-23)

| AC | Status | Evidence |
| --- | --- | --- |
| AC1 | PASS | impl 72520363; non-debugger capture probe lines |
| AC2 | PASS | final-QC; code-review PASS |
| AC3 | PASS | feature-review PASS (deterministic MSTest) |
| AC4 | PASS | final-QC (pre-existing timer flake noted, not a regression) |
| AC5 | PASS | non-debugger capture 2026-06-23T13-51 |
| AC6 | PASS (no-fix branch) | waitMs=0.6 < 5000 ms threshold; stall debugger-only; Phase 2 not warranted |

Phase-1/IntelConfig sub-result: AC1–AC6 verify the Phase 1 attribution instrumentation and disprove the *IntelConfig continuation* sub-hypothesis (the IntelConfig `Task.Run` continuation resumes on the STA in ~0.6 ms in a non-debugger cold start; the earlier multi-minute IntelConfig attribution was debugger-induced).

**Issue #211 is NOT resolved.** The non-debugger capture shows the startup latency is real and now attributed to the `Engines` phase (`1:52.59` of a `1:58.79` total). The original issue title named one suspected sub-cause (IntelConfig); the actual goal of #211 is to eliminate the multi-minute startup latency. That goal is unmet. Scope is expanded below to localize and fix the `Engines`-phase cost. See `## Scope Expansion (2026-06-23)` and AC7–AC10.

## Scope Expansion (2026-06-23)

### Why
The Phase 1 instrumentation succeeded as a diagnostic but disproved only the narrow IntelConfig-continuation sub-hypothesis. The same non-debugger capture relocated essentially all of the startup latency to the `Engines` phase (`InitializeEnginesPhaseAsync` -> `Task.Run(() => Engines.InitAsync())` in `ApplicationGlobals.LoadSequentialAsync`). Closing #211 on the IntelConfig sub-result would misrepresent an unsolved multi-minute startup regression as resolved. Per maintainer direction, #211 remains open with expanded scope covering the full startup latency, with `Engines` as the now-primary suspect.

### Restated objective
Eliminate the multi-minute Outlook startup latency. The current evidence localizes the dominant cost to the `Engines` phase; confirm and attribute that cost with targeted instrumentation, then apply the minimal TaskMaster-side fix (subject to the same "in scope iff this add-in causes it" rule).

### Newly in-scope (Phase 3 — Engines attribution)
- Per-engine attribution instrumentation inside `AppItemEngines.InitAsync` (and the per-engine `CreateEngineAsync` paths: `SpamBayes`, `Triage`, `CategoryClassifierGroup` project/context, `ActionableClassifierGroup`) to record per-engine construction/model-load wall-clock, the thread/apartment each runs on, and whether the cost is CPU, I/O (model deserialization), or STA-marshaling.
- A non-debugger re-capture that attributes the `Engines` phase cost to specific engines/resources.
- The `AF.Manager.Configuration` await at the top of `InitAsync` (a candidate large-deserialize / lazy-config dependency shared with the AutoFile path).

### Out of scope / non-goals (unchanged + clarified)
- The Microsoft Teams Meeting Add-in (external; any remedy is TaskMaster-side scheduling).
- Moving genuinely COM-bound work off the STA where correctness depends on the STA.
- A speculative fix before Phase 3 attribution identifies the dominant engine/resource cost. Phase 4 (fix) is evidence-gated on Phase 3.

### Phase 3 / Phase 4 acceptance criteria

- [x] AC7: `AppItemEngines.InitAsync` emits per-engine attribution instrumentation (one structured log line per engine init) capturing engine name, wall-clock duration (`Stopwatch`, F1 ms), the resolving thread id / apartment, and a coarse cost classification signal, using the existing `log4net` logger. Behavior-preserving; `Stopwatch` only; no banned API (`DateTime.Now`/`UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`); net48; all touched files <= 500 lines.
- [x] AC8: a deterministic MSTest (MSTest + Moq + FluentAssertions) covering any extracted pure attribution/aggregation logic and the per-engine emission seam, with no live COM, no live timer, no network/filesystem, no temporary files; the new seam meets the coverage policy and there is no repository-wide coverage regression.
- [~] AC9 (runtime, maintainer): a non-debugger cold-start capture produces the per-engine attribution lines and identifies which engine(s)/resource(s) dominate the `Engines`-phase wall-clock; recorded under `evidence/`. — SUPERSEDED/REOPENED. The T17-42 cold capture appeared to attribute the latency to SpamBayes (~67.5 s), but the later Phase 3.1 capture `evidence/other/runtime-capture-uiheartbeat-gc-2026-06-23T21-55.md` overturned that: in that run Spam was 2.5 s and the latency moved to IntelConfig (60 s) and ToDo (55.7 s). The cost is NOT phase-specific or SpamBayes-specific; it is a cross-cutting, intermittent STA stall (~2 min total) correlated with external Outlook MAPI/Gmail-sync/address-book provider churn. GC is disproven (Phase 3.1 `[gc-delta]`: 1 Gen0, 0 Gen2). Attribution of the slow phase is reopened pending an all-phase UI-heartbeat capture.
- [ ] AC10 (Phase 4, evidence-gated): apply the minimal TaskMaster-side fix indicated by AC9, with a unit test asserting the behavior/ordering invariant the fix relies on and a re-capture confirming the startup-latency reduction. — STILL GATED; fix target RETRACTED. The SpamBayes target is withdrawn (run-specific artifact). The current evidence points to a cross-cutting STA stall during startup correlated with external Outlook provider churn (the slow phase varies: IntelConfig, ToDo, or Engines). The fix cannot be chosen until the all-phase UI-heartbeat capture establishes whether the STA is actually frozen during the slow phase (vs an async continuation merely waiting) and which COM/Outlook-object-model call blocks. `PreserveReferencesHandling.All` is retained regardless.

### Phase 3.4 acceptance criterion (store-filter attribution probe)

- [x] AC16: Per-store filter attribution probe (diagnosis-only). For each store enumerated by `StoresWrapper.GetFilteredStores()`/`ShouldIncludeStore`, exactly one structured `[store-filter]` line is emitted via the existing `log4net` logger capturing store DisplayName (guarded), `Stopwatch` ms to read `ExchangeStoreType`, `Stopwatch` ms to read `FilePath`, the include/exclude boolean, and the matched rule (`PublicFolder`/`NameContains`/`GwsoFilePath`/`FilePathContains`/`Included`). On the synchronous `Init()` path a `GetFilteredStores`-level `[store-filter]` summary (count + total ms) is emitted; the async `RewireOlObjectsAsync` path is unchanged. Behavior-preserving (identical filter result, included set, order, predicate short-circuit semantics); `Stopwatch` only; no banned APIs; net48; all touched files <= 500 lines; `PreserveReferencesHandling.All` and existing instrumentation tags untouched. Pure decision + line formatter live in the coverable `StoreFilterAttribution` helper and are covered by deterministic MSTest (no live COM/timer/filesystem/network; no temporary files). — AUTOMATED PORTION VERIFIED: implementation in `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs` (new, 100% new-code coverage) and `StoresWrapper.cs`; 13 deterministic tests pass; full C# toolchain passes in order (CSharpier -> analyzers -> nullable/TWAE -> MSTest with coverage); no repo-wide regression; all touched files <= 500 lines. Evidence under `evidence/qa-gates/*-2026-06-24T14-30.md` and `evidence/baseline/*-2026-06-24T14-30.md`.
- [~] AC16 (runtime, maintainer): a non-debugger cold-start capture records the `[store-filter]` lines during a slow startup so the largest `FilePath`/`ExchangeStoreType` read time and the Gmail store's include/exclude decision are identifiable. — MAINTAINER-GATED (runtime, not CI-automatable). Capture instructions: `evidence/other/coldstart-store-filter-capture-instructions-2026-06-24T14-30.md`; placeholder: `evidence/other/runtime-capture-store-filter-PLACEHOLDER.md`. Pending maintainer capture.

> Separate finding (new scope, not the latency): the Spam/Triage/Actionable inbox custom-column fields remain empty for new emails (`ProcessNewInboxItemsAsync` elapsedMs=0). The classification path `AppEvents.OlInboxItems_ItemAdd -> ProcessMailItemAsync` is independent of engine construction and was not modified by Phase 3. Tracked as a separate concern; see the cold-start capture file.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
