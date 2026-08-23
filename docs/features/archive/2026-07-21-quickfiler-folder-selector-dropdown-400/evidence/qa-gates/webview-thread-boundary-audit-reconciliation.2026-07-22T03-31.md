# WebView thread-boundary audit reconciliation

Timestamp: 2026-07-22T03:31:01.9879087Z

Command: `& { $production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/WebView2Messenger.cs','QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'); $tests=@('QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs','QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs'); 'CORE_AND_DISPATCH_SITES'; rg -n 'CoreWebView2|DispatchValue|Dispatch\(|RunAsync|PostJson|ConfigureAwait\(false\)|SelectionChanged|SelectorOpenStateChanged|RaiseSyntheticArrowKey|PublishRouterOutputs' $production $tests; 'PRODUCTION_PROHIBITED_BLOCKING_OR_ASYNC_VOID'; $blocked=& rg -n 'async void|\.Wait\(|\.Result\b|\.Send\(' $production 2>$null; if ($LASTEXITCODE -eq 1) { 'NONE' } else { $blocked }; 'PROJECT_INCLUDE'; $matches=@(Get-Content -LiteralPath 'QuickFiler/QuickFiler.csproj' | Select-String -SimpleMatch '<Compile Include="Viewers\BreadcrumbUiDispatcher.cs" />'); 'BreadcrumbUiDispatcher.cs|COUNT=' + $matches.Count; $matches | ForEach-Object { 'LINE=' + $_.LineNumber + '|' + $_.Line.Trim() }; 'LINE_COUNTS'; foreach($path in @($production+$tests)){ '{0}|LINES={1}' -f $path,(Get-Content -LiteralPath $path).Count }; 'DIFF_CHECK'; git diff --check -- $production $tests 'QuickFiler/QuickFiler.csproj' }`

EXIT_CODE: 0

Output Summary: The singular current-tree inspection reconciles and restates completed P3-T9. Issue-400 coordinator and messenger SDK requests remain inside `BreadcrumbUiDispatcher` or bounded direct adapters. Provider continuations reach `PostRenderAndSelectorAsync`, selector transitions reach `PublishTransition`, inbound routing reaches `RaiseSyntheticArrowKey`/`PublishRouterOutputs`, and theme publication reaches `PostJson` only from dispatcher actions. No production `async void`, blocking `.Wait`, `.Result`, or `.Send` site was found. `BreadcrumbUiDispatcher.cs` has exactly one project include and every reconciled P3/core file remains at most 500 lines. Scoped `git diff --check` returned zero errors.

## P3-T9 call-chain reconciliation

- `WebView2Messenger` dispatches SDK event subscription, outbound `PostWebMessageAsJson`, inbound event-argument access/callback delivery, and unsubscription.
- `BreadcrumbBridgeCoordinator` awaits provider work with `ConfigureAwait(false)` only before entering dispatcher actions for render/selector publication, selection/open-state callbacks, arrow routing, and theme publication.
- `BreadcrumbWebViewSurfaceFactory` and `BreadcrumbPopupUiOperations` are the bounded P5 direct-adapter layer for control creation, initialization, core access, navigation, readiness detachment, and disposal. Their retained deterministic tests are enumerated in the paired recovery artifact; P5-T17 will re-audit them after the core gates.
- The production prohibited-pattern search returned `NONE`. Test-only synchronization primitives are not production dispatcher calls.

## Mechanical reconciliation

| File | Lines |
|---|---:|
| `BreadcrumbUiDispatcher.cs` | 270 |
| `WebView2Messenger.cs` | 147 |
| `BreadcrumbBridgeCoordinator.cs` | 455 |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 |
| `BreadcrumbPopupUiOperations.cs` | 491 |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 |
| `BreadcrumbSelectorCoordinatorTests.cs` | 424 |
| `BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 168 |

`BreadcrumbUiDispatcher.cs` compile include count: 1 at `QuickFiler.csproj` line 391.

This artifact explicitly supersedes `webview-thread-boundary-audit.2026-07-22T00-07.md` for canonical schema compliance because that historical artifact used `Commands:` rather than the required singular `Command:` field. Its completed P3-T9 behavioral proof remains historical evidence and is restated here against the current tree.
