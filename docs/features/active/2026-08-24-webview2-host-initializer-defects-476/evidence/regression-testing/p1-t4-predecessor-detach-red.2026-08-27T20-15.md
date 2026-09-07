# [P1-T4] [expect-fail] — Predecessor-Detach Regression Test, Compile-Red State

Timestamp: 2026-08-27T20-15

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `4 Error(s)`, `5 Warning(s)`.
- `[TestMethod] public async Task SecondHost_DetachesThePredecessorAndTakesOwnership()` exists in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at line 135.
- The build is red with at least one error naming `IsAttached`, which is what this task requires.

### Compiler errors, recorded verbatim (`<repo-root>` substituted for the workspace root)

```
<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(157,26): error CS1061:
'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible extension
method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be found (are
you missing a using directive or an assembly reference?)
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]

<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(162,26): error CS1061:
'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible extension
method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be found (are
you missing a using directive or an assembly reference?)
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]

<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(45,33): error CS1729:
'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]

<repo-root>\QuickFiler.Test\Viewers\WebView2BreadcrumbHostTests.cs(95,33): error CS1729:
'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments
[<repo-root>\QuickFiler.Test\QuickFiler.Test.csproj]
```

The two `CS1061` errors are new in this task and name `IsAttached` directly. The two `CS1729` errors
carry over from `[P1-T3]`. The `IsAttached` errors are reported despite the pre-existing `CS1729`
errors because they sit in a different method body, so the missing seam is genuinely observed rather
than masked.

### Test design points

- One `WebView2` control is constructed on `WinFormsPumpHost`; host A and then host B are constructed
  over that single control, both on the pump thread.
- Both hosts are constructed through the existing **public two-argument** constructor, so this test
  contributes no `CS1729` of its own; the only errors it introduces are the two `IsAttached` lookups.
- The primary assertions are `first.IsAttached == false` and `second.IsAttached == true` — statements
  about the hosts' own attachment state. No reflection-based assertion on the raw handler count of
  `WebView2.CoreWebView2InitializationCompleted` is used, because whether the SDK implements that
  event as a field-like backing delegate or through a WinForms `EventHandlerList` is unverified.
- The control is disposed on the pump thread in a `finally` block.
