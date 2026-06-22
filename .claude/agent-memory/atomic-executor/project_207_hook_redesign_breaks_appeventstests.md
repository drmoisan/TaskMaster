---
name: 207-hook-redesign-breaks-appeventstests
description: #207 readiness-gate Hook() redesign breaks pre-existing AppEventsTests (out of scope-lock); test asserts superseded synchronous Hook-complete ordering
metadata:
  type: project
---

The #207 corrective-fix Hook() redesign (readiness-gated, DispatcherTimer-poll, coordinator-driven deferred hookup) breaks a PRE-EXISTING test that is NOT in the plan's scope-lock: `TaskMaster.Test/AppGlobals/AppEventsTests.cs` → `LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs`.

**Why:** At HEAD, `AppEvents.Hook()` ran the three COM hookups synchronously and emitted "Hook complete | startup hook" synchronously, never reading `Globals.Ol.App`; the test passed. The fix (task P2-T2) now reads `Globals.Ol.App` to build `OutlookReadinessGate` (strict `Mock<IOlObjects>` has no `App` setup → `MockException`) and defers `PerformReadinessHookup()` (which emits "Hook complete") to a `DispatcherTimer` that never ticks in the pump-less MSTest host. So the test fails two ways: immediate strict-mock throw, and structural — the deferred "Hook complete" never fires without a Dispatcher.

**How to apply:** The plan's scope-lock (items 12/12a/13/14/15) does not list `AppEventsTests.cs`. Completing the gated run requires a plan revision to (a) add `AppEventsTests.cs` to scope-lock and (b) add a task to set up `IOlObjects.App` and replace the superseded synchronous "Hook complete" ordering assertion with the coordinator-driven deferred-hookup contract. The delay-helper rework itself ([[project_dispatcherdelay_hangs_unit_tests]]) is correct: the gated run completes in ~3s with no hang (116/117 pass). Do not weaken the test or revert the spec-mandated AC1/AC2/AC3 behavior to make it green.
