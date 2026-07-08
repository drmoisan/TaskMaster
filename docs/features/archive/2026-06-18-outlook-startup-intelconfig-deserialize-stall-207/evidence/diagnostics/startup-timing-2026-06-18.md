# Startup timing evidence — Issue #207

- Captured: 2026-06-18
- Source: `[Startup timing]` table emitted by `StartupTimingRecorder.EmitTable` with `Settings.Default.StartupTimingEnabled = true`
- Host: `outlook.exe`, TaskMaster VSTO add-in, STA `VSTA_Main` thread

## Captured table

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

## Interpretation

| Phase | Duration (s) | Note |
|---|---|---|
| LoadBasic | 0.13 | negligible |
| IntelConfig | 112.31 | dominant; alone exceeds the 60 s `ContextSwitchDeadlock` threshold |
| OlObjects | 0.02 | negligible |
| ToDo | 0.56 | negligible |
| AutoFile | 0.36 | negligible |
| Engines | 3.66 | minor |
| Events | 12.24 | real but not dominant; under the 60 s threshold |
| TOTAL | 129.31 | |

## Refutation of the prior handoff

The GitHub Copilot exception-analysis handoff attributed the `ContextSwitchDeadlock` MDA to `AppEvents.ProcessNewInboxItemsAsync` (the Events phase) with stated "high confidence," based on source inspection only (`callstack_status: "unavailable"`). Runtime evidence shows the Events phase accounts for ~12 of ~129 seconds. The dominant cost is the `IntelConfig` phase (`IntelligenceConfig` deserialization) at ~112 s.

Mitigations the handoff recommended for the Events path (batching of 10, inter-batch `Task.Yield()`, inter-phase yielding, timing instrumentation) already exist on HEAD as outputs of closed work items #139 (PR #158), #141, and #202 (PR #203).

## Next diagnostic step

Add per-resource timing inside `IntelligenceConfig.ReadConfigurationAsync` to localize the ~112 s to specific configuration resource entries (key, payload size, `DeserializeLoaderAsync` elapsed ms) before scoping the corrective fix. Governed by the Acceptance Criteria in `issue.md`.
