# [P2-T4] — Detach Tolerates a Null `CoreWebView2`, Green

Timestamp: 2026-08-27T20-25

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~PredecessorDetach_ToleratesNullCoreWebView2" "/Logger:trx;LogFileName=p2-t4-null-core-tolerance-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t4
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.** `Passed PredecessorDetach_ToleratesNullCoreWebView2` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

`DetachCore()` in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` now reads
`_control.CoreWebView2` into a local `CoreWebView2? core`, null-checks it, and unsubscribes
`OnWebMessageReceived` from `core.WebMessageReceived` only when it is non-null. The
`CoreWebView2InitializationCompleted` unsubscription is unconditional, because that event lives on
the control itself and is always available.

A predecessor that never completed initialization therefore no longer risks a null dereference in
the detach path, while still being detached: the test asserts both that constructing the successor
does not throw and that the predecessor reports `IsAttached == false`.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
