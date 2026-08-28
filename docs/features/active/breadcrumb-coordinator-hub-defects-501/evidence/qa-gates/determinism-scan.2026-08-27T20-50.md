# QA Gate — Determinism Scan (P6-T2, AC-27)

Timestamp: 2026-08-27T20-50

## Command and complete output

Command:

```
git grep -n -E 'Thread\.Sleep|Task\.Delay|DateTime\.Now|DateTime\.UtcNow|new Thread\(|Path\.GetTempFileName|Path\.GetTempPath' -- QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
```

EXIT_CODE: 1 (`git grep` exits 1 when there are no matches)

Complete output: **empty. Zero matched lines.**

## Tracked-path proof (no path was silently skipped)

`git grep` searches TRACKED files only, so an untracked path would produce zero matches whatever it
contained. Each of the five scanned paths was therefore confirmed tracked.

Command: `git ls-files -- <the same five paths>`

Output — all five paths returned:

```
QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs
QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs
QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs
```

| Scanned path | `git ls-files` returned it |
| --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | yes |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | yes |

The two files this feature CREATED (`BreadcrumbBridgeCoordinatorSupersessionTests.cs` and the production
part) were made visible to `git grep` by `git add -N`, which adds an index entry without staging content.
The zero-match result is therefore a real measurement over all five files, not an artefact of
untracked-file skipping.

## Ordering mechanism of every new test method

Ten new test methods, none of which uses `Thread.Sleep`, `Task.Delay`, a real wall-clock wait, a
temporary file, or a second thread for ordering.

| Task | Test method | Ordering mechanism |
| --- | --- | --- |
| P1-T1 | `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync` | explicit synchronization-context drain (`CapturingSynchronizationContext.DrainUntil` / `DrainAll`) plus injected `ControlledHost.Enqueue` / `SetOpen` delegates |
| P1-T3 | `CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce` | explicit synchronization-context drain (`DrainUntil` / `DrainAll`) plus injected host delegates |
| P3-T1 | `TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync` | reflected `Monitor.IsEntered` probe, read from inside an injected delegate |
| P3-T3 | `TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation` | injected re-entrant delegate (the guarded action calls `Invalidate()` itself) |
| P4-T1 | `RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource` | injected delegate; supersession driven synchronously by `BeginPopulation()` then `Invalidate()` on the calling thread |
| P4-T7 | `RunSynchronous_SupersededLeaseReportsSkipToCaller` | injected delegate, same synchronous supersession, asserted in both directions |
| P4-T8 | `SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade` | explicit synchronization-context drain (`DrainAll`) plus a gating `TaskCompletionSource` that is deliberately NEVER completed, so the pending task is pending by construction rather than by timing |
| P5-T1 | `PostJson_SurfaceFailureDoesNotStarveOtherSurfacesOrFalsifyReplayCache` | injected delegates: two `CountingThrowingMessenger` fakes whose `PostJson` invokes an injected `Action` before throwing |
| P5-T3 | `PostJson_SurfaceInvocationRunsAfterHubLockIsReleased` | reflected `Monitor.IsEntered` probe, read from inside an injected Moq `Callback` |
| P5-T4 | `PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified` | injected re-entrant delegate (a Moq `Callback` that calls `hub.Attach` re-entrantly) |

Every mechanism is one of the three the task authorizes: an injected delegate, a reflected
`Monitor.IsEntered` probe, or an explicit synchronization-context drain.

Two points worth stating explicitly, because they are where a determinism violation would most plausibly
have crept in:

- The P4-T8 gating `TaskCompletionSource` is never completed and never waited on. It makes
  `SuggestionsUpgrade` incomplete as a matter of construction. No wait, no timeout, no polling.
- `Monitor.IsEntered` reports whether the CURRENT thread holds the lock. Both lock probes therefore need
  no second thread and no synchronization primitive; they are exact on one thread.

Acceptance: zero matched lines; per-test ordering mechanism stated for all ten new test methods; all
five scanned paths confirmed tracked. PASS (AC-27).
