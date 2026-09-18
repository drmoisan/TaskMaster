# [P6-T1] AC-U6 structural gate — observed PASSING on the fixed tree

- Issue: #792
- Timestamp: 2026-09-17T20-51
- Command: `pwsh -NoProfile -WorkingDirectory <repo-root> -File coverage/plan792-helper.ps1` where `coverage/plan792-helper.ps1` was rewritten in place (convention 8) to hold the CMD-AC-U6-GATE block from the plan verbatim, character-identical to the block run in [P0-T14] (`<repo-root>` is the item worktree root; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `9ac987a969d1f4786d296fee25817c8e5dde9233`)
- EXIT_CODE: 0
- Output Summary: `AC-U6-STRUCTURAL: PASS` — one primary construction site (the contract file), zero direct `CoreWebView2Environment.CreateAsync` calls outside the adapter, three seam callers, three contract readers. Every line of the output matches the [P6-T1] declared post-fix output, including `git ls-files` ordering.

OBSERVED-FAILING: [P0-T14] (`$FEATURE/evidence/regression-testing/p0-t14-ac-u6-structural-fail-before.md`) recorded the identical script printing `PRIMARY-CONSTRUCTION-COUNT: 3`, `CREATEASYNC-OUTSIDE-ADAPTER: 1`, `SEAM-CALLER-COUNT: 2`, `CONTRACT-READER-COUNT: 0`, `AC-U6-STRUCTURAL: FAIL` on the unfixed tree at HEAD `11b107a55fc32078f97e0cd48f893c175be5b6f4`. The gate was therefore seen failing before it was seen passing. The gate text was not adjusted between the two runs.

## Gate output (verbatim, complete)

```
PRIMARY-CONSTRUCTION-COUNT: 1
PRIMARY-SITE: QuickFiler/Viewers/WebView2EnvironmentContract.cs:50
CREATEASYNC-OUTSIDE-ADAPTER: 0
SEAM-CALLER-COUNT: 3
SEAM-CALLER: QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs:46
SEAM-CALLER: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:66
SEAM-CALLER: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:279
CONTRACT-READER-COUNT: 3
CONTRACT-READER: QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs:40
CONTRACT-READER: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:57
CONTRACT-READER: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:264
AC-U6-STRUCTURAL: PASS
```

## Clause-by-clause comparison with the [P6-T1] declaration

| Declared | Observed | Match |
|---|---|---|
| `PRIMARY-CONSTRUCTION-COUNT: 1` | `PRIMARY-CONSTRUCTION-COUNT: 1` | true |
| one `PRIMARY-SITE: QuickFiler/Viewers/WebView2EnvironmentContract.cs:<n>` | `PRIMARY-SITE: QuickFiler/Viewers/WebView2EnvironmentContract.cs:50` (the `return new CoreWebView2EnvironmentOptions(AdditionalBrowserArguments);` statement) | true |
| `CREATEASYNC-OUTSIDE-ADAPTER: 0`, no `CREATEASYNC-SITE:` line | `CREATEASYNC-OUTSIDE-ADAPTER: 0`, no `CREATEASYNC-SITE:` line printed | true |
| `SEAM-CALLER-COUNT: 3` | `SEAM-CALLER-COUNT: 3` | true |
| three `SEAM-CALLER:` lines naming `EfcItemController.WebViewEnvironment.cs`, `QfcItemController.ViewerSetup.cs`, `WebView2BreadcrumbHost.cs` in that (`git ls-files`) order | `:46`, `:66`, `:279` in that order | true |
| `CONTRACT-READER-COUNT: 3` | `CONTRACT-READER-COUNT: 3` | true |
| three `CONTRACT-READER:` lines naming the same three files in the same order | `:40`, `:57`, `:264` in that order | true |
| `AC-U6-STRUCTURAL: PASS` | `AC-U6-STRUCTURAL: PASS` | true |

The `git ls-files` byte order places `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs` before `QuickFiler/Controllers/EfcItemController.cs` and both `Controllers/` entries before `Viewers/`, which is the order observed.

## Positive control (independent tool, before trusting the counts)

A separate Grep over `QuickFiler/**/*.cs` (no Designer exclusion, no comment filter) was run alongside the gate:

- Type name `CoreWebView2EnvironmentOptions`: 10 hits in 6 files — `IWebViewCoreInitializer.cs:17` (doc comment), `:51` (parameter); `WebView2BreadcrumbHost.cs:264`; `WebView2EnvironmentContract.cs:48` (return type), `:50` (the sole `new` construction); `WebView2CoreInitializer.cs:37`, `:69` (parameters); `EfcItemController.WebViewEnvironment.cs:25` (doc comment), `:40`; `QfcItemController.ViewerSetup.cs:57`. The two `///` doc-comment lines are removed by the gate's `^\s*//` filter; of the remaining eight, only `WebView2EnvironmentContract.cs:50` matches either construction regex (the three `options = WebView2EnvironmentContract.CreateOptions();` lines contain no `new`), which reproduces `PRIMARY-CONSTRUCTION-COUNT: 1` by an independent path.
- `\.CreateEnvironmentAsync\(`: exactly 3 hits, `EfcItemController.WebViewEnvironment.cs:46`, `WebView2BreadcrumbHost.cs:279`, `QfcItemController.ViewerSetup.cs:66` — the same three lines the gate lists.
- `WebView2EnvironmentContract\.CreateOptions\(`: exactly 3 hits, `EfcItemController.WebViewEnvironment.cs:40`, `QfcItemController.ViewerSetup.cs:57`, `WebView2BreadcrumbHost.cs:264` — the same three lines the gate lists.
- `CoreWebView2Environment\.CreateAsync\(`: 2 hits, `WebView2CoreInitializer.cs:72` (the adapter forward, excluded by the gate's adapter-path filter) and `QfcItemController.ViewerSetup.cs:119` (`//var task = ...`, excluded by the comment filter). No live call outside the adapter exists, so `CREATEASYNC-OUTSIDE-ADAPTER: 0` is a true zero and not a mis-scoped pattern. `EfcItemController.cs:195`, the live call reported by [P0-T14], is gone.

The dead comment lines `QfcItemController.ViewerSetup.cs:61` and `EfcItemController.cs:187` that [P0-T14] reported as excluded by the comment filter no longer exist on the fixed tree (the type-name Grep above returns no hit in `EfcItemController.cs` and no `//`-prefixed hit in `QfcItemController.ViewerSetup.cs`); the comment filter is still exercised by the two `///` doc-comment lines and by `QfcItemController.ViewerSetup.cs:119`.

Gate script not weakened: the block enumerates by type name over `git ls-files -- ':(glob)QuickFiler/**/*.cs'`, excludes Designer files and comment lines, and requires the construction regex; it is not a literal search for `new CoreWebView2EnvironmentOptions`.
