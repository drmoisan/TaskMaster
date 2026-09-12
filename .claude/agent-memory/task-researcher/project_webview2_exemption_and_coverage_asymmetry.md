---
name: webview2-exemption-and-coverage-asymmetry
description: F13/#455 findings — class-level vs method-level [ExcludeFromCodeCoverage] lambda asymmetry; CLAUDE.md §UT2's three grounds do not cover WebView2/third-party SDK adapters; which WebView2 SDK types are constructible in tests
metadata:
  type: project
---

Three non-obvious findings from epic #136 child F13 (issue #455) research, 2026-08-07.

**1. `[ExcludeFromCodeCoverage]` lambda-suppression asymmetry (measured, not assumed).**
A *method-level* attribute does NOT suppress instrumentation of lambdas nested inside the attributed
member — `BreadcrumbPopupUiOperations.cs:394` and `:457` are attributed, yet source lines 406, 409
and 471-490 are instrumented and permanently uncovered (22 of that file's 24 uncovered lines).
A *class-level* attribute DOES suppress nested lambdas — `WebView2Messenger.cs` has four dispatcher
lambdas and is wholly absent from Cobertura under its class-level attribute at `:20`.

**Why:** decides whether an exempt production forwarder should be a method-level-exempt static
(leaves permanently-uncovered lambda lines in the denominator) or a dedicated class-level-exempt
adapter type (leaves nothing).
**How to apply:** when designing any coverage seam in this repo, prefer a small class-level-exempt
adapter TYPE over method-level-exempt static forwarders. Cite the two measurements above.

**2. `CLAUDE.md` §UT2's three exemption grounds do not cover third-party SDK adapters.**
The grounds are (a) VSTO lifecycle, (b) WinForms form-derived/Designer-generated, (c) Outlook
Interop event handlers without an injectable seam. `WebView2CoreInitializer`,
`WebView2BreadcrumbHost` and `WebView2Messenger` match none — they derive from nothing, are not
Designer-generated, and import no `Microsoft.Office.Interop.Outlook` type. Their existing
`[ExcludeFromCodeCoverage]` attributes rest on a ground that is not in the text.

**Why:** the epic reconciles policy as "exempt only the irreducible remainder" but supplies no new
ground; §UT2 is the only place grounds are enumerated.
**How to apply:** before accepting or writing any exemption rationale, check the file against the
three literal grounds. If none matches, say so and route a fourth-ground ratification to the ledger
child (F1) rather than deciding it locally. Proposed wording: "third-party SDK adapter with zero
branches and zero mutable state, where an interface seam already exists" — self-policing, because a
file with branches is disqualified. See [[quickfiler-percoverage-epic-136]].

**3. WebView2 SDK type constructibility in tests (package 1.0.4129.50).**
- `CoreWebView2InitializationCompletedEventArgs` has a **public `(Exception)` constructor** — it is
  directly constructible in a unit test. Surprising; enlarges what is reachable without a seam.
- `CoreWebView2WebMessageReceivedEventArgs` has **no public constructor**, non-virtual members, and
  a `Finalize()` override (native resource) — never construct it, not even via `GetUninitializedObject`.
- `CoreWebView2` / `CoreWebView2Environment`: no public ctor, no virtual members, unmockable by Moq;
  usable only as opaque `FormatterServices.GetUninitializedObject` tokens that are never dereferenced.
  No repo test has ever successfully subscribed to a `CoreWebView2` event on such a token.
- `CoreWebView2EnvironmentOptions` IS constructible (`new ...()` compiles in production today).

**How to apply:** use these to decide seam-versus-direct-test before proposing a refactor for any
WebView2-touching file. Verify against the Learn page whose `defaultMoniker` matches the exact
package version in `packages.config`, not a generic docs page.
