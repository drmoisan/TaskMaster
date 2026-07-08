# debug-startup-timing-instrumentation — Spec

- **Issue:** #202
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-15T14-30
- **Status:** Ready for Planning
- **Version:** 1.0

## Overview

TaskMaster's Outlook add-in startup continues to lock up the UI for an extended period. Prior work (#139, #141) added targeted log4net `[Startup timing]` entries around the store-rewire COM path, but there is no enable-on-demand, whole-of-startup view that attributes wall-clock time to each startup sub-component. Without a per-sub-component breakdown surfaced in a single readable table, diagnosing which sub-component dominates startup remains a manual log-correlation task.


## Behavior

Add diagnostic timing instrumentation to the add-in startup path that:

- Is gated behind a `Settings.Default.StartupTimingEnabled` boolean user setting (default `false`). When the setting is off, startup behavior and output are unchanged.
- When the setting is on, measures the wall-clock time spent in each startup sub-component during `ApplicationGlobals` load along the sequential startup path (`LoadAsync(parallel: false)`). The instrumented sub-components are the seven established phase seams: LoadBasic (synchronous), IntelConfig, OlObjects, ToDo, AutoFile, Engines, and Events.
- Emits a single formatted plain-text table via the `ApplicationGlobals` log4net logger using `logger.Info(...)` with the `[Startup timing]` prefix, consistent with the prior #139/#141 timing entries. The table lists each sub-component, its elapsed time, and a TOTAL row.
- Does not alter functional startup behavior when the setting is off, and adds only measurement and formatting overhead when on.

The flag is read once in `ApplicationGlobals.LoadAsync` (following the existing `Settings.Default.EventsHooked` consumption pattern in `AppEvents.LoadAsync`), before the sequential/parallel branch. Phase recording occurs after each `await ...PhaseAsync()` in `LoadSequentialAsync`. The table is emitted at the end of `LoadAsync` so the full breakdown appears in one log statement.


## Inputs / Outputs

- **Inputs:**
  - `Settings.Default.StartupTimingEnabled` (boolean user setting, default `false`) — the single enable/disable control. Toggled by editing the user settings file (`%LOCALAPPDATA%\TaskMaster\...\user.config`) or via a future settings UI. Requires an Outlook restart to take effect.
  - Per-phase elapsed wall-clock spans captured around the existing `...PhaseAsync()` seams during sequential startup.
- **Outputs:**
  - A single multi-line `logger.Info(...)` statement on the `ApplicationGlobals` log4net logger, prefixed `[Startup timing]`, containing a bordered table with `Duration` (right-justified) and `Action` columns plus a TOTAL row. Output is routed to whatever log4net appenders are configured (file/debug), the same channel used by the existing `[Startup timing]` entries.
- **Config keys and defaults:**
  - `StartupTimingEnabled` = `False` (user scope) added to `TaskMaster/Properties/Settings.settings` and auto-generated into `Settings.Designer.cs`.
- **Versioning / backward-compatibility:**
  - No public API changes. The new setting defaults off; existing behavior is unchanged for all current users. No migration required (a missing setting resolves to its default `False`).

## API / CLI Surface

This feature has no CLI surface. The new internal API surface within the `TaskMaster` assembly is:

- `internal interface IStartupTimingRecorder` — recording and formatting contract:
  - `void RecordPhase(string phaseName, TimeSpan elapsed)` — records a named span.
  - `string FormatTable()` — returns the formatted plain-text table.
  - `void EmitTable(log4net.ILog logger)` — emits the table via the supplied logger with the `[Startup timing]` prefix.
- `internal sealed class StartupTimingRecorder : IStartupTimingRecorder` — production implementation that maintains its own ordered `(phaseName, elapsed)` collection. It reuses the `UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText(string[][], ...)` formatting primitive (the same overload `SegmentStopWatch.GetDurations()` uses) for column layout and computes a TOTAL row equal to the sum of recorded spans. It does not wrap `SegmentStopWatch` (whose TOTAL is derived from the watch's own `Elapsed`, which is zero for injected spans) and does not reimplement column alignment.
- `internal sealed class NullStartupTimingRecorder : IStartupTimingRecorder` — no-op default used on the flag-off path. `RecordPhase` returns immediately; `FormatTable` returns an empty string; `EmitTable` emits nothing.

These members are `internal`; `TaskMaster.Test` consumes them via the existing `InternalsVisibleTo("TaskMaster.Test")`.

- **Contracts and validation rules:**
  - `RecordPhase` accepts any non-null `phaseName`; phases are recorded in call order.
  - `FormatTable` is pure and deterministic given the recorded spans; empty input yields a table with only the TOTAL row (or an empty/zero result for the null recorder).
  - The recorder performs no Outlook/COM access and no filesystem or network I/O.

## Data & State

- **Data flow:** `LoadBasicMethod()` is measured with a `Stopwatch` and the elapsed value is stored in a private field; each sequential phase produces an elapsed `TimeSpan`. Both are passed to `IStartupTimingRecorder.RecordPhase` (LoadBasic first, then the six sequential phases when the flag is on). The recorder accumulates named spans in its own ordered collection. At the end of `LoadAsync`, `EmitTable` formats (via `PrettyPrinters.ToFormattedText`) and logs the accumulated spans with a summed TOTAL row.
- **State changes introduced:**
  - `ApplicationGlobals` gains one new private field, `IStartupTimingRecorder _timingRecorder`, assigned in `LoadAsync` based on the flag (concrete recorder when on, null/no-op recorder when off).
  - One new persisted user setting, `StartupTimingEnabled`, in the application settings.
- **Data transformations and invariants:** Spans are recorded once per phase, in startup order. The TOTAL row reflects aggregate elapsed time. No span data is persisted beyond the single log emission; the recorder holds spans only for the duration of one startup.
- **Caching or persistence details:** None beyond the user setting persistence already provided by the .NET settings infrastructure.
- **Migration or backfill requirements:** None. Absence of the setting resolves to the default `False`.

## Constraints & Risks

- The add-in runs as a .NET Framework VSTO add-in on the Outlook main STA thread; instrumentation must not introduce COM-thread affinity changes or additional async restructuring of the startup path.
- The flag mechanism must be simple to toggle and must default to off.
- New production code must meet the repository coverage floor (>= 90% for new modules/classes); the recorder and table formatter must be designed with a seam so they can be unit-tested without a live Outlook process.
- Measurement overhead must be negligible when the flag is off.


## Implementation Strategy

- **Implementation scope (what changes, not sequencing):**
  - Add the `StartupTimingEnabled` user setting to `TaskMaster/Properties/Settings.settings` (and the auto-generated `Settings.Designer.cs`), default `False`.
  - Add a flag read in `ApplicationGlobals.LoadAsync` that selects the concrete or no-op recorder. Add the `_timingRecorder` field and a `_loadBasicElapsed` field. Instrument `LoadBasicMethod()` itself with a `Stopwatch` (the construction-time `BasicLoaded` Lazy runs before `LoadAsync`, so measuring inside `LoadAsync` would record ~0); record `("LoadBasic", _loadBasicElapsed)` as the first phase when the flag is on. Add per-phase `RecordPhase` calls after each `await ...PhaseAsync()` in `LoadSequentialAsync`. Add the table emission at the end of `LoadAsync`. Add `using TaskMaster.Properties;` to `ApplicationGlobals.cs`.
  - Add the recorder interface and implementations as new files in `TaskMaster/AppGlobals/`.
  - Add MSTest coverage in `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` using the existing `TestableApplicationGlobals` seam.
- **New classes/functions to add or update:**
  - New: `TaskMaster/AppGlobals/IStartupTimingRecorder.cs`, `TaskMaster/AppGlobals/StartupTimingRecorder.cs` (own ordered collection + `PrettyPrinters.ToFormattedText` reuse + summed TOTAL), and `NullStartupTimingRecorder` (co-located or separate file).
  - Update: `TaskMaster/AppGlobals/ApplicationGlobals.cs` (flag check, field, recording calls, emission), `TaskMaster/Properties/Settings.settings`.
- **Dependency changes:** None. `UtilitiesCS.HelperClasses.PrettyPrinters.ToFormattedText`, `System.Diagnostics.Stopwatch`, and `log4net` are already present and approved. `TimeProvider` injection is not required because elapsed timing uses `Stopwatch` (hardware-counter based), which does not trigger the BannedApiAnalyzers `DateTime.Now`/`UtcNow` rule.
- **Logging/telemetry additions and locations:** One `logger.Info(...)` emission at the end of `ApplicationGlobals.LoadAsync` (flag-on path only), prefixed `[Startup timing]`, on the `ApplicationGlobals` log4net logger. The alternative `Console`/`DebugTextWriter` (`OutputDebugString`) path was rejected to keep all startup-timing data on the single log4net channel already used by #139/#141.
- **Rollout plan:** Controlled by the `StartupTimingEnabled` user setting, default off. No staged deploy needed; the feature is inert until a diagnostician enables the setting. Fallback is the existing behavior, which is the default state.

## Acceptance Criteria

- [x] A flag exists that enables or disables startup timing instrumentation; when disabled there is no behavioral or output change to startup.
- [x] When enabled, each startup sub-component's elapsed wall-clock time is captured during startup.
- [x] When enabled, a formatted plain-text table of sub-component names and elapsed times (plus a total row) is written to the output screen after startup completes.
- [x] The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with MSTest coverage meeting the repository floor for new code.
- [x] Instrumentation uses existing logging/output infrastructure and existing approved dependencies; it does not change functional startup behavior.

## Definition of Done

- [ ] Acceptance criteria documented and mapped to tests or demos
- [ ] Behavior matches acceptance criteria in all documented environments
- [ ] Tests updated/added (unit/integration as applicable)
- [ ] Edge cases and error handling covered by tests
- [ ] Docs updated (README, docs/features/active/... links)
- [ ] Telemetry/logging added or updated (if applicable)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] Recorder captures named spans and computes elapsed time correctly (positive, zero-duration, and ordering cases).
- [ ] Table formatter renders aligned plain-text columns deterministically for representative inputs and an empty input.
- [ ] Flag off => no spans recorded and no table emitted; flag on => spans recorded and table emitted.
- [ ] Wiring into the startup phase sequence preserves existing phase ordering and behavior.
