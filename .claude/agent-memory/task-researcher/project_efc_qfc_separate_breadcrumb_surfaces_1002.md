---
name: efc-qfc-separate-breadcrumb-surfaces-1002
description: EFC and QFC folder pickers are two unrelated WebView2 surfaces; #997 descendant code never reaches EFC; node .mjs suites are not in TaskMaster CI
metadata:
  type: project
---

EFC folder area = `QuickFiler.Controllers.BreadcrumbBridgeRouter` + UtilitiesCS `BreadcrumbRow`/`BreadcrumbHtmlRenderer`/`BreadcrumbDocumentAssets.BridgeJs` (C# string JS, NavigateToString). QFC = `FolderBreadcrumb.html/.js` + `FolderBreadcrumbBridgeRouter` + `BreadcrumbSelectionSession` + `BreadcrumbDescendantNode` (internal; UtilitiesCS grants no InternalsVisibleTo QuickFiler). They share only `BreadcrumbRowBuilder.BannerPrefix`.

**Why:** #1002 research (2026-10-22) found a "port the QFC fix to EFC" request means building EFC's own keyed tree; also EFC `BindRowsAsync` clears selection then re-navigates, so a selection posted as a `render` message right after a bind can race the navigation.

**How to apply:** for any EFC/QFC "same fix" request, check which surface first; preselect inside the bind (rendered document) rather than post-bind messages; `tests/quickfiler-breadcrumb/*.mjs` give no CI signal, so pair them with C# text-contract tests. See [[percoverage-epic-136]] for QuickFiler coverage context.
