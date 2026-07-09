---
name: store-lockup-watchdog-null-model-hazard
description: The #260 store-lockup watchdog is LIVE in production and its responder crashes the watchdog thread if attribution fires before the store model is assigned
metadata:
  type: project
---

The #260 store-lockup resilience system (`ThreadMonitor` + `CurrentStoreContext` + `StoreLockupResponder`) is wired live in production (`TaskMaster/ThisAddIn.cs`, `monitorUiThread: true`). Any future change that opens a `CurrentStoreContext.Begin(...)` attribution scope around a startup COM call must account for two hazards, both verified during #292 / PR #294:

1. **Null-model crash path.** If the responder runs its normal disable path while `AppOlObjects.StoresWrapper` is still null (the entire fresh-build window, before `Init()` returns), `StoreDisableService.GetModelForWriteOrThrow()` throws `InvalidOperationException`. `ThreadMonitor.Tick()` has NO catch, so the exception escapes a `TimeProvider` timer callback on a threadpool thread — a watchdog-thread/process-termination risk on .NET Framework.
2. **Disabled-store UI pollution.** On the rewire path (model non-null), the same disable path "succeeds" and writes a bogus synthetic identity into `SessionDisabledStoreIdentities` / the #265 disabled-stores settings UI.

**Why:** the responder's blank-attribution guard only silences a `null` identity; a non-null synthetic phase identity (e.g. `"<Stores-enumeration>"`) passes the guard and reaches the disable-service write. The fix pattern (PR #294): give the responder a phase-identity terminal branch, ordered `blank → unresolved → phase-identity → already-disabled → disable/notify`, that emits a WARN (`autoDisabled: false`) and RETURNS before any `IStoreDisableService` call.

**How to apply:** when hardening a NEW startup COM call site for the recurring store-logon stall (the #207/#211/#260 class), attribution parity is the achievable relief (the block itself cannot be cancelled — no managed cancellation for a blocked `IEnumVARIANT::Next()`), but it MUST be paired with a responder branch that skips disable-service writes for the phase identity. Relates to [[feedback_vsto_startup_sta_threading_directive]] and [[feedback_verify_repro_before_bugfix_cycle]].
