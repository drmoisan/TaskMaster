# [P3-T2] — Member-Level Exemptions on the Host, Green

Timestamp: 2026-08-27T20-44

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers" "/Logger:trx;LogFileName=p3-t2-member-exemptions.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p3-t2
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.**
  `Passed WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers` / `Test Run Successful.` /
  `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented

Two SDK forwards were extracted from members that also carry a testable decision:

- `_control.NavigateToString(html)` moved into `private void ForwardNavigateToString(string html)`,
  called from inside the single existing `Dispatch` callback and from the inline fallback. The
  marshalling decision stays in `NavigateToString`.
- `core.PostWebMessageAsJson(json)` moved into
  `private static void ForwardWebMessage(CoreWebView2 core, string json)`, called from inside the
  single existing `Dispatch` callback. The `CoreWebView2` read, the null guard and the log-and-drop
  stay in `PostMessageJson`.

`[ExcludeFromCodeCoverage]` with a member-specific rationale was applied to exactly four members:

| Member | Rationale recorded in code |
| --- | --- |
| `OnCoreInitializationCompleted` | raised only by the live SDK; `CoreWebView2InitializationCompletedEventArgs` has no public constructor |
| `OnWebMessageReceived` | raised only by the live SDK; `CoreWebView2WebMessageReceivedEventArgs` has no public constructor |
| `ForwardNavigateToString` | `WebView2.NavigateToString` throws unless a live CoreWebView2 exists, which needs the Evergreen runtime |
| `ForwardWebMessage` | `CoreWebView2.PostWebMessageAsJson` is reachable only through a live CoreWebView2 |

## What the test asserts, and how it discriminates

The test enumerates every declared method of the type across public, non-public, instance and static
binding flags, filters to those carrying `ExcludeFromCodeCoverageAttribute`, drops
compiler-generated names beginning with `<` (the local function inside `PostMessageJson`), and
asserts the resulting name set is equivalent to exactly those four names. It then asserts, member by
member, that `IsAttached`, `HasUiDispatcher`, `IsCoreInitialized`, `NavigateToString`,
`PostMessageJson`, `InitializeAsync` and `DetachCore` each exist and carry **no** exemption, and that
neither constructor carries one. The `NotBeEmpty` check on each name means a typo or a rename would
fail the test rather than silently passing over a missing member.

`InitializeAsync` is measured, not exempt, per Decisions Record item 5: its only SDK-reaching
statements go through the mockable `IWebViewCoreInitializer` seam, and
`InitializeAsync_InstallsUiDispatcherFromUiSyncContext` and
`InitializeAsync_PreservesAnInjectedDispatcher` exercise it end-to-end against a
`Mock<IWebViewCoreInitializer>`. Exempting a member this plan demonstrably tests would recreate the
false-rationale defect #477 reports.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
