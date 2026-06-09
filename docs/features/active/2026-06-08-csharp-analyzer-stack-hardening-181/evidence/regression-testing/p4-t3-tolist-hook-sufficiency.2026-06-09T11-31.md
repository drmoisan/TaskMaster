# P4-T3 — ToList Hook Sufficiency for F2

Timestamp: 2026-06-09T11-31

Disposition: PHASE 3 `ToList` HOOK IS SUFFICIENT — no further production change; `WithProgressReporting`
does NOT need a separate hook.

Rationale:
- The F2 test `ToList_InternalHelper_ConsumesEnumerableAndReportsProgress` invokes the internal
  `IEnumerableExtensions.ToList<T>` directly via reflection. The deterministic assertion it needs is
  `tracker.Reports.Should().Contain(report => report.Value > 0)`, which at baseline depended on the
  500 ms `System.Threading.Timer` firing while `completed > 0` (the wall-clock dependency).
- The Phase 3 `ToList` hook (`Action<int> onItemCompleted = null`, invoked per consumed item inside the
  existing `WithProgressReporting` callback) is the production seam. The F2 test passes an
  `onItemCompleted` delegate that calls `tracker.Report(percent, "...of 3")` per item, producing a
  deterministic `Value > 0` report without the wall-clock timer.
- `WithProgressReporting` itself does not require a hook: the test exercises `ToList` (which composes
  `WithProgressReporting` internally), and the `onItemCompleted` parameter on `ToList` is the single
  injection point that makes the F2 assertions deterministic.

Reflection-signature note (handled in P4-T4): because `ToList` now has a 4th optional parameter,
the F2 test's reflection `Invoke` (previously a fixed 3-element object[]) must pass the
`onItemCompleted` delegate as the 4th argument; reflection does not auto-apply optional defaults.

Conclusion: No production edit in P4-T3. The Phase 3 `ToList` hook suffices for F2.
