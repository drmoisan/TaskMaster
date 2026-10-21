# `CaptureCurrent` Is Not Called From `WebView2BreadcrumbHost.cs` ([P5-T11])

Timestamp: 2026-08-27T23-27

Command:

```
Select-String -SimpleMatch -Pattern 'CaptureCurrent' -Path 'QuickFiler\Viewers\WebView2BreadcrumbHost.cs'
Select-String -SimpleMatch -Pattern 'DispatchValue'  -Path 'QuickFiler\Viewers\WebView2BreadcrumbHost.cs'
```

(run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Output Summary

`DispatchValue` returns **0 matching lines**, as `[P5-T10]` requires.

`CaptureCurrent` returns **1 matching line**, not the zero the task text anticipated. The single match
is inside a `//` comment and is not a call:

```
L255: // injected boundary would be discarded here. BreadcrumbUiDispatcher.CaptureCurrent() is
L256: // deliberately not used: it throws when SynchronizationContext.Current is null.
```

Measured with comment lines removed, the match count is **0**. The two lines above are the only
occurrence in the file and both begin with `//`, so stripping single-line comments removes the whole
match set.

## Reconciliation

The acceptance criterion this evidence supports reads: "`BreadcrumbUiDispatcher.CaptureCurrent()` is
not called from `WebView2BreadcrumbHost.cs`". That is a statement about a **call**, and it holds: the
only occurrence of the identifier in the file is a comment explaining why the member is deliberately
not used, which is the opposite of a call. The comment was written by `[P2-T8]` precisely to record
that design decision, so removing it to make a raw text search return zero would delete the
explanation the code-change policy asks for and would leave the search gate passing for a worse
reason.

The plan's evidence sentence for `[P5-T11]` states that the search returns "zero matching lines".
That expectation is not met literally. Both figures are recorded here — raw 1, comment-stripped 0 —
and the criterion is checked off on the comment-stripped reading plus the direct source reading below,
not on the raw count. This deviation is reported in the Phase 5 status summary.

## Direct source reading

`InitializeAsync` builds the dispatcher from its `uiSyncContext` argument, which is capture variant
V1, and does so only when no dispatcher was supplied through the internal constructor
(`WebView2BreadcrumbHost.cs:257-260`):

```csharp
if (_dispatcher == null)
{
    _dispatcher = new BreadcrumbUiDispatcher(uiSyncContext, LogDispatchFailure);
}
```

The token `new BreadcrumbUiDispatcher(uiSyncContext` is present exactly where the plan's Asserted
Literals list says it should be. No constructor of `WebView2BreadcrumbHost` gained a new throwing
precondition: the internal three-argument constructor throws only the two `ArgumentNullException`
guards on `control` and `initializer` that the public constructor already implied, and the
`dispatcher` parameter is explicitly permitted to be null.
