# P2-T5 — Call-site substitution audit

Timestamp: 2026-09-01T19-59
Command: `Select-String -Path 'QuickFiler/Controllers/QfcItemController.Initialization.cs' -SimpleMatch '<literal>'` for each of `InitializeWebViewGuardedAsync`, `InitializeWebViewAsync`, `.Unwrap()` and `ContinueWith`, plus `git diff --numstat 988d35a8f8eb7436cc46a9f6424db917ed93807a -- QuickFiler/Controllers/QfcItemController.Initialization.cs`
EXIT_CODE: 0

## Base-ref substitution

The plan's stated `git diff` names `2b85134b42872e405602e6064e02dc9cda6c319b`. That SHA is superseded and is a stale ancestor rather than the current merge base, so `988d35a8f8eb7436cc46a9f6424db917ed93807a` was used instead. Rationale and supporting measurement: `evidence/baseline/p0-t7-base-ref.md`.

## `InitializeWebViewGuardedAsync` — exactly 3 matches, at lines 192, 288, 324

    192  _ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);
    288  _ = InitializeWebViewGuardedAsync();
    324  _ = InitializeWebViewGuardedAsync();

The line numbers match the three sites named by AC7 exactly. All three are executable code and all three are fire-and-forget discards, so no call site gained an `await` and the fire-and-forget latency invariant is preserved.

Site 192 requires no `.Unwrap()`. `_itemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher`, and a method group returning `Task` has no method-group conversion to `Action`, so overload resolution binds `InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task` and the discarded expression is a `DispatcherOperation<Task>`. Because the dispatched delegate is now an `async Task` method that catches `Exception`, the inner task cannot transition to `Faulted`, so the discarded operation carries no observable fault and unwrapping it would observe nothing.

## `InitializeWebViewAsync` — exactly 5 matches, at lines 165, 193, 200, 256, 345

    165  // InitializeWebViewAsync through the viewer's WPF dispatcher; both require a live message
    193  //Task.Run(() => InitializeWebViewAsync());
    200  // `await InitializeWebViewAsync()` is not completable in a unit test (the CoreWebView2
    256  await InitializeWebViewAsync();
    345  //    _ = InitializeWebViewAsync();

Of these five, **only line 256 is executable code**. Lines 165 and 200 are prose inside `//` comment blocks; lines 193 and 345 are commented-out code. Line 256 is the deliberately unchanged, already-observed site pinned by AC8 and verified independently in P2-T4.

The two counts are independent rather than nested. `InitializeWebViewAsync` is **not** a substring of `InitializeWebViewGuardedAsync` — the guarded name interposes `Guarded` between `InitializeWebView` and `Async` — so a fixed-string search for the shorter name cannot match the longer one. The observed results confirm this directly: line 192 appears under the guarded literal and does not appear under the unguarded one.

## No new observation construct was introduced

    .Unwrap()      0 matches
    ContinueWith   0 matches

Both return zero, so neither observation construct was introduced at any of the three sites. Together with the three discard forms above, this establishes that the fix routes fault observation through the guard rather than through a call-site continuation.

## Diff shape — exactly three added and three deleted lines

    git diff --numstat <base> -- QuickFiler/Controllers/QfcItemController.Initialization.cs
    3	3	QuickFiler/Controllers/QfcItemController.Initialization.cs

Three added and three deleted. The substitutions are net-zero-line replacements, which is why the file's line count is unchanged at 489 and why every line citation in this plan remains valid after Phase 2. No `#670` comment was added at any of the three sites: adding one would have shifted subsequent line citations and would have broken this exact three-added/three-deleted gate.

The gate is discriminating in both directions. The same command shape returns empty output for a genuinely unmodified file — demonstrated against `ViewerSetup.cs` in P2-T4 — and returns a populated row here; a fourth substitution, or an added comment line, would move the counts off `3 3`.
