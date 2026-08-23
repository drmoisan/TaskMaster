# Popup UI-boundary composition test-hang diagnostic

Timestamp: `2026-07-22T07:57Z`

## Exact composition attempt

The P5-T66 VSTest command used `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, `/InIsolation`, detailed console logging, and the exact nine-class filter specified by the plan. The process remained active for `91.7` seconds after reporting 19 passing cases. The last reported case was `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`; the next source-ordered case was `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`.

Only the verified workspace process was terminated:

- PID: `31620`
- Image: `vstest.console.exe`
- Command line contained the current worktree, QuickFiler test assembly, and exact P5-T66 filter.
- Exit after termination: `-1`

This attempt is nonpassing diagnostic evidence and does not satisfy P5-T66.

## Isolation

Three isolated reruns bounded the failure to a timing-sensitive test-harness continuation capture rather than production behavior:

| Filter | Result |
| --- | --- |
| `BreadcrumbPopupControlDispatchTests` | 13 passed, 0 failed, 0 skipped in 1.4034 seconds |
| `BreadcrumbSelectorToggleUiBoundaryTests|BreadcrumbPopupControlDispatchTests` | 17 passed, 0 failed, 0 skipped in 1.4842 seconds |
| `BreadcrumbBridgeCoordinatorProbabilityTests|BreadcrumbUiThreadDispatchTests|BreadcrumbSelectorToggleUiBoundaryTests|BreadcrumbPopupControlDispatchTests` | 29 passed, 0 failed, 0 skipped in 1.5742 seconds |

`SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface` awaited an asynchronously completing factory task without `ConfigureAwait(false)`. When a non-null ambient test or WinForms synchronization context was present and the task did not complete before the await, the test continuation could be posted to an unpumped context while VSTest synchronously waited for the test. This explains both the hang and the passing isolation runs when the timing completed the task before continuation capture.

## Bounded correction

The authorized correction adds `.ConfigureAwait(false)` to that one test factory await. It does not change production code, assertions, filters, packages, project configuration, settings, or coverage configuration. P5-T63 through P5-T65 were reopened so the formatter, analyzer, and nullable sequence restarts before the exact nine-class test gate.
