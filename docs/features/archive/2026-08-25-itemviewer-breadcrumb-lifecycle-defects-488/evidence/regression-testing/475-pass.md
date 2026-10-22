# #475 — Pass-After Evidence, All Three Parts Landed ([P6-T8])

Timestamp: 2026-08-28T06-09

## Step 1 — intermediate build

Command (under `pwsh -NoProfile`, worktree root):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo
```

GATE: none (intermediate build)
EXIT_CODE: 0 — `0 Error(s)`, 3 warnings, elapsed 00:00:03.30. Warning count unchanged from the Phase 0
baseline. The `/p:Platform=AnyCPU` substitution is the documented deviation recorded in
`488-d1-fail.md`.

## Step 2 — the test run

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow|FullyQualifiedName~LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException|FullyQualifiedName~CaptureCurrent_NullAndControlledContexts_FailFastAndCapture" "/Logger:trx;LogFileName=475-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p6-t8-475-pass
```

EXIT_CODE: 0

| Test | Outcome | Covers |
| --- | --- | --- |
| `ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` | **Passed** | part 3, the lazy operations factory |
| `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | **Passed** | part 2, the constructor-chain swap |
| `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` | **Passed** | part 1, the deletion and its replacement boundary test |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## What changed against [P6-T4]

`ConfigureBreadcrumbDropDown_SeededLifecycleNullUiContext_DoesNotThrow` moved from **Failed** to
**Passed**. `[P6-T4]` recorded it throwing `InvalidOperationException` from
`BreadcrumbUiDispatcher.CaptureCurrent()`, reached through the **eagerly evaluated**
`EnsureBreadcrumbLifecycle` argument, before the already-initialized early return could discard it.

`[P6-T7]` changed that parameter to a `Func<BreadcrumbPopupUiOperations>` and moved its single
invocation to **after** the early return. On a viewer whose lifecycle is already seeded the factory is
therefore never invoked at all, the no-op call stays a no-op, and the injected seam is preserved.

The other two tests were already green from `[P6-T5]` and stay green, which confirms part 3 did not
regress parts 1 and 2: the constructor still fails fast without an ambient context, and `CaptureCurrent`
still throws under a null context while capturing a controlled one normally.

## Red-then-green pairing

This artifact is the **green** half of the criterion `[P6-T16]` flips. `475-lazy-fail.md`, produced by
`[P6-T4]`, is the red half. Decision D-12 authorizes that criterion to cite both, as one of exactly two
multi-artifact citations in this plan.

## All three parts of #475 have now landed

1. **Part 1** — `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` is deleted; a repository-wide
   search of tracked `.cs` files returns zero hits. `CreateForCurrentThreadTests` survives on both
   `BreadcrumbPopupUiOperations` and `BreadcrumbUiDispatcher`, unchanged.
2. **Part 2** — both `BreadcrumbDropDownHost` seven-parameter constructor chains supply
   `CaptureCurrent()`, with no argument reordered.
3. **Part 3** — `EnsureBreadcrumbLifecycle` takes a factory delegate, invoked exactly once after the
   early return, with all three call sites updated.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p6-t8-475-pass/475-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The seam-preservation
test is green where `[P6-T4]` observed it red, because the operations argument is now evaluated only
after the already-initialized early return.
