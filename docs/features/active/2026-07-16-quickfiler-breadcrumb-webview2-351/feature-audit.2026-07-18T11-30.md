# Feature Audit — quickfiler-breadcrumb-webview2 (Issue #351)

- Feature folder: `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351`
- Work mode: `full-feature` → AC sources: `spec.md` (AC-1..AC-13) and `user-story.md` (US-1..US-8), plus spec Definition of Done
- Base: `8e242692` → Head: `c80ec54a`
- Date: 2026-07-18T11-30

## Scope and Baseline

Audit is against the full branch diff vs. the epic integration base `8e242692`. Runtime-behavior
ACs are host-bound: the delivering environment has no live Outlook host or interactive desktop, so
runtime capture is structurally impossible. Per the CLAUDE.md COM/VSTO host-bound exemption practice
and the plan's explicit authorization, these ACs are delivered in code, pinned by named deterministic
unit tests, and recorded in schema-valid structural-impossibility dossiers
(`WhyRuntimeCaptureImpossible` + per-item alternative-proof mapping + `MANUAL-VERIFICATION-REQUIRED: yes`).
They are evaluated **PARTIAL** (delivered and unit-pinned; live verification pending maintainer),
which is not a Blocking finding. Each cited unit test was confirmed to exist in the test assemblies.

## Acceptance Criteria Inventory

- spec.md: AC-1 .. AC-13 (13 criteria)
- user-story.md: US-1 .. US-8 (8 criteria)
- spec.md Definition of Done: 5 items

## Acceptance Criteria Evaluation

### Spec ACs

| AC | Verdict | Evidence |
|---|---|---|
| AC-1 (CboFolders → WebView2 breadcrumb in live ItemViewer; dead variants unchanged) | PASS | `ItemViewer.Designer.cs` swaps CboFolders for WebView2 in the `_l0vh_Tlp` cell; init via `IWebViewCoreInitializer`+`NavigateToString` (`QfcItemController.ViewerSetup.cs`, `ItemViewer.Breadcrumb.cs`); G8 PASS (nine dead variants untouched); `guardrail-verification.2026-07-18T10-08.md`. |
| AC-2 (single-line breadcrumb anchored at leaf, order from 9101 chain) | PARTIAL | Delivered in `BreadcrumbRenderProjection`/`FolderBreadcrumb.html`; unit-pinned by `Project_FullChainSuggestionRow_RendersOrderedSegmentsArrowsAndPercent`, `Project_TruncationEligibility_MarksInteriorSegmentsOnly`; runtime dossier `percentage-visibility-postfix`/`breadcrumb-runtime-interaction`. Live render pending. |
| AC-3 (leaf plus/minus affordance only when leaf has subfolders) | PARTIAL | `Project_LeafWithoutSubfolders_RendersNoAffordance`, `Project_LeafAffordance_PlusWhenClosedMinusWhenOpen`, `LeafHasSubfolders_TrueOnlyWhenLeafSegmentHasChildren`; `breadcrumb-runtime-interaction.2026-07-18T10-13.md` item (d). Live render pending. |
| AC-4 (non-leaf double-click collapse; plus re-expand) | PARTIAL | `Route_SegmentDoubleClick_ProducesCollapsedRenderPayload`, `CollapseAfter_NonLeafSegment_HidesDownstreamAndClosesLeafExpansion`, `ReExpand_AfterCollapse_RestoresTheFullChain`; page `dblclick`/`affordanceToggle` handlers. Dossier item (c). Live interaction pending. |
| AC-5 (expand lists real immediate Outlook subfolders via 9101; FolderHierarchyBuilder.Build removed) | PARTIAL | `Route_AffordanceToggleExpand_QueriesProviderAndReturnsRenderPlusResponse`, `ExpandComposition_SegmentKey_ListsRealImmediateSubfolders`; `FolderHierarchyBuilder.Build` no longer called in production (grep: comments only). Live query pending. Dossier item (e). |
| AC-6 (percentage-obscuring repro before fix; CSS fix; post-fix evidence) | PARTIAL | CSS fix present in `FolderBreadcrumb.html` (`.pct` `margin-left:auto; flex-shrink:0; min-width:5ch`; `.crumbs` truncation); repro/post-fix captures structurally impossible — `percentage-visibility-postfix.2026-07-18T10-12.md` and `regression-testing/percentage-obscuring-analysis.2026-07-18T08-53.md` dossiers; parity-pinned by `Project_PercentFormatting_MatchesPercentageFormatterParity`. Live screenshot matrix pending. |
| AC-7 (JS↔.NET bridge: double-click/plus-minus/arrows; routes subfolder query; legacy fall-through) | PARTIAL | `IWebViewMessenger`/`WebView2Messenger`/`BreadcrumbBridgeRouter`/`BreadcrumbBridgeCoordinator`; `Route_RightArrow_ExpandsWhenExpandable`, `Route_*Arrow_*ReportsUnhandled*`, coordinator `UnhandledRightArrow_RaisesUnhandledArrowRight`; fall-through wired to `KeyboardHandler.BreadcrumbArrowFallThrough`. Dossier item (a). Live keyboard pending. |
| AC-8 (selection contract: GetSelectedFolder full-path/verbatim incl. "Trash to Delete"; Path A + Path B) | PARTIAL | `GetSelectedFolder()`→coordinator; `GetSelectedFolder_SuggestionRow_YieldsTheLeafFullPath`, `TrashToDelete_IsReturnedByteIdentical`, `SetItems_PlainRows_RenderVerbatimIncludingTrashToDelete`; consuming sites textually unchanged. `selection-contract-runtime.2026-07-18T10-14.md`. Live filing pending. |
| AC-9 (no third-party control; no new NuGet) | PASS | G1/G2 PASS; `packages.config` diff 0 lines; only new control is WebView2. |
| AC-10 (scoring/ranking unchanged) | PASS | G3 PASS; scoring sources diff empty. |
| AC-11 (host-neutral core unit-tested without live Outlook/WebView2; I/O only via 9101 seam) | PASS | 114 MSTest+Moq+FluentAssertions tests; core in `UtilitiesCS.OutlookObjects.Folder`; `final-qc-test-coverage.2026-07-18T10-50.md`. |
| AC-12 (full toolchain green; new code >= 90% line; changed lines no coverage loss; Compile Include) | PASS | csharpier/analyzer/nullable/vstest all EXIT 0; new-code 98.18%; no changed-line regression; all new files have `<Compile Include>`. `coverage-delta-verification.2026-07-18T11-15.md` VERDICT: PASS. |
| AC-13 (9101 contract reconciled; ASSUMED-PENDING-9101-MERGE resolved) | PASS | `9101-contract-reconciliation.2026-07-18T08-55.md` — RECONCILIATION: DIRECT-CONSUME; merged `IFolderHierarchyProvider` consumed directly. |

### User-Story ACs

| US | Verdict | Evidence |
|---|---|---|
| US-1 (suggestion as single-line breadcrumb anchored at leaf) | PARTIAL | Same basis as AC-2; runtime dossier, MANUAL-VERIFICATION-REQUIRED. |
| US-2 (leaf plus/minus only when subfolders exist) | PARTIAL | Same basis as AC-3. |
| US-3 (double-click collapse; plus restores) | PARTIAL | Same basis as AC-4. |
| US-4 (expand lists every real immediate subfolder) | PARTIAL | Same basis as AC-5. |
| US-5 (percentage always fully visible across themes/paths/rows/scaling) | PARTIAL | Same basis as AC-6; live screenshot matrix pending. |
| US-6 (Left/Right arrows work with legacy fall-through) | PARTIAL | Same basis as AC-7. |
| US-7 (selection files to shown full path; search + "Trash to Delete" unchanged) | PARTIAL | Same basis as AC-8. |
| US-8 (percentages themselves unchanged; only presentation changes) | PASS | G3 PASS + `Project_PercentFormatting_MatchesPercentageFormatterParity`; scoring untouched. |

### Definition of Done

| Item | Verdict | Evidence |
|---|---|---|
| All ACs checked off with evidence | PARTIAL | AC-2..AC-8 remain PARTIAL/unchecked pending live verification. |
| Tests added (positive/negative/edge/error) | PASS | 114 tests across state model, router, projection, selection map, coordinator. |
| Runtime evidence (reproduction and post-fix) committed | PARTIAL | Structural-impossibility dossiers committed in lieu of runtime captures; live captures pending maintainer. |
| Docs updated (spec, user-story, issue) | PASS | All three updated with AC-evidence sections. |
| Toolchain pass completed with commands reported | PASS | Four-stage single clean pass, EXIT 0, commands recorded in `final-qc-*` evidence. |

## Check-off Actions

No source-file check-off changes were made this cycle. Every AC evaluated **PASS** (spec AC-1, AC-9,
AC-10, AC-11, AC-12, AC-13; US-8) is already marked `[x]` in its source file. All PARTIAL criteria
(spec AC-2..AC-8; US-1..US-7) remain `[ ]` pending live-add-in verification, consistent with the
acceptance-criteria-tracking rule (only PASS items are checked off).

## Definition-of-Done Verdict

Delivered and policy-compliant in code and unit tests. The only outstanding items are the
host-bound runtime behaviors, which require maintainer verification in the live add-in and are not
Blocking under the established host-bound exemption practice.

### Acceptance Criteria Status
- Source: `spec.md` (AC-1..AC-13), `user-story.md` (US-1..US-8)
- Total AC items: 21 (13 spec + 8 user-story)
- Checked off (delivered/PASS): 7 (AC-1, AC-9, AC-10, AC-11, AC-12, AC-13, US-8)
- Remaining (unchecked / PARTIAL — runtime host-bound): 14 (AC-2..AC-8, US-1..US-7)
- Items remaining: AC-2, AC-3, AC-4, AC-5, AC-6, AC-7, AC-8, US-1, US-2, US-3, US-4, US-5, US-6, US-7 — each delivered in code, unit-pinned, and recorded in a schema-valid structural-impossibility dossier; pending live-Outlook verification by the maintainer.

Blocking findings: 0
