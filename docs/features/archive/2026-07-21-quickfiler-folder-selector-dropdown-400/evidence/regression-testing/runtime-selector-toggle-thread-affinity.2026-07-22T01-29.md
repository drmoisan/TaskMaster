# Runtime selector toggle and thread-affinity intake

Timestamp: 2026-07-22T01:29:14.9918764Z

Command: `git rev-parse HEAD`

EXIT_CODE: 0

Output Summary: `dfb202fc5dbc50638a9519c66b64005bcb5de116`

Command: `rg -n "ConfigureAwait|SelectorOpenStateChanged|OpenAsync|CreateSurfaceAsync|EnsureCoreWebView2Async|NavigateToString|RectangleToScreen|Screen.FromControl|PointToClient|Show\(|Focus\(|Close\(|Dispose\(" QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs QuickFiler/Viewers/BreadcrumbUiDispatcher.cs QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`

EXIT_CODE: 0

Output Summary: The inspection found 12 matches in `BreadcrumbBridgeCoordinator.cs`, no matches in `BreadcrumbUiDispatcher.cs`, 7 matches in `BreadcrumbWebViewSurfaceFactory.cs`, 22 matches in `BreadcrumbDropDownHost.cs`, and 18 matches in `ItemViewer.Breadcrumb.cs`. The relevant residual route includes asynchronous surface creation and WebView initialization/navigation, asynchronous host open completion, ItemViewer anchor/screen conversion, native show/focus/close, and cleanup/disposal. These operations are not yet all expressed as individually dispatched UI-owned operations after each await.

## Exact supplied pre-remediation observations

1. `When I used the up and down arrows to select an item, certain entries existed in both the suggestions and the recent history. This is to be expected. But when I hit the down arrow to highlight it, it highlighted both entries. I believe this will be solved by your requirement for a unique entry.`
2. `The down arrow did not alway work. I would click the down arrow and it would not expand. And yet, when I used the keyboard command to select the folder picker, it would expand properly.`
3. `I experienced an unhandled exception.`

Exact exception:

`System.InvalidOperationException: Cross-thread operation not valid: Control 'L0vhBreadcrumb_WebView2' accessed from a thread other than the thread it was created on.`

The supplied Copilot analysis is a hypothesis, not a confirmed diagnosis. The governing direction is: `Please ensure that the remediation solves for what ever the real diagnosis is. Use this as a starting place only.`

## Three-part disposition

1. Duplicate highlighting maps to the completed P2 unique-logical-row proof. It is preserved as historical runtime evidence and is not replanned.
2. The old coordinator/provider-post defect maps to the completed P3 dispatcher proof. The coordinator callback-entry and provider-post guards remain required regression coverage.
3. The ambient-null or worker-originated popup factory/host/anchor continuation remains open for P5. Failure-first tests must independently locate the residual boundary violation, and implementation must dispatch each WebView2 and WinForms operation explicitly after every await.
