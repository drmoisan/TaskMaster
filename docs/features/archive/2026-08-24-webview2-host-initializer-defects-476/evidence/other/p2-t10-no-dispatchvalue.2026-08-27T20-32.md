# [P2-T10] — `DispatchValue` Is Not Used by the Host

Timestamp: 2026-08-27T20-32

Command:
```
Select-String -SimpleMatch -Pattern 'DispatchValue' -Path 'QuickFiler\Viewers\WebView2BreadcrumbHost.cs'
```
(run through `pwsh -NoProfile` from the workspace root)

EXIT_CODE: 0

## Output Summary

Search output, verbatim: **no matching lines**. `@($result).Count` = `0`.

`BreadcrumbUiDispatcher.DispatchValue` is therefore not used anywhere in
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, neither as a call nor as a textual mention.

### One correction was required to reach zero, and is recorded rather than concealed

The first run of this search returned **one** match:

```
        /// deliberately not performed as a separate <c>DispatchValue</c> step: that overload runs
```

That line is an XML documentation comment added by `[P2-T7]` explaining why the value-returning
overload is deliberately avoided. It was a documentation mention, not a use, so the spec's acceptance
criterion ("`BreadcrumbUiDispatcher.DispatchValue` is not used anywhere in
`WebView2BreadcrumbHost.cs`") was already satisfied — but the plan's mechanical gate requires zero
matching lines, and the plan lists `DispatchValue` among the tokens that must stay absent from this
file. The comment was reworded to "a separate value-returning dispatch step", which preserves the
explanation and its reasoning while satisfying the gate as written. No executable code changed.

### Why the overload is avoided

`DispatchValue<T>` runs inline only when `ReferenceEquals(_executingDispatcher, this)` holds
(`BreadcrumbUiDispatcher.cs:166`) — that is, only from inside an already-executing `Dispatch`
callback. Called outside one on an owner-thread-only test dispatcher it returns a faulted task
(`BreadcrumbUiDispatcher.cs:180-188`). Reading the `CoreWebView2` and posting inside one `Dispatch`
callback both avoids that trap and matches the established precedent in the sibling adapter.
