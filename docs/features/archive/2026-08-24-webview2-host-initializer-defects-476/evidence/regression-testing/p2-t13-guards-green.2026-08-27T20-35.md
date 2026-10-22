# [P2-T13] — Argument Guards on `WebView2CoreInitializer`, Green

Timestamp: 2026-08-27T20-35

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException|FullyQualifiedName~CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException|FullyQualifiedName~EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException" "/Logger:trx;LogFileName=p2-t13-guards-green.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p2-t13
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **3 passed, 0 failed.** `Test Run Successful.` / `Total tests: 3` / `Passed: 3`.
  - `Passed CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException`
  - `Passed CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException`
  - `Passed EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException`
- TRX `<Counters>`: `total="3" executed="3" passed="3" failed="0"`.

## What was implemented in `QuickFiler/Viewers/WebView2CoreInitializer.cs`

- `CreateEnvironmentAsync` throws `ArgumentNullException(nameof(cacheFolder))` when `cacheFolder` is
  null, and `ArgumentException` naming `cacheFolder` when it is empty or whitespace. Both guards run
  **before** `CoreWebView2Environment.CreateAsync` is reached. The null check precedes the
  whitespace check so the more specific exception type wins, which is what the test's
  `ThrowExactly<ArgumentException>` distinguishes.
- `EnsureCoreWebView2Async` throws `ArgumentNullException(nameof(control))` when `control` is null,
  before `control.EnsureCoreWebView2Async(...)` is reached.
- `using System;` was added, required by `ArgumentNullException` and `ArgumentException`.
- Both members changed from expression bodies to block bodies. No member signature changed.
- **`environment` is not guarded.** Every diff line mentioning `environment` is either a signature
  line, the comment recording the decision, or the forward itself:

  ```
  +        public Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment)
  +            // environment is deliberately not guarded: null is a valid SDK input meaning "create a
  +            // default environment".
  +            return control.EnsureCoreWebView2Async(environment);
  ```

  No `if (environment == null)` or equivalent appears anywhere in the diff.
- **`options` is not guarded**, per Decisions Record item 1. A comment at the call site records the
  ground: whether the SDK tolerates a null `options` is unverified, and guarding an unverified
  contract would narrow behaviour on unmeasured grounds.
- **No `#nullable enable` directive was added** to this file. Nullability is expressed only through
  the runtime guards. The mechanical check is `[P2-T16]`.

The three tests failed at `[P2-T2]` with "no exception was thrown" twice and
"found `<System.NullReferenceException>`" once, and now pass with the correct exception type and
`ParamName` in each case.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
