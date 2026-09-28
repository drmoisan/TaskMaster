# Bug: Sibling breadcrumb dispatcher tests share the Task.Run distinct-thread assumption fixed in #900 (Issue #905)

- Work Mode: full-bug
- Reported: 2026-09-17
- Source: run `bugs-2026-09-17`, found during item 900's delivery (PR #904)

- Issue: #905
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/905
- Last Updated: 2026-09-17
- Status: Promoted -> docs/features/active/Bug_Sibling_breadcrumb_dispatcher_tests_share_the_TaskRun_distinct-thread_assumption_fixed_in_900/ (Issue #905)
## Summary

Issue #900 fixed two tests that obtained a "worker thread" from `Task.Run` and asserted a
thread-identity property against it. `Task.Run` guarantees only *a* thread-pool thread, never a
*different* one, so under parallel execution the constructing thread can be the same pooled thread
and the guard under test is never exercised.

**The same assumption remains in sibling tests that were out of #900's declared scope**, including a
third test in the very file #900 modified, at line 332.

## Scope

1. **`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` line 332** — the
   out-of-scope third test in the file #900 corrected. Same pattern, untouched.
2. **Sibling breadcrumb dispatcher tests** carrying the same `Task.Run`-derived worker-thread
   assumption. Enumerate them rather than assuming the two named here are the full set.

## The fix that works, from #900

Replace `Task.Run(...).GetAwaiter().GetResult()` with a dedicated `Thread` the test creates and
joins, plus an in-delegate assertion that `CheckAccess()` is false **before** the guarded call. That
makes the scheduling property *controlled* rather than tolerated.

**Do not** serialise the tests, add `[DoNotParallelize]`, pin thread counts, add retries, or widen
tolerances. Tests must always run in parallel in this repository; a suite that needs serial execution
has already violated unit-test isolation, and the failing tests are the defect. `TaskMaster.cli.runsettings`
must remain byte-identical.

## Acceptance — non-vacuity must be proven, not asserted

#900 established the bar and it should be met here too: prove the assertion is non-vacuous by
mutation, with **each mutation failing on its pre-predicted assertion**. A thread-affinity test that
has never been observed failing does not demonstrate the guard works — it can pass while the guard is
never reached at all, which is what made the original defect invisible.

Per the lesson recorded on #895, any criterion adopted must be observed FAILING before acceptance.

## Constraint the fix must respect

`ItemViewerBreadcrumbThreadAffinityTests.cs` currently sits at **490 lines against the repository's
500-line file ceiling**. The #900 fix pattern adds lines per test, so correcting the remaining tests
in place will breach the ceiling. Plan the split as part of this item rather than discovering it at
the QA gate.

## Related caveat, recorded so it is not lost

The null-owner test's discrimination remark holds **only in the thread-stolen case**. That narrows
what the test actually establishes and should be stated accurately in the test's own documentation
rather than left implying broader coverage.

## Related

- **#900** — the two tests already fixed, and the working fix pattern.
- **#781** — the breadcrumb UI boundary guard these tests protect; dispatcher operations install a
  throwaway `DispatcherSynchronizationContext`, so context reference-equality guards can behave
  unexpectedly on the UI thread.
- **#743** — exists because pump-hosted QuickFiler tests expire under CPU contention. Same area, same
  pressure.

## Next step

Triage and schedule. Not urgent — these tests are not currently failing — but they are failing to
*test*, which is the more expensive condition because it is silent.
