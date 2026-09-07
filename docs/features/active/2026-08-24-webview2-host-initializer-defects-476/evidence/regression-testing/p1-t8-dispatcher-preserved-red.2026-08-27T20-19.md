# [P1-T8] [expect-fail] — Dispatcher-Preservation Test, Compile-Red State

Timestamp: 2026-08-27T20-19

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `10 Error(s)`, `5 Warning(s)`. The build is red.
- `[TestMethod] public async Task InitializeAsync_PreservesAnInjectedDispatcher()` exists in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at line 301. The file is 388 lines, within
  the 500-line limit.

### Complete distinct compiler error list

All ten errors are in `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs`. Message texts:

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
| `(315,33)` | `CS1729` | 3-argument constructor |

Location 315 is the reference this task added. The complete missing-seam set across Phase 1 is
exactly three members: the internal three-argument constructor, `IsAttached`, and `HasUiDispatcher`
— which is precisely what `[P2-T1]` declares.

### Test design points

- The dispatcher passed to the internal three-argument constructor is built over this file's
  non-draining recording `SynchronizationContext`, so a post it receives is counted and never
  executed.
- `InitializeAsync` is awaited with `pump.SyncContext`, the draining pump context, using the same
  loose `Mock<IWebViewCoreInitializer>` shape as `[P1-T7]`.
- `PostMessageJson` is then called from the MSTest thread and the recording context must have
  observed exactly one `Post`. If `InitializeAsync` overwrote the field with a dispatcher built from
  `uiSyncContext`, the post would go to the pump context instead and the recording count would stay
  zero, so the assertion discriminates the two behaviours.

## Phase 1 summary

Eleven tests were authored across three files before any production change, per the Bugfix Workflow
in `CLAUDE.md`:

| # | Test | File |
| --- | --- | --- |
| 1 | `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` |
| 2 | `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | same |
| 3 | `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | same |
| 4 | `IsCoreInitialized_HasAnExplicitBackingField` | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` |
| 5 | `PostMessageJson_PostsExactlyOnceToTheUiContext` | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` |
| 6 | `NavigateToString_PostsExactlyOnceToTheUiContext` | same |
| 7 | `SecondHost_DetachesThePredecessorAndTakesOwnership` | same |
| 8 | `PredecessorDetach_ToleratesNullCoreWebView2` | same |
| 9 | `ControlDisposed_DetachesTheHost` | same |
| 10 | `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | same |
| 11 | `InitializeAsync_PreservesAnInjectedDispatcher` | same |

Tests 1 through 4 were observed failing at assertion time in real test runs (`[P1-T1]`, `[P1-T2]`).
Tests 5 through 11 reference seams that do not exist yet, so the test assembly does not compile and
`[P1-T3]` through `[P1-T8]` record compile-time red states. `[P2-T2]` re-runs all eleven at assertion
time and is the authoritative fail-before record for the whole set.
