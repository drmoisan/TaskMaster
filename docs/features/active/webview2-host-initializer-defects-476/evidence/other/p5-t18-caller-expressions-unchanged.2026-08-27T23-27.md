# Seam Caller Expressions Are Textually Unchanged ([P5-T18])

Timestamp: 2026-08-27T23-27

Command:

```
git show 4f238289090e4c97ca505511a5a73e8092dce0f9:QuickFiler/Viewers/WebView2BreadcrumbHost.cs | grep -n -A4 "_initializer.CreateEnvironmentAsync\|_initializer.EnsureCoreWebView2Async"
grep -n -A4 "_initializer.CreateEnvironmentAsync\|_initializer.EnsureCoreWebView2Async" QuickFiler/Viewers/WebView2BreadcrumbHost.cs
git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD
```

EXIT_CODE: 0

## Output Summary

The acceptance criterion requires the `CreateEnvironmentAsync` and `EnsureCoreWebView2Async` member
signatures on `IWebViewCoreInitializer` to be unchanged and no in-repo caller and no Moq `Setup`
expression to be modified. There are four in-repo callers of either member. Three sit outside this
feature's writable set and are discharged by their absence from the change inventory. The fourth sits
inside `WebView2BreadcrumbHost.InitializeAsync`, which `[P2-T8]` did edit, so it is discharged here by
direct textual comparison rather than by assumption.

### The three external callers, discharged by absence

None of the following appears in this feature's own change set
(`git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD`), which is recorded in
full in `evidence/qa-gates/change-inventory.2026-08-27T23-23.md`:

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs`

Per the spec's Interface Contract Change section these three are the complete set of in-repo callers
of either member outside this feature's own files.

`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` does appear in the diff
against the recorded `BASELINE_SHA`, because the merged integration base modified it under feature
444's ownership. It does not appear in this feature's own change set, so this feature did not modify
it.

### The fourth caller, inside a file this feature modifies

Pre-change, at `BASELINE_SHA` `4f238289090e4c97ca505511a5a73e8092dce0f9`,
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs:108-112`:

```csharp
            CoreWebView2Environment environment = await _initializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await _initializer.EnsureCoreWebView2Async(_control, environment);
```

Post-change, `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:265-269`:

```csharp
            CoreWebView2Environment environment = await _initializer.CreateEnvironmentAsync(
                cacheFolder,
                options
            );
            await _initializer.EnsureCoreWebView2Async(_control, environment);
```

The two call expressions are **character-for-character identical**, including indentation and the
CSharpier line breaking. Only their line numbers moved, from 108-112 to 265-269, because `[P2-T8]`
inserted the dispatcher-installation block and XML documentation above them inside the same method.
The obligation "no in-repo caller is modified" is therefore discharged for the owned file by
measurement.

### Signatures

`QuickFiler/Viewers/IWebViewCoreInitializer.cs:49-52` and `:64` declare:

```csharp
        Task<CoreWebView2Environment> CreateEnvironmentAsync(
            string cacheFolder,
            CoreWebView2EnvironmentOptions options
        );
        Task EnsureCoreWebView2Async(WebView2 control, CoreWebView2Environment environment);
```

Both are unchanged from the pre-change file; `[P2-T15]` edited only the XML documentation around
them, which `evidence/other/p2-t15-interface-documentation.2026-08-27T20-38.md` records. A changed
signature would additionally have broken the eleven Moq `Setup` expressions across seven test files;
the full suite passed 6734 of 6734 with no `Setup` expression edited, recorded in
`evidence/qa-gates/qa-4-tests-coverage.2026-08-27T23-17.md`, and the analyzer rebuild recorded in
`evidence/qa-gates/qa-2-analyzers-rebuild.2026-08-27T23-14.md` compiled every one of those call sites
with zero errors.
