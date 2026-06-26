Timestamp: 2026-06-24T10-24
Command: Maintainer non-debugger cold-start DebugView capture (Phase 3.2 all-phase UI-heartbeat + per-phase GC probe), branch bug/outlook-startup-latency-211, commit f41f8f94.
EXIT_CODE: 0

# Runtime Capture: all-phase UI-heartbeat + per-phase GC (Phase 3.2)

## Headline

The all-phase, phase-annotated heartbeat and per-phase GC probe work as designed.
This run was FAST (TOTAL 0:03.16); it did NOT reproduce the multi-minute stall, so
it serves as a clean baseline rather than a slow-path attribution. A slow run with
this build is still required to observe heartbeat behavior during a 60 s+ phase.

## Startup timing (fast run)

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.12 |
| IntelConfig | 0:00.35 |
| OlObjects | 0:00.00 |
| ToDo | 0:00.42 |
| AutoFile | 0:00.28 |
| Engines | 0:01.96 |
| Events | 0:00.00 |
| TOTAL | 0:03.16 |

## UI heartbeat (per phase)

| phase | gapMs samples |
| --- | --- |
| IntelConfig | 17.5 |
| ToDo | -7.0, 4.6 |
| Engines | 52.3, **676.5**, 32.9, 78.9 |

All phases were fast this run, so the UI stayed responsive. The only notable hitch
was a single `gapMs=676.5` during the Engines phase while the Spam engine loaded
(`engineMs=1740.1` on pool thread 7). That shows a ~1.7 s pool-thread deserialize
can cause a sub-second UI hitch, but not a freeze. No data on the 60 s+ stall this
run because no phase stalled.

## Per-phase GC

| phase | gen0/1/2 | allocatedBytesDelta |
| --- | --- | ---: |
| IntelConfig | 0/0/0 | 1,194,480 |
| OlObjects | 0/0/0 | 16,384 |
| ToDo | 0/0/0 | 2,093,520 |
| AutoFile | **2/1/1** | **76,754,824** |
| Engines | 1/0/0 | 10,982,120 |
| Events | 0/0/0 | 81,920 |

`isServerGC=False`, `latencyMode=Batch` throughout. The AutoFile phase allocated
~76.8 MB and triggered a Gen2 collection in a 0.28 s phase — a notable allocation
spike worth a follow-up look, though it did not cause a stall here.

## Engine attribution (fast)

```
[engine-init] Actionable engineMs=9.0  threadId=5 threadPriority=Normal isThreadPoolThread=True
[engine-init] Spam       engineMs=1740.1 threadId=7 threadPriority=Normal isThreadPoolThread=True
[engine-init] Triage     engineMs=37.1  threadId=4 threadPriority=Normal isThreadPoolThread=True
```

## External provider churn (present again)

`address_book_provider` (`0x80040107`/`0x80040201`), Gmail/Google sync
(`EmailAliases::*`, `GmailSyncImpl::Init`, `GLookSyncer`), `WrappedMSProvider::Logon`
(`0x80040401`). These appear in BOTH fast and slow runs; in slow runs the
associated provider logon/sync evidently takes far longer.

## Status / next

- Phase 3.2 instrumentation validated end-to-end (per-phase heartbeat + GC).
- The slow path is still not captured with the heartbeat. The latency is
  intermittent and (per accumulated evidence) tracks external Outlook
  MAPI/Gmail-sync/address-book provider logon time, not TaskMaster compute.
- Next: capture during a SLOW startup occurrence to confirm whether the STA is
  frozen (heartbeat gaps ~= phase duration) during the 60 s+ phase.
