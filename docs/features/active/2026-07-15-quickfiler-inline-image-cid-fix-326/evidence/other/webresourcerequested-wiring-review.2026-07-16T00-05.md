# WebResourceRequested Wiring Review — P2-T10

- **Timestamp:** 2026-07-16T00-05
- **Command:** `grep -n "AddWebResourceRequestedFilter\|WebResourceRequested" QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- **EXIT_CODE:** 0
- **Output:**

```
74:            coreWebView2.AddWebResourceRequestedFilter(
78:            coreWebView2.WebResourceRequested += (sender, e) =>
109:        // Minimal in-memory extension-to-MIME-type lookup for the WebResourceRequested handler
```

- **Output Summary:** Exactly one `AddWebResourceRequestedFilter` call (line 74) and exactly one
  `WebResourceRequested` event-handler *registration* (`+=` at line 78), both inside
  `InitializeWebViewAsync`. The third match at line 109 is an explanatory code comment above the
  `ResolveImageMimeType` helper, not an additional registration. The filter argument is
  `$"https://{CidImageResolver.DefaultVirtualHost}/*"` and the context argument is
  `CoreWebView2WebResourceContext.Image`, confirming scoping matches the acceptance criterion. This
  is the code-review confirmation for the AC covering `InitializeWebViewAsync`'s host-bound glue
  (not unit-testable, remains under the method's pre-existing `[ExcludeFromCodeCoverage]` attribute).

## Note on plan deviation (CidImageResolver accessibility)

`CidImageResolver` (P2-T5) was specified as `internal static class`, but this task (P2-T9) requires
`QfcItemController.ViewerSetup.cs` (assembly `QuickFiler`) to call `CidImageResolver.DefaultVirtualHost`
and `CidImageResolver.BuildContentIdMap(...)` directly. `UtilitiesCS`'s `AssemblyInfo.cs` grants no
`InternalsVisibleTo("QuickFiler")`, and adding one is outside the plan's listed production-file scope.
To satisfy both tasks without touching an out-of-scope file, `CidImageResolver` was made `public`
instead of `internal` (the class's own accessibility is the only viable minimal-footprint fix; no other
file was touched to resolve this). This deviates from the literal `internal` wording in P2-T5's task
text; it is recorded here as an explicit escalation for review.
