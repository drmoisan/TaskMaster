# Feature Audit — efcviewer-breadcrumb-webview2 (#349)

- Timestamp: 2026-07-18T09-54
- Branch: `feature/efcviewer-breadcrumb-webview2-349`
- Diff base: `8e242692` (merge-base with `origin/epic/folder-tree-breadcrumb-redesign-integration`)
- HEAD: `be6f38d1`
- Work mode: `full-feature`

## Scope and Baseline

AC sources per `full-feature` mode: `spec.md` (Definition of Done / Acceptance Criteria) and
`user-story.md` (Acceptance Criteria). Both files carry an identical 12-item list and are jointly
authoritative; verdicts below apply to both files. Baseline is the merge-base above; the audit
evaluates the full branch diff against tests and evidence artifacts under the feature `evidence/`
tree.

Manual-verification ground rule (epic mode, supplied by the orchestrator): live-Outlook runtime
verification is structurally unavailable to agents in this environment. Five ACs that rest on live
runtime observation are evaluated as PARTIAL (non-blocking, deferred to maintainer runtime QA per the
plan fallback) when the automated evidence (unit tests + geometry proof + documented fallback
records) is present and sound, and only as blocking when that automated evidence is missing or
contradicted. This rule governs verdict severity only, not audit scope.

## Acceptance Criteria Inventory

Twelve criteria, identical in `spec.md` and `user-story.md`:

1. Single-line breadcrumb per suggestion in the live EfcViewer via a WebView2 control replacing the TreeListView.
2. Leaf-only plus/minus affordance, gated on `HasSubfolders`.
3. Non-leaf double-click collapse-after with a re-expand plus.
4. Expand lists real immediate Outlook subfolders via the 9101 `IFolderHierarchyProvider` seam; no prefix-matching.
5. Prediction percentage always fully visible; runtime repro captured first, CSS fix after.
6. JS<->.NET bridge carries double-click, arrow keys, and the live subfolder query.
7. EfcViewer3 handled as a mechanical Designer-only swap/removal with no behavioral wiring.
8. No third-party WinForms tree/list control and no WPF/ElementHost; technology is WebView2.
9. Scoring/ranking unchanged; feature-324 percentage plumbing reused as-is.
10. Behavior parity preserved (focusSearch, Trash pseudo-row, `"===="` banners, `'F'`, dark mode).
11. Pure model/state-machine, bridge contracts, renderer, and router unit-tested (MSTest+Moq+FluentAssertions), >= 90% new modules, no new logic in EfcFormController.
12. Full C# toolchain passes single-pass; no banned APIs.

## Acceptance Criteria Evaluation

| # | Verdict | Blocking? | Evidence and reasoning |
|---|---|---|---|
| 1 | PARTIAL | No | Implementation delivered: `BreadcrumbRowBuilder` builds single-line breadcrumb rows from 9101 ancestor chains; `BreadcrumbHtmlRenderer` emits one `.row` per suggestion; Designer swap installs the WebView2 control; router delivers the generated document. Unit-verified by `BreadcrumbRowBuilderTests`, `BreadcrumbHtmlRendererTests`, and `BreadcrumbBridgeRouterTests.BindRowsAsync_WithInitializedHost_DeliversGeneratedDocument`. GAP: live-EfcViewer runtime observation is structurally unavailable (documented in `manual-parity-verification.2026-07-18T11-50.md`). Automated evidence present and sound -> PARTIAL, deferred to maintainer runtime QA. |
| 2 | PASS | No | Affordance gated on `HasSubfolders` in both the state model (`CanExpandLeaf`) and the renderer (`AppendLeafAffordance`). Tests: `RenderRowFragment_LeafWithSubfolders_EmitsPlusWhenCollapsedMinusWhenExpanded`, `RenderRowFragment_LeafWithoutSubfolders_EmitsNoAffordance`, `ToggleLeafExpanded_WithoutSubfolders_IsNoOp`. Verified in source (BreadcrumbHtmlRenderer.cs:186-197, BreadcrumbRow.cs:260-263). |
| 3 | PASS | No | `CollapseAfter` hides downstream segments and marks the terminal; the renderer emits the re-expand plus to the left of the now-terminal segment; `ReExpand`/`RightArrow` restore. Tests: `CollapseAfter_OnNonLeafSegment_HidesDownstreamAndMarksTerminal`, `ReExpand_AfterCollapse_RestoresFullBreadcrumb`, `RenderRowFragment_CollapsedState_RendersReExpandPlusAtTerminalSegment`, `SegmentDoubleClick_OnNonLeafSegment_CollapsesAndReRenders`. Verified in source (BreadcrumbRow.cs:104-148, BreadcrumbHtmlRenderer.cs:152-159). |
| 4 | PARTIAL | No | Subfolders sourced only via the 9101 seam (`ResolveLeafKeyAsync` + `GetImmediateSubfoldersAsync`) with `requestId` correlation; zero prefix-matching in the breadcrumb path (verified in `BreadcrumbRowBuilder`/`BreadcrumbBridgeRouter` source). Test: `LeafExpandToggle_IssuesSubfolderQueryAndPostsCorrelatedResult`; seam presence recorded in `phase0-9101-provider-gate.md`. GAP: live-Outlook subfolder listing (incl. a folder not among ranked suggestions) outstanding. Automated evidence sound -> PARTIAL. |
| 5 | PARTIAL | No | Fail-before dossier `fail-before-exception.2026-07-18T09-00.md` records the plan-authorized geometry proof (live repro structurally unavailable); the CSS fix (`.pct { flex: 0 0 auto; margin-left: auto }`, trailing item on every row) is implemented in `BreadcrumbDocumentAssets` and unit-asserted by `RenderRowFragment_EveryRowKind_EmitsTrailingPctFlexItem` + `RenderRowFragment_CollapsedRow_StillEmitsTrailingPercent`; the defect mechanism (fixed unscaled ColumnHeader widths) is removed by the Designer swap. GAP: runtime pass-after at minimum width outstanding (`percent-visible-pass-after.2026-07-18T11-45.md`, marked remediation-required). Deviation noted: the repro is a geometry proof under `evidence/regression-testing/` rather than a screenshot under `evidence/repro/`; this is plan-authorized given the environment constraint. Automated evidence present and sound -> PARTIAL, deferred to maintainer runtime QA. |
| 6 | PARTIAL | No | Bridge implemented end-to-end: `BridgeJs` posts inbound messages and applies `render`/`subfolderResult`; `WebView2BreadcrumbHost` wires `WebMessageReceived`/`PostWebMessageAsJson`/`NavigateToString`; codec round-trips verified by `BreadcrumbMessageCodecTests`; router interaction tests cover routing. GAP: live WebView2 round-trip verified only against `Mock<IBreadcrumbWebHost>`. Automated evidence sound -> PARTIAL. |
| 7 | PASS | No | `EfcViewer3.Designer.cs` swapped mechanically to the WebView2 control with both OLVColumns removed; `EfcViewer3.cs` diff is empty (byte-identical); zero construction sites / controller wiring. Evidence: `efcviewer3-mechanical-swap-verification.md`; verified against the branch diff. |
| 8 | PASS | No | No `<PackageReference>`/`<Reference>`/`packages.config` change in the diff (only `<Compile Include>` entries); the control is the already-referenced WebView2; zero `BrightIdeasSoftware` references remain in the Efc Designer files. No WPF/ElementHost introduced. |
| 9 | PASS | No | No scoring/ranking source touched (diff is confined to breadcrumb, viewer, controller-wiring, test, and csproj files). The renderer reuses `PercentageFormatter.FormatPercent`; probabilities join by full-path equality (`BuildRow_WithMatchingProbability_JoinsByFullPathEquality`). |
| 10 | PARTIAL | No | All eight parity behaviors are wired and unit-verified: `focusSearch` (`ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent` + controller wiring), SearchText down-arrow (`SelectFirstRow`), Trash pseudo-row selectable, `"===="` banners non-interactive and rejected (`RowSelected_OnBannerRow_IsIgnored`, `IsValidSelection` unchanged), `'F'` focuses the control, dark-mode re-theme (`ApplyTheme_Dark_ReDeliversDarkDocument`), leaf expand, selection feeds `SelectedFolder`. GAP: all eight runtime confirmations outstanding (`manual-parity-verification.2026-07-18T11-50.md`). Automated evidence sound -> PARTIAL. |
| 11 | PASS | No | 6 new test files, ~102 breadcrumb unit tests (MSTest+Moq+FluentAssertions, AAA), router against mocked provider/host. Per-module line coverage all >= 90% (100/100/100/98.02/97.87/96.90/95.83/95.56%). `EfcFormController` is wiring-only (net -36 lines); host adapter exempt with in-code justification. Verified against `phase9-final-tests-coverage.md`, `phase9-coverage-delta.md`, and source. |
| 12 | PASS | No | csharpier EXIT 0 + check clean; analyzer build EXIT 0 (0 errors/warnings); nullable/TreatWarningsAsErrors EXIT 0; vstest 4935/4935 passed with coverage. Banned-API scan zero hits (independently reconfirmed by reviewer grep). Evidence: the four `phase9-final-*.md` gates + `banned-api-scan.md`. |

## Acceptance Criteria Check-off

Reviewer check-off protocol applied. PASS-verdict items are checked in both source files; PARTIAL
items remain unchecked and are documented as gaps above.

- PASS (checked): AC2, AC3, AC7, AC8, AC9, AC11, AC12 — all seven were already `[x]` in both
  `spec.md` and `user-story.md`; no new check-off was required.
- PARTIAL (unchecked): AC1, AC4, AC5, AC6, AC10 — correctly remain `[ ]` in both source files per
  the acceptance-criteria-tracking rule (PARTIAL items are not checked off).
- No modifications were made to `spec.md` or `user-story.md` because the checkbox state already
  matches the evaluated verdicts (7 PASS checked, 5 PARTIAL unchecked).

## Acceptance Criteria Status

- Source: `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/spec.md` and `.../user-story.md`
- Total AC items: 12
- Checked off (delivered/PASS): 7 (AC2, AC3, AC7, AC8, AC9, AC11, AC12)
- Remaining (unchecked, PARTIAL): 5 (AC1, AC4, AC5, AC6, AC10)
- Items remaining: AC1 (live single-line render), AC4 (live subfolder listing), AC5 (runtime percent-visibility pass-after), AC6 (live bridge round-trip), AC10 (runtime behavior-parity confirmations)

## Summary

- 7 PASS, 5 PARTIAL, 0 FAIL. Every PARTIAL rests solely on the structurally-unavailable live-Outlook
  runtime observation; all automated deliverables, unit tests, and toolchain gates for those items
  are complete and green. Under the epic manual-verification ground rule, these five are non-blocking
  and deferred to maintainer runtime QA.
- No delivered code defect was found that would make any PARTIAL a blocking FAIL.
- Blocking count: 0.
