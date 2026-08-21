# qfc-item-controller-webview-handler-unguarded-inputs (Issue #485)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-webview-handler-unguarded-inputs/ (Issue #485)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #485
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/485
- Last Updated: 2026-08-08
## Summary

The `WebResourceRequested` handler installed by `InitializeWebViewAsync` dereferences two
externally-supplied values without guarding them. Both throw inside a WebView2 event handler, where
the exception has no useful propagation path.

## Affected Code

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:81-102`

### 1. Unguarded `new Uri(...)` — line 83

```csharp
var requestedId = new Uri(e.Request.Uri).Segments.LastOrDefault()?.Trim('/');
```

`e.Request.Uri` is supplied by the WebView2 runtime from whatever the rendered mail body requested.
A malformed or non-absolute value throws `UriFormatException`. The result is only tested for
emptiness on the following line, so the guard is applied one step too late — the constructor has
already run. `Uri.TryCreate` is the appropriate form here.

### 2. Unguarded `new MemoryStream(match.AttachmentData)` — line 97

```csharp
e.Response = _webViewEnvironment.CreateWebResourceResponse(
    new MemoryStream(match.AttachmentData),
    ...
);
```

`match.AttachmentData` is not null-checked. `new MemoryStream(null)` throws
`ArgumentNullException`. `match` comes from `CidImageResolver.BuildContentIdMap(...)`, which is
built from `ItemHelper.AttachmentsInfo`; an attachment entry whose data failed to load yields a null
payload while still contributing a map entry, so the `TryGetValue` success at line 90 does not imply
a non-null `AttachmentData`.

## Why This Is a Defect

Both faults occur on the WebView2 message-handler path. An exception there does not surface to the
user as a handled error; it either terminates the handler silently (leaving the inline image
unrendered with no diagnostic) or escapes as an unhandled exception on a runtime callback thread,
depending on the host's dispatch behavior. Neither outcome is diagnosable from the logs, because
neither path logs.

## Suspected Fix

Replace line 83 with `Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri)` and return early
on failure. Add a null check on `match.AttachmentData` before constructing the stream, returning
early and logging at debug level so a missing attachment payload is visible.

## Severity

Low-Medium. No data loss. Causes silent failure to render inline images and produces an
undiagnosable exception on a callback thread.

## Related

- #463 — WebView2 `-–incognito` argument uses an en dash (same initialization method, already filed;
  this report does not duplicate it).

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows. Adding guards changes observable behavior on the failure path and must be scheduled with its
own regression tests.
