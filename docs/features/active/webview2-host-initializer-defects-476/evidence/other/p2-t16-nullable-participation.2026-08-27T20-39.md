# [P2-T16] — Neither Initializer File Participates in Nullable Analysis

Timestamp: 2026-08-27T20-39

Command:
```
Select-String -SimpleMatch -Pattern '#nullable' -Path 'QuickFiler\Viewers\WebView2CoreInitializer.cs','QuickFiler\Viewers\IWebViewCoreInitializer.cs'
```
(run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Output Summary

Search output, verbatim: **no matching lines**. `@($result).Count` = `0` across both files.

Neither `QuickFiler/Viewers/WebView2CoreInitializer.cs` nor
`QuickFiler/Viewers/IWebViewCoreInitializer.cs` contains a `#nullable` directive of any form after
this feature's changes. Nullable participation in this repository is strictly per-file opt-in: there
is no `Directory.Build.props` and no `<Nullable>` element in `QuickFiler/QuickFiler.csproj`, so a file
without the directive is not conscripted into the `/p:TreatWarningsAsErrors=true` nullable gate.

Nullability in these two files is expressed only through runtime guards:

- `ArgumentNullException(nameof(cacheFolder))` and `ArgumentException` naming `cacheFolder` in
  `CreateEnvironmentAsync`;
- `ArgumentNullException(nameof(control))` in `EnsureCoreWebView2Async`;
- `<exception>` and `<param>` XML documentation on the interface recording those guards and recording
  which arguments are deliberately unguarded.

The third in-scope file, `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, **is** `#nullable enable` at
line 1 and therefore is in the gate; that its new code is nullable-clean is verified separately by
`[P2-T17]` and `[P4-T3]`.

The search is non-vacuous: the same command run against `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
returns a match on its line 1, so the pattern and the invocation form do find the directive when it
is present.
