# Research: Outlook startup store-enumeration COM stall (Issue #292)

- **Issue:** #292
- **Date:** 2026-07-09
- **Author:** task-researcher agent
- **Evidence sources:** `artifacts/log.txt` (capture 2026-07-09 14:14:42 → 14:16:39), repository HEAD (`c9ddbf28` lineage, branch `TaskMaster-wt-2026-07-09T14-19`)

---

## 1. Current State Analysis

### 1.1 Reachability of the blocking site on HEAD (Question 1) — CONFIRMED

The synchronous fresh-build path is the reachable production path at cold start, triggered when the persisted `StoresWrapper` config is **missing or deserializes to null**:

- `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:35-73` (`LoadStoresAsync`):
  - Config key present but deserializes to null → warn at line 51-53, falls through to fresh build.
  - Config key absent → warn at line 57, falls through to fresh build.
  - Fresh build at line 64: `StoresWrapper = BuildFreshStoresWrapper();`
- `AppOlObjects.StoreLoading.cs:32-33`: `BuildFreshStoresWrapper() => new StoresWrapper(_globals).Init();`
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:44`: `var filteredStores = GetFilteredStores().ToList();` — the blocking materialization.
- `StoresWrapper.cs:167-172` (`GetFilteredStores`): `Globals.Ol.NamespaceMAPI.Stores.Cast<Outlook.Store>().Where(ShouldIncludeStoreInstrumented)`. The first `MoveNext()` of `Cast<>` invokes `IEnumVARIANT::Next()` on the raw COM enumerator.

Log ground truth for the captured incident:

- `log.txt:122` — `2026-07-09 14:14:48,000 WARN TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.` (the deserialized-to-null trigger, not the missing-key trigger).
- `log.txt:131` — first watchdog stack capture at `14:14:49,861` showing `EnumeratorViewOfEnumVariant.MoveNext()` → `StoresWrapper.Init() ... StoresWrapper.cs:line 44` → `BuildFreshStoresWrapper() ... AppOlObjects.StoreLoading.cs:line 33`. Line numbers in the captured binary match HEAD exactly.
- Identical stacks repeat every ~1.8 s until the log ends at `14:16:39,150` (`log.txt:6463`), followed by the `ContextSwitchDeadlock` MDA text at `log.txt:6561-6562` ("unable to transition from COM context 0x7c34c290 to COM context 0x7c34c168 for 60 seconds"). Total observed stall: ~111 s, blocked on the FIRST `Next()` before any store was yielded.

**Deserialize path has the same unguarded site — CONFIRMED.** `StoresWrapper.cs:89` (`RewireOlObjectsAsync`): `var stores = GetFilteredStores().ToList();` — same materialization, no `CurrentStoreContext` scope, no readiness gate. The per-store `CurrentStoreContext.Begin(storeDisplayName)` scope (added by #264) opens only inside `AddOrRestoreStore` at `StoresWrapper.cs:146`, i.e. only **after** a store has been yielded by the enumeration. Both paths therefore stall with blank attribution when the enumeration itself blocks.

### 1.2 State of the #260 watchdog pipeline during this stall

- The watchdog is **live in production**: `TaskMaster/ThisAddIn.cs:35-40` passes `monitorUiThread: true` and wires `onLockupDetected` to `GetStoreLockupResponder()?.OnLockupDetected(attribution)`. (This supersedes the #264-era memory that the monitor was dormant.)
- `ThreadMonitor.EvaluatePoll` (`UtilitiesCS/Threading/ThreadMonitor.cs:173-194`) reads `CurrentStoreContext.Current` and fires the callback once per stall episode after the 5000 ms default threshold.
- `StoreLockupResponder.OnLockupDetected` (`UtilitiesCS/Threading/StoreLockupResponder.cs:86-89`) has a hard **no-context guard**: a blank attribution performs no disable, no notify, and no WARN line. Because the enumeration blocks before any `Begin` scope, `CurrentStoreContext.Current == null` for the entire 111 s — consistent with the log containing **zero** `[store-lockup]` WARN lines despite the watchdog demonstrably polling (Debug stack captures throughout). This is the precise mechanism by which the existing #260 pipeline was unable to act.
- `StoreDisable` service availability: `ApplicationGlobals.cs:122` constructs `StoreDisableService` in `LoadBasicMethod`, which ran at ~14:14:42 (`log.txt:62`), before the stall — so the responder itself was resolvable; only the blank attribution stopped it.

### 1.3 Latent crash hazard discovered (load-bearing for the fix design)

If attribution parity were added naively (synthetic identity, no responder change), the fresh-build stall would follow this verified chain:

1. `StoreLockupResponder.OnLockupDetected` passes the non-blank guard, `IsDisabled` returns false (`StoreDisableService.cs:133-142` — model null → false).
2. `DisableSessionOnly` → `GetModelForWriteOrThrow()` **throws `InvalidOperationException`** (`StoreDisableService.cs:64-75`), because `AppOlObjects.StoresWrapper` is assigned only after `Init()` returns (`AppOlObjects.StoreLoading.cs:64`) — i.e. the model is null for the entire duration of a fresh-build stall.
3. The exception propagates out of `_onLockupDetected?.Invoke` (`ThreadMonitor.cs:192`) through `Tick()` (`ThreadMonitor.cs:104-133` — `try/finally` with **no catch**) into a `TimeProvider` timer callback on a threadpool thread → unhandled exception → `outlook.exe` process-termination risk on .NET Framework.
4. On the deserialize-rewire path (model non-null), `DisableSessionOnly` would instead "succeed", polluting `SessionDisabledStoreIdentities` and the #265 disabled-stores settings UI with a bogus non-store entry.

Conclusion: any attribution-parity change **must** be paired with a phase-identity-aware branch in `StoreLockupResponder` (or equivalent) that skips the disable-service write path.

---

## 2. Candidate Approaches (Question 2)

### 2a. Readiness-gating the enumeration on the existing #260 signal — NOT VIABLE AS PRIMARY FIX

- The only pre-enumeration readiness signal is app-wide: `OutlookReadinessGate.IsReady()` (`UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs:61-72`) probes `Session.DefaultStore.GetDefaultFolder(olFolderInbox)`. The store-scoped overload `IsReady(Store)` (line 82-92) requires a live `Store` object, which is **unobtainable before the enumeration** — the enumeration is the only way to get one, and it is the thing that blocks.
- Pumping-wait primitives exist and are proven: `HookReadinessCoordinator` driven by a `DispatcherTimer` (`AppEvents.cs:167-192`, #207 pattern; STA keeps pumping between ticks) and awaited `NonBlockingDelay.WaitAsync` loops (`StoreRehookCoordinator.cs:157-173`, #263 pattern; pump-independent, unit-testable).
- However, the gate covers only the **default** store. In the captured incident, Outlook's UI was up and the IntelConfig phase had completed reads at 14:14:47,991 (`log.txt:109`), and the stall began at 14:14:48,691 — there is **no evidence the gate would have reported not-ready** at that moment. The stalling store is, by the epic's established causal model (#207/#211/#260), a non-default store mid-`WrappedMSProvider::Logon`; the default-store probe says nothing about it. A gate that would have passed immediately cannot be claimed to prevent this stall, and a regression test asserting "gate prevents the stall" would be unfalsifiable against the captured defect.
- Verdict: reject as the fix. May be filed as separate, optional hardening (it narrows the race window on machines where the default store is also slow), but it does not address issue #292's evidence.

### 2b. Attribution parity via `CurrentStoreContext.Begin("<Stores-enumeration>")` — VIABLE, RECOMMENDED, DIAGNOSTICS + SAFE RESPONSE ONLY

- Wrapping the materialization at `StoresWrapper.cs:44` and `:89` in an ambient scope gives the watchdog a non-blank attribution at `ThreadMonitor.cs:191`. `CurrentStoreContext.Normalize` (`CurrentStoreContext.cs:52-65`) passes any non-`"<unavailable>"` string through, and nested per-store `Begin` scopes inside the `Where` filter restore correctly (the `Scope` class restores the previous value on dispose).
- **Can the #260 auto-disable pipeline recover from the synthetic identity? No.** The identity is not a store DisplayName; disabling it cannot exclude the actual stalling store because (i) the disabled-set check runs inside `ShouldIncludeStoreInstrumented` (`StoresWrapper.cs:210`) per **yielded** store, after `MoveNext()` returns — and the block is inside `MoveNext()` before the first yield; and (ii) on the fresh-build path the persisted config is missing/null by definition, so there is no disabled state to consult. Additionally, per §1.3, letting the responder run its normal disable path on the synthetic identity either crashes the watchdog thread (fresh path) or pollutes the disabled-store model/UI (rewire path).
- What it achieves: one `[store-lockup]` WARN line with the phase identity and measured stall duration, plus a user-visible modeless notification that startup store enumeration is stalled — instead of today's total silence. This is the maximum the pipeline can do when the stalling store's identity is unknowable.

### 2c. Indexed access (`Stores.Count` + `Stores[i]`) instead of `Cast<>().MoveNext()` — NO

- Both are synchronous COM calls into the same `Outlook.Stores` collection object served by the same MAPI provider; the observed block is a cross-COM-context transition (`ContextSwitchDeadlock` text, `log.txt:6561-6562`) into a context that is not pumping — any call into that context blocks identically. The Outlook object model exposes no non-blocking readiness pre-check on a `Stores` collection.
- Honest caveat: this cannot be empirically differentiated without a live reproduction of the misbehaving store. No evidence exists that indexed access avoids the block, and the mechanism (provider logon stall) is call-shape-independent. `ApplicationGlobals.StoreRehook.cs:67` (`ResolveLiveStore`) uses `foreach` over the same collection and carries the same exposure.

### 2d. Detecting the stalling store BEFORE enumerating — NO

- The persisted `DisabledStoreIdentities` mechanism (#261) filters per yielded store; it cannot prevent entry into the blocking `Next()`.
- On the exact path that stalled (fresh build), the persisted config is missing or null — the disabled set is empty/unavailable by definition.
- No add-in-side data source identifies which store is mid-logon without touching `Namespace.Stores`/`Session.Accounts` (both COM). The stalling store's identity is structurally unknowable from managed code at this point. The only achievable attribution is the coarse phase identity of 2b.

### Rejected alternatives (summary)

- **Worker-thread offload of the enumeration:** Outlook OOM objects are apartment-bound; a call from another thread marshals into the owning/stalled context and blocks the same way, while adding marshaling failure modes. Consistent with the issue's stated constraint.
- **Timeout/cancellation wrapper (e.g. `TimeOutTask`):** there is no managed cancellation for a blocked `IEnumVARIANT::Next()`; a timeout would abandon, not abort, the call, leaving the STA blocked regardless. Unfalsifiable as a fix.
- **Readiness-gating (2a):** see above; unfalsifiable against the captured incident.

---

## 3. Recommended Fix (Question 3)

Minimal, causation-scoped, two-part change plus tests:

### 3.1 `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — enumeration-phase attribution scope

- Add a phase-identity constant, recommended home: `CurrentStoreContext` (`UtilitiesCS.Threading`), e.g. `public const string StoresEnumerationPhaseIdentity = "<Stores-enumeration>";` — same assembly as both the writer (`StoresWrapper`) and the reader-consumer (`StoreLockupResponder`), and consistent with the existing `"<unavailable>"` angle-bracket convention (which `Normalize` treats specially; the new constant must NOT be `"<unavailable>"`).
- Extract one private helper (e.g. `MaterializeFilteredStores()`) that performs `GetFilteredStores().ToList()` inside `using (CurrentStoreContext.Begin(CurrentStoreContext.StoresEnumerationPhaseIdentity))`, and call it from both `Init()` (line 44) and `RewireOlObjectsAsync` (line 89). The existing Stopwatch/log lines stay as-is. Included set and enumeration order are untouched (the scope is observational only); nested per-store `Begin` scopes inside the filter/`AddOrRestoreStore` continue to work via scope restore.
- File is 449 lines; the addition stays under the 500-line ceiling.

### 3.2 `UtilitiesCS/Threading/StoreLockupResponder.cs` — phase-identity branch (required, see §1.3)

- After the existing blank/unresolved guards, add: if the attribution identity equals the phase constant, emit the `[store-lockup]` WARN line via the injected `_logSink` with `autoDisabled: false` (the `StoreLockupAttribution.FormatLine` overload already takes this flag, see usage at line 114-118), optionally dispatch an informational modeless notification, and **return without calling `IsDisabled`/`DisableSessionOnly`/the action-button wiring**. This closes both the watchdog-thread `InvalidOperationException` crash path and the disabled-list pollution path.
- All four collaborators are already constructor-injected seams (`IStoreDisableService`, `IUiDispatcher`, `StoreLockupNotifier`, `Action<string> logSink`) — no new seam needed here.

### 3.3 Seam assessment — no new production seam is required for a RED-on-HEAD test

The delegation asked for an injectable seam. The load-bearing finding is that the seam already exists and is proven:

- `StoresWrapper` takes `IApplicationGlobals` by constructor; tests already drive `Globals.Ol.NamespaceMAPI.Stores` end-to-end with `ReflectionRealProxy` proxies for `IOlObjects`/`NameSpace`/`Stores`/`Store` — see `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs:359-401` (`CreateOlObjectsProxy`, `CreateNamespaceProxy`, `CreateStoresProxy` with a controllable `GetEnumerator`) and the Moq variant `Mock<Stores>().As<IEnumerable>().Setup(x => x.GetEnumerator())` at `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:251-261`.
- Because the test controls the enumerator, it can observe `CurrentStoreContext.Current` **from inside `MoveNext()`** — exactly the instant the production stall occurs. That makes the attribution test genuinely RED on HEAD without any production change. Introducing a new `Func<IEnumerable<Outlook.Store>>` override seam is therefore unnecessary; prefer the existing proxy chain (smaller diff, no new public surface).

### 3.4 Optional secondary change (planner judgment, same defect class, same path)

`Init()` line 48 creates each `StoreWrapper` via `new StoreWrapper(store).Init()` WITHOUT a per-store `CurrentStoreContext` scope — unlike the rewire path, which routes through `AddOrRestoreStore` (scoped at line 146). Routing the fresh-build loop through `AddOrRestoreStore` would give per-store attribution parity for the next COM touch after the enumeration succeeds, with identical set/order (fresh `Stores` starts empty, so `AddOrRestoreStore` always creates). This widens the diff slightly; include only if the planner accepts it as the same causal scope. The primary fix (§3.1/§3.2) is sufficient for the captured defect, which stalled before any store was yielded.

---

## 4. Behavior Semantics

- **Healthy path (unchanged):** enumeration yields stores; included set and order are byte-identical (the ambient scope has no effect on filtering); the scope is disposed when materialization completes, restoring the previous ambient value (null at startup).
- **Stall during enumeration (new):** after `lockupAttributionThresholdMs` (5000 ms default), `ThreadMonitor.EvaluatePoll` raises `LockupAttribution(stallDuration, "<Stores-enumeration>")` exactly once per episode; the responder emits one WARN line (`autoDisabled: false`) and (optionally) one informational notification; no disable-service write occurs; no exception escapes the watchdog thread.
- **Stall during per-store work (unchanged):** existing `AddOrRestoreStore` per-store scope and full auto-disable/notify behavior are unaffected.
- **Failure inside enumeration (exception):** the `using` guarantees scope restore, so a thrown `COMException` cannot leak the phase identity into later, unrelated attributions.
- **Ordering rule:** blank-guard → unresolved-guard → phase-identity guard → already-disabled guard → disable/notify (the phase guard must precede any `IStoreDisableService` call).

---

## 5. Requirements Mapping (draft ACs from issue.md → achievable design)

| Draft AC (issue.md) | Assessment |
|---|---|
| AC1 "enumeration does not block the STA pump past the point at which the #260 watchdog can act" | Needs rewording. The watchdog runs on an independent background thread and demonstrably acted (stack captures) throughout the 111 s block; what failed is that its action was empty (blank attribution). No managed mechanism can prevent or bound the block once `Next()` is entered (§6). Achievable form: "a stall at this site produces a watchdog action (attributed WARN + safe response) within the attribution threshold." |
| AC2 "meaningful lockup attribution (non-null store identity)" | Achievable as a non-null **phase** identity (`"<Stores-enumeration>"`), not a store identity — the store identity is structurally unknowable pre-yield (§2d). The auto-disable pipeline must explicitly NOT act on it (§1.3); AC2's "usable by the existing auto-disable/notify pipeline" should be narrowed to "handled safely by the pipeline: WARN + notify, no disable write". |
| AC3 "preserves included-store set and enumeration order" | Achievable and directly testable (§7, T4). |
| AC4 "regression via injectable seam, no live Outlook, no temp files" | Achievable with existing proxy/Moq seams; genuinely RED on HEAD (§7, T1–T3). |

State model: `CurrentStoreContext.Current` gains one new value class (phase identity) alongside the existing null / store-DisplayName values; `StoreLockupResponder` gains one new terminal branch. No persistence schema change; no public API break (`StoresWrapper` members added are private/const).

Files to change:
1. `UtilitiesCS/Threading/CurrentStoreContext.cs` — add the phase constant (2-4 lines).
2. `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — extract scoped materialization helper; call from lines 44 and 89.
3. `UtilitiesCS/Threading/StoreLockupResponder.cs` — phase-identity branch.
4. Tests: `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs` (or a sibling focused test class in the same folder to respect file-size limits) and `UtilitiesCS.Test/Threading/StoreLockupResponderTests.cs`.

---

## 6. Honest Residual Verdict (Question 4)

- **What the code fix cannot do:** it cannot prevent, shorten, cancel, or bound the ~108-111 s STA block once `IEnumVARIANT::Next()` is entered. There is no managed cancellation for a blocked COM call; the call is apartment/context-bound (offload does not help, §2 rejected alternatives); and the stall's duration is determined by the misbehaving store's MAPI provider logon inside `outlook.exe`, not by add-in code. The `ContextSwitchDeadlock` MDA will still fire under a debugger during a real stall.
- **Scope by causation:** the add-in does not cause the stall; its causal contribution is touching `Namespace.Stores` during startup, which any OOM client would also block on. The add-in's actual defect — the thing this issue can fix — is that its own #260 resilience pipeline is silent (blank attribution) and, worse, one code path away from a watchdog-thread crash (§1.3) at this site.
- **What the code fix achieves:** attributed, logged, user-visible, crash-safe handling of the stall; unchanged healthy-path behavior; regression coverage that fails on HEAD.
- **Environmental residual (human action):** the stall itself is relieved only by repairing, re-syncing, or removing the misbehaving store/account in Outlook (account settings / OST-PST repair). This is outside code scope and is recorded under Automation Feasibility below.

---

## 7. Testing Implications (strategy only; MSTest + Moq + FluentAssertions, no live Outlook, no temp files)

All tests use the existing `ReflectionRealProxy`/`Mock<Stores>().As<IEnumerable>()` patterns (§3.3) and `StubApplicationGlobals` (`StoresWrapperTests.cs:236`).

- **T1 (RED on HEAD)** — `Init()` attribution parity: stores proxy whose `GetEnumerator` returns an enumerator that records `CurrentStoreContext.Current` on each `MoveNext()`. Assert the recorded value equals the phase constant. On HEAD it records null → fails. This is the primary regression for the captured stack (blocked inside `MoveNext` at `StoresWrapper.cs:44`).
- **T2 (RED on HEAD)** — same observation for the `RewireOlObjectsAsync` materialization (`StoresWrapper.cs:89`), driven through `RewireAfterDeserializeAsync`.
- **T3 (RED on HEAD)** — responder phase branch: `OnLockupDetected(new LockupAttribution(6 s, "<Stores-enumeration>"))` with a `MockBehavior.Strict` `IStoreDisableService`. Assert exactly one WARN line via the injected `logSink` (with `autoDisabled: false` formatting) and **zero** disable-service calls. On HEAD, `IsDisabled`/`DisableSessionOnly` are invoked → Strict mock fails.
- **T4 (behavior-preserving, GREEN before and after)** — healthy multi-store enumeration yields the identical included set and order as today, and `CurrentStoreContext.Current` is null after `Init()` returns (scope disposed).
- **T5** — scope restore on failure: an enumerator that throws mid-enumeration leaves `CurrentStoreContext.Current` null afterwards.
- Determinism notes: no clocks are involved in T1/T2/T4/T5; T3's `LockupAttribution` takes an explicit `TimeSpan`. No `FakeTimeProvider` is needed because `ThreadMonitor.EvaluatePoll` itself is already covered (`UtilitiesCS.Test/Threading/ThreadMonitorTests.cs`) and is not modified.
- Coverage: all touched lines are host-neutral (no COM in the new helper body other than the delegated call already exempt at its boundary); new lines carry the >= 90 % new-code obligation and are fully reachable via the proxy seams.

---

## Automation Feasibility

**Code fix delivery and verification: fully autonomous.** All recommended changes (§3.1-§3.3) are host-neutral or proxy-testable; the regression tests (T1-T5) run headless under `vstest.console.exe` with the repository's existing COM-interface proxy patterns — no live Outlook process, no temp files, no human interaction. The full toolchain (csharpier → analyzers → nullable → vstest) is executable end-to-end by an agent.

**Not automatable (environmental / manual, recorded separately, non-gating for the code fix):**

1. **End-to-end reproduction of the real 108 s stall.** Requires the specific misbehaving store account present in an Outlook desktop profile and a cold start. The unit tests reproduce the *observable contract* (attribution present at MoveNext-time; responder behavior), not the wall-clock stall.
2. **Environmental remediation of the stalling store** (repair/re-add the account, rebuild the OST, or remove the store via Outlook account settings). This is the only relief for the stall duration itself (§6) and is a human action in the Outlook desktop UI.
3. **In-situ confirmation** that a subsequent real stall now produces the `[store-lockup]` WARN line and the modeless notification: requires a human-observed cold start on the affected machine while the misbehaving store is still present. Recommended as a post-merge manual validation note, not an acceptance gate.

No part of implementing or unit-verifying the fix requires configuring or interacting with Outlook desktop.

---

## Appendix: Key evidence index

| Claim | Evidence |
|---|---|
| Fresh-build trigger fired | `artifacts/log.txt:122` (WARN, 14:14:48,000) |
| Block at first `Next()`, `StoresWrapper.cs:44` | `artifacts/log.txt:131-135`, `:6463-6470` |
| Stall span ~111 s, MDA | `artifacts/log.txt:131` → `:6463`, MDA at `:6561-6562` |
| Rewire path same site | `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:89` |
| Per-store scope opens only post-yield | `StoresWrapper.cs:146` (`AddOrRestoreStore`) |
| Watchdog live, callback wired | `TaskMaster/ThisAddIn.cs:35-40` |
| Blank-attribution guard silences responder | `UtilitiesCS/Threading/StoreLockupResponder.cs:86-89` |
| Crash hazard chain | `StoreLockupResponder.cs:110` → `StoreDisableService.cs:64-75` throw → `ThreadMonitor.cs:192` invoke → `Tick` no catch (`ThreadMonitor.cs:104-133`); model null until `AppOlObjects.StoreLoading.cs:64` completes |
| Readiness gate is default-store-only | `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs:61-92` |
| Pumping-wait precedents | `TaskMaster/AppGlobals/AppEvents.cs:167-192` (DispatcherTimer, #207); `TaskMaster/AppGlobals/NonBlockingDelay.cs:42-59` (#207 AC10); `StoreRehookCoordinator.cs:157-173` (#263) |
| Test seam precedents | `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs:359-401`; `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:251-261` |
