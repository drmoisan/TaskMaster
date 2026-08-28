# Determinism and External-Dependency Audit ([P5-T36])

Timestamp: 2026-08-27T23-30

Command:

```powershell
$files = @(
  'QuickFiler.Test\Controllers\WebView2CoreInitializerTests.cs',
  'QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs',
  'QuickFiler.Test\Viewers\WebView2BreadcrumbHostContractTests.cs')
foreach ($p in @('Task.Delay','Thread.Sleep','Path.GetTempPath','Path.GetTempFileName','DateTime.Now','Random.Shared','File.Create','new FileStream')) {
  Select-String -SimpleMatch -Pattern $p -Path $files
}
```

(run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Output Summary

Every pattern returned **zero matching lines** across all three test files.

| Pattern | Matching lines | Required by the criterion |
| --- | --- | --- |
| `Task.Delay` | **0** | Yes |
| `Thread.Sleep` | **0** | Yes |
| `Path.GetTempPath` | **0** | Yes |
| `Path.GetTempFileName` | **0** | Yes |
| `DateTime.Now` | **0** | Yes |
| `Random.Shared` | **0** | additional, from the standing banned-API list |
| `File.Create` | **0** | additional, temporary-file check |
| `new FileStream` | **0** | additional, temporary-file check |

The search is non-vacuous: the same `Select-String -SimpleMatch` invocation over the same three
paths returns matches for other patterns, for example `because:` and `.Should()`, which
`evidence/other/p5-t35-test-policy-audit.2026-08-27T23-29.md` records with non-zero counts. The zero
results above are therefore a real absence, not a mis-spelled path.

## No test drives the WebView2 Evergreen runtime to completion

The criterion also requires that no new test depends on an external process, the network, or the
WebView2 Evergreen runtime. Each of the three routes into the runtime is closed by construction:

1. **`EnsureCoreWebView2Async` is never driven to the SDK.** The only tests that call
   `InitializeAsync` — `InitializeAsync_InstallsUiDispatcherFromUiSyncContext`,
   `InitializeAsync_PreservesAnInjectedDispatcher`, and
   `PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` — supply a
   `Mock<IWebViewCoreInitializer>` built by `BuildCompletingInitializer()`, whose
   `CreateEnvironmentAsync` returns `Task.FromResult<CoreWebView2Environment>(null)` and whose
   `EnsureCoreWebView2Async` returns `Task.CompletedTask`. Neither `CoreWebView2Environment.CreateAsync`
   nor `WebView2.EnsureCoreWebView2Async` is reached.

2. **`CoreWebView2Environment.CreateAsync` is never called.** The three guard tests in
   `WebView2CoreInitializerTests.cs` invoke `CreateEnvironmentAsync` through an `Action` with a null
   or whitespace `cacheFolder`, and after the guards landed in `[P2-T13]` the guard throws before any
   SDK statement executes. `CreateEnvironmentAsync` is never invoked with an argument that would pass
   the guards, so the SDK-reaching `return ForwardCreateEnvironmentAsync(...)` statement is never
   evaluated. That is directly corroborated by coverage: line 55 of
   `QuickFiler/Viewers/WebView2CoreInitializer.cs` records **zero hits** in
   `evidence/qa-gates/coverage-postchange.cobertura.xml`.

3. **The recording `SynchronizationContext` never drains.** Its `Post` override increments a counter
   with `Interlocked.Increment` and returns without invoking the callback. Because the callback never
   runs, `ForwardNavigateToString` and `ForwardWebMessage` are never executed from the marshalling
   tests. Coverage corroborates this too: `WebView2BreadcrumbHost.cs` lines 162 and 207, the two SDK
   forwards' call sites, both record zero hits.

## No wall-clock waiting

No test waits on wall-clock time. The pump-hosted tests await `WinFormsPumpHost.InvokeAsync`, which
completes when the posted delegate has run on the pump thread rather than after an elapsed interval.
The `[Timeout(PumpTimeoutMs)]` attributes are MSTest failure ceilings, not waits: a passing test never
consumes them, and the reversed-order re-run recorded in
`evidence/other/p5-t37-test-independence.2026-08-27T23-30.md` completed all fifteen tests in
1.3235 seconds in total.

## No temporary files

No test creates a file. The three temporary-file patterns above return zero, and no test constructs a
`FileStream`, calls `File.WriteAllText`, or names a path under `%TEMP%`. The only path expression
anywhere in this feature's production code is
`Path.Combine(Environment.GetFolderPath(LocalApplicationData), "WindowsFormsWebView2")` inside
`InitializeAsync`, which is a string computation; the folder is created by the SDK on the
`ForwardCreateEnvironmentAsync` path, which no test reaches.
