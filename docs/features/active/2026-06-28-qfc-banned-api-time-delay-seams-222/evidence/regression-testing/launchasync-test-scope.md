# Regression-Testing Scope Exclusion — LaunchAsync catch-block timestamp (P4-T6)

Timestamp: 2026-06-28T20-06

## Production site
`QuickFiler/Controllers/QfcHomeController.cs`, static factory `LaunchAsync`, catch block (site 3):

```
catch (OperationCanceledException) {
    logger.Info(
        $"{controller.TimeProvider.GetLocalNow().LocalDateTime.ToString("mm:ss.fff")} "
            + $"{nameof(QfcHomeController)}.{nameof(LaunchAsync)} was cancelled");
    ...
}
```

## Why a deterministic, COM-free test is not feasible (binary outcome: scope-exclusion dossier)
P4-T6 is explicitly optional with a documented-exclusion fallback. A deterministic test of this
catch block would have to force `OperationCanceledException` out of `controller.InitAsync(...)` /
`controller.RunAsync(...)` while avoiding live Outlook COM. The cancellation would have to originate
from one of the controller's injectable loader seams (e.g., `QfcAsyncDataModelLoader`). However,
`LaunchAsync` constructs the controller internally via the private parameterless constructor
(`var controller = new QfcHomeController();`) and only then calls `InitAsync`. There is no injection
point for the loader seams before `InitAsync` runs — the only external injection point added by this
change is the optional `TimeProvider timeProvider` parameter. With the default loaders in place,
`InitAsync` invokes the real `QfcDatamodel.LoadAsync`, which requires a live Outlook
`Application`/`Explorer` (COM). Therefore the OCE path cannot be triggered deterministically without
live COM, which the unit-test policy prohibits.

## Coverage of the underlying change
- The seam swap at this site (DateTime.Now -> controller.TimeProvider.GetLocalNow().LocalDateTime)
  is verified by the banned-API sweep (P3-T7) and the analyzer/nullable builds (P3-T9, P5-T2/T3).
- The injected-clock formatting behavior ("mm:ss.fff" and the GetLocalNow().LocalDateTime pattern) is
  exercised by the WriteMetricsAsync/QuickFileMetrics_WRITE timestamp tests (P4-T1, P4-T2), which use
  the same `TimeProvider.GetLocalNow().LocalDateTime` mechanism via `Mock<TimeProvider>`.
- The optional `timeProvider` parameter on `LaunchAsync` is exercised at compile time by all existing
  callers (RibbonController) and defaults to `TimeProvider.System`, preserving production behavior.

## Conclusion
No deterministic COM-free test added for the LaunchAsync catch block. This documented exclusion is
the accepted P4-T6 outcome; the production seam swap remains covered by the sweep and the shared
GetLocalNow timestamp tests.
