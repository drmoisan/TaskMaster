# [P0-T14] AC-U6 structural gate — observed FAILING on the unfixed tree

- Issue: #792
- Timestamp: 2026-09-17T18-48
- Command: `pwsh -NoProfile -WorkingDirectory <repo-root> -File coverage/plan792-helper.ps1` where `coverage/plan792-helper.ps1` holds the CMD-AC-U6-GATE block from the plan verbatim (`<repo-root>` is the item worktree root; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed)
- EXIT_CODE: 0
- Output Summary: `AC-U6-STRUCTURAL: FAIL` — three primary construction sites, one direct `CoreWebView2Environment.CreateAsync` outside the adapter, two seam callers, zero contract readers. Every line of the output matches the plan's declared unfixed-tree output.

OBSERVED-FAILING: AC-U6 structural gate

## Gate output (verbatim, complete)

```
PRIMARY-CONSTRUCTION-COUNT: 3
PRIMARY-SITE: QuickFiler/Controllers/EfcItemController.cs:188
PRIMARY-SITE: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:62
PRIMARY-SITE: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:250
CREATEASYNC-OUTSIDE-ADAPTER: 1
CREATEASYNC-SITE: QuickFiler/Controllers/EfcItemController.cs:195
SEAM-CALLER-COUNT: 2
SEAM-CALLER: QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:71
SEAM-CALLER: QuickFiler/Viewers/WebView2BreadcrumbHost.cs:265
CONTRACT-READER-COUNT: 0
AC-U6-STRUCTURAL: FAIL
```

No `CONTRACT-READER:` line was printed. The three `PRIMARY-SITE:` lines and the two `SEAM-CALLER:` lines appear in `git ls-files` order.

## Comment-filter observation

The dead comment lines `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:61` and `QuickFiler/Controllers/EfcItemController.cs:187` (both `// CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");`) were excluded by the `^\s*//` comment filter, so the gate counts the live target-typed construction at `ViewerSetup.cs:62` and not the dead comment above it; the commented-out `//var task = CoreWebView2Environment.CreateAsync(...)` at `ViewerSetup.cs:124` was excluded the same way. The adapter forward at `QuickFiler/Viewers/WebView2CoreInitializer.cs:72` was excluded by the adapter-path filter, which is why `CREATEASYNC-OUTSIDE-ADAPTER` is 1 and names only `EfcItemController.cs:195`.

## Positive control

Independent re-derivation with a separate tool before the gate ran (Grep over `QuickFiler/**/*.cs`) reached the same 3 + 2 dead-comment hits for the type name, the same 1 live + 1 adapter + 1 commented `CreateAsync` hits, the same 2 seam callers, and 0 occurrences of `WebView2EnvironmentContract` anywhere under `QuickFiler/`; the gate's zero contract-reader count is therefore a true zero rather than a mis-scoped pattern.
