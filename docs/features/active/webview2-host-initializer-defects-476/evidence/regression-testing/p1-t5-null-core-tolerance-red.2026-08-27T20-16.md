# [P1-T5] [expect-fail] — Null-Core Detach Tolerance Test, Compile-Red State

Timestamp: 2026-08-27T20-16

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `5 Error(s)`, `5 Warning(s)`. The build is red.
- `[TestMethod] public async Task PredecessorDetach_ToleratesNullCoreWebView2()` exists in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at line 179.

### Complete distinct compiler error list, recorded verbatim

```
WebView2BreadcrumbHostTests.cs(45,33):  error CS1729: 'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
WebView2BreadcrumbHostTests.cs(95,33):  error CS1729: 'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
WebView2BreadcrumbHostTests.cs(157,26): error CS1061: 'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible extension method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be found (are you missing a using directive or an assembly reference?)
WebView2BreadcrumbHostTests.cs(162,26): error CS1061: 'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible extension method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be found (are you missing a using directive or an assembly reference?)
WebView2BreadcrumbHostTests.cs(206,26): error CS1061: 'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible extension method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be found (are you missing a using directive or an assembly reference?)
```

All five errors are in the one new test file. Line 206 is the `IsAttached` reference this task added;
the other four carry over from `[P1-T3]` and `[P1-T4]`. Every error path names a seam member
`[P2-T1]` declares. The full project path suffix `[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]`
was present on each line and is elided here for brevity; `<repo-root>` stands for the workspace root,
which is not written into committed artifacts.

### Test design points

- Host A is constructed over the control and never initialized, so `control.CoreWebView2` remains
  null and A never subscribed to `core.WebMessageReceived`.
- The act is the construction of host B, asserted through `NotThrowAsync` so that a dereference of a
  null `CoreWebView2` inside the detach path would surface as a test failure rather than as an
  unobserved exception.
- The test additionally asserts `first.IsAttached == false`, so tolerating the null core is not
  achieved by skipping the detach altogether.
- One control per test, constructed on `WinFormsPumpHost` and disposed on the pump thread in a
  `finally` block.
