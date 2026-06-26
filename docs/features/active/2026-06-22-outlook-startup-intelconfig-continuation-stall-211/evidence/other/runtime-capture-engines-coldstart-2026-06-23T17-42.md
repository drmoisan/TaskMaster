Timestamp: 2026-06-23T17-42
Command: Maintainer-provided non-debugger COLD-START Outlook DebugView capture (AC9 slow path), built from branch bug/outlook-startup-latency-211 with the Phase 3 per-engine attribution probe.
EXIT_CODE: 0

# Runtime Capture: Cold-Start Outlook Startup (AC9 slow-path attribution)

## Summary

This cold start REPRODUCED the multi-minute startup latency and the Phase 3
per-engine probe ATTRIBUTED it. The dominant cost is a single engine: the
**SpamBayes engine deserialization at 67,536.6 ms (~67.5 s)**.

## Per-engine attribution (Phase 3 probe)

| Item | ms | thread | costHint | timestamp |
| --- | ---: | ---: | --- | --- |
| Configuration | 0.0 | 15 | (config) | 17:42:55.450 |
| Actionable | 9.5 | 6 | Deserialization | 17:42:55.496 |
| Spam | **67536.6** | 13 | Deserialization | 17:44:03.034 |
| Triage | 43.7 | 8 | Deserialization | 17:44:03.078 |

There is a ~67 s gap between the Actionable line (17:42:55) and the Spam line
(17:44:03); the Spam factory (`SpamBayes.CreateEngineAsync`) is the entire gap.

## Startup timing table

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.07 |
| IntelConfig | 0:00.40 |
| OlObjects | 0:00.00 |
| ToDo | 0:00.32 |
| AutoFile | 0:00.34 |
| Engines | 1:07.63 |
| Events | 0:00.00 |
| TOTAL | 1:08.80 |

The `Engines` phase (1:07.63) ≈ the Spam engineMs (67.5 s). Spam alone accounts
for essentially the whole multi-minute startup cost.

## Cold vs warm comparison (Spam engine)

| Run | Spam engineMs | Engines phase | TOTAL |
| --- | ---: | ---: | ---: |
| T17-33 (warm) | 1328.7 | 0:01.42 | 0:02.59 |
| T17-42 (cold) | 67536.6 | 1:07.63 | 1:08.80 |

A ~50x difference between warm and cold for the same engine indicates the cost is
dominated by cold-disk model read plus deserialization (the research flagged
Newtonsoft `PreserveReferencesHandling.All`, whose cost is super-linear in object
graph size). Triage and Actionable are trivial in both runs (≈10–44 ms).

## STA / threading note

The Spam load runs on a thread-pool thread (threadId=13) under
`Task.Run(Engines.InitAsync)`, and the post-Engines continuation resumes on the
STA in `waitMs=0.0` with `staIsIdle=True`. So the 67.5 s is thread-pool work that
gates startup *completion* (`Finished loading globals` is delayed 67.5 s), but the
STA itself is not CPU-blocked by it.

## Diagnosis (AC9 — slow path attributed)

Root cause of the #211 multi-minute startup latency: **`SpamBayes.CreateEngineAsync`
model deserialization, ~67.5 s on a cold start**, awaited inside the sequential
`Engines` startup phase. This is a TaskMaster-side cost ("in scope iff this add-in
causes it" — it does).

## Separate finding (NOT the latency; new scope)

The same run shows `AppEvents.ProcessNewInboxItemsAsync complete … elapsedMs=0`
(processed nothing), and the maintainer reports the Spam/Triage/Actionable custom
column fields remain empty for new emails. The event-driven classification path is
`AppEvents.OlInboxItems_ItemAdd -> ProcessMailItemAsync`
(`AppEvents.cs:272,284`), which is independent of engine construction and was NOT
modified by the Phase 3 change. This "engines not classifying new mail" defect is a
separate concern from the startup latency and should be tracked/investigated on its
own.

## Phase 4 fix direction (AC10, now actionable)

Two non-exclusive options, to be settled by a focused plan:
1. Move engine initialization (at minimum the Spam engine) off the sequential
   startup critical path so startup completion is not gated by the 67.5 s load
   (the `IdleAsyncQueue` / `LoadWhenIdle` path already exists and already enqueues
   `Engines.InitAsync`).
2. Reduce the SpamBayes deserialization cost itself (serialization format / caching
   a compact binary form / avoiding `PreserveReferencesHandling.All` on a large
   graph). This also benefits engine readiness for classification.
