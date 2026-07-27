# Selector transition publication failure root cause

- Timestamp: 2026-07-27T05:41:34Z
- Scope: Read-only follow-on diagnosis of the sole P8-T75 aggregate failure. No source, test, project, coverage, settings, filter, exclusion, threshold, or postprocessor file was changed.
- Result: `ROOT_CAUSE: CLASSIFIED`

## Commands

```powershell
[xml]$trx = Get-Content -LiteralPath 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/member-coverage-bridge-stale-aggregate-blame.2026-07-27T05-34.trx' -Raw
$result = $trx.SelectSingleNode("//t:UnitTestResult[contains(@testName,'TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased')]", $namespaceManager)

Select-String -LiteralPath 'QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs' -Pattern 'TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased|AssertRouterAvailable' -Context 5,18
Select-String -LiteralPath 'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs' -Pattern 'AddItems|SelectRow|ApplyTransition' -Context 3,12
Select-String -LiteralPath 'UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs' -Pattern 'private readonly object _sync|Mutate' -Context 3,12
Select-String -LiteralPath 'QuickFiler/Viewers/BreadcrumbUiDispatcher.cs' -Pattern 'Dispatch|CreateForCurrentThreadTests' -Context 3,12
```

EXIT_CODE: 0

## Output Summary

The sole P8-T75 failure was:

`QuickFiler.Test.Viewers.BreadcrumbSelectorCoordinatorTests.TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased`

The failing TRX records a duration of `00:00:02.1230922`, with `posts == 0` at `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:172`. The same test passed in both P8-T67 aggregate TRXs with durations of `00:00:01.1340739` and `00:00:00.2742372`.

The test's `AssertRouterAvailable` helper submits `Task.Run(() => coordinator.GetFolderItems())` and waits up to one second. The publication and selection callbacks each call that helper before incrementing their counters. P8-T75 verbose diagnostics show 23 other class-level tests active when this test began. With this test, all 24 worker slots selected by `Workers=0` and `Parallelize Scope=ClassLevel` were occupied. Both one-second probes can therefore remain queued until their waits time out. `BreadcrumbUiDispatcher` catches and reports each callback assertion exception, so neither callback reaches its counter increment and the final assertion reports the secondary zero-post result.

The two-second failure duration, active-worker evidence, and prior variable durations classify the failure as scheduler-dependent test-helper behavior under class-level thread-pool saturation. A cross-thread dispatcher rejection would return immediately and does not match the observed duration.

Production source review confirms that router transitions finish before publication:

- `BreadcrumbBridgeCoordinator.AddItems` obtains the router transition before dispatching.
- `BreadcrumbBridgeCoordinator.SelectRow` obtains its transition before `ApplyTransition` dispatches.
- `FolderBreadcrumbBridgeRouter.Mutate` returns from `lock (_sync)` before coordinator publication.

The corrected stale-lease test began after the failing transition-publication test ended and passed in less than one millisecond. It does not share coordinator, lifetime, messenger, or scheduler state with the failing test and is not causal.

## Deterministic correction boundary

Modify only `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs`.

Replace the scheduler-dependent `Task.Run` plus one-second wait probe with direct lock-state observation:

1. Resolve `BreadcrumbBridgeCoordinator._router` and `FolderBreadcrumbBridgeRouter._sync` by reflection during Arrange.
2. In each inline publication/event callback, record `System.Threading.Monitor.IsEntered(routerSync)` and increment the existing counter without throwing.
3. After `AddItems` and `SelectRow`, assert that no callback observed the router lock held.
4. Retain the exact `posts == 2` and `selections == 1` assertions.
5. Remove `System.Threading.Tasks` if no longer used and add `System.Threading` only if required.

This assertion directly tests the lock-release contract because C# `lock (_sync)` uses `Monitor` and the owner-only test dispatcher executes these callbacks inline. It adds no wait, timeout, retry, sleep, parallelization exclusion, filter change, or production seam. The test file is 424 lines before correction and remains below the 500-line repository limit.
