---
name: 349-breadcrumb-webview2-gotchas
description: "#349 EfcViewer breadcrumb swap: retyped Designer field breaks reflection test injection; async-state-machine classes hide coverage gaps; QuickFiler.Test has no Newtonsoft"
metadata:
  type: project
---

Three gotchas from executing #349 (EfcViewer TreeListView -> WebView2 breadcrumb), directly relevant to sibling epic child 9103 (QuickFiler breadcrumb, same epic):

1. Retyping the Designer `FolderListBox` field (TreeListView -> WebView2) compile-broke `EfcHomeControllerExecuteMovesTests`, which injected the removed private `_selectedNode` field via reflection. Fix: drive a REAL `BreadcrumbBridgeRouter` over `Mock<IBreadcrumbWebHost>` + `Mock<IFolderHierarchyProvider>` to a selected state (`BindRowsAsync` + `SelectFirstRow`) and inject `_router` instead.
2. Cobertura per-class rates hide async gaps: `BreadcrumbBridgeRouter` showed 91.6% at class level while its compiler-generated `<HandleLeafToggleAsync>d__` machine sat at 29%. Per-module >= 90% verification MUST aggregate the parent type plus its `<...>d__`/`<>c__DisplayClass` nested classes (line-level dedup). Scratchpad script pattern: fold `name -replace '\.<[^>]*>.*$'`.
3. QuickFiler.Test has NO Newtonsoft.Json reference (and must not gain one — Newtonsoft-consuming code lives only in UtilitiesCS). Router tests must assert on raw JSON payload strings (JSON-escape HTML fragments: `\` -> `\\`, `"` -> `\"`), not JObject.

**Why:** 9103 consumes the same 9101 provider surface (`FolderBreadcrumbSegment` with `Key`/`FolderPath`/`HasChildren`, key-based `GetAncestorChainAsync`/`GetImmediateSubfoldersAsync`, string-path bridge `ResolveLeafKeyAsync`) and will hit the same three traps.
**How to apply:** when a plan retargets a QuickFiler viewer's folder list to WebView2, pre-check reflection-based tests for removed private fields, aggregate nested-type coverage before claiming >= 90%, and keep bridge-JSON assertions Newtonsoft-free in QuickFiler.Test.
