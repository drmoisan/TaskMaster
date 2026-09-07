# D3 — Pass-After Evidence ([P3-T4])

Timestamp: 2026-08-28T05-39

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.16. The `/p:Platform=AnyCPU` substitution is
the documented deviation recorded in full in `488-d1-fail.md`. The warning count is unchanged from the
Phase 0 baseline, so `[P3-T3]`'s edit introduced no new diagnostic.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException|FullyQualifiedName~InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator|FullyQualifiedName~SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions" "/Logger:trx;LogFileName=488-d3-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p3-t4-d3-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `InitializeBreadcrumbPipeline_SecondDifferentProvider_ThrowsInvalidOperationException` | **Passed** |
| `InitializeBreadcrumbPipeline_RepeatSameProvider_DoesNotThrowAndKeepsCoordinator` | **Passed** |
| `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` | **Passed** |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## What changed against [P3-T2]

The negative case moved from **Failed** to **Passed**. `[P3-T2]` recorded "no exception was thrown"
when a second, distinct strict provider mock was supplied; after `[P3-T3]`'s fix the call throws
`InvalidOperationException`. The assertion additionally requires that the thrown instance is **not** an
`ObjectDisposedException`, which rules out a D5 disposal throw satisfying a D3 assertion by inheritance
— `ObjectDisposedException` derives from `InvalidOperationException`. The instance thrown is a plain
`InvalidOperationException`, so that exclusion holds.

The positive case remained **Passed** across the change, but for a different reason on each side, as
`488-d3-fail.md` records: before the fix a blanket early return refused every second call; after the
fix the guard's reference comparison finds the supplied provider reference-equal to the retained one
and takes the return-without-effect branch deliberately. `viewer.BreadcrumbCoordinator` is asserted
`BeSameAs` its pre-repeat value, so the retained coordinator is confirmed unchanged rather than merely
non-null.

`SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` is the reference-comparison
precedent D3 mirrors. It stays green because `[P3-T3]` did not touch
`BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` at all; `[P3-T5]` records the
corroborating diff evidence for that.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p3-t4-d3-pass/488-d3-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The D3 negative case
now throws `InvalidOperationException` — and not an `ObjectDisposedException` — where `[P3-T2]`
observed no exception at all; the positive case still returns without effect and keeps the same
breadcrumb coordinator instance.
