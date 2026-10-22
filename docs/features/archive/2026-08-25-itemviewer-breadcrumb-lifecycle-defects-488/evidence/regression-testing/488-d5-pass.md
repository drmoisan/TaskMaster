# D5 — Pass-After Evidence ([P5-T4])

Timestamp: 2026-08-28T05-52

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.05. Warning count unchanged from the Phase 0
baseline. The `/p:Platform=AnyCPU` substitution is the documented deviation recorded in full in
`488-d1-fail.md`.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException|FullyQualifiedName~DisposedCoordinator_SetBridgeCoordinatorThrows" "/Logger:trx;LogFileName=488-d5-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p5-t4-d5-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` | **Passed** |
| `DisposedCoordinator_SetBridgeCoordinatorThrows` | **Passed** |

Total tests 2, Passed 2, **Failed 0**. `Test Run Successful.`

## What changed against [P5-T2]

The D5 test moved from **Failed** to **Passed**. `[P5-T2]` recorded "no exception was thrown" — the
call completed normally against a disposed viewer, creating a container and adding a resource owner
after teardown had begun. After `[P5-T3]`'s fix, `EnsureBreadcrumbResourceOwnership` throws
`ObjectDisposedException` when the viewer reports `IsDisposed` or `Disposing`, before any container is
created and before any `BreadcrumbResourceOwner` is added.

Both of the test's assertions hold:

- The thrown type is `ObjectDisposedException`, asserted by name rather than by its base
  `InvalidOperationException`, so the D4 affinity guard's own throw could not have satisfied it.
- `scope.Viewer.BreadcrumbCoordinator` is **null** afterwards, which pins that no pipeline was built
  against the dead viewer. The throw happens inside `EnsureBreadcrumbResourceOwnership`, which
  `EnsureBreadcrumbLifecycle` calls before constructing the lifecycle coordinator, so the exception
  propagates out of `InitializeBreadcrumbPipeline` before `BreadcrumbCoordinator` is ever assigned.

## The throw is attributable to the disposal guard alone

`ThrowIfOffUiBoundary` was already live in this member from `[P4-T5]` and did not fire: the test
disposes the viewer and makes the call on the same ambient context the `ViewerScope` installed before
constructing it, so `SynchronizationContext.Current` is reference-equal to the captured
`UiSyncContext` and the affinity guard returns without effect. `[P5-T2]` confirmed this empirically by
observing no exception at all rather than an `InvalidOperationException` from the affinity guard.

## `DisposedCoordinator_SetBridgeCoordinatorThrows` is the precedent D5 mirrors

That existing test pins the `ObjectDisposedException` contract on
`BreadcrumbItemViewerLifecycleCoordinator` after its own disposal. It stays green, which confirms D5
introduces a matching contract at the `ItemViewer` layer rather than a conflicting one, and that the
coordinator's own disposal behaviour is unchanged by this feature.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p5-t4-d5-pass/488-d5-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, both named tests `Passed`.
`InitializeBreadcrumbPipeline` now throws `ObjectDisposedException` against a disposed viewer where
`[P5-T2]` observed the call succeeding, and `BreadcrumbCoordinator` remains null.
