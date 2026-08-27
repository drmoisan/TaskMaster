# [P1-T3] [expect-fail] — Marshalling Regression Tests, Compile-Red State

Timestamp: 2026-08-27T20-14

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```
(run through `pwsh -NoProfile` from the workspace root; MSBuild resolved through `vswhere` to
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`)

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `2 Error(s)`, `5 Warning(s)` (the five pre-existing packaging advisories).
- Both required `[TestMethod]` methods exist in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs`:
  `PostMessageJson_PostsExactlyOnceToTheUiContext` at line 32 and
  `NavigateToString_PostsExactlyOnceToTheUiContext` at line 82.
- The build fails with an error that names the `WebView2BreadcrumbHost` constructor arity, which is
  the outcome this task requires.

### Compiler errors, recorded verbatim

```
<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(45,33): error CS1729:
'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]

<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(95,33): error CS1729:
'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]
```

Two errors, one per test, both `CS1729` and both naming the arity. `<repo-root>` stands for the
workspace root, which is not written into committed artifacts. From this task onward the test
assembly does not compile, so `[P1-T3]` through `[P1-T8]` each record a compile-time red state and
`[P2-T2]` is the authoritative assertion-time fail-before record for the whole set of eleven tests.

### Compile Include registration

`<Compile Include="Viewers\WebView2BreadcrumbHostTests.cs" />` was inserted immediately after the
entry `[P1-T2]` added. `git diff` on `QuickFiler.Test/QuickFiler.Test.csproj` now reports
**2 insertions, 0 deletions**, contiguous, with no moved line and no re-sort. The anchor remains the
`Controllers\WebView2CoreInitializerTests.cs` entry; the line-number drift from the plan's cited
`:159` to the current `:170` is recorded in
`p1-t2-structural-test-red.2026-08-27T20-11.md`.

### Test design points that make the steady-state test host-neutral

- Each test constructs its own `WebView2` on `WinFormsPumpHost` via
  `pump.InvokeAsync(() => new WebView2())`, and constructs the host on the pump thread as well, so
  every touch of the control happens on the STA pump thread.
- The recording `SynchronizationContext` is a private sealed nested class that increments a counter
  in its `Post` override and **never invokes the queued callback**. Because the callback is never
  drained, the control is never touched by the dispatched work and no Evergreen runtime is involved.
- The recording context is never installed as the ambient `SynchronizationContext.Current` on the
  test thread. `BreadcrumbUiDispatcher.Dispatch` executes inline when
  `ReferenceEquals(SynchronizationContext.Current, _context)` holds
  (`BreadcrumbUiDispatcher.cs:269-272`), which would record zero posts and make the assertion
  vacuous.
- The member under test is called from the MSTest thread, not from the pump thread, which is the
  condition the defect describes.
- Each test disposes its control on the pump thread in a `finally` block before the pump host is
  disposed.
