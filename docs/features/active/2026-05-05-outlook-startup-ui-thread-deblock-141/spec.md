# 2026-05-05-outlook-startup-ui-thread-deblock (Spec)

- **Issue:** #141
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-05T08-43
- **Status:** Draft
- **Version:** 0.1

## Context
Outlook add-in startup can block the main STA/UI thread for several seconds while startup coordination, store rewire, and other initialization work run without yielding often enough. The likely fix direction is to keep Outlook COM access on the UI thread while moving computation, configuration loading, and disk-backed initialization onto background threads so Outlook remains responsive during startup.

Environment:
- OS/version: Windows 10/11 with Outlook desktop and the TaskMaster VSTO add-in enabled
- Python version: Not applicable; this path is a .NET Framework Outlook add-in startup path
- Command/flags used: Standard Outlook launch into `ThisAddIn.Application_Startup()` with no special flags
- Data source or fixture: Live Outlook profile with multiple configured stores/providers and the normal TaskMaster persisted startup data/config files

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Start Outlook with the TaskMaster add-in enabled on a profile that has multiple Outlook stores.
2. Let startup reach `ThisAddIn.Application_Startup()`, which queues `_globals.LoadAsync(false)` through `IdleAsyncQueue.AddEntry(true, ...)`.
3. During startup, try to interact with Outlook while store rewire and related initialization phases are running.
4. Observe whether the Outlook window stops repainting or ignores input until the current startup phase finishes.

Expected:
Outlook should remain responsive during add-in startup even if total initialization takes longer. Background-safe work such as configuration loading, deserialization, and disk I/O should run off the UI thread, while Outlook COM access remains on the main STA thread and yields between heavy phases.

Actual:
Outlook can remain unresponsive for several seconds during startup while the add-in performs synchronous UI-thread coordination and COM-bound store rewire work. There is typically no explicit error dialog; the observable failure is a startup UI freeze, with prior timing evidence showing an 11+ second gap around store-related startup work before `Finished loading globals` is logged.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: Prior startup timing evidence and the current analysis are captured in `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md` and `artifacts/research/20260504-outlook-startup-ui-thread-deblock-research.md`, including the previously observed gap between store-related COM logging and `Finished loading globals`.


## Scope & Non-Goals
- In scope:
- Refactor the Outlook startup load path so `ThisAddIn.Application_Startup()` keeps the existing STA-thread entry point for Outlook COM work but separates background-safe computation, parsing, and disk I/O from UI-thread coordination.
- Define explicit phase boundaries for `ApplicationGlobals.LoadAsync(false)` so `_olObjects.LoadAsync()` and `_events.LoadAsync()` remain on the main Outlook STA/UI thread, while configuration loading, deserialization, classifier/model initialization, and other non-COM work are offloaded.
- Require cooperative yield points between heavy startup phases and between store-rewire iterations so Outlook can repaint and accept input while startup continues.
- Inspect and contain follow-up COM-affinity risk areas identified by research, especially `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()`, where background tasks currently receive `Parent.Ol.App` and may hide cross-thread COM access.
- Out of scope / non-goals:
- Moving Outlook COM access to `Task.Run`, a worker thread, or a dedicated secondary STA thread.
- Changing user-facing startup configuration, introducing new feature flags, or redesigning the add-in startup sequence beyond the targeted de-blocking and correctness fixes.
- Reworking unrelated startup subsystems unless they are needed to preserve COM affinity, await correctness, or responsiveness for this bug.
- Explicitly excluded systems, integrations, or datasets:
- Non-Outlook application paths, non-startup workflows, and unrelated UI surfaces outside the Outlook add-in startup/load path.
- Schema changes to persisted TaskMaster configuration or data files.
- Any requirement to author `user-story.md`; this spec is the full-bug artifact for issue `#141`.

## Root Cause Analysis
`Application_Startup()` currently routes `_globals.LoadAsync(false)` through `IdleAsyncQueue.AddEntry(true, ...)`, so the startup coordinator runs on the UI thread. Some sub-steps already use `Task.Run`, but their continuations resume on the dispatcher, and `_olObjects.LoadAsync()` / `StoresWrapper.RewireOlObjectsAsync()` still perform required Outlook COM work on the STA thread without enough cooperative yielding. Prior fixes for issues `#124`, `#126`, `#128`, and `#139` indicate the fix must preserve UI-thread COM access while splitting background-safe computation and disk I/O away from the UI-bound phases. Additional research notes two follow-up hazards to inspect before implementation: `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()` both pass the Outlook `Application` object into background work and may hide latent cross-thread COM access.

The research is sufficient to complete this spec. It identifies the blocking topology, the COM-only call sites, the already-safe background seams, and the principal unknowns that still require code inspection during implementation. The dominant blocking segment is store materialization and rewire: `AppOlObjects.LoadStoresAsync()` deserializes persisted store state and drives `StoresWrapper.RewireOlObjectsAsync()`, which synchronously accesses `NamespaceMAPI`, `Store` properties, `StoreWrapper.Init()`, SMTP address lookup, and folder restoration on the Outlook STA thread. That work is COM-correct but currently occupies the UI thread in long uninterrupted spans.

The coordinator problem is separate from the COM requirement. Because the full `LoadSequentialAsync()` chain is dispatched on the UI thread, each awaited background task resumes on the dispatcher before the next phase begins. This means the add-in yields too infrequently between COM-heavy and non-COM-heavy phases, and users observe Outlook as frozen even when part of the work is already background-capable. Research also identified a correctness risk in the current store-load path: `[OnDeserialized] RewireOlObjects` is `async void`, so the rewire completion is not reliably observable by callers.


## Proposed Fix

### Design summary (what changes where):
- Keep `IdleAsyncQueue.AddEntry(true, ...)` as the outer startup entry so the add-in continues to marshal startup coordination through the main Outlook STA/UI thread when Outlook becomes idle.
- Refactor `ApplicationGlobals.LoadAsync(false)` / `LoadSequentialAsync()` into explicit startup phases that separate UI-thread COM phases from background-safe phases. The coordinator may remain UI-thread-owned, but it must offload background-safe work with `Task.Run` and insert cooperative yield points such as `await Task.Yield()` between heavy phases.
- Preserve `_olObjects.LoadAsync()` and `_events.LoadAsync()` as UI-thread phases. These phases must continue to own all Outlook COM access, including `NamespaceMAPI`, store enumeration, store restoration, folder traversal, reminder hookup, inbox event hookup, and any MailItem materialization that depends on COM.
- Update the store rewire flow so it remains STA-thread-bound but yields between expensive units of work, especially between filtered-store enumeration completion and per-store restore/init iterations, and between successive store iterations.
- Replace any completion-obscuring `async void` store-rewire callback behavior with an awaitable flow so callers do not proceed as if store restoration has completed when it has not.
- Inspect the `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()` paths. If their background delegates dereference `Parent.Ol.App` or otherwise touch Outlook COM, split them into a pure background phase plus a UI-thread COM phase, or remove the background offload for the COM-dependent segment.

### Boundaries and invariants to preserve:
- Outlook COM access must remain on the main Outlook STA/UI thread. This includes, at minimum, `Application`, `NamespaceMAPI`, `Store`, `Folder`, `Items`, reminder collections, inbox event hookup, and any `MailItem` property access or materialization that depends on Outlook COM.
- Background work is limited to computation, parsing, deserialization of non-COM objects, classifier/model loading, configuration loading, and disk I/O.
- Prior COM-safety regressions fixed in issues `#124`, `#126`, and `#128` must remain fixed: no new `Task.Run` wrapper may directly execute Outlook COM calls or use COM-backed objects on worker threads.
- The startup sequence must remain functionally equivalent: globals still initialize, stores still restore, event hooks still attach, and prior persisted state remains readable without data migration.
- `Application_Startup()` must not regress into synchronous long-running work before the idle-queued startup path begins.

### Dependencies or blocked work:
- Implementation depends on targeted inspection of `IDList` and `ProjectData.Rebuild` to determine whether the current background paths dereference the passed Outlook `Application` object.
- If `Manager.InitAsync()` or classifier engine initializers consume `Globals.Ol` or other COM-backed state, those call sites must be split or marshaled explicitly before background execution.
- The store-load refactor depends on replacing or bypassing the current `[OnDeserialized] async void` rewire callback with an awaitable call path.

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:
- `TaskMaster/ThisAddIn.cs` — preserve the idle-queued startup entry point while routing startup through the phased coordinator.
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` — split `LoadSequentialAsync()` into explicit background-safe and UI-thread phases with cooperative yields between them.
- `TaskMaster/AppGlobals/AppOlObjects.cs` — make store load/rewire completion awaitable and keep COM restoration on the UI thread.
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — preserve COM-thread affinity, add per-phase/per-store yield boundaries, and remove completion ambiguity from rewire orchestration.
- `TaskMaster/AppGlobals/AppEvents.cs` — preserve STA-thread event hookup and inbox materialization rules while reducing long uninterrupted UI-thread occupancy during startup processing.
- `TaskMaster/AppGlobals/AppToDoObjects.cs` — inspect and, if needed, refactor `LoadIdListAsync()` and `LoadProjInfoAsync()` so background work does not carry live COM references.
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs` and `TaskMaster/AppGlobals/AppItemEngines.cs` — verify background-safe initialization boundaries and marshal any COM-dependent work back to the UI thread if required.

#### Functions/classes/CLI commands impacted:
- `ThisAddIn.Application_Startup()`
- `ApplicationGlobals.LoadAsync(bool)` and `LoadSequentialAsync()`
- `AppOlObjects.LoadAsync()` and `LoadStoresAsync()`
- `StoresWrapper.RewireOlObjectsAsync()` and related store restore/init helpers
- `AppEvents.LoadAsync()`, `Hook()`, and startup inbox-processing logic
- `AppToDoObjects.LoadIdListAsync()`, `LoadProjInfoAsync()`, and any downstream `IDList` or `ProjectData.Rebuild` call paths that dereference `Parent.Ol.App`

#### Data flow and validation changes:
- Startup data flow must become phase-based: background-safe config and persisted-state reads may execute off-thread, but COM-backed materialization must marshal back to the UI thread before touching Outlook objects.
- Persisted JSON/XML/config deserialization remains unchanged in format; the change is limited to when and on which thread each phase executes.
- Store restoration must not be considered complete until the rewire/restore task has fully finished on the STA thread.
- Any background task that currently accepts `Parent.Ol.App` or a COM-derived wrapper must either prove the value is never dereferenced off-thread or be refactored so only pure data crosses the background boundary.

#### Error handling and logging updates:
- Preserve existing `log4net` startup timing and debug logging added by issue `#139` so before/after comparisons remain possible.
- Add or retain phase-level timing logs around `_globals.LoadAsync(false)`, `_olObjects.LoadAsync()`, per-store rewire iterations, `_toDoObjects.LoadAsync(false)`, `_autoFileObjects.LoadAsync(false)`, and `_events.LoadAsync()`.
- If a suspected background COM-risk path is forced back to the UI thread, log enough context to identify the guarded phase without adding noisy per-item production logging beyond existing startup timing patterns.
- Exceptions from background-safe phases must continue to surface through the existing startup error path; refactoring must not swallow failures behind fire-and-forget tasks.

#### Rollback/feature-flag considerations (if applicable):
- No new runtime feature flag is required for this bug fix.
- Rollback is source-level: revert the phased coordinator and awaitability changes if startup correctness regresses.
- Because this change touches startup ordering, rollback validation must include both responsiveness and prior COM-affinity regression checks.

### Technical specifications (interfaces/contracts):
- Any new helper introduced for startup phasing must encode thread ownership in its contract: background helpers accept only pure data and return pure data; UI-thread helpers may accept or return COM-backed objects.
- Awaitable startup contracts must complete only after the work they represent is actually finished. `async void` is not acceptable for store-rewire completion signaling in this path.
- If a seam is introduced for testability around store sources or COM-dependent rebuild logic, the seam must be narrow, internal where possible, and must not weaken the current public behavior.

#### Inputs/outputs and formats:
- Input trigger: standard Outlook desktop startup into `ThisAddIn.Application_Startup()`.
- Input data: existing persisted TaskMaster configuration/state files, Outlook profile stores, and runtime settings already used by the add-in.
- Output behavior: Outlook remains responsive during startup, startup logs continue to record timing for major phases, and persisted state is restored with no schema changes.
- No new CLI commands, flags, or external file formats are introduced.

#### Required configuration keys and defaults:
- No new configuration keys or defaults are required.
- Existing persisted configuration and settings must remain backward compatible and load without migration.

#### Backward-compatibility expectations:
- No user-facing workflow, configuration schema, or startup entry point changes.
- Store restoration, event hookup, and startup data loading must remain behaviorally compatible with the fixes from issues `#124`, `#126`, `#128`, and the timing instrumentation from `#139`.
- Total startup duration may remain the same or increase slightly, but UI responsiveness must improve and COM correctness must not regress.

#### Performance constraints (latency/throughput/memory):
- Primary constraint: Outlook must continue repainting and accept user input during startup; responsiveness takes precedence over minimizing total startup wall-clock time.
- COM-heavy phases may still be slow per store, but they must be broken into smaller cooperatively yielded units where possible.
- Background offload must not materially increase memory pressure by duplicating large persisted datasets solely for threading convenience.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- The affected scenario is Outlook desktop on Windows 10/11 running the TaskMaster VSTO add-in with one or more configured stores, including slower Exchange-backed stores.
- Existing persisted startup files are valid and representative of normal production startup data.
- The research artifact and issue history are sufficient to define the bug-fix scope even though two follow-up COM-risk implementations still need direct code inspection.
- Constraints (budget, performance, compatibility):
- Outlook Object Model thread affinity is non-negotiable: COM access must remain on the main STA/UI thread.
- The spec is limited to an in-place bug fix and must not require broad architectural replacement of the add-in startup framework.
- Any change must remain compatible with the .NET Framework Outlook add-in host and the existing idle-queue / `UiThread` infrastructure.
- External dependencies (services, libraries, releases):
- Outlook desktop/VSTO host behavior and the existing repository threading helpers (`IdleAsyncQueue`, `UiThread`) are the relevant runtime dependencies.
- No new third-party libraries or external services are required by this fix.

## Data / API / Config Impact
- User-facing or API changes:
- No new user-facing commands, controls, or configuration knobs.
- User-visible change is limited to improved Outlook responsiveness during add-in startup.
- Data or migration considerations:
- No persisted data migration or schema update is planned.
- The same startup data files must remain readable; only execution timing and thread ownership change.
- Logging/telemetry updates (if any):
- Continue using the existing startup timing log pattern established by issue `#139`.
- Expand timing coverage only as needed to confirm phase boundaries, await completion, and any COM-risk fallback paths.
- Compatibility notes (CLI flags, config schemas, versioning):
- No CLI flags or versioned config changes.
- Compatibility requirement is behavioral: prior COM-safety fixes must remain intact.

## Test Strategy
Seeded from issue:

- [x] Validate a phased startup design where only COM-bound segments stay on the UI thread and background-safe config/file work is explicitly offloaded
- [ ] Retest Outlook startup with a multi-store profile and confirm the UI continues repainting and accepting input between startup phases
- [ ] Re-verify prior COM-safety regressions by confirming store access, event hookup, and mail-item materialization still occur on the STA/UI thread
- [ ] Capture before/after startup timing around `_globals.LoadAsync(false)`, `_olObjects.LoadAsync()`, and per-store rewire work to confirm responsiveness improves even if total startup duration increases

- Regression tests to add or update:
- Add or update C# unit tests that verify the phased startup coordinator preserves ordering and does not mark store restoration complete until the awaitable rewire path completes.
- Add or update tests around any extracted helper that separates background-safe work from UI-thread COM work, including negative tests that fail if a COM-dependent delegate is routed through a background-only helper.
- If a seam is introduced for store enumeration or startup phase orchestration, add tests that verify yield-aware batching does not skip stores or reorder phase completion.
- Unit tests (pytest) for the fixed behavior and boundaries:
- Not applicable. This repository path is C#/.NET Framework Outlook add-in code; unit coverage should be added with MSTest, Moq, and FluentAssertions where seams permit deterministic verification.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Multi-store profiles with slow secondary Exchange stores.
- Profiles with minimal persisted startup data or empty/first-run persisted lists.
- Suspected `AppToDoObjects` follow-up paths where `IDList` or `ProjectData.Rebuild` may dereference `Parent.Ol.App`; tests or code inspection must prove no worker-thread COM access occurs.
- Startup with previously restored stores/events to ensure no duplicate hook-up or partially initialized globals after yields.
- Error handling and logging verification:
- Verify startup timing logs still identify `_globals.LoadAsync(false)`, `_olObjects.LoadAsync()`, and per-store timing, and that any new awaited phases surface exceptions rather than silently failing.
- Verify store load completion logging does not appear before the actual rewire work finishes.
- Coverage impact and targets for changed lines/modules:
- Maintain or improve coverage on touched C# modules, with new or modified logic targeting repository policy expectations for changed lines.
- Prioritize deterministic unit coverage for any extracted pure helper or awaitability fix; document any COM-host-only behavior that remains manually validated.
- Toolchain commands to run (format → lint → type-check → test):
- `csharpier .`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (if required):
- Launch Outlook with the TaskMaster add-in enabled on a representative multi-store profile.
- During startup, repeatedly interact with the Outlook window and confirm repaint/input responsiveness between phases.
- Capture before/after startup timing logs and compare the store-rewire gap and overall responsiveness.
- Re-verify store access, inbox event hookup, reminder hookup, and MailItem materialization behaviors covered by issues `#124`, `#126`, and `#128`.


## Acceptance Criteria
- [ ] Outlook startup no longer presents the documented long unresponsive interval during the repro path; the Outlook window continues repainting and accepts input while TaskMaster startup phases continue.
- [ ] All Outlook COM access in the affected startup path remains on the main STA/UI thread, including store enumeration/rewire, folder restoration, event hookup, reminders access, and any MailItem materialization required by startup processing.
- [x] Background execution in the affected startup path is limited to computation, parsing, deserialization of non-COM objects, classifier/model initialization, and disk I/O.
- [x] `AppOlObjects.LoadStoresAsync()` and the store-rewire path complete via an awaitable contract; callers do not observe store restoration as complete before the rewire work has actually finished.
- [x] The implementation either proves `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()` are COM-safe on worker threads or refactors them so any COM-dependent segment is marshaled back to the UI thread.
- [ ] Regression tests are added or updated for the phased startup/order/awaitability behavior, and manual validation confirms no regression of the COM-safety fixes from issues `#124`, `#126`, and `#128`.
- [ ] Startup timing/logging remains sufficient to compare before/after behavior for `_globals.LoadAsync(false)`, `_olObjects.LoadAsync()`, and per-store rewire timing.
- [x] No configuration schema, persisted data format, or user-facing startup control changes are introduced outside the defined scope.

## Risks & Mitigations
- Technical or operational risks:
- A single synchronous Outlook COM call can still block the UI thread even after phase splitting; yields can only occur between discrete COM calls, not inside one call.
- `Task.Yield()` on the dispatcher may not provide enough practical responsiveness improvement in every Outlook host state; verification must be based on actual startup interaction and timing logs.
- The current store-deserialization path uses an `async void` callback model that may mask completion ordering bugs; refactoring it incorrectly could introduce partial initialization or race conditions.
- `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()` may currently hide cross-thread COM access through `Parent.Ol.App`, and similar risk may exist in `Manager.InitAsync()` or engine initializers if they dereference `Globals.Ol`.
- Startup event processing in `AppEvents.ProcessNewInboxItemsAsync()` may remain a secondary UI-occupancy hotspot after store rewire work is improved.
- Mitigations and rollbacks:
- Preserve the existing UI-thread boundary for all known COM call sites and add tests or explicit code inspection notes for each suspected COM-carrying background path.
- Retain and extend timing instrumentation so the team can determine whether responsiveness improved and where any remaining hotspots exist.
- Keep the refactor narrowly scoped to startup phase orchestration and await correctness; if regressions appear, revert the phased coordinator and re-run the prior startup timing validation.
- Validate on a representative multi-store Outlook profile before release and compare against the known issue `#139` timing evidence.

## Rollout & Follow-up
- Release/rollout steps:
- Merge the startup de-blocking fix behind the existing startup path with no new configuration switch.
- Validate on a local or staging Outlook profile that reproduces the multi-store startup delay before broader distribution.
- Capture final startup timing evidence as part of the fix review so later regressions can be compared to the pre-fix baseline.
- Post-fix monitoring or clean-up tasks:
- Review whether `AppEvents.ProcessNewInboxItemsAsync()` remains a user-visible startup responsiveness hotspot after the main fix.
- Confirm the final implementation result for `IDList`, `ProjectData.Rebuild`, `Manager.InitAsync()`, and classifier engine initialization with respect to COM-thread affinity, and open follow-up bugs if any remain unsafe or overly blocking.
- Remove only temporary diagnostic logging if it exceeds the long-term startup timing signal needed for regression detection; otherwise keep the targeted timing logs introduced by issue `#139`.
- Links: issue, PRs, related docs
- Issue: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/issue.md`
- Related research: `artifacts/research/20260504-outlook-startup-ui-thread-deblock-research.md`
- Related prior issue: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`
- Related prior fixes: issues `#124`, `#126`, `#128`, and `#139`
