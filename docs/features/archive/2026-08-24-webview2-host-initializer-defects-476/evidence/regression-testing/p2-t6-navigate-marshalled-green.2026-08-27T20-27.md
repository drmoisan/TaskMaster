# [P2-T6] — `NavigateToString` Marshalled Through One Dispatch Callback, Green

Timestamp: 2026-08-27T20-27

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~NavigateToString_PostsExactlyOnceToTheUiContext" "/Logger:trx;LogFileName=p2-t6-navigate-marshalled-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t6
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed NavigateToString_PostsExactlyOnceToTheUiContext` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

`NavigateToString` in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` now reads `_dispatcher` into a
local and:

- when the dispatcher is non-null, executes `_ = dispatcher.Dispatch(() => _control.NavigateToString(html));`
  — a single callback containing the single SDK forward;
- when it is null (the pre-initialization window), executes the same forward inline on the calling
  thread, exactly as before this change.

`BreadcrumbUiDispatcher.DispatchValue` is not used. The `<remarks>` on the member records the
behaviour change: `Dispatch` is fire-and-forget, so the member now returns before the forward
executes, and order between successive calls is preserved by the single `SynchronizationContext.Post`
queue.

The same test failed at `[P2-T2]` with
`System.InvalidOperationException: The instance of CoreWebView2 is uninitialized and unusable.`,
thrown because the un-marshalled call reached the control inline. It now passes with the recording
context observing exactly one `Post` and the SDK never touched, because the recording context never
drains the queued callback.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
