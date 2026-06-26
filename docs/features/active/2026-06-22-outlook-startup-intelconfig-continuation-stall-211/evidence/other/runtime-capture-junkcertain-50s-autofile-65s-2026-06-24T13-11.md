Timestamp: 2026-06-24T13-11
Command: Maintainer non-debugger cold-start DebugView capture (all probes, commit 66806b82).
EXIT_CODE: 0

# Runtime Capture: PRECISE attribution — Globals.Ol.JunkCertain (50s) + AutoFile (65s)

## Headline (conclusive)

The ~2-minute startup freeze is direct Outlook folder/store COM resolution against
the failing Gmail store blocking the STA. Two back-to-back blocks this run:
- AutoFile phase: 65.26 s.
- Engines phase (Spam): 51.29 s, of which `ValidatePathsSet.JunkCertain` = 50.17 s.
The Spam model deserialize is 0.9 ms (NOT the cause).

## Startup timing table

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.13 |
| IntelConfig | 0:00.16 |
| OlObjects | 0:00.00 |
| ToDo | 0:00.42 |
| AutoFile | 1:05.26 |
| Engines | 0:51.29 |
| Events | 0:00.00 |
| TOTAL | 1:57.28 |

## [phase-net] (StoreWrapper-init ambient clock)

```
[phase-net] phase=IntelConfig grossMs=160.4  storeWrapperInitMs=0.0 netMs=160.4
[phase-net] phase=OlObjects   grossMs=4.2     storeWrapperInitMs=0.0 netMs=4.2
[phase-net] phase=ToDo        grossMs=424.4   storeWrapperInitMs=0.0 netMs=424.4
[phase-net] phase=AutoFile    grossMs=65264.1 storeWrapperInitMs=0.0 netMs=65264.1
[phase-net] phase=Engines     grossMs=51297.3 storeWrapperInitMs=0.0 netMs=51297.3
[phase-net] phase=Events      grossMs=7.2     storeWrapperInitMs=0.0 netMs=7.2
```

`storeWrapperInitMs=0.0` for every phase, and there are NO `[store-wrapper-init]`
or `[store-filter]` lines anywhere in the capture. Therefore `StoreWrapper.Init()` /
`StoresWrapper.GetFilteredStores()` did NOT run during this startup. The block is
NOT in the StoreWrapper enumeration path; it is in direct `Globals.Ol.*` folder
resolution.

## [spam-init] (Spam CreateAsync sub-steps) — the precise Spam attribution

```
[spam-init] step=ValidatePathsSet.JunkCertain  ms=50172.2
[spam-init] step=ValidatePathsSet.JunkPotential ms=1016.6
[spam-init] step=ValidatePathsSet.Inbox        ms=4.4
[spam-init] step=ValidatePathsSet              ms=51195.0
[spam-init] step=ValidateSpamClassifier        ms=16.1
[spam-init] step=InitAsync(modelLoad)          ms=0.9
[engine-init] engineName=Spam engineMs=51214.8 (Actionable 9.1, Triage 37.8)
```

The Spam engine's entire cost is `Globals.Ol.JunkCertain` resolution (50.17 s) plus
a smaller `JunkPotential` (1.0 s). `Inbox` is fast (4.4 ms), model load 0.9 ms,
classifier validate 16 ms. So the Spam path blocks on resolving the JunkCertain
(and to a lesser extent JunkPotential) Outlook folder against the failing store.

## STA freeze (heartbeat)

```
[ui-heartbeat] phase=Engines actualMs=113471.4 gapMs=113221.4
[startup-lifetime-heartbeat] stageLabel=Loading actualMs=114761.9 gapMs=114511.9
```
A single continuous ~113 s STA freeze spanning the AutoFile (65 s) + Engines (50 s)
window. Not GC (Engines `gen0=1 gen1=0 gen2=0`; AutoFile `gen0=2 gen1=1 gen2=1`, 78 MB).
Provider churn throughout: `WrappedMSProvider::Logon`, `GLookSyncer`,
`GmailSyncImpl::Init`, address-book (`0x80040401`/`0x80040201`/`0x80040107`).

## Diagnosis (precise)

Root cause: accessing Outlook folders backed by the failing Gmail store's MAPI
provider blocks the STA ~50-65 s per access. Two independent startup accesses hit it
this run:
1. `AppAutoFileObjects.LoadAsync` (AutoFile phase) — 65 s (sub-call not yet instrumented).
2. `SpamBayes.ValidatePathsSet` -> `Globals.Ol.JunkCertain` (Engines phase) — 50 s.

The "shifts module to module" pattern is explained: multiple startup code paths
resolve store-backed Outlook folders, and each blocks on the failing provider; which
phase shows the cost depends on which paths run and in what order. The model
deserialization and `StoreWrapper.Init` enumeration are NOT the cause (0.9 ms / did
not run).

## Fix direction (now actionable)

The fix must make startup-time Outlook folder/store resolution resilient to a
non-logged-on / failing store so these COM accesses cannot freeze the STA. Candidates:
- Resolve `Globals.Ol.JunkCertain`/`JunkPotential` (and the AutoFile folder accesses)
  without blocking on a store whose provider is not ready (bounded/guarded access,
  or skip stores that are not logged on), and/or
- Exclude the failing Gmail store from TaskMaster folder resolution before any
  blocking COM property is touched, and/or
- Move these COM-bound resolutions off the startup critical path until Outlook
  signals the stores are ready (extend the #207 readiness gate).

Remaining sub-instrumentation gap: AutoFile's 65 s blocking call is not yet broken
down to the exact COM accessor (only Spam's is). The fix increment should either
instrument it or address the shared store-resolution layer that covers both.
