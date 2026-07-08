# Gated MSTest Failure — Pre-existing AppEventsTests Conflicts With Spec-Mandated Hook() Redesign

Timestamp: 2026-06-22T15-14

Command:
```
vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage \
  /Settings:TaskMaster.runsettings /TestCaseFilter:"TestCategory!=LiveOutlook" \
  /Logger:"trx;LogFileName=p6t4-gated.trx"
```

EXIT_CODE: 1

## Summary

- The gated run COMPLETED in ~3.0s with NO HANG. The original `NonBlockingDelay` hang
  regression (P1-T4/P3-T2 rework) is RESOLVED: the `AppEvents` retry-delay tests now terminate
  under the pump-less MSTest host.
- Result: Total 117, Passed 116, Failed 1.
- The single failure is `TaskMaster.Test.AppGlobals.AppEventsTests.LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs`.

## Failure detail

```
Moq.MockException: IOlObjects.App invocation failed with mock behavior Strict.
All invocations on the mock must have a corresponding setup.
   at Castle.Proxies.IOlObjectsProxy.get_App()
   at TaskMaster.AppEvents.Hook() in ...\AppEvents.cs:line 173
   at TaskMaster.AppEvents.<LoadAsync>d__5.MoveNext() in ...\AppEvents.cs:line 75
```

## Root cause (design conflict, not a defect in the delay-helper rework)

This is a conflict between the spec-mandated Hook() redesign and a PRE-EXISTING test that
encodes the SUPERSEDED synchronous Hook() behavior.

1. At branch HEAD (committed), `AppEvents.Hook()` performed the three readiness-dependent COM
   hookups SYNCHRONOUSLY at default settings and emitted the "Hook complete | startup hook"
   log line synchronously, and did NOT read `Globals.Ol.App`. The test
   `LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs` therefore PASSED at HEAD: it
   asserts the ordering `LoadAsync startup hook dispatch < Hook start < Hook complete <
   LoadAsync startup hook complete`.

2. The corrective fix (task P2-T2, implemented and checked off by a prior run) changes
   `Hook()` per AC1/AC2/AC3 so the three hookups are NO LONGER run synchronously. `Hook()`
   now (a) reads `Globals.Ol.App` to construct `OutlookReadinessGate`, and (b) defers
   `PerformReadinessHookup()` (which emits "Hook complete") to a `DispatcherTimer` poll driven
   through `HookReadinessCoordinator`, running it exactly once when the gate reports ready.

3. Under the pump-less MSTest host, this redesign breaks the pre-existing test in two ways:
   - immediate: the strict `Mock<IOlObjects>` from `CreateGlobalsWithHookableOutlookObjects()`
     has no setup for `App`, so the new `Globals.Ol.App` read throws `MockException`;
   - structural: even with an `App` setup, the `DispatcherTimer` never ticks without a running
     Dispatcher, so "Hook complete" would never be emitted and the ordering assertion would
     still fail. The test fundamentally asserts the synchronous "Hook complete" emission that
     the spec deliberately removed.

## Scope-lock collision

`TaskMaster.Test/AppGlobals/AppEventsTests.cs` is NOT in the plan's scope-lock list (plan
items 12, 12a, 13, 14, 15 enumerate only the new/removed test files). The plan states:
"No other production or test file may be modified. If the work cannot be completed within
these files ... STOP and report rather than widening scope."

Resolving this failure requires modifying `AppEventsTests.cs` (to set up `IOlObjects.App` and
to update or remove the now-superseded synchronous "Hook complete" ordering assertion so it
reflects the coordinator-driven deferred hookup). That file is out of scope-lock, and the
delegation constraint explicitly says: "If the gated run still fails ... STOP and report
precisely — do not weaken tests or relax assertions."

## Verdict

BLOCKED at P6-T4. The delay-helper rework is correct and the hang is resolved (116/117 pass,
no hang). The remaining failure is a pre-existing out-of-scope test asserting the superseded
synchronous Hook() behavior that AC1/AC2/AC3 deliberately changed. A plan revision is required
to add `TaskMaster.Test/AppGlobals/AppEventsTests.cs` to the scope-lock and a task to update
`LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs` to the coordinator-driven design
(set up `IOlObjects.App`; assert the deferred hookup contract rather than the removed
synchronous "Hook complete" ordering). I did not modify the test or weaken any assertion.

TRX: TestResults/p6t4-gated.trx
Coverage attachment: TestResults/07bf953e-50bb-4c3d-b79e-0029ad3e48b4/...coverage
