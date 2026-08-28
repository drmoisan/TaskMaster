# [P1-T1] [expect-fail] — Argument-Guard Regression Tests, Red State

Timestamp: 2026-08-27T20-08

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException|FullyQualifiedName~CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException|FullyQualifiedName~EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException" "/Logger:trx;LogFileName=p1-t1-guard-tests-red.trx" /ResultsDirectory:docs\features\active\webview2-host-initializer-defects-476\evidence\regression-testing\p1-t1
```

EXIT_CODE: 1
ExpectedExitCode: 1

Resolved MSBuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

## Output Summary

- Build: `EXIT_CODE=0`, `0 Error(s)`, 5 pre-existing packaging warnings. `/t:Build` is authorized for
  this task by the plan's Execution Conventions because the task has just edited a source file, so
  `CoreCompile` cannot be skipped for `QuickFiler.Test`.
- Test run: **3 tests discovered, 3 failed, 0 passed.** `Total tests: 3` / `Failed: 3` /
  `Test Run Failed.` No `Passed:` line was emitted, so the passed count is zero.
- The results directory holds **exactly one** TRX file, `p1-t1-guard-tests-red.trx`, whose
  `<Counters>` element reads
  `total="3" executed="3" passed="0" failed="3" error="0" timeout="0" aborted="0"`.

### Failure observed for each test, recorded verbatim rather than assumed

| Test | Observed failure |
| --- | --- |
| `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | `Expected a <System.ArgumentNullException> to be thrown ... but no exception was thrown.` |
| `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | `Expected System.ArgumentException ... but no exception was thrown.` |
| `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | `Expected a <System.ArgumentNullException> to be thrown ... but found <System.NullReferenceException>` |

This is the pre-fix behaviour the plan anticipated without committing to a particular shape. The two
`CreateEnvironmentAsync` calls threw nothing at all: `CoreWebView2Environment.CreateAsync` began its
work and returned a task, and because the test invokes the member through an `Action` and never
awaits or otherwise observes the returned `Task`, no fault surfaced on the calling thread. The
`EnsureCoreWebView2Async` call produced the bare `NullReferenceException` with no parameter name that
issue #477 defect 2 describes. In all three cases the assertion failed, which is the expected red
outcome for this task.

### Test authoring notes

- The three tests were added to the existing `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs`,
  already registered at `QuickFiler.Test/QuickFiler.Test.csproj:170`, so **no `.csproj` edit was
  required** for them. (The research artifact and the plan cite that entry as line `:159`; the
  ItemGroup has since grown by eleven lines and the entry now sits at `:170`. The entry itself is
  unchanged.)
- Each test invokes the member through an `Action` whose body is `_ = initializer.Member(...);`, so
  the returned `Task` is discarded and never awaited. After the guards land in `[P2-T13]` the guard
  throws before any SDK call is reached, making the steady-state test fully host-neutral.
- `CreateEnvironmentAsync` is called with a null `options` argument in both tests, so no
  `CoreWebView2EnvironmentOptions` instance is constructed and the test touches no SDK type it does
  not have to. Per Decisions Record item 1, `options` is not guarded, so its value cannot affect
  which guard fires.
- MSTest `[TestClass]`/`[TestMethod]`, FluentAssertions with an explicit `because:` argument, and
  explicit `// Arrange` / `// Act` / `// Assert` comments are used. No mock is required: the subject
  is the concrete type and the guards take no collaborator.

### Artifact hygiene

The TRX was written with an explicit `LogFileName=` so it does not carry the default
`<account>_<HOST>_<timestamp>.trx` name. Host identifiers embedded in the TRX body by vstest were
replaced in place before commit: the workspace root with `REPO-ROOT`, the account name with `USER`,
and the machine name with `HOST`. Bracketed placeholders are not used inside the TRX because `<` is
not legal in an XML attribute value. The `<Counters>` element and every `outcome="Failed"` result are
unmodified. The empty `Deploy_*` deployment directory vstest created under the results directory,
whose name embeds the account and machine names, was removed.
