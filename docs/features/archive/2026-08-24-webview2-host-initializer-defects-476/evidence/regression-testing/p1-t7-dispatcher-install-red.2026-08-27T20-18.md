# [P1-T7] [expect-fail] — Dispatcher Installation Test, Compile-Red State

Timestamp: 2026-08-27T20-18

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `9 Error(s)`, `5 Warning(s)`. The build is red.
- `[TestMethod] public async Task InitializeAsync_InstallsUiDispatcherFromUiSyncContext()` exists in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at line 256.

### Complete distinct compiler error list

All nine errors are in `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs`. Two error texts
recur; both are given in full once.

`CS1729`: `'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments`
`CS1061`: `'WebView2BreadcrumbHost' does not contain a definition for '<member>' and no accessible
extension method '<member>' accepting a first argument of type 'WebView2BreadcrumbHost' could be
found (are you missing a using directive or an assembly reference?)`

| Location | Error | Missing member named |
| --- | --- | --- |
| `(46,33)` | `CS1729` | 3-argument constructor |
| `(96,33)` | `CS1729` | 3-argument constructor |
| `(158,26)` | `CS1061` | `IsAttached` |
| `(163,26)` | `CS1061` | `IsAttached` |
| `(207,26)` | `CS1061` | `IsAttached` |
| `(242,22)` | `CS1061` | `IsAttached` |
| `(267,33)` | `CS1729` | 3-argument constructor |
| `(271,26)` | `CS1061` | `HasUiDispatcher` |
| `(281,26)` | `CS1061` | `HasUiDispatcher` |

Locations 267, 271 and 281 are the three references this task added. Line numbers of the earlier
errors shifted by one because this task also added `using Microsoft.Web.WebView2.Core;`.

### Test design points

- The host is constructed through the internal three-argument constructor passing `null` for the
  dispatcher, and `HasUiDispatcher` is asserted false before the act, so the post-condition is not
  satisfiable by a dispatcher that was present all along.
- The seam is a **loose** `Mock<IWebViewCoreInitializer>` whose `CreateEnvironmentAsync` returns
  `Task.FromResult<CoreWebView2Environment>(null)` and whose `EnsureCoreWebView2Async` returns
  `Task.CompletedTask`. `InitializeAsync` therefore runs end-to-end without reaching the WebView2 SDK
  and without an Evergreen runtime. The null environment is forwarded to the mock rather than
  dereferenced, and `CoreWebView2Environment` cannot be constructed in a unit test.
- `InitializeAsync` is awaited with `pump.SyncContext`, a real `WindowsFormsSynchronizationContext`
  that drains, so the `await uiSyncContext` inside `InitializeAsync` resumes. A non-draining
  recording context is deliberately not used here: it would never resume and the test would time
  out rather than assert.
- The mock builder is shared with `[P1-T8]`, which needs the same seam shape.
