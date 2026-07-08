# P4-T1 — SubjectMapSco.Consume S4 Hook Disposition

Timestamp: 2026-06-09T11-31

Disposition: NO FURTHER PRODUCTION CHANGE (existing #181 per-item hook is sufficient).

Rationale:
- `SubjectMapSco.Consume<T>` (UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs,
  lines 47-87) already contains a #181 per-item reporting hook: the `WithProgressReporting(count, (x) => { completed = x; progress.Report(x, ...); })` callback reports progress synchronously for
  every consumed item, in addition to the retained `System.Threading.Timer`.
- `IEnumerableExtensions.WithProgressReporting` invokes its `Action<int> progress` callback once per
  enumerated item (foreach -> yield -> progress(...)). Therefore, consuming a 3-item sequence produces
  one initial report plus one report per item — at least 3 reports — synchronously during enumeration,
  independent of the wall-clock 500 ms timer.
- The plan's P4-T1 explicitly permits a no-further-change disposition when the existing hook is
  sufficient (executor note S4). Adding a duplicate `Action<int> onItemCompleted` parameter would be
  redundant: the per-item `progress.Report` already gives the G1 test (P4-T5) a deterministic >= 2
  reports without any wall-clock wait.

Conclusion: The existing per-item `progress.Report` in `Consume` is the S4 hook for G1. P4-T5 removes
the test's `Thread.Sleep(20)` per item and the `SpinWait.SpinUntil(..., 1000)` and asserts directly on
the synchronously-accumulated `tracker.Reports`. No production edit to `Consume` is made this task.
