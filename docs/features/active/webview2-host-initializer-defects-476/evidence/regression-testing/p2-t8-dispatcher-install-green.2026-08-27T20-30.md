# [P2-T8] — Dispatcher Installed in `InitializeAsync` (Variant V1), Green

Timestamp: 2026-08-27T20-30

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~InitializeAsync_InstallsUiDispatcherFromUiSyncContext|FullyQualifiedName~InitializeAsync_PreservesAnInjectedDispatcher" "/Logger:trx;LogFileName=p2-t8-dispatcher-install-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t8
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **2 passed, 0 failed.** `Passed InitializeAsync_InstallsUiDispatcherFromUiSyncContext` /
  `Passed InitializeAsync_PreservesAnInjectedDispatcher` / `Test Run Successful.` /
  `Total tests: 2` / `Passed: 2`.
- TRX `<Counters>`: `total="2" executed="2" passed="2" failed="0"`.

## What was implemented

- The dispatcher field changed from `private readonly BreadcrumbUiDispatcher?` to a plain
  `private BreadcrumbUiDispatcher?`, to permit the assignment.
- In `InitializeAsync`, after the existing `uiSyncContext` null guard and before the seam calls:

  ```
  if (_dispatcher == null)
  {
      _dispatcher = new BreadcrumbUiDispatcher(uiSyncContext, LogDispatchFailure);
  }
  ```

  The `if (_dispatcher == null)` condition implements Decisions Record item 3: a dispatcher supplied
  through the internal three-argument constructor is preserved rather than discarded. Both directions
  are pinned by the two tests in this run.
- `private static void LogDispatchFailure(Exception exception)` is the error sink, logging through
  the file's existing log4net logger. Dispatch is fire-and-forget, so there is no caller left to
  observe a propagated exception.
- `BreadcrumbUiDispatcher.CaptureCurrent()` is **not** called. It throws
  `InvalidOperationException` when `SynchronizationContext.Current` is null, which would add a new
  throwing precondition; whether the ambient context is non-null at the production construction site
  is unverified.
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` was **not** edited. Its two-argument `internal`
  constructor is already assembly-visible and accepts a caller-supplied error sink.

## The two seam call expressions in `InitializeAsync` are textually unchanged

`git diff -- QuickFiler/Viewers/WebView2BreadcrumbHost.cs` filtered for added or removed lines
matching `CreateEnvironmentAsync` or `EnsureCoreWebView2Async` returns **no output**. Neither
`_initializer.CreateEnvironmentAsync(cacheFolder, options)` nor
`_initializer.EnsureCoreWebView2Async(_control, environment)` appears as a `+` or `-` line, so this
task edited the method without altering either call expression. This is the observation `[P5-T18]`
requires for the one in-repo caller that sits inside a file this feature modifies.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
