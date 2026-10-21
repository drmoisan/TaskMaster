# Fail-before exception dossier — the `WebResourceRequested` detachment

Timestamp: 2026-08-26T10-56
Task: [P5-T12]
Subject: the one subscription among the #481 detachments that cannot carry a regression test.

## WhyFailingRunImpossible

The `WebResourceRequested` subscription is not made by a wire method. It is made inside
`QfcItemController.InitializeWebViewAsync()`, which carries a pre-existing
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute — at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:41` in the `<BASE_SHA>` source this feature
started from, and at `:47` in the delivered file after this feature's edits above it. That member reads
`((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`, which is `null` unless the real WebView2
runtime has initialized the control. Initializing it requires an external process, which the repository
unit-test policy (`.claude/rules/general-unit-test.md`, External Dependencies) prohibits. A unit test
therefore cannot reach the `+=` at all, and consequently cannot observe a `-=` failing to undo it. No
failing run of a regression test for this one subscription is constructible; research section 2.4
records the same barrier.

## Inspection proof

The capture assignments, quoted verbatim from the delivered
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`:

- `:33` — `private EventHandler<CoreWebView2WebResourceRequestedEventArgs> _webResourceRequestedHandler;`
- `:34` — `private CoreWebView2 _coreWebView2;`
- `:85` — `_coreWebView2 = ((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2;`
- `:92` — `_webResourceRequestedHandler = (sender, e) =>`
- `:107` — `_coreWebView2.WebResourceRequested += _webResourceRequestedHandler;`

The matching detach statement, introduced by `[P5-T10]`, quoted verbatim:

- `:488` — `if (_coreWebView2 != null && _webResourceRequestedHandler != null)`
- `:490` — `_coreWebView2.WebResourceRequested -= _webResourceRequestedHandler;`
- `:492` — `_webResourceRequestedHandler = null;`
- `:493` — `_coreWebView2 = null;`

Its enclosing method is `DetachWebResourceRequestedHandler`, declared at
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:486`. The delegate identity is exact: the same
field instance captured at `:92` and subscribed at `:107` is the operand of the `-=` at `:490`, so no
re-formed delegate can miss.

## Reachability under unit test — partial, not zero

The guarded `-=` statement at `:490` is unreachable under unit test, because `_coreWebView2` and
`_webResourceRequestedHandler` are assigned only inside `InitializeWebViewAsync`, whose
`.CoreWebView2` value is `null` without a live WebView2 runtime (research section 2.4). Both fields are
therefore `null` in every unit test and the guard at `:488` is never taken.

The method's entry, its null guard at `:488`, and its two field-nulling statements at `:492` and `:493`
**do** execute under unit test: `Cleanup()` calls `UnwireEvents()`, which calls
`DetachWebResourceRequestedHandler()` as its third statement (after `UnwireControlTreeEvents()` and
`UnwireIntentEvents()`), and `Cleanup()` is exercised by
`docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-pass.md`
(`Cleanup_NullsTrackedPrivateFields`, `Cleanup_ResetsInjectedHostForPooledViewerReuse`,
`Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow`, all `Passed`). The method is
therefore **partially** covered rather than zero covered. This is the fact `[P7-T7]` cites for
carve-out (b).

## Failing-run search (negative-evidence record)

SearchScope:
- `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/`
- `docs/features/active/qfc-item-controller-defects-484/evidence/`

SearchPatterns:
- `fail-before-exception.*.md`
- `*webresource*`
- `*481*`

SearchResult:
- `481-empty-bodies-fail.md` and `481-unguarded-fail.md` — fail-before runs for the control-tree and
  intent detachments, neither of which covers the `WebResourceRequested` subscription.
- No failing-run artifact exists for the `WebResourceRequested` subscription, and none is constructible
  for the reason stated under `WhyFailingRunImpossible`.
- No prior `fail-before-exception.*.md` existed under the search scope before this dossier was written.

Output Summary: The `WebResourceRequested` detachment cannot carry a fail-before regression test because
its `+=` sits inside the `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`, which needs a live
WebView2 runtime. Correctness is established by inspection: the same captured delegate and source used
for the `+=` at `ViewerSetup.cs:107` are the operands of the `-=` at `ViewerSetup.cs:490`, inside
`DetachWebResourceRequestedHandler` at `:486`, which `UnwireEvents()` calls unconditionally, and whose
entry, guard, and two nulling statements are executed by the passing `Cleanup()` tests.
