# Issue update mirror — issue #584

Timestamp: 2026-09-03T22-24

> **Filename note.** This file's name carries the plan's timestamp `2026-09-02T09-02`, while the
> `Timestamp:` field above records the posting instant `2026-09-03T22-24`. The two differ. This file
> is committed evidence, so it is deliberately neither renamed nor re-stamped: changing either
> would rewrite a record of what was posted and when. A future update to issue #584 must use its own
> posting timestamp in its filename, so that the two artifacts sort in posting order and cannot
> collide on a shared name.

PostedAs: comment

Comment URL: https://github.com/drmoisan/TaskMaster/issues/584#issuecomment-5534846382

`gh` was available and authenticated, so posting was not blocked. The exact text posted is reproduced
verbatim below.

---

## Fix implemented — `UiThread.Dispatcher` null-dispatcher guard

`UiThread.Dispatcher`'s getter now throws a named `InvalidOperationException` when its backing field has not been captured, instead of returning `null` and leaving the consumer to fail later with an unattributed `NullReferenceException`. The `null!` null-forgiving suppression is removed and the backing field is redeclared `private static Dispatcher? _dispatcher;`, so the nullable analyser verifies the guard rather than being suppressed around it.

### Changes

| File | Change |
|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | Guarded getter; backing field retyped to `Dispatcher?`; `null!` removed |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | New `UiThread_Dispatcher_Tests` class with two deterministic regression tests |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `[DoNotParallelize]` added (writer of the process-global static) |
| `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `[DoNotParallelize]` added |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `[DoNotParallelize]` added |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | Reflective setup/teardown snapshot retargeted from the public `Dispatcher` property to the private `_dispatcher` backing field |

`UtilitiesCS/Threading/ProgressTrackerAsync.cs` is unmodified. Its `UiDispatcher = UiThread.Dispatcher;` statement now raises the named exception at the assignment, before the first dereference, so the fix in `UiThread.cs` alone converts the downstream failure into a self-diagnosing one.

### Verification

- Regression test: `Failed: 1` against the unfixed accessor, `Passed: 2` against the fixed accessor. No sleep, retry, or timing tolerance anywhere in the diff.
- `csharpier check .` — exit 0, 1576 files checked, no unformatted path.
- Analyzer build (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) — exit 0, 0 warnings, 0 errors.
- Nullable build (`/t:Rebuild`, `TreatWarningsAsErrors`) — exit 0, 0 errors.
- `UtilitiesCS.Test` — 4787 of 4787 passed (baseline 4785, plus the two new tests).
- `QuickFiler.Test` — 1312 of 1312 passed (baseline 1312).
- Coverage: baseline line-rate 0.70733, post-change 0.70736; `lines-valid` delta +42. Changed-line coverage 100% (8 of 8 coverable added lines).

All seven acceptance criteria in the feature spec are checked off against evidence artifacts recorded under the feature folder.
