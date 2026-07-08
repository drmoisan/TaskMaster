# `debug-startup-timing-instrumentation` — User Story

- Issue: #202
- Owner: drmoisan
- Status: Ready for Planning
- Last Updated: 2026-06-15T14-30

## Story Statement

- As the TaskMaster maintainer diagnosing the startup UI lock, I want an enable-on-demand timing breakdown of each startup sub-component surfaced as one readable table, so that I can identify which sub-component dominates startup wall-clock time without manually correlating scattered log entries.
- As a TaskMaster end user, I want the timing instrumentation to be off by default and to have no effect on normal runs, so that diagnostic tooling never changes my startup behavior or performance.

## Problem / Why

TaskMaster's Outlook add-in startup continues to lock up the UI for an extended period. Prior work (#139, #141) added targeted log4net `[Startup timing]` entries around the store-rewire COM path, but there is no enable-on-demand, whole-of-startup view that attributes wall-clock time to each startup sub-component. Without a per-sub-component breakdown surfaced in a single readable table, diagnosing which sub-component dominates startup remains a manual log-correlation task.


## Personas & Scenarios

- Persona: Maintainer / diagnostician (primary)
  - Who: The developer responsible for diagnosing TaskMaster's startup performance.
  - What they care about: Attributing startup wall-clock time to specific sub-components quickly and reliably.
  - Constraints: The add-in runs as a .NET Framework VSTO add-in on the Outlook main STA thread. The problematic scenario is a Release-built, installed add-in, so the diagnostic must be controllable without recompilation. Instrumentation must not introduce COM-thread affinity changes or async restructuring.
  - Goals and frustrations: Wants a single, aligned per-sub-component table. Frustrated that prior `[Startup timing]` entries (#139/#141) only cover the store-rewire COM path and require manual log correlation to compare sub-component contributions.
  - Context and motivations: The UI continues to lock during startup; the maintainer needs evidence of which sub-component dominates before deciding where to invest remediation effort. This feature is diagnostic instrumentation only and does not itself fix the UI lock.

- Persona: End user (secondary)
  - Who: A TaskMaster user running the add-in for normal email/task work.
  - What they care about: Stable, unchanged startup behavior.
  - Constraints / context: Does not run diagnostics and should not be exposed to instrumentation overhead or output.
  - Goals and frustrations: Wants assurance that diagnostic tooling stays inert unless explicitly enabled.

- Scenario: Capturing a startup timing breakdown
  - Who is acting: The maintainer/diagnostician.
  - What triggered the action: A reproduction of the startup UI lock that needs sub-component attribution.
  - Steps: (1) Sets `StartupTimingEnabled` to `True` in the user settings file. (2) Restarts Outlook so the setting takes effect. (3) Allows the add-in to complete sequential startup. (4) Opens the configured log4net output and locates the single `[Startup timing]` table entry.
  - Obstacles or decisions: The setting requires an Outlook restart to take effect; the maintainer must enable it before the run being measured. The table is emitted once, at the end of `LoadAsync`.
  - Expected outcome: A single bordered table listing LoadBasic, IntelConfig, OlObjects, ToDo, AutoFile, Engines, and Events with right-justified durations plus a TOTAL row, on the same log channel as prior timing entries.

- Scenario: Normal run with the flag off
  - Who is acting: An end user.
  - What triggered the action: Routine Outlook startup with the default settings.
  - Steps: Starts Outlook; the add-in loads normally.
  - Obstacles or decisions: None; the flag is off by default.
  - Expected outcome: No timing spans are recorded, no timing table is emitted, and startup behavior and performance are identical to the pre-feature baseline.


## Acceptance Criteria

- [x] A flag exists that enables or disables startup timing instrumentation; when disabled there is no behavioral or output change to startup.
- [x] When enabled, each startup sub-component's elapsed wall-clock time is captured during startup.
- [x] When enabled, a formatted plain-text table of sub-component names and elapsed times (plus a total row) is written to the output screen after startup completes.
- [x] The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with MSTest coverage meeting the repository floor for new code.
- [x] Instrumentation uses existing logging/output infrastructure and existing approved dependencies; it does not change functional startup behavior.


## Non-Goals

The following are explicitly excluded from this feature:

- **Fixing the startup UI lock.** This feature provides diagnostic measurement only; it does not change startup performance or remediate the lock.
- **Instrumenting the parallel startup path (`LoadAsync(parallel: true)` / `LoadParallelAsync`).** Only the sequential path (`LoadAsync(parallel: false)`), which is the path used at startup via `Application_Startup`, is in scope.
- **COM-thread affinity or async restructuring changes.** Instrumentation only wraps existing `await ...PhaseAsync()` seams with pre/post timestamps; no marshalling or async-flow changes are introduced.
- **Sub-phase granularity below the seven named phases.** The existing per-store/per-COM-call `[Startup timing]` instrumentation from #139/#141 is unchanged; this feature operates at the phase level above those points.
- **A settings UI for toggling the flag.** The flag is controlled through the user settings file; a settings-UI control is potential follow-up, not part of this feature.
- **Splitting output across channels.** Output goes to the single log4net channel only; no `Console`/`DebugTextWriter`/`OutputDebugString` emission is added.
