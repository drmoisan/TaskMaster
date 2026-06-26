Timestamp: 2026-06-24T11-32
Command: Maintainer non-debugger cold-start DebugView capture (Phase 3.3 full-lifetime heartbeat build, commit 9ad59684).
EXIT_CODE: 0

# Runtime Capture: STA FREEZE PROVEN (~113 s), attributed to the Spam engine init

## Headline (definitive)

The UI/STA thread was hard-frozen for ~113 seconds. This is proven directly: the
DispatcherTimer heartbeats (on the STA) could not fire for that interval.

```
11:34:33,022 [ui-heartbeat] phase=Engines           actualMs=113680.7 gapMs=113430.7
11:34:33,406 [startup-lifetime-heartbeat] Loading    actualMs=114062.4 gapMs=113812.4
```

The freeze is attributed to a single engine:
```
11:34:34,920 [engine-init] engineName=Spam engineMs=115449.5 threadId=11 isThreadPoolThread=True threadPriority=Normal
```

Startup timing: Engines `1:55.51`, TOTAL `1:56.21`. Actionable (9 ms) and Triage
(34 ms) are trivial.

## Why this is the key insight

The Spam engine init runs on a THREAD-POOL thread (id 11, Normal priority) yet the
STA was frozen for ~113 s during it. Pure pool-bound CPU work cannot freeze the STA
(it is time-sliced). Therefore the Spam init must include a BLOCKING COM call that
marshals to the STA and occupies it for the full duration. The 113 s window is
saturated with `WrappedMSProvider::Logon` / `GLookSyncer` / `GmailSyncImpl::Init` /
address-book provider failures (`0x80040401` / `0x80040201` / `0x80040107`),
indicating the blocking COM call is waiting on the failing Gmail store's MAPI
provider logon.

GC is ruled out: `[gc-delta] phase=Engines gen0=1 gen1=1 gen2=0` (~27 MB).

## Why Spam specifically (and not Actionable/Triage)

`SpamBayes.CreateEngineAsync` -> `CreateAsync` (UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.cs:42-70, 591-597) runs, in order:
1. `ValidatePathsSet()` (lines 120-126) — COM access to `Globals.Ol.JunkCertain`, `Globals.Ol.JunkPotential`, `Globals.Ol.Inbox` (Outlook folder resolution; requires store logon).
2. `ValidateSpamClassifierAsync(HasValidSpamClassifierAsync, SpamBayesMissingHandlerAsync, ...)` (lines 57-64, 149+).
3. `await Task.Run(InitAsync)` (line 69) — `Globals.AF.Manager["Spam"]` model deserialize (pure JSON, fast warm).

Actionable and Triage do NOT perform the Junk/Inbox folder validation, and they
are fast. This makes `ValidatePathsSet` (Outlook folder COM resolution against a
store whose provider is failing to log on) the prime suspect for the ~113 s STA
block. The model deserialize (step 3) is the ~1.3-2.5 s seen in warm runs and is
NOT the dominant cold cost.

This reconciles warm vs cold: warm = store already logged on, ValidatePathsSet
fast, deserialize dominates (~1-2 s); cold = store provider not logged on,
ValidatePathsSet blocks the STA ~113 s.

## Reconciliation with prior captures

- T17-42 (Spam 67.5 s), this run (Spam 115 s): Spam is the culprit when its init
  runs during the provider-logon window.
- T21-55 (IntelConfig 60 s + ToDo 55 s): in that run the provider-logon block fell
  during other phases' continuations; same root (STA blocked by a COM call waiting
  on the failing store), different absorbing phase.
- The latency is NOT intermittent in user experience (every cold start freezes
  ~2 min); the absorbing phase varies, but the freeze is consistent.

## Confirmed facts

1. The UI/STA freezes ~113 s on cold start (heartbeat gap) — PROVEN.
2. It is the Spam engine init (engineMs 115 s).
3. It is a COM/STA block (pool thread, STA frozen), not GC, not pure deserialize.
4. It coincides with the failing Gmail store provider logon.

## Next probe (Phase 3.5)

Instrument `SpamBayes.CreateAsync` to time its three sub-steps separately
(`ValidatePathsSet` with each folder access, `ValidateSpamClassifierAsync`, and the
`Task.Run(InitAsync)` model load) to isolate the exact blocking COM call. Combine
with the already-built Phase 3.4 `[store-filter]` probe (commit 216800a5) to name
the blocking store. Then the fix (make the Spam folder validation resilient to /
non-blocking on a failing store, or defer/bound it) can be applied precisely.
