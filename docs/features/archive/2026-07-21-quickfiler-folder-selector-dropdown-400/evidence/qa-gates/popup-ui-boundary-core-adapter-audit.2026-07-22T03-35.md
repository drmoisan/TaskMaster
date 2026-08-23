# Popup UI-boundary core adapter audit after recovery

Timestamp: 2026-07-22T03:35:59.2488954Z

Command: `& { $production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'); $tests=@('QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs'); 'PRODUCTION_METHODS'; rg -n '^\s*(internal|private) (static )?(async )?(Task|Task<|void|CoreWebView2|Control|NavigationSurface|BreadcrumbPopupUiOperations|Func<)|Create\(|CreateSurfaceAsync|DisposeSurfaceAfterFailureAsync|CreateDispatchedReadiness' $production; 'DETERMINISTIC_TESTS'; rg -n '^\s*public (async )?(Task|void) (SurfaceFactory|Readiness|DisposeSurfaceAsync|DispatchValue|CaptureCurrent|ProductionCapture|OpenAsync|RunAsync|Observe)' $tests; 'CONTEXT_AND_AWAIT'; rg -n 'SynchronizationContext|ConfigureAwait\(false\)|DispatchValue|RunAsync' $production $tests; 'INCLUDES_AND_LINES'; $project=Get-Content -LiteralPath 'QuickFiler/QuickFiler.csproj'; foreach($name in @('BreadcrumbUiDispatcher.cs','BreadcrumbWebViewSurfaceFactory.cs','BreadcrumbPopupUiOperations.cs')){ $matches=@($project | Select-String -SimpleMatch ('<Compile Include="Viewers\'+$name+'" />')); $name+'|COUNT='+$matches.Count; $matches | ForEach-Object { 'LINE='+$_.LineNumber+'|'+$_.Line.Trim() } }; foreach($path in @($production+$tests)){ $path+'|LINES='+(Get-Content -LiteralPath $path).Count }; 'DIFF_CHECK'; git diff --check -- $production $tests 'QuickFiler/QuickFiler.csproj' }`

EXIT_CODE: 0

Output Summary: The recovered current-tree audit maps validation, dispatcher capture, every WebView lifecycle stage, dispatched readiness detachment, primary-preserving failure cleanup, and exactly-once disposal to deterministic tests. All asynchronous continuations either perform state-only work or re-enter `RunAsync`/`DispatchValue` before a UI-owned operation. The helper has exactly one project include. All six core files are readable and at most 500 lines. The stale 70.04%-era helper coverage claim is invalid and is not used for approval; fresh numeric approval is deferred to P5-T27.

## Method-by-method boundary matrix

| Surface | Owning-boundary behavior | Deterministic proof |
|---|---|---|
| `BreadcrumbWebViewSurfaceFactory.Create` overloads | Validate initializer, HTML, and operations before dispatcher capture or factory creation. | `SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture`; `ProductionCaptureWithoutUiContext_FailsFast`; `CaptureCurrent_ControlledContext_CreatesOperationsWithoutInvokingWebView`. |
| `BreadcrumbPopupUiOperations.CaptureCurrent`, `CreateForCurrentThreadTests`, `CaptureCurrentOrTests` | Production capture requires a real current context; the explicit test fallback is used only when no context exists. | Production/no-context and controlled-context capture tests; ItemViewer fallback remains compiled and is exercised by composition tests in P5-T26. |
| `CreateControlAsync` / `CreateProductionControl` | WebView control construction is one dispatched direct-adapter call. | `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` records the owning context for `create`. |
| `BeginInitializationAsync` / `BeginProductionInitialization` | `EnsureCoreWebView2Async` invocation occurs inside a dispatched action; only its returned task is awaited off-context. | Worker-completion test records `initialize` on the owning context; initialization-failure test proves one observation and cleanup. |
| `ReadCoreAsync` / `ReadProductionCore` | Post-initialization `CoreWebView2` access is dispatched separately after the await. | Worker-completion test records `core`; invalid/null core flows are covered by the failure tests. |
| `BeginNavigationAsync` / `BeginProductionNavigation` | Navigation-handler attachment and navigation start occur inside the owning boundary; the correlated readiness task alone crosses the await. | Worker-completion test records `navigate`; navigation-action and invalid-navigation data tests prove one failure observation and cleanup. |
| `CreateDispatchedReadiness` and readiness `DetachHandlers` | Navigation completion/cancellation state and all handler detachment are dispatched; an ambient-null worker never detaches directly. | `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment`; `Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach`. |
| `ObserveInitializationAsync`, `ObserveReadinessAsync`, `ObserveExternalAsync` | External task completion is observed without depending on an ambient continuation context; cancellation/reporting rules are explicit. | `ObserveReadinessAsync_CancellationRethrowsWithoutReporting`; `ObserveInitializationAsync_CancellationReportsIdenticalExceptionOnce`; readiness-failure test. |
| `DisposeSurfaceAsync` / `DisposeProductionSurface` | Messenger and control disposal run in one dispatched operation; the second resource is attempted even when the first throws. | `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce`; initialization/navigation/readiness failure tests. |
| `DisposeSurfaceAfterFailureAsync`, `IgnoreFailureAsync`, `CompleteAll` | Secondary rollback uses non-reporting cleanup so the original operation error remains authoritative while every cleanup is attempted. | Initialization-failure test uses a throwing control and asserts the original initialization exception plus one cleanup observation; null-navigation data cases assert one report. |
| `InstallSurfaceAsync`, `PlaceSurfaceAsync`, `DisposeHostedSurfaceAsync`, `CreateAndInstallSurfaceAsync` | ToolStrip installation, sizing, hosted cleanup, and current-generation checks are dispatched operations. These composition-facing methods remain in the same helper and receive their full race/disposal proof in P5-T26. | Existing worker-stage test covers current creation/cleanup; the revised P5 composition batch supplies stale-generation and host-dispose tests before acceptance. |
| `BreadcrumbUiDispatcher.DispatchValue<T>` / `RunAsync` | Each value/action schedules on the captured owner, executes nested owner calls inline only from that owner, faults the returned task on scheduling/action failure, and reports according to the explicit flag. | `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`; nested-dispatch test; scheduling-failure test; `RunAsync_NullAction_ThrowsArgumentNullException`. |

## Ambient-context and cleanup conclusions

- The focused 31-test gate completed with ambient-null worker factories, explicit queued/recording contexts, and an injected pump context. No test depends on a continuation automatically returning to an ambient context.
- Every `ConfigureAwait(false)` continuation before a control, WebView, handler, or resource operation calls `RunAsync`/`DispatchValue` again.
- Initialization, navigation, readiness, handler-detach scheduling, and null-navigation failures are observed once. Primary failures are preserved while each owned resource is attempted exactly once.
- `BreadcrumbPopupUiOperations.cs` has exactly one compile include at `QuickFiler.csproj` line 392. The dispatcher and surface factory also each have one include.

## Mechanical compliance

| File | Lines |
|---|---:|
| `BreadcrumbUiDispatcher.cs` | 270 |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 |
| `BreadcrumbPopupUiOperations.cs` | 497 |
| `BreadcrumbPopupControlDispatchTests.cs` | 472 |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 |
| `BreadcrumbDropDownReadinessTests.cs` | 498 |

`git diff --check` returned zero scoped errors. Numeric coverage is intentionally not approved here. The prior 70.04% helper result predates the recovered implementation and is explicitly invalidated; P5-T27 must produce and parse fresh current-tree coverage for approval.
