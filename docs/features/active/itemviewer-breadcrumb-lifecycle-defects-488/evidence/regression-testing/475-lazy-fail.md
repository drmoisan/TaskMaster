# #475 Part 3 — Fail-Before Evidence for the Seam-Preservation Test ([P6-T4]) `[expect-fail]`

Timestamp: 2026-08-28T06-05

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:04.29. This build carries `[P6-T3]`'s edit set:
the `CaptureCurrentOrTests` declaration is deleted and all five production references now name
`CaptureCurrent`, but `EnsureBreadcrumbLifecycle`'s argument is still **eagerly** evaluated. The
`/p:Platform=AnyCPU` substitution is the documented deviation recorded in `488-d1-fail.md`.

## Step 2 — the failing test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow" "/Logger:trx;LogFileName=475-lazy-fail.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p6-t4-475-lazy-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

| Test | Outcome |
| --- | --- |
| `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` | **Failed** |

Total tests 1, Failed 1. `Test Run Failed.`

## The eagerly evaluated operations argument threw before the early return could discard it

```
Did not expect any exception because the already-seeded lifecycle discards the operations argument, so
it must never be evaluated on a thread without an ambient context, but found
System.InvalidOperationException: Breadcrumb UI components must be constructed on an owning UI
synchronization context.
   at QuickFiler.Viewers.BreadcrumbUiDispatcher.CaptureCurrent() in ...\BreadcrumbUiDispatcher.cs:line 46
   at QuickFiler.Viewers.BreadcrumbPopupUiOperations.CaptureCurrent() in ...\BreadcrumbPopupUiOperations.cs:line 81
```

The stack trace is the whole point of this observation. The throw originates in
`BreadcrumbUiDispatcher.CaptureCurrent()` — reached through
`BreadcrumbPopupUiOperations.CaptureCurrent()`, which is the **argument** to
`EnsureBreadcrumbLifecycle`. C# evaluates arguments before the call, so the operations object is
constructed before `EnsureBreadcrumbLifecycle` runs a single statement, and therefore long before its
already-initialized early return can discard it.

The viewer in this test has a **seeded** lifecycle: `InitializeBreadcrumbPipeline(provider, operations)`
was called with injected operations, so `_breadcrumbLifecycleCoordinator` is already non-null and the
three-argument `ConfigureBreadcrumbDropDown` call is a pure no-op with respect to that argument. It is
constructed and thrown away on every such call. Under an ambient-null context it now throws instead.

**This is exactly why laziness is required rather than opportunistic.** Parts 1 and 2 of #475 replaced
the silently-degrading `CaptureCurrentOrTests` with the fail-fast `CaptureCurrent`, which is the
intended behaviour change; but without part 3 that change converts a harmless no-op into a throw and
removes the injected seam every such test relies on. The failure recorded here is the cost of parts 1
and 2 landing without part 3, observed deliberately.

## Red-then-green pairing

This artifact is the **red** half of the criterion `[P6-T16]` flips, which requires evidence that the
seam-preservation test is red before the laziness change and green after. `475-pass.md`, produced by
`[P6-T8]` after `[P6-T7]` delivers part 3, is the green half. Decision D-12 authorizes that criterion
to cite both artifacts.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p6-t4-475-lazy-fail/475-lazy-fail.trx`

Output Summary: EXIT_CODE 1 with `ExpectedExitCode: 1`.
`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` records outcome **Failed**,
with `InvalidOperationException` thrown from `BreadcrumbUiDispatcher.CaptureCurrent()` inside the
eagerly evaluated `EnsureBreadcrumbLifecycle` argument — before the already-initialized early return
could discard it.
