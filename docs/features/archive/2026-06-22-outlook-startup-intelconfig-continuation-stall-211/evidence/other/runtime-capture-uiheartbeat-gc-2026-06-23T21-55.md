Timestamp: 2026-06-23T21-55
Command: Maintainer non-debugger cold-start DebugView capture (Phase 3.1 UI-heartbeat + GC probe), branch bug/outlook-startup-latency-211, commit 869b8906.
EXIT_CODE: 0

# Runtime Capture: UI-heartbeat + GC probe (Phase 3.1) — diagnosis reframed

## Headline

This capture DISPROVES the GC hypothesis and shows the SpamBayes attribution was
a single-run artifact. The multi-minute latency is a cross-cutting, intermittent
stall that lands on DIFFERENT phases run-to-run, and it correlates with external
Outlook MAPI / Google-sync / address-book provider churn — not with any TaskMaster
phase's own compute.

## Startup timing table (this run)

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.12 |
| IntelConfig | 1:00.05 |
| OlObjects | 0:00.01 |
| ToDo | 0:55.74 |
| AutoFile | 0:00.49 |
| Engines | 0:02.65 |
| Events | 0:00.00 |
| TOTAL | 1:59.09 |

The cost is in **IntelConfig (60 s) and ToDo (55.7 s)** this run; Engines was fast.

## Per-engine (Engines phase was fast this run)

```
[engine-init] engineName=Spam engineMs=2544.8 threadId=14 threadPriority=Normal isThreadPoolThread=True
[engine-init] engineName=Triage engineMs=37.2 threadId=17 threadPriority=Normal isThreadPoolThread=True
```

Spam was 2.5 s here vs 67.5 s in the T17-42 cold run. So SpamBayes is NOT the root
cause; in T17-42 the Engines phase merely happened to be the phase holding the
stall.

## GC probe (across the Engines phase)

```
[gc-delta] gen0=1 gen1=0 gen2=0 allocatedBytesDelta=10932432 isServerGC=False latencyMode=Batch
```

One Gen0 collection, zero Gen1/Gen2, ~10.9 MB allocated. **GC is not the cause of
the stall.** (Note: GC coverage was scoped to the Engines phase, which was not the
slow phase this run.)

## UI heartbeat (across the Engines phase only)

```
[ui-heartbeat] nominalMs=250.0 actualMs=248.0 gapMs=-2.0
[ui-heartbeat] nominalMs=250.0 actualMs=262.1 gapMs=12.1
[ui-heartbeat] nominalMs=250.0 actualMs=255.9 gapMs=5.9
[ui-heartbeat] nominalMs=250.0 actualMs=277.8 gapMs=27.8
```

Gaps are small (UI responsive) — but the heartbeat only covered the Engines phase
(fast this run). It did NOT cover IntelConfig/ToDo where the 60 s + 55.7 s stalls
occurred. So this run does not yet tell us whether the STA was frozen during the
actual stalls. The probe scope must be widened to the entire LoadSequentialAsync.

## External provider churn (candidate cause)

The log is saturated with Outlook provider assertion failures during startup:
`address_book_provider` (`0x80040107`, `0x80040201`), Google/Gmail sync
(`EmailAliases::*`, `GmailSyncImpl::Init`, `GLookSyncer`), and
`WrappedMSProvider::Logon` (`0x80040401`). Total startup is ~2 minutes in the slow
runs regardless of which phase absorbs it, suggesting TaskMaster startup phases
make STA-bound COM calls that block while Outlook's own MAPI/sync providers are
initializing or failing.

## Revised hypothesis

The multi-minute startup latency is a cross-cutting STA stall: one or more
TaskMaster startup phases issue synchronous COM / Outlook-object-model calls that
block on the STA until not-yet-ready (or repeatedly-failing) Outlook providers
respond. The phase that records the cost is whichever phase is awaiting a COM-bound
result during the provider-churn window; it is not the phase's own compute, and it
is not GC or SpamBayes.

## Next diagnostic step (probe scope correction)

Widen the UI-heartbeat + GC probe to span the ENTIRE `LoadSequentialAsync` (all
phases, with per-phase boundary markers), so the next capture shows (a) whether the
STA is actually frozen during the IntelConfig/ToDo stall (heartbeat gaps) vs merely
an async continuation waiting, and (b) the GC state during the real slow phase. If
the STA is frozen, instrument the slow phase bodies (`LoadIntelConfigAsync`,
`LoadToDoPhaseAsync`) to identify the specific blocking COM call.
