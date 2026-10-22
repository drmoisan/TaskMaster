# [P4-T4] Site-3 seam test authored after [P4-T3]; non-vacuity mutation observed

- Issue: #792
- Timestamp: 2026-09-17T20-04
- Command: CMD-OUTLOOK (`Get-Process -Name OUTLOOK`, printed `OUTLOOK-CLOSED: true` before each build), then for each of the two runs CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST (vswhere resolved `vstest.console.exe`), then CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~WebView2EnvironmentContractTests.EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam` and `<task>` = `p4-t4` (unmutated) and `p4-t4-mutation` (mutated); all run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; build and test consoles captured to the gitignored `coverage/p4-t4-build.log`, `coverage/p4-t4-scoped.log`, `coverage/p4-t4-mutation-build.log`, `coverage/p4-t4-mutation-scoped.log`; TRX under the gitignored `coverage/test-results/p4-t4/` and `coverage/test-results/p4-t4-mutation/`
- EXIT_CODE: 0
- Output Summary: unmutated run `Test Run Successful.`, `Total tests: 1`, `Passed: 1` (exit 0); mutated run `Test Run Failed.`, `Total tests: 1`, `Failed: 1` (exit 1) on the pre-predicted assertion; site-3 file restored byte-identical (SHA-256 equal before mutation and after restoration; `git diff --no-index --numstat` between the pre-mutation snapshot and the restored file prints nothing and exits 0).

## Test authored (after [P4-T3]; never run against the direct-SDK body)

`QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` now carries `EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam` (method name on a single line; `Select-String -SimpleMatch` count 1). The file is 192 lines (ceiling 200), UTF-8 without BOM, CRLF, `dotnet tool run csharpier check` exit 0.

Arrangement: `new SynchronizationContext()` installed as `SynchronizationContext.Current` and restored in `finally`; `EfcItemController` and `ItemViewer` obtained through `FormatterServices.GetUninitializedObject`; the viewer's `_context` field set to the same context so `await _itemViewer.UiSyncContext` completes inline (`SynchronizationContextAwaiter.IsCompleted` returns true on reference equality, `UtilitiesCS/Threading/UiThread.cs:155-163`); `controller.WebViewInitializer` set to a `Mock<IWebViewCoreInitializer>` capturing the `CreateEnvironmentAsync` arguments and returning completed tasks. Assertions: captured folder equals `WebView2EnvironmentContract.ResolveUserDataFolder()`; captured `options` not null; `options.AdditionalBrowserArguments` equals `WebView2EnvironmentContract.AdditionalBrowserArguments`; `EnsureCoreWebView2Async(null, null)` verified `Times.Once` (the uninitialized viewer's control and the mocked environment are both null, so the exact-argument form pins the one awaited seam call).

The direct-SDK body was replaced by [P4-T3] before this test was written, so no run of this test ever reached `CoreWebView2Environment.CreateAsync`; the only WebView2 SDK type the test constructs is none (the options object is produced by the code under test).

## Unmutated run (`p4-t4`)

Build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 19 `CoreCompile:` lines (unanchored count), 2 `csc.exe` lines both naming `QuickFiler.Test`; `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` rewritten 20:03:02 to 20:04:53 (the earlier 20:03:02 build was the first compile of the Phase 4 production changes plus the initial 206-line draft of this test, which also passed 1/1 before the draft was tightened to fit the ceiling).

Run: `Passed EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam [161 ms]`; `Test Run Successful.`; `Total tests: 1`; `Passed: 1`; `Failed: 0 (omitted category)`; exit 0.

## Mutation (temporary; restored)

MUTATION: in `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` the second argument of the `WebViewInitializer.CreateEnvironmentAsync(` call was changed from `options` to `new CoreWebView2EnvironmentOptions()` (needle matched exactly once; mutated line 48 read `new CoreWebView2EnvironmentOptions()`).

PREDICTED-FAILING-ASSERTION: `actualArguments.Should().Be(expectedArguments, "the shared browser arguments")` - expected `"--incognito "`, found `<null>` (a parameterless options object carries null `AdditionalBrowserArguments`); the folder assertion and the not-null assertion before it still pass, so this is the first assertion to fail.

Mutated build: exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 29 `CoreCompile:` lines, 10 `csc.exe` lines (2 naming `QuickFiler.Test`, 3 naming the `QuickFiler` production project); test dll rewritten 20:04:53 to 20:04:58.

OBSERVED-FAILING-ASSERTION (first `Error Message` line, verbatim from the console): `Expected actualArguments to be "--incognito " because the shared browser arguments, but found <null>.`

Observed run: `Failed EfcItemController_InitializeWebViewAsync_PassesTheContractValuesThroughTheSeam [255 ms]`; `Test Run Failed.`; `Total tests: 1`; `Failed: 1`; exit 1.

PREDICTION-MATCHES-OBSERVATION: true

## Restoration proof

- SITE3-SHA256-BEFORE-MUTATION: `9E887E98A48FC1DD5AF2043DD6D979864A3807E0020314C6420BC18419BC62E4`
- SITE3-SHA256-AFTER-RESTORE: `9E887E98A48FC1DD5AF2043DD6D979864A3807E0020314C6420BC18419BC62E4`
- RESTORED-IDENTICAL: true
- `Select-String -SimpleMatch 'new CoreWebView2EnvironmentOptions()'` over the restored file: 0
- `git diff --no-index --numstat coverage/p4-t4-site3-snapshot.cs QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` (snapshot = the bytes captured immediately before the mutation, i.e. the [P4-T3] state): prints nothing (identical files; git emits no numstat row for a zero-change pair), exit 0.

DEVIATION (recorded, not a change to the plan): the task text asks for `git diff --numstat HEAD -- QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` showing `0 0` after restoration. At this point HEAD is the Phase 3 commit `494f71381`, which predates the [P4-T3] rewrite of this file, so that command necessarily reports the uncommitted [P4-T3] change: observed `18 32`. The clause is unsatisfiable by construction until [P4-T12] commits; the intended fact (the mutation left no residue) is proven above by the SHA-256 identity and the no-index diff against the pre-mutation snapshot.

## Note on the [P4-T3] alias literal

`Select-String -SimpleMatch 'IncognitoArgument = WebView2EnvironmentContract.AdditionalBrowserArguments;'` returns 0 in the site-3 file because the declaration is 105 columns at its 8-space indent and CSharpier 1.2.6 breaks it after `=` (`dotnet tool run csharpier check` exits 0 on that two-line shape, so it is the formatter's own output). The delivered form is verified by `internal const string IncognitoArgument =` (1 hit) immediately followed by `WebView2EnvironmentContract.AdditionalBrowserArguments;` (1 hit); a multiline regex `IncognitoArgument =\s+WebView2EnvironmentContract\.AdditionalBrowserArguments;` over the raw text matches once. Every other [P4-T3] clause passed as written (`CoreWebView2Environment.CreateAsync(` 0, `ContinueWith(` 0, `WebViewInitializer.CreateEnvironmentAsync(` 1, `WebViewInitializer.EnsureCoreWebView2Async(` 1, `"WindowsFormsWebView2"` 0, `using System.IO;` absent, 56 lines; positive controls at HEAD for the four zero-gates: 1, 1, 1, 1).

## Line-ending and encoding note

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` carry a UTF-8 BOM at HEAD (the other eight Phase 4 targets do not). A normalisation pass in [P4-T2] briefly stripped the ViewerSetup BOM (numstat read 4/9 with a line-1 hunk); it was restored before the task was accepted (numstat 3/8 against BASE-SHA, the only hunk being lines 55-62). No later step rewrites whole files, so the router's BOM is untouched.
