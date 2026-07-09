# outlook-startup-store-enumeration-com-stall (Spec)

- **Issue:** #292
- **Parent (optional):** recurring COM-blocking startup stall class (#207, #211, epic #260)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug

## Context

- Summary of the bug and its impact: On Outlook cold start, the STA/UI thread (`VSTA_Main`) blocks for ~108-111 seconds inside the raw COM enumeration of `Namespace.Stores`, tripping the `ContextSwitchDeadlock` MDA (CLR unable to transition COM contexts for 60 seconds). The block occurs on the first `IEnumVARIANT::Next()` — before any store is yielded. During the entire stall the #260 watchdog is live and polling but produces a blank attribution (`CurrentStoreContext.Current == null`), so the resilience pipeline is silent and, on the fresh-build path, is one code path away from a watchdog-thread crash.
- Observed environment(s): Outlook desktop (VSTO add-in), .NET Framework, single misbehaving mail store mid-`WrappedMSProvider::Logon`. Ground-truth capture `artifacts/log.txt` at 2026-07-09 14:14:42 → 14:16:39 against repository HEAD (`c9ddbf28` lineage, branch `TaskMaster-wt-2026-07-09T14-19`).
- Customer impact and severity: Startup UI thread unresponsive for the full logon-stall duration on affected profiles; no diagnostic surfaced to the user or logs; a latent process-termination risk on the fresh-build path. Frequency is data-dependent (a specific misbehaving store must be present); severity is high when it occurs.
- First observed date and version(s) impacted: Captured 2026-07-09 on HEAD. This is a new, previously-unhardened instance of the recurring startup COM stall documented in #207, #211, and epic #260. Prior incidents hit the same class of problem at other call sites (`JunkCertain` folder enumeration, `App.Reminders`, `LoadInboxes.GetDefaultFolder`); the raw `Namespace.Stores` enumeration itself was never hardened.

## Repro & Evidence

- Steps to reproduce (real environment): cold-start Outlook with the add-in on a profile that contains a store whose MAPI provider stalls during logon, on the fresh-build path (persisted `StoresWrapper` config missing or deserializes to null). Managed reproduction of the observable contract uses the existing proxy/Moq seams (see Test Strategy); the wall-clock stall itself requires the live affected profile.
- Expected vs actual behavior:
  - Expected: a stall inside the enumeration produces an attributed, crash-safe watchdog action (a `[store-lockup]` WARN line, optional notification), and healthy startups are unchanged.
  - Actual (HEAD): the enumeration blocks with a blank attribution; zero `[store-lockup]` WARN lines are emitted despite the watchdog polling; a naive attribution-parity change would crash the watchdog thread (fresh-build path) or pollute the disabled-store model/UI (rewire path).
- Logs / error snippets (ground truth):
  - `artifacts/log.txt:122` — `WARN TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.` (the deserialized-to-null fresh-build trigger, 14:14:48,000).
  - `artifacts/log.txt:131-135` — first watchdog stack capture (14:14:49,861): `EnumeratorViewOfEnumVariant.MoveNext()` → `StoresWrapper.Init() ... StoresWrapper.cs:line 44` → `BuildFreshStoresWrapper() ... AppOlObjects.StoreLoading.cs:line 33`. Captured line numbers match HEAD exactly.
  - Identical stacks repeat every ~1.8 s to `artifacts/log.txt:6463` (14:16:39,150), followed by the `ContextSwitchDeadlock` MDA text at `:6561-6562` ("unable to transition from COM context 0x7c34c290 to COM context 0x7c34c168 for 60 seconds"). Total observed stall ~111 s, blocked on the FIRST `Next()` before any store yielded.
- Frequency / determinism: data-dependent (requires the specific misbehaving store); deterministic once that store is present at cold start on the fresh-build path.

Reference stack (from `issue.md`):

```
at System.Runtime.InteropServices.CustomMarshalers.EnumeratorViewOfEnumVariant.MoveNext()
at System.Linq.Enumerable.<CastIterator>d__97`1.MoveNext()
at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
at UtilitiesCS.OutlookObjects.Store.StoresWrapper.Init() ... StoresWrapper.cs:line 44
at TaskMaster.AppOlObjects.BuildFreshStoresWrapper() ... AppOlObjects.StoreLoading.cs:line 33
at TaskMaster.AppOlObjects.LoadStoresAsync() ... AppOlObjects.StoreLoading.cs:line 64
at TaskMaster.ApplicationGlobals.LoadOlObjectsPhaseAsync()
at TaskMaster.ApplicationGlobals.LoadSequentialAsync()
```

## Scope & Non-Goals

- In scope:
  - Attribution parity at both `Namespace.Stores` materialization sites — `StoresWrapper.Init()` (line 44) and `RewireOlObjectsAsync` (line 89) — via an ambient `CurrentStoreContext` phase-identity scope, so a stall produces a non-blank attribution.
  - A crash-safe phase-identity branch in `StoreLockupResponder` that emits WARN + optional notify with `autoDisabled: false` and returns without touching the disable-service write path.
  - Deterministic RED-before-GREEN regression coverage using the existing proxy/Moq seams (no live Outlook, no temp files).
- Out of scope / non-goals (see Root Cause Analysis and research §6):
  - The code fix cannot prevent, shorten, cancel, or bound the ~108-111 s STA block once `IEnumVARIANT::Next()` is entered. There is no managed cancellation for a blocked COM call; the call is apartment/context-bound; the stall duration is set by the misbehaving store's MAPI provider logon inside `outlook.exe`, not by add-in code.
  - Environmental remediation of the stalling store (repair/re-sync/remove the account, rebuild the OST) is a human action in the Outlook desktop UI and is the only relief for the stall duration. Non-gating.
  - End-to-end reproduction of the real stall and in-situ confirmation of the WARN/notification require a live affected profile; recorded as post-merge manual validation notes, not acceptance gates.
  - Optional secondary hardening (default-store readiness pre-gate; routing the fresh-build loop through `AddOrRestoreStore` for per-store parity) is deferred unless the planner accepts it under the same causal scope (research §3.4).
- Explicitly excluded approaches (rejected in research §2, do not reintroduce): worker-thread offload of the enumeration; timeout/cancellation wrappers on the blocked `Next()`; indexed `Stores.Count`/`Stores[i]` access; readiness-gating as the primary fix; pre-enumeration detection of the stalling store's identity.

## Root Cause Analysis

- Confirmed root cause (research §1): the fresh-build path materializes `GetFilteredStores().ToList()` at `StoresWrapper.cs:44`, whose first `Cast<>` `MoveNext()` invokes `IEnumVARIANT::Next()` on the raw COM enumerator. When a store is mid-logon, that call blocks the STA before any store is yielded. Because the per-store `CurrentStoreContext.Begin(storeDisplayName)` scope opens only inside `AddOrRestoreStore` (`StoresWrapper.cs:146`) — i.e. after a store is yielded — the watchdog reads `CurrentStoreContext.Current == null` for the whole stall.
- Supporting signals:
  - The watchdog is live in production: `TaskMaster/ThisAddIn.cs:35-40` passes `monitorUiThread: true` and wires `onLockupDetected` to `GetStoreLockupResponder()?.OnLockupDetected(attribution)`.
  - `ThreadMonitor.EvaluatePoll` (`UtilitiesCS/Threading/ThreadMonitor.cs:173-194`) reads `CurrentStoreContext.Current` and fires the callback once per stall episode past the threshold.
  - `StoreLockupResponder.OnLockupDetected` (`UtilitiesCS/Threading/StoreLockupResponder.cs:86-89`) has a hard no-context guard: a blank attribution performs no disable, no notify, and no WARN line — the precise mechanism producing today's silence.
- Latent crash hazard (research §1.3, load-bearing for the fix): a naive attribution-parity change (synthetic identity, no responder change) would, on the fresh-build path, pass the non-blank guard, get `IsDisabled == false` (model null → false, `StoreDisableService.cs:133-142`), then `DisableSessionOnly` → `GetModelForWriteOrThrow()` throws `InvalidOperationException` (`StoreDisableService.cs:64-75`) because `AppOlObjects.StoresWrapper` is assigned only after `Init()` returns. The exception propagates through `_onLockupDetected?.Invoke` (`ThreadMonitor.cs:192`) and `Tick()` (`ThreadMonitor.cs:104-133`, try/finally with no catch) into a threadpool timer callback → unhandled exception → `outlook.exe` process-termination risk. On the rewire path (model non-null), the same path would instead pollute `SessionDisabledStoreIdentities` and the #265 disabled-stores UI with a bogus non-store entry.
- Affected components/paths: `UtilitiesCS/Threading/CurrentStoreContext.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/Threading/StoreLockupResponder.cs`; reads by `UtilitiesCS/Threading/ThreadMonitor.cs` and `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` (not modified).

## Proposed Fix

Minimal, causation-scoped, two-part change plus tests (research §3). The verdict is settled: a pump-preserving fix is NOT achievable; the deliverable is attribution parity via a `CurrentStoreContext` phase identity plus a crash-prevention phase-identity branch in `StoreLockupResponder`.

### Design summary (what changes where):

- Add a phase-identity constant `"<Stores-enumeration>"` to `CurrentStoreContext` (distinct from the special `"<unavailable>"` value, which `Normalize` treats specially).
- Wrap both `Namespace.Stores` materializations in an ambient `using (CurrentStoreContext.Begin(...))` scope via one extracted private helper on `StoresWrapper`, so the watchdog gets a non-blank attribution.
- Add a phase-identity terminal branch to `StoreLockupResponder` that emits WARN (+ optional notify) with `autoDisabled: false` and returns without any `IStoreDisableService` call.

### Boundaries and invariants to preserve:

- Included-store set and enumeration order are unchanged; the scope is observational only.
- `CurrentStoreContext.Current` is null after materialization completes (scope disposed), and nested per-store scopes continue to restore correctly.
- No new public API surface: `StoresWrapper` additions are private/const; `CurrentStoreContext` adds one constant; `StoreLockupResponder` adds one terminal branch. No persistence schema change; no breaking change.
- The `using` scope guarantees restore-on-failure, so a thrown `COMException` inside enumeration cannot leak the phase identity into a later, unrelated attribution.

### Dependencies or blocked work:

- Consumes existing #260/#261/#264/#265 seams (`IApplicationGlobals`, `IStoreDisableService`, `IUiDispatcher`, `StoreLockupNotifier`, `Action<string> logSink`, `ReflectionRealProxy`). No new production seam is required (research §3.3) — the existing `IApplicationGlobals` + proxy patterns suffice. No blocked work.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change (research §5):

1. `UtilitiesCS/Threading/CurrentStoreContext.cs` — add the phase constant, e.g. `public const string StoresEnumerationPhaseIdentity = "<Stores-enumeration>";` (2-4 lines). Home chosen because it is the same assembly as both the writer (`StoresWrapper`) and the reader-consumer (`StoreLockupResponder`), and consistent with the existing angle-bracket convention. The constant MUST NOT be `"<unavailable>"`.
2. `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — extract one private helper (e.g. `MaterializeFilteredStores()`) performing `GetFilteredStores().ToList()` inside `using (CurrentStoreContext.Begin(CurrentStoreContext.StoresEnumerationPhaseIdentity))`, and call it from both `Init()` (line 44) and `RewireOlObjectsAsync` (line 89). Existing Stopwatch/log lines stay as-is.
3. `UtilitiesCS/Threading/StoreLockupResponder.cs` — add the phase-identity branch after the existing blank/unresolved guards.
4. Tests: `TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs` (or a sibling focused test class in the same folder to respect the file-size limit) and `UtilitiesCS.Test/Threading/StoreLockupResponderTests.cs`.

#### Functions/classes/CLI commands impacted:

- `StoresWrapper.Init()` and `StoresWrapper.RewireOlObjectsAsync` (call the new helper instead of materializing inline).
- New private `StoresWrapper.MaterializeFilteredStores()`.
- `StoreLockupResponder.OnLockupDetected` (new terminal phase-identity branch).
- `CurrentStoreContext` (new constant only).

#### Guard ordering rule (research §4 — required):

The `StoreLockupResponder` guards must execute in this exact order:

`blank-guard → unresolved-guard → phase-identity guard → already-disabled guard → disable/notify`

The phase-identity guard must precede any `IStoreDisableService` call (`IsDisabled` / `DisableSessionOnly` / action-button wiring). The phase branch emits WARN with `autoDisabled: false`, optionally dispatches an informational modeless notification, and returns.

#### Data flow and validation changes:

- `CurrentStoreContext.Current` gains one new value class (the phase identity) alongside the existing null / store-DisplayName values. `CurrentStoreContext.Normalize` passes any non-`"<unavailable>"` string through unchanged.

#### Error handling and logging updates:

- New behavior on stall: one `[store-lockup]` WARN line attributed to the enumeration phase with the measured stall duration and `autoDisabled: false`, landing in the existing JSON important-logs appender (no config change). Optional informational modeless notification via `IUiDispatcher.BeginInvoke`. No disable-service write; no exception escapes the watchdog thread.

#### Rollback/feature-flag considerations (if applicable):

- Not applicable. The change is additive and observational; reverting the three source edits restores prior behavior.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- `CurrentStoreContext.Begin(string)` returns an `IDisposable` scope restoring the prior ambient value on dispose (existing contract; reused).
- The `[store-lockup]` line is produced by the existing `StoreLockupAttribution.FormatLine` overload that accepts the `autoDisabled` flag (usage at `StoreLockupResponder.cs:114-118`).

#### Required configuration keys and defaults:

- None. The attribution threshold (`lockupAttributionThresholdMs`, 5000 ms default) is existing #264 config and is unchanged.

#### Backward-compatibility expectations:

- No public API break. `StoresWrapper` members added are private/const; `StoreLockupResponder` gains one terminal branch; `CurrentStoreContext` gains one constant. Existing tests remain the spec.

#### Performance constraints (latency/throughput/memory):

- Healthy-path enumeration is byte-identical in included set and order; the ambient scope adds one field set/restore per materialization and has no measurable cost. The change does not alter the stall duration (out of scope).

## Assumptions, Constraints, Dependencies

- Assumptions: the watchdog is live and wired (confirmed, `ThisAddIn.cs:35-40`); `StoreDisableService` is resolvable before the stall (`ApplicationGlobals.cs:122`, constructed ~14:14:42); the captured stack line numbers match HEAD.
- Constraints:
  - No managed cancellation or timeout exists for a blocking `IEnumVARIANT::Next()`; the enumeration cannot be aborted once entered.
  - Outlook Interop objects are STA-apartment-bound; COM calls cannot be offloaded to a worker thread (they marshal back to the STA).
  - File-size limit: 500 lines. `StoresWrapper.cs` is currently 449 lines; the helper extraction stays under the ceiling. If a test file would exceed 500 lines, add a sibling focused test class in the same folder rather than growing an existing file (research §5).
  - The change is cross-cutting into the #260 resilience system; scope is attributed by causation.
- External dependencies: none added. Existing MSTest + Moq + FluentAssertions; existing proxy/Moq seams.

## Data / API / Config Impact

- User-facing changes: on a stall, an optional informational modeless notification stating that startup store enumeration is stalled (worded as informational, not an auto-disable action). No change on healthy startups.
- Data or migration considerations: none. No persistence schema change; no disabled-store model write on the phase branch.
- Logging/telemetry updates: one new `[store-lockup]` WARN line (phase-attributed, `autoDisabled: false`) where today there is silence.
- Compatibility notes: no CLI flags, config schema, or versioning changes.

## Test Strategy

Verification plan is T1–T5 from research §7. MSTest + Moq + FluentAssertions; no live Outlook; no temporary files; no real waits/timers. All tests use the existing `ReflectionRealProxy` / `Mock<Stores>().As<IEnumerable>()` patterns (research §3.3) and `StubApplicationGlobals`. T1, T2, and T3 MUST be RED on HEAD.

- **T1 (RED on HEAD) — `Init()` attribution parity.** A stores proxy whose `GetEnumerator` returns an enumerator that records `CurrentStoreContext.Current` on each `MoveNext()`. Assert the recorded value equals the phase constant. On HEAD it records null → fails. Primary regression for the captured stack (blocked inside `MoveNext` at `StoresWrapper.cs:44`).
- **T2 (RED on HEAD) — `RewireOlObjectsAsync` attribution parity.** Same observation for the `StoresWrapper.cs:89` materialization, driven through `RewireAfterDeserializeAsync`.
- **T3 (RED on HEAD) — responder phase branch.** `OnLockupDetected(new LockupAttribution(6 s, "<Stores-enumeration>"))` with a `MockBehavior.Strict` `IStoreDisableService`. Assert exactly one WARN line via the injected `logSink` (with `autoDisabled: false` formatting) and zero disable-service calls. On HEAD, `IsDisabled`/`DisableSessionOnly` are invoked → the Strict mock fails.
- **T4 (behavior-preserving, GREEN before and after).** Healthy multi-store enumeration yields the identical included set and order, and `CurrentStoreContext.Current` is null after `Init()` returns (scope disposed).
- **T5 — scope restore on failure.** An enumerator that throws mid-enumeration leaves `CurrentStoreContext.Current` null afterwards.
- Determinism: no clocks in T1/T2/T4/T5; T3's `LockupAttribution` takes an explicit `TimeSpan`. No `FakeTimeProvider` is needed (`ThreadMonitor.EvaluatePoll` is already covered and is not modified).
- Coverage impact and targets: new lines are host-neutral and fully reachable via the proxy seams; they carry the >= 90% new-code obligation with no repository-wide regression.
- Toolchain commands (format → lint → type-check → test), per repository policy:
  1. `csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation (post-merge, non-gating): on a live affected profile, confirm a subsequent real stall now emits the `[store-lockup]` WARN line and the optional modeless notification, and that no watchdog-thread crash occurs. Recorded as validation notes, not acceptance gates.

## Acceptance Criteria

Carried from `issue.md` (revised after research §5). Track each item independently; check off only after implemented and verified.

- [x] **AC1 — Attributed watchdog action at both enumeration sites.** A stall inside the `Namespace.Stores` materialization at `StoresWrapper.Init()` (line 44) and `RewireOlObjectsAsync` (line 89) produces a watchdog action within the attribution threshold: a `[store-lockup]` WARN line attributed to the enumeration phase (and optionally an informational modeless notification), instead of today's total silence (blank attribution). _Delivered: [P2-T1] (Init site) and [P2-T2] (Rewire site) wrap both materializations in the `CurrentStoreContext` enumeration-phase scope; verified by T1/T2 (RED->GREEN, `evidence/regression-testing/red-before-fix` and `green-after-fix`)._
- [x] **AC2 — Non-null phase identity, handled safely (no disable write, no crash).** The stall attribution is a non-null phase identity (`"<Stores-enumeration>"`, distinct from `"<unavailable>"`). `StoreLockupResponder` gains a phase-identity branch that emits WARN + optional notify with `autoDisabled: false` and returns WITHOUT calling `IsDisabled`/`DisableSessionOnly`/the action-button wiring. This closes the verified `InvalidOperationException` watchdog-thread crash path (null model during the fresh-build window) and the #265 disabled-store UI pollution path. _Delivered: [P2-T3] adds the phase-identity terminal branch (guard order blank -> unresolved -> phase-identity -> already-disabled -> disable/notify); verified by T3 with a `MockBehavior.Strict` `IStoreDisableService` asserting zero disable calls._
- [x] **AC3 — Behavior-preserving for healthy stores.** The included-store set and enumeration order are unchanged; `CurrentStoreContext.Current` is null after materialization completes (scope disposed); nested per-store scopes continue to work. _Delivered: the scope is observational only ([P2-T1]/[P2-T2]); verified by T4 (identical included set/order, `Current` null after `Init()`) and T5 (scope-restore-on-failure) in [P1-T5], GREEN before and after._
- [x] **AC4 — Deterministic RED-before-GREEN regression coverage.** Coverage via the existing `ReflectionRealProxy`/`Mock<Stores>().As<IEnumerable>()` seams (no live Outlook, no temp files) covering: attribution observable from inside `MoveNext()` at both sites (RED on HEAD), the responder phase branch with a `MockBehavior.Strict` `IStoreDisableService` asserting zero disable calls (RED on HEAD), behavior preservation, and scope-restore-on-failure. New code meets the >= 90% new-code coverage obligation. _Delivered: RED capture [P1-T7] (EXIT 1, T1/T2/T3 fail), GREEN [P2-T4] (4519/4519 pass); new executable-code coverage 14/14 = 100% (>= 90%) per [P3-T4]/[P3-T5]._

## Risks & Mitigations

- Risk: a naive attribution-parity change crashes the watchdog thread or pollutes the disabled-store model/UI. Mitigation: the phase-identity branch in `StoreLockupResponder` is required and paired with the scope change; guard ordering places the phase guard before any disable-service call (research §1.3, §4).
- Risk: the fix could be misread as preventing the stall. Mitigation: Scope & Non-Goals and Root Cause Analysis state explicitly that the block is not preventable, shortenable, or cancelable in managed code; the deliverable is attributed, crash-safe handling only.
- Risk: test file growth past the 500-line ceiling. Mitigation: add a sibling focused test class in the same folder rather than extending an existing file.
- Risk: indexed access or other call-shape changes reintroduced as "fixes". Mitigation: research §2 rejects these as unfalsifiable against the captured defect; they are listed under explicitly excluded approaches.

## Rollout & Follow-up

- Release/rollout: standard branch delivery; no feature flag. Reverting the three source edits restores prior behavior.
- Post-fix monitoring: watch for `[store-lockup]` phase-attributed WARN lines in the important-logs appender on affected machines.
- Follow-up (deferred, non-gating): optional secondary hardening — default-store readiness pre-gate and routing the fresh-build loop through `AddOrRestoreStore` for per-store parity (research §3.4) — only if a planner accepts the same causal scope. Environmental remediation of the stalling store remains a human action.
- Links: issue #292 (https://github.com/drmoisan/TaskMaster/issues/292); research `research/2026-07-09-outlook-startup-store-enumeration-com-stall-research.md`; related #207, #211, epic #260, #264, #265.
