---
name: breadcrumb-navigation-defects-439-440-498-499
description: Breadcrumb feature (#439/#440/#498/#499) research 2026-08-24 — fixing the lineage defect REGRESSES the percentage join and the filing target; #440 contradicts landed #400 AC-9
metadata:
  type: project
---

Research for `docs/features/active/breadcrumb-router-navigation-defects-498/` (2026-08-24, HEAD 988e819b).
Four counter-intuitive facts that cost real tracing time and are not visible from reading any one file.

**1. The percentage works BECAUSE the lineage is broken.** `BreadcrumbRowBuilder.BuildRow` joins the
probability on `segments[last].FullPath`. With the chain unresolved, that is the presented text — which
equals the `FolderScorer` key, so the join hits. Resolve the chain and it becomes the full Outlook path
while the score index stays keyed on the archive-relative stem, so **every suggestion row silently loses
its percentage**. The #439 potential feared the opposite. Same shape hits the filing target:
`SelectedFolderPath = row.LeafSegment?.FullPath` flips from stem to full path, and `EfcDataModel` passes
it as `DestinationOlStem` next to `OlAncestor = ArchiveRootPath`. Existing router tests do not catch
either, because the provider mock echoes the presented path back as the key.

**Why:** the presented row text is an archive-root-relative stem (`FolderPredictor.GetOlSubpath` against
`_globals.Ol.ArchiveRootPath`), while `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` compares against
the raw full `MAPIFolder.FolderPath`. The snapshot node's *other* field, `RelativePath`, does not help —
it is **store**-root-relative (`Archive\Projects\Alpha`), not archive-root-relative (`Projects\Alpha`).

**How to apply:** any future breadcrumb path-form work must carry explicit ACs for the percentage join AND
the filing target, and must use a `MockBehavior.Strict` path-form-sensitive provider mock so the RED test
fails for the intended reason.

**2. #440 contradicts a landed, checked-off acceptance criterion.** `#400 spec.md:247` AC-9 is `[x]`:
"Left and Right preserve the existing breadcrumb expand, collapse, and unhandled-key behavior". It is
enforced by a live test (`FolderBreadcrumbAssetContractTests.LeftAndRightBreadcrumbMessages_RemainSupported`).
#440 proposes changing exactly that. **How to apply:** treat this as a spec-level supersession decision,
never as an implementation detail.

**3. Efc and Qfc use DIFFERENT breadcrumb documents.** `FolderBreadcrumb.html` is Qfc only. The Efc surface
generates its document from `BreadcrumbDocumentAssets.BridgeJs` + `BreadcrumbHtmlRenderer`, whose separator
is `&gt;`, not `→`. Several #439 citations point at the wrong surface. **How to apply:** when a breadcrumb
report says "the html", establish which surface first.

**4. `_host.Raise(h => h.MessageReceived += null, _host.Object, json)` is the only seam that exercises the
`async void` host-event boundary** (`BreadcrumbBridgeRouterQueueTests.cs:201`). Every other test calls
`ProcessInboundAsync` directly and therefore cannot observe an escaping exception.

See also [[qfc-breadcrumb-webview2-351]], [[efcviewer-breadcrumb-webview2-349]],
[[folder-hierarchy-provider-350]], [[qfc-folder-tree-percentage-325]].
