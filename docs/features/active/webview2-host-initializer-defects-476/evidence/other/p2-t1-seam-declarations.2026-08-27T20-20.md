# [P2-T1] — Seam Declarations, No Behaviour Change

Timestamp: 2026-08-27T20-20

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 0

## Output Summary

- Build summary: `0 Error(s)`, `5 Warning(s)` (the five pre-existing packaging advisories). The test
  assembly compiles again, so all eleven Phase 1 tests can now be observed failing at assertion time
  by `[P2-T2]`.
- Declared in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, with no behaviour change:
  - `private readonly BreadcrumbUiDispatcher? _dispatcher;`
  - `private bool _isAttached;`
  - `internal WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer, BreadcrumbUiDispatcher? dispatcher)`,
    carrying the two existing null guards and the existing event hookup verbatim, assigning
    `_dispatcher`, and setting `_isAttached = true`.
  - `internal bool IsAttached => _isAttached;`
  - `internal bool HasUiDispatcher => _dispatcher != null;`
- The **public** two-argument constructor's signature is unchanged; its body is now
  `: this(control, initializer, null) { }`. `QuickFiler/Controllers/EfcFormController.cs:836-839`
  therefore requires no edit and behaves identically.
- Per Decisions Record item 2, the internal constructor's third parameter is declared
  `BreadcrumbUiDispatcher?`. `?` is a nullability annotation, not a different parameter type, so the
  constructor signature `(WebView2, IWebViewCoreInitializer, BreadcrumbUiDispatcher)` required by the
  spec is satisfied. `WebView2BreadcrumbHost.cs:1` is `#nullable enable`, so the field is declared
  nullable and every use is null-checked.

## What this task deliberately did NOT do

- No owner registry and no `ConditionalWeakTable` (that is `[P2-T3]`).
- No change to `NavigateToString`, `PostMessageJson`, `InitializeAsync`, or `IsCoreInitialized`.
- The dead constructor-side `_control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted;`
  and its misleading comment are still present, so `[P2-T2]` observes the unfixed behaviour.
- `_isAttached` is set to `true` in the constructor but nothing ever sets it to `false` yet, so
  `SecondHost_DetachesThePredecessorAndTakesOwnership`, `PredecessorDetach_ToleratesNullCoreWebView2`,
  and `ControlDisposed_DetachesTheHost` will fail at assertion time rather than at compile time,
  which is the intended fail-before shape.
