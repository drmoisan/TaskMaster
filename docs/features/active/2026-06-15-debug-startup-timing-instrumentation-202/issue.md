# debug-startup-timing-instrumentation (Issue #202)

- Date captured: 2026-06-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/debug-startup-timing-instrumentation/ (Issue #202)

- Issue: #202
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/202
- Last Updated: 2026-06-15
- Work Mode: full-feature

## Problem / Why

TaskMaster's Outlook add-in startup continues to lock up the UI for an extended period. Prior work (#139, #141) added targeted log4net `[Startup timing]` entries around the store-rewire COM path, but there is no enable-on-demand, whole-of-startup view that attributes wall-clock time to each startup sub-component. Without a per-sub-component breakdown surfaced in a single readable table, diagnosing which sub-component dominates startup remains a manual log-correlation task.

## Proposed Behavior

Add debug-only timing instrumentation to the add-in startup path that:

- Is gated behind a flag that can be turned on/off so the instrumentation has no effect on normal (non-diagnostic) runs.
- Measures the wall-clock time spent in each startup sub-component during `ApplicationGlobals` load (the IntelConfig, OlObjects, ToDo, AutoFile, Engines, and Events phases at minimum), plus the total startup wall-clock time.
- Emits a formatted plain-text table to the output screen (the debug output window the add-in already writes to via `DebugTextWriter`/`Console`) listing each sub-component and its elapsed time.
- Does not alter functional startup behavior when the flag is off, and adds only measurement/formatting overhead when on.

## Acceptance Criteria (early draft)

- [x] A flag exists that enables or disables startup timing instrumentation; when disabled there is no behavioral or output change to startup.
- [x] When enabled, each startup sub-component's elapsed wall-clock time is captured during startup.
- [x] When enabled, a formatted plain-text table of sub-component names and elapsed times (plus a total row) is written to the output screen after startup completes.
- [x] The timing recorder/formatter is a testable unit (no Outlook/COM dependency) with MSTest coverage meeting the repository floor for new code.
- [x] Instrumentation uses existing logging/output infrastructure and existing approved dependencies; it does not change functional startup behavior.

## Constraints & Risks

- The add-in runs as a .NET Framework VSTO add-in on the Outlook main STA thread; instrumentation must not introduce COM-thread affinity changes or additional async restructuring of the startup path.
- The flag mechanism must be simple to toggle and must default to off.
- New production code must meet the repository coverage floor (>= 90% for new modules/classes); the recorder and table formatter must be designed with a seam so they can be unit-tested without a live Outlook process.
- Measurement overhead must be negligible when the flag is off.

## Test Conditions to Consider

- [ ] Recorder captures named spans and computes elapsed time correctly (positive, zero-duration, and ordering cases).
- [ ] Table formatter renders aligned plain-text columns deterministically for representative inputs and an empty input.
- [ ] Flag off => no spans recorded and no table emitted; flag on => spans recorded and table emitted.
- [ ] Wiring into the startup phase sequence preserves existing phase ordering and behavior.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/debug-startup-timing-instrumentation/` folder from the template