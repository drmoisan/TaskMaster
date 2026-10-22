# [P2-T9] — Pre-Initialization Inline Fallback, Green

Timestamp: 2026-08-27T20-31

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload" "/Logger:trx;LogFileName=p2-t9-inline-fallback-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t9
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.**
  `Passed PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` /
  `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What this test pins

`PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` was added to
`QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs`. It constructs a distinct `WebView2` on
`WinFormsPumpHost`, constructs the host through the internal three-argument constructor passing
`null` for the dispatcher, and asserts:

- `HasUiDispatcher` is false;
- calling `PostMessageJson` from the MSTest thread does not throw;
- `IsCoreInitialized` remains false afterwards.

This documents the boundary of the fix rather than hiding it. Under capture variant V1 the dispatcher
is installed by `InitializeAsync`, so before initialization there is no dispatcher and the callback
executes inline on the caller's thread — the pre-initialization window remains unmarshalled. Within
that window the existing log-and-drop behaviour is unchanged: the null `CoreWebView2` guard fires,
the existing `log.Error` message is emitted, and no exception escapes.

This is behaviour-change #2 from the spec's Behaviour Changes and Residual Risks section, recorded as
a passing test rather than as prose.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
