# #475 Parts 1 and 2 — Fail-Fast Pass Evidence ([P6-T5])

Timestamp: 2026-08-28T06-05

Command (under `pwsh -NoProfile`, worktree root):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException|FullyQualifiedName~CaptureCurrent_NullAndControlledContexts_FailFastAndCapture|FullyQualifiedName~Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory" "/Logger:trx;LogFileName=475-failfast-pass.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\regression-testing\p6-t5-475-failfast-pass
```

EXIT_CODE: 0

| Test | Outcome |
| --- | --- |
| `LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException` | **Passed** |
| `CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` | **Passed** |
| `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` | **Passed** |

Total tests 3, Passed 3, **Failed 0**. `Test Run Successful.`

## What each test establishes

- **`LegacySurfaceFactoryConstructor_AmbientContextNull_ThrowsInvalidOperationException`** — the
  `public` seven-parameter `BreadcrumbDropDownHost` constructor now fails fast without an ambient
  synchronization context. It supplies a **non-null** surface factory so the argument-null guard is not
  reached and the operations argument is what throws. This is the one publicly observable behaviour
  change in the whole change-set.
- **`CaptureCurrent_NullAndControlledContexts_FailFastAndCapture`** — the replacement boundary test in
  `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`. It asserts that `CaptureCurrent` under a null ambient
  context throws `InvalidOperationException`, and retains the deleted test's controlled-context half:
  under a `PumpSynchronizationContext` the captured operations still post to the owning thread, with
  `PostCount` exactly 1.
- **`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`** — the constraining test that pins
  the constructor argument order.

## Why the third test passes without an ambient context

This is the load-bearing note for `[P6-T3]`'s "do not reorder any constructor argument" instruction.

`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` passes a **null** surface factory and no
ambient context, and asserts `ArgumentNullException` with `ParamName` `"surfaceFactory"`. In the
`public` seven-parameter constructor's chain, the `surfaceFactory ?? throw new
ArgumentNullException(nameof(surfaceFactory))` inside `BreadcrumbPopupUiOperations.NormalizeFactory` is
evaluated **before** the operations argument that now calls `CaptureCurrent()`. C# evaluates arguments
left to right, so the argument-null guard fires first and the fail-fast capture is never reached.

Had `[P6-T3]` reordered those arguments — placing the operations argument before the surface factory —
this test would now throw `InvalidOperationException` from `CaptureCurrent()` instead of
`ArgumentNullException`, and it would fail. It was not reordered: `[P6-T3]` changed only the
identifier `CaptureCurrentOrTests` to `CaptureCurrent` in place, a pure identifier-for-identifier swap
that left the argument list positions untouched. The `git diff` for
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is exactly 2 added and 2 deleted lines, one pair per
constructor chain.

`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` lives in
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`, a forbidden file that
`[P1-T6]` and `[P8-T8]` independently confirm is byte-identical to `BASE_SHA`, so its pass is not the
result of an edit.

## TRX

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p6-t5-475-failfast-pass/475-failfast-pass.trx`

Output Summary: EXIT_CODE 0, failed count **0**, all three named tests `Passed`. The third passes
without an ambient context because the surface-factory argument-null guard is evaluated before the
operations argument, which is why `[P6-T3]` must not — and did not — reorder the constructor arguments.
