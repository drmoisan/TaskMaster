# Follow-Up Promotion Handoff — `EfcItemController` Bypasses the `IWebViewCoreInitializer` Seam ([P5-T40])

Timestamp: 2026-08-27T23-31

Command:

```
grep -n "CoreWebView2Environment.CreateAsync\|EnsureCoreWebView2Async" QuickFiler/Controllers/EfcItemController.cs
sed -n '218,240p' QuickFiler/Controllers/EfcItemController.cs
git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD
```

EXIT_CODE: 0

## No GitHub issue was created by the executor

**This executor created no GitHub issue and ran no promotion tooling.** Promotion through the feature
promotion lifecycle is an orchestrator action, not an executor action, and the issue-promotion tool
has no idempotent path, so a speculative promotion here could not be safely retried. This artifact is
the handoff record; the orchestrator decides whether and when to promote.

## The defect

`QuickFiler/Controllers/EfcItemController.cs` calls the WebView2 SDK directly instead of going
through the `IWebViewCoreInitializer` seam that this feature documents and guards. Confirmed by
direct search of the file at the current `HEAD`:

| Line | Statement | Nature |
| --- | --- | --- |
| 223 | `Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(null, cacheFolder, options);` | **Primary site.** Direct SDK environment creation, bypassing `IWebViewCoreInitializer.CreateEnvironmentAsync`. The spec cites this as `:223-227`; the statement spans lines 223 to 227 after CSharpier's argument wrapping. |
| 188 | `Task<CoreWebView2Environment> task = CoreWebView2Environment.CreateAsync(` | Earlier variant of the same pattern in a sibling method. The spec cites this as `:186-192`; the statement now begins at 188. |
| 201 | `_itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);` | Direct SDK call, bypassing `IWebViewCoreInitializer.EnsureCoreWebView2Async`. |
| 236 | `_itemViewer.L0v2h2_WebView2.EnsureCoreWebView2Async(_webViewEnvironment);` | Same, in the second method. |

The line numbers the spec records (`:223-227`, `:186-192`, `:201`, `:236`) resolve to the same four
statements. Two of the four have drifted by two lines relative to the spec's citation, because the
merged integration base at `9cb2c4f6` changed lines above them in a file this feature does not own.
The statements themselves are the ones the spec describes.

## Why it matters

The two `CoreWebView2Environment.CreateAsync` sites pass `null` for `browserExecutableFolder`, which
is the same Evergreen-only decision this feature has just documented on the interface. Because they
bypass the seam, they get none of the guards added under #477: a null or whitespace `cacheFolder`
there still produces whatever the SDK does rather than an `ArgumentNullException` naming the
parameter. They are also unmockable, so the surrounding logic in `EfcItemController` cannot be
unit-tested the way `QfcItemController.ViewerSetup` can. Any future move from the Evergreen runtime
to a fixed-version WebView2 distribution would have to change these four sites as well as the seam.

## Why this feature does not fix it

`QuickFiler/Controllers/EfcItemController.cs` is on this feature's forbidden-file list. The spec's
Scope & Non-Goals section places it outside the writable production set, and the acceptance criterion
for scope containment requires that it not be modified. It is absent from this feature's own change
set: `git diff --name-only origin/epic/quickfiler-bug-family-integration..HEAD` does not list it,
which is recorded in `evidence/qa-gates/change-inventory.2026-08-27T23-23.md`. Fixing it would also
widen the change beyond the three in-scope files and would collide with sibling feature 464, which
owns `Controllers\Efc*` in a concurrent worktree.

## Recorded in the spec

The defect is recorded in this spec's Cross-Feature Notes section
(`docs/features/active/webview2-host-initializer-defects-476/spec.md`, `## Cross-Feature Notes`),
which is where the acceptance criterion requires it to live.

## Requested orchestrator action

Promote this as a follow-up defect through the promotion lifecycle, scoped to
`QuickFiler/Controllers/EfcItemController.cs` only, and coordinate it against feature 464's ownership
of `Controllers\Efc*`. Suggested title: "EfcItemController bypasses the IWebViewCoreInitializer seam
with four direct WebView2 SDK calls".
