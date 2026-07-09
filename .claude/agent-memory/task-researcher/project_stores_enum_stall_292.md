---
name: stores-enum-stall-292
description: "Issue #292 research: Namespace.Stores enumeration stall has blank watchdog attribution AND a latent watchdog-thread crash path; RED-on-HEAD tests possible via existing Stores proxy seams (no new production seam)"
metadata:
  type: project
---

Researched 2026-07-09 for issue #292 (startup `Namespace.Stores` enumeration blocks STA ~111 s at
`StoresWrapper.cs:44`/`:89`, first `IEnumVARIANT::Next()`, before any store is yielded). Research doc:
`docs/features/active/2026-07-09-outlook-startup-store-enumeration-com-stall-292/research/2026-07-09-outlook-startup-store-enumeration-com-stall-research.md`.

**Why:** the non-obvious findings below shape any future work touching the #260 pipeline or
StoresWrapper, and one is a crash hazard easy to reintroduce.

**How to apply:**

1. **Latent crash chain — do not add non-store identities to `CurrentStoreContext` without a
   responder guard.** `StoreLockupResponder.OnLockupDetected` → `DisableSessionOnly` →
   `StoreDisableService.GetModelForWriteOrThrow()` throws `InvalidOperationException` whenever
   `AppOlObjects.StoresWrapper` is still null (true for the whole fresh-build stall window), and
   `ThreadMonitor.Tick()` has try/finally with NO catch → unhandled exception in a TimeProvider
   timer callback → outlook.exe termination risk. Any attribution-parity change must add a
   phase-identity branch in the responder that skips all disable-service writes.

2. **The stalling store's identity is structurally unknowable pre-yield.** Readiness gate is
   default-store-only (`OutlookReadinessGate.IsReady()`); store-scoped `IsReady(Store)` needs a
   Store object obtainable only via the enumeration that blocks; DisabledStoreIdentities filtering
   runs per YIELDED store; fresh-build precondition means persisted config is absent anyway. Only a
   coarse phase identity (e.g. `"<Stores-enumeration>"`) is achievable; auto-disable cannot recover.

3. **RED-on-HEAD tests against COM enumeration need no new production seam.** `StoresWrapper`
   takes `IApplicationGlobals`; `TaskMaster.Test\OutlookObjects\Store\StoresWrapperTests.cs:359-401`
   has `ReflectionRealProxy` builders for `IOlObjects`/`NameSpace`/`Stores` with a controllable
   `GetEnumerator`, and `AppOlObjectsCoverageTests.cs:251-261` shows the
   `Mock<Stores>().As<IEnumerable>()` variant. A test enumerator can observe
   `CurrentStoreContext.Current` from inside `MoveNext()` — the exact instant of the production
   stall.

4. `ThreadMonitor` is now LIVE in production (`ThisAddIn.cs:36`) — supersedes the dormant claim in
   [[project-store-lockup-resilience-f4-research]]. Blank attribution is why the log shows zero
   `[store-lockup]` lines during the 111 s stall despite continuous watchdog stack captures.
