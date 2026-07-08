Timestamp: 2026-06-23T17-33
Command: Maintainer-provided non-debugger Outlook startup DebugView capture (AC9), built from branch bug/outlook-startup-latency-211 with the Phase 3 per-engine attribution probe.
EXIT_CODE: 0

# Runtime Capture: Non-Debugger Outlook Startup (AC9, Engines per-engine attribution)

## Summary

This capture satisfies the AC9 runtime-evidence requirement: the Phase 3
per-engine instrumentation emits the `[engine-init-config]` and `[engine-init]`
attribution lines in a non-debugger startup. It also produces a result that
materially refines the diagnosis.

**In this run the multi-minute startup stall did NOT reproduce.** Total startup
was `0:02.59`, with the `Engines` phase at `0:01.42`.

## Per-engine attribution (new Phase 3 probe)

| Item | ms | thread | costHint |
| --- | ---: | ---: | --- |
| Configuration | 0.0 | 6 | (config) |
| Actionable | 33.9 | 5 | Deserialization |
| Spam | 1328.7 | 7 | Deserialization |
| Triage | 37.4 | 6 | Deserialization |

Sum of engine init ≈ 1400 ms ≈ the recorded `Engines` phase (`1.42 s`), so in
this run the entire `Engines`-phase cost is accounted for by engine
deserialization, with the **Spam classifier model the dominant single cost
(~1.33 s) even on this (apparently warm) start**. Engines run on thread-pool
threads (5/6/7) under `Task.Run(Engines.InitAsync)`; the post-Engines
continuation resumes on the STA in `waitMs=0.0`.

## Startup timing table

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.12 |
| IntelConfig | 0:00.34 |
| OlObjects | 0:00.00 |
| ToDo | 0:00.41 |
| AutoFile | 0:00.27 |
| Engines | 0:01.42 |
| Events | 0:00.00 |
| TOTAL | 0:02.59 |

## Comparison with the prior non-debugger capture (2026-06-23T13-51)

| | T13-51 | T17-33 (this) |
| --- | ---: | ---: |
| Engines phase | 1:52.59 | 0:01.42 |
| TOTAL | 1:58.79 | 0:02.59 |
| Per-engine probe present | no | yes |

The two non-debugger captures disagree by ~110 s on the `Engines` phase. The
multi-minute latency is therefore **intermittent / condition-dependent**, not a
deterministic per-startup cost. The T13-51 run placed ~112 s inside the `Engines`
window without per-engine data; this run shows the engines themselves complete in
~1.4 s.

## External (non-TaskMaster) signals in this capture

The log shows a high volume of Outlook provider assertion failures unrelated to
TaskMaster code:
- Address-book provider: `StandardizeEntryId` / `CompareEntryIds` /
  `ABLogon::CompareEntryIDs` (`0x80040107`); `ABContainer::OpenEntry` /
  `PrepareRecipient` / `ABLogon::PrepareRecips` (`0x80040201`).
- Google/Gmail sync stack: `EmailAliases::*`, `GmailSyncImpl::Init`,
  `GLookSyncer::TryCreateInstance`, `WrappedMsgStore::*`,
  `WrappedMSProvider::Logon` (`0x80040401`).
- Teams Meeting Add-in load and WebView2/OCDI activity.

These are Outlook MAPI / Google-sync / address-book provider components, external
to TaskMaster. They are candidates for the intermittent shared-resource
contention that could inflate the `Engines` window on a slow start, but this
capture does not prove that.

## Interpretation

1. AC9 instrumentation is validated: the per-engine attribution lines are emitted
   in a non-debugger start and correctly attribute the `Engines` phase.
2. The dominant TaskMaster engine cost is the **Spam classifier deserialization**
   (~1.33 s here; the research predicts this is a large `PreserveReferencesHandling.All`
   JSON model that would be substantially larger on a cold-disk first load).
3. The multi-minute stall did NOT reproduce in this run, so the slow-path root
   cause is **not yet attributed**. We now have the instrumentation to attribute
   it: a capture taken during a slow/cold start will show either (a) a specific
   engine with a multi-second/multi-minute `engineMs` (TaskMaster cold model load),
   or (b) engines fast while the `Engines` phase duration is large (cost is in
   `Task.Run` scheduling or external provider contention, not the engine code).

## Status

- AC9 (capture produced + per-engine attribution): satisfied as an instrumentation
  result.
- AC10 (Phase 4 fix): remains evidence-gated. The slow path is not yet attributed.
  Either (i) capture a cold/slow start with this build to attribute the multi-minute
  case, or (ii) proceed with an evidence-supported defensive improvement to the
  warm-path cost (parallelize independent engine loads, defer non-critical engines
  via the existing `IdleAsyncQueue`, and/or pre-warm `Configuration`), accepting
  that it may only partially address a cold-disk- or provider-dominated slow start.
