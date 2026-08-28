# [P3-T3] — Member-Level Exemptions on the Initializer, Green

Timestamp: 2026-08-27T20-46

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2CoreInitializer_ExemptsOnlyTheSdkForwards" "/Logger:trx;LogFileName=p3-t3-initializer-exemptions.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p3-t3
```

EXIT_CODE: 0

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`.
- Test run: **1 passed, 0 failed.**
  `Passed WebView2CoreInitializer_ExemptsOnlyTheSdkForwards` / `Test Run Successful.` /
  `Total tests: 1` / `Passed: 1`.
- TRX `<Counters>`: `total="1" executed="1" passed="1" failed="0"`.

## What was implemented in `QuickFiler/Viewers/WebView2CoreInitializer.cs`

- The class-level `[ExcludeFromCodeCoverage]` is removed.
- `CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` moved into
  `private static Task<CoreWebView2Environment> ForwardCreateEnvironmentAsync(string cacheFolder, CoreWebView2EnvironmentOptions options)`.
- `control.EnsureCoreWebView2Async(environment)` moved into
  `private static Task ForwardEnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment)`.
- Each public member now runs its guards and then calls its forward.
- `[ExcludeFromCodeCoverage]` is applied to those two private forwards **only**, each carrying the
  accurate rationale:
  - `ForwardCreateEnvironmentAsync` — requires the external Evergreen WebView2 runtime, a separate
    process, and additionally creates a user-data folder on disk; a unit test may do neither.
  - `ForwardEnsureCoreWebView2Async` — requires the external Evergreen WebView2 runtime.
- The argument guards in `CreateEnvironmentAsync` and `EnsureCoreWebView2Async` are therefore
  measured, which is what the repository rule requires of a testable seam with no SDK dependency.
- `using System.Diagnostics.CodeAnalysis;` remains required and is retained.

## What the test asserts

`WebView2CoreInitializer_ExemptsOnlyTheSdkForwards` was added to the existing
`QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs`. By reflection it asserts that the type
carries no class-level `ExcludeFromCodeCoverageAttribute`, that both private forwards exist and carry
it, and that `CreateEnvironmentAsync` and `EnsureCoreWebView2Async` do not. The `NotBeNull` checks on
the two forwards mean a rename or a failure to extract would fail the test rather than pass
vacuously.

## Artifact hygiene

TRX written with an explicit `LogFileName=`; host identifiers replaced in place; `<Counters>`
unmodified; empty vstest directories removed.
