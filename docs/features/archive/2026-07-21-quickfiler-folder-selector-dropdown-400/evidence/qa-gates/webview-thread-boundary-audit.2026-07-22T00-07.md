# WebView Thread Boundary Audit

Timestamp: 2026-07-22T00:07:17Z
Commands: scoped `Select-String` searches for `CoreWebView2`, `ConfigureAwait(false)`, `PostJson`, the four UI callbacks, `async void`, blocking wait/result/send patterns, and dispatcher call sites; exact project-include count; batch line counts; and `git diff --check` over the batch files and `QuickFiler.csproj`.
EXIT_CODE: 0
Output Summary: The issue-#400 messenger SDK requests and coordinator delivery paths are dispatcher-confined. No scoped whitespace error, blocking dispatcher call, or `async void` handler remains. The new dispatcher has one project include and all batch files are below 500 lines.

## SDK and direct-adapter boundaries

- `WebView2Messenger` schedules SDK event subscription, `PostWebMessageAsJson`, event-argument reads, inbound callback delivery, and event unsubscription through `BreadcrumbUiDispatcher.Dispatch`. Disposal is checked before scheduling and again inside every queued SDK action.
- The broader `QuickFiler/Viewers` search separately enumerated existing direct-adapter surfaces: `BreadcrumbWebViewSurfaceFactory` owns WebView initialization/navigation correlation, `WebView2BreadcrumbHost` owns its legacy control initialization and message adapter, `WebView2CoreInitializer` is a forwarding initializer, and `ItemViewer.Breadcrumb` hands an initialized core to `WebView2Messenger`. Generated designer type/property declarations contain no issue-#400 request logic.
- No issue-#400 coordinator code accesses `CoreWebView2` directly.

## Continuation and callback call chain

- Provider continuations at coordinator lines 90-94 and 122-126 call `PostRenderAndSelectorAsync`, whose entire render/selector publication is one dispatcher action.
- Selector transitions call `PublishTransition` only from a dispatcher action; render, selector state, `SelectionChanged`, and `SelectorOpenStateChanged` therefore retain one ordered UI batch.
- Inbound routing calls `RaiseSyntheticArrowKey` and `PublishRouterOutputs` only through awaited dispatcher actions. Those methods contain `FolderArrowKeyDown`, `UnhandledArrow`, render posts, selector posts, and `SelectionChanged` callbacks.
- Theme posting is directly wrapped in a dispatcher action.
- Every `ConfigureAwait(false)` continuation either performs pure/router work or awaits/schedules one of these dispatcher actions; no continuation directly posts or raises a UI-owned callback.
- The synchronous inbound event handler assigns `LastDispatch = ObserveInboundAsync(json)`; the observer catches current failures and routes them once to the dispatcher error sink.

## Mechanical checks

- `BreadcrumbUiDispatcher.cs` project include count: 1.
- `BreadcrumbUiDispatcher.cs`: 180 lines.
- `WebView2Messenger.cs`: 147 lines.
- `BreadcrumbBridgeCoordinator.cs`: 455 lines.
- `BreadcrumbUiThreadDispatchTests.cs`: 379 lines.
- `BreadcrumbSelectorCoordinatorTests.cs`: 424 lines.
- `BreadcrumbBridgeCoordinatorProbabilityTests.cs`: 168 lines.
- `git diff --check` EXIT_CODE: 0. Git emitted only its configured LF-to-CRLF working-copy advisory for the project file.
