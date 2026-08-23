# Popup UI-boundary core adapter audit

Timestamp: 2026-07-22T02:20:05.9453191Z

Command: `dotnet-coverage collect --output docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\popup-ui-boundary-core-coverage.2026-07-22T02-20.cobertura.xml --output-format cobertura --include-files C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\bin\Debug\QuickFiler.dll -- 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests' '/Logger:console;Verbosity=minimal'`

Inspection Command: `[xml]$coverage = Get-Content -Raw -LiteralPath 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/popup-ui-boundary-core-coverage.2026-07-22T02-20.cobertura.xml'; $coverage.coverage.packages.package.classes.class | Where-Object { $_.name -like '*BreadcrumbPopupUiOperations*' } | ForEach-Object { $lines = @($_.lines.line); $covered = @($lines | Where-Object { [int]$_.hits -gt 0 }).Count; "CLASS=$($_.name) COVERED=$covered/$($lines.Count) RATE=$($_.'line-rate')" }; (Select-String -LiteralPath 'QuickFiler/QuickFiler.csproj' -SimpleMatch '<Compile Include="Viewers\BreadcrumbPopupUiOperations.cs" />').Count; @('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs') | ForEach-Object { "LINES=$((Get-Content -LiteralPath $_).Count) FILE=$_" }`

EXIT_CODE: 0

Output Summary: The targeted production-assembly collection passed 20 of 20 focused tests. `BreadcrumbPopupUiOperations` has 53 of 53 directly measurable class lines covered (100%). Including its non-adapter compiler-generated closures and asynchronous state machine produces 77 of 78 covered Cobertura points (98.72%). Both measurements exceed the 90% new-code requirement. The coverage artifact is 26,895,435 bytes with SHA-256 `23ce5775ccbf5f9921ecd264f82756d929c9289ee218c058c005bb1b45840887`.

## Adapter and deterministic-test matrix

| UI-boundary operation | Production boundary | Deterministic evidence |
| --- | --- | --- |
| Create popup control | `CreateControlAsync` calls the captured creation delegate through `RunAsync` | `SurfaceFactory_WorkerInitializationCompletion_DispatchesEveryUiStage` |
| Invoke external WebView initialization | `BeginInitializationAsync` dispatches delegate invocation; `ObserveInitializationAsync` separately observes the returned task | `SurfaceFactory_WorkerInitializationCompletion_DispatchesEveryUiStage`; `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUpOnBoundary`; `ObserveInitializationAsync_CancellationReportsIdenticalExceptionOnce` |
| Read `CoreWebView2` | `ReadCoreAsync` dispatches the injected or production core-read adapter | `SurfaceFactory_WorkerInitializationCompletion_DispatchesEveryUiStage` |
| Attach handlers, navigate, and construct messenger | `BeginNavigationAsync` dispatches the navigation adapter; the production adapter creates `WebView2Messenger` with the same captured dispatcher | `SurfaceFactory_WorkerInitializationCompletion_DispatchesEveryUiStage`; `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUpOnBoundary` |
| Observe readiness failure | `ObserveReadinessAsync` reports non-cancellation failure once and preserves cancellation as expected disposal behavior | `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurfaceOnBoundary`; `ObserveReadinessAsync_CancellationRethrowsWithoutReporting` |
| Dispose partial or completed surface | `DisposeSurfaceAsync` dispatches messenger/control cleanup and returns synchronously only for the null/null no-op | `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUpOnBoundary`; `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUpOnBoundary`; `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurfaceOnBoundary`; `DisposeSurfaceAsync_NullSurface_ReturnsCompletedTask` |
| Dispatch value scheduling, nesting, and failure | `DispatchValue<T>` schedules through the captured synchronization context unless the exact dispatcher is already executing | `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`; `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost`; `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask` |

## Ambient-context and scope findings

- `BreadcrumbUiDispatcher.DispatchValue<T>` does not use ambient `SynchronizationContext.Current` as a survival condition. It executes inline only when thread-static `_executingDispatcher` is the exact captured dispatcher; all other calls post to that dispatcher.
- `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` uses `ConfigureAwait(false)` after external completions and explicitly invokes `BreadcrumbPopupUiOperations` before each later WebView or WinForms operation. Ambient continuation context is therefore not required.
- The direct production adapters `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, `DisposeProductionSurface`, and `NavigateToDocument` are bounded WebView2/WinForms integration points marked at method level with `[ExcludeFromCodeCoverage]`. Compiler-generated closure classes for only those excluded methods (`<>c__DisplayClass22_0` and the `<>c__DisplayClass24_*` types) remain separately identifiable in Cobertura and are not counted as measurable helper logic.
- All non-adapter generated helper points remain measured: the ordinary helper class is 53/53; its tested closures are 11/11; and `ObserveExternalAsync` is 13/14. Aggregate measurable coverage is 77/78 (98.72%).
- `QuickFiler.csproj` contains exactly one compile include for `Viewers\BreadcrumbPopupUiOperations.cs`.
- Final batch line counts are 267, 245, 269, 439, 459, and 413. Every production and test file remains at most 500 lines.
