# D5 — Fail-Before Evidence ([P5-T2]) `[expect-fail]`

Timestamp: 2026-08-28T05-51

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:02.13. The `/p:Platform=AnyCPU` substitution is
the documented deviation recorded in full in `488-d1-fail.md`.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException" "/Logger:trx;LogFileName=488-d5-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p5-t2-d5-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` | **Failed** |

Total tests 1, Failed 1. `Test Run Failed.`

## The call SUCCEEDED against a disposed viewer — which is the defect

```
Expected a <System.ObjectDisposedException> to be thrown because no breadcrumb resource may be created
after teardown has begun, but no exception was thrown.
```

The test disposes a real `ItemViewer` and then calls `InitializeBreadcrumbPipeline(provider,
operations)` on it. Against the unfixed code that call **completes normally**. Nothing in
`EnsureBreadcrumbResourceOwnership` inspects the viewer's teardown state, so it proceeds to
`components ??= new Container()` and adds a `BreadcrumbResourceOwner` to a container belonging to a
viewer whose own `Dispose(bool)` has already run. That container is never disposed, and the resource
owner it holds is never invoked, so the breadcrumb pipeline is built against a dead viewer and leaks.

"No exception was thrown" is therefore the load-bearing observation, not merely the absence of the
expected type.

## The throw is attributable to the disposal guard, not to the D4 affinity guard

`[P4-T5]` has already placed `ThrowIfOffUiBoundary` as the first statement of
`EnsureBreadcrumbResourceOwnership` and of `InitializeBreadcrumbPipeline`, so it was live during this
run. It did not fire and could not have masked the observation: the test disposes the viewer and makes
the call on the **same ambient context** the `ViewerScope` installed before constructing it, so
`SynchronizationContext.Current` is reference-equal to the viewer's captured `UiSyncContext` and the
affinity guard returns without effect.

That is why the observed result is "no exception was thrown" rather than an
`InvalidOperationException` from the affinity guard, and it is what makes the `ObjectDisposedException`
this test will assert after `[P5-T3]` attributable to the disposal guard alone.

The assertion names `ObjectDisposedException` rather than its base `InvalidOperationException`, so the
D4 guard's own throw could not satisfy it even if the boundary were violated.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p5-t2-d5-fail/488-d5-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`. The D5 regression test
`InitializeBreadcrumbPipeline_AfterViewerDisposed_ThrowsObjectDisposedException` records outcome
**Failed** against the unfixed code, with **no exception thrown** — the call succeeded against a
disposed viewer, creating a container and a resource owner after teardown had begun.
