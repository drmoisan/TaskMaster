# outlook-startup-intelconfig-deserialize-stall (Issue #207)

- Date captured: 2026-06-18
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-intelconfig-deserialize-stall/ (Issue #207)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #207
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/207
- Last Updated: 2026-06-18
- Work Mode: minor-audit

## Summary

The Outlook VSTO add-in triggers a `ContextSwitchDeadlock` Managed Debugging Assistant during startup. Runtime startup-timing instrumentation shows the `IntelConfig` phase (`IntelligenceConfig` deserialization) consumes ~112 s of a ~129 s startup, which alone exceeds the 60 s MDA threshold. The prior suspect — `AppEvents.ProcessNewInboxItemsAsync` (the Events phase) — accounts for only ~12 s and is not the dominant cost.

## Environment

- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread
- Command/flags used: Add-in startup with `Settings.Default.StartupTimingEnabled = true` to emit the `[Startup timing]` table
- Data source or fixture: Live `IntelligenceResources` embedded `.resx` classifier/people configuration set

## Steps to Reproduce

1. Enable `StartupTimingEnabled` and launch Outlook with the TaskMaster add-in loaded.
2. Allow `ApplicationGlobals.LoadAsync(false)` to run the sequential startup phases.
3. Observe the emitted `[Startup timing]` table and the debugger's `ContextSwitchDeadlock` MDA.

## Expected Behavior

Add-in startup completes well within the 60 s COM-apartment threshold and Outlook remains responsive; no `ContextSwitchDeadlock` MDA is raised.

## Actual Behavior

The `ContextSwitchDeadlock` MDA is raised (CLR unable to transition COM context for 60 s). Captured startup timing:

```
==========================
| Duration  Action       |
==========================
|  0:00.13  LoadBasic    |
|  1:52.31  IntelConfig  |
|  0:00.02  OlObjects    |
|  0:00.56  ToDo         |
|  0:00.36  AutoFile     |
|  0:03.66  Engines      |
|  0:12.24  Events       |
|  2:09.31  TOTAL        |
==========================
```

`IntelConfig` ≈ 112 s (dominant), `Events` ≈ 12 s, total ≈ 129 s.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `[Startup timing]` table above; MDA message: "The CLR has been unable to transition from COM context ... for 60 seconds."

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Startup unresponsiveness exceeding two minutes and a COM-apartment MDA on every affected profile.

## Suspected Cause / Notes

`IntelConfig` maps to `ApplicationGlobals.LoadIntelConfigAsync` → `IntelligenceConfig.LoadAsync` → `InitAsync` → `ReadConfigurationAsync` (`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:66-103`). That method enumerates every `IntelligenceResources` resource entry and, per entry, calls `DeserializeLoaderAsync` → `SmartSerializableLoader.DeserializeAsync`, including large `PeopleScoDictionaryNew` / `ScoDictionaryNew<,>`-derived dictionaries with custom JSON converters. The deserialization is wrapped in `Task.Run` (`ApplicationGlobals.cs:220-224`), so the cost is wall-clock of that phase; the precise STA-stall mechanism (synchronous block vs COM marshaling back to the STA) is not yet confirmed.

This corrects an earlier source-inspection handoff that attributed the MDA to `AppEvents.ProcessNewInboxItemsAsync`. Runtime evidence refutes that attribution. Note that batching/yielding mitigations already exist on HEAD from closed work items #139 (PR #158), #141, and #202 (PR #203).

Files to inspect:
- `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`
- `UtilitiesCS` `SmartSerializableLoader` deserialization path
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`LoadIntelConfigAsync`)

## Proposed Fix / Validation Ideas

- [ ] Diagnostic step first: add per-resource timing inside `ReadConfigurationAsync` (log each resource key, payload size, and `DeserializeLoaderAsync` elapsed ms) to pinpoint which configuration entry consumes the ~112 s.
- [ ] Unit coverage areas: deterministic test of the per-resource timing/breakdown seam on `IntelligenceConfig` (the class already exposes `protected internal virtual` seams).
- [ ] Integration scenario to retest: capture a second `[Startup timing]` table plus the per-resource breakdown after instrumentation.
- [ ] Manual verification notes: confirm whether the dominant cost is one outlier dictionary or broad-based, which determines the fix direction (lazy/deferred classifier load, off-critical-path load, faster serialization, or caching).

## Acceptance Criteria

This issue's first deliverable is diagnostic instrumentation to localize the ~112 s `IntelConfig` cost to specific configuration resource entries before any fix is scoped. The acceptance criteria below govern that instrumentation deliverable only; the corrective fix will be scoped in a follow-up after the per-resource breakdown is captured.

- AC1: `IntelligenceConfig.ReadConfigurationAsync` produces a per-resource timing breakdown that records, for each enumerated `IntelligenceResources` entry, the resource key, the serialized payload size, and the `DeserializeLoaderAsync` elapsed time measured with `System.Diagnostics.Stopwatch`.
- AC2: The breakdown is emitted via the existing `log4net` logger as a single consolidated, readable block (consistent in style with the existing `[Startup timing]` table) so it is captured on the same console/Debug output path during startup.
- AC3: Instrumentation is behavior-preserving: the returned `Config` dictionary contents and the deserialization semantics are unchanged relative to the pre-change implementation.
- AC4: A deterministic MSTest unit test (Moq + FluentAssertions) verifies the per-resource breakdown is produced for a known fixture set of resource entries, exercising the existing `protected internal virtual` seams (`GetSerializedConfigurations`, `DeserializeLoaderAsync`) with no live COM, no network/filesystem dependency, and no temporary files.
- AC5: No banned API is introduced; timing uses `Stopwatch` rather than `DateTime.Now`/`DateTime.UtcNow`.
- AC6: The full C# toolchain passes in order (CSharpier → .NET analyzers → nullable/`TreatWarningsAsErrors` → MSTest with coverage). New and changed lines meet the repository coverage policy and introduce no repository-wide coverage regression.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch