# Code Review — efcviewer-breadcrumb-webview2 (#349)

- Timestamp: 2026-07-18T09-54
- Branch: `feature/efcviewer-breadcrumb-webview2-349`
- Diff base: `8e242692` (merge-base with `origin/epic/folder-tree-breadcrumb-redesign-integration`)
- HEAD: `be6f38d1`

## Executive Summary

The change replaces the EfcViewer `TreeListView` matching-folders control with a WebView2-hosted
HTML/CSS/JS breadcrumb, plus a JS<->.NET message bridge. The implementation follows a clean
separation: pure, host-neutral logic (row model + state machine, HTML renderer, JSON codec, row
builder, message contracts) lives in UtilitiesCS; the bridge router and outbound queue are pure
QuickFiler classes tested against mocked seams; and the only host-bound code (the WebView2 adapter,
the WinForms form, the wholly-exempt controller wiring) is confined behind `[ExcludeFromCodeCoverage]`
seams with in-code justification. The diff shrinks the exempt `EfcFormController` by 36 lines while
adding coverage-bearing logic in non-exempt classes.

Reviewed for the specific behaviors named in the task: the row state machine, codec fail-fast, router
queue ordering and idempotent re-init, HTML encoding of folder names, the renderer percent-visibility
invariant, the wiring-only nature of `EfcFormController`, the Designer swaps, and the csproj
`<Compile Include>` wiring. No correctness defects were found. Findings are limited to low-severity
observations that do not affect correctness or policy compliance.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs | `CollapseAfter`, `LeftArrow`, `RightArrow`, `ToggleLeafExpanded` | Row state machine transitions are total and side-effect-scoped: banner/pseudo rows and empty-segment rows short-circuit to no-op; collapse-after rejects the leaf index and out-of-range; leaf toggle is gated on `CanExpandLeaf()` and suppressed while collapsed. No state-corruption path found. | None (confirming). | Matches spec state-machine invariants; verified against `BreadcrumbRowStateTests` scenarios (collapse-after, re-expand, leaf toggle no-op, arrow transitions). | BreadcrumbRow.cs:104-263; ac-verification-map AC2/AC3. |
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs | `DeserializeInbound` | Fail-fast is correct and specific: empty/whitespace, invalid JSON, unknown `type`, missing/wrong-typed `rowId`, missing `segmentIndex` on `segmentDoubleClick`, and empty `key` on `arrowKey` each log a specific log4net error and throw `BreadcrumbMessageException` (no silent swallow). | None (confirming). | Satisfies the spec validation rule and the fail-fast policy; the boundary catch in the router rethrows-by-suppress only after the codec has logged. | BreadcrumbMessageCodec.cs:72-182; BreadcrumbBridgeRouter.cs:187-198. |
| Info | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | `BindRowsAsync`, `ExpandLeafAsync` | Hierarchy is sourced only from the 9101 provider (`ResolveLeafKeyAsync` + `GetAncestorChainAsync`/`GetImmediateSubfoldersAsync`); there is no prefix-matching over suggestion rows. Provider I/O is wrapped with `OperationCanceledException` propagation on bind and specific-error-plus-unchanged-state on expand. | None (confirming). | Satisfies AC4 "no prefix-matching" and the I/O-isolation policy. | BreadcrumbBridgeRouter.cs:74-116, 285-362. |
| Info | QuickFiler/Controllers/BreadcrumbOutboundQueue.cs | `OnInitializationCompleted` | Outbound ordering and idempotent re-init are correct: payloads posted before init are FIFO-buffered and flushed in enqueue order; a duplicate completion with an empty buffer is a no-op, matching the pooled-viewer lifecycle. Event-driven only (no polling/timers/delays). | None (confirming). | Satisfies the pre-init queueing invariant and the banned-API/no-delay constraint. | BreadcrumbOutboundQueue.cs:37-65; BreadcrumbBridgeRouter.cs:140-149. |
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs | `AppendSegment`, `AppendPercent`, `AppendChildren`, `AppendLeafAffordance` | HTML encoding is applied to every dynamic value (`RowId`, segment `FullPath`/`DisplayName`, child names, formatted percent) via `WebUtility.HtmlEncode`. The percent is unconditionally emitted as the trailing `.pct` item on every row kind (banner, trash, suggestion, collapsed). Leaf affordance is emitted only when `LeafSegment.HasSubfolders`. | None (confirming). | Satisfies the HTML-encoding invariant, the always-visible-percent invariant (AC5 CSS side), and the affordance-gating invariant (AC2). | BreadcrumbHtmlRenderer.cs:119-223; BreadcrumbDocumentAssets.cs:21 (`.pct { flex: 0 0 auto }`). |
| Info | UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs | `BridgeJs` inbound listener | XSS surface is handled defensively on both sides: server-generated fragments are HtmlEncoded before delivery (`render` applies them via `outerHTML`/`innerHTML`), and provider-sourced child names are inserted via `textContent`/`title` (never markup). Row-id selector concatenation uses only server-generated `row-{i}` ids, not user data. | None (confirming). | No injection path from folder names or provider data. | BreadcrumbDocumentAssets.cs:94-113. |
| Info | QuickFiler/Controllers/EfcFormController.cs | `ConfigureBreadcrumbControl`, `BindBreadcrumbRowsAsync`, `DarkMode_Changed` | Controller changes are wiring-only: construct host adapter + router, connect `CoreInitialized`/`FocusSearchRequested` events, delegate binds and theme swaps to the router. No decision logic added; net -36 lines. `SelectedFolder` now derives from `_router.SelectedFolderPath` with `IsValidSelection` retaining its `"===="` rejection as a second guard. | None (confirming). | Satisfies "no new testable logic in EfcFormController" (AC11) and behavior-parity wiring (AC10). | EfcFormController.cs diff (constructor/event wiring); WebView2BreadcrumbHost.cs:29 exemption. |
| Info | QuickFiler/Viewers/EfcViewer.Designer.cs, EfcViewer3.Designer.cs | `FolderListBox` field + init | Designer swap replaces the `TreeListView`/two `OLVColumn`s with `Microsoft.Web.WebView2.WinForms.WebView2` in the same TableLayoutPanel cell (span 14, Dock=Fill). The fixed 3200/500 (and 1600/300) `ColumnHeader` widths — the ranked percent-obscuring cause — are removed. `EfcViewer3.cs` is byte-identical (empty diff), confirming the mechanical-only swap for the dead variant. | None (confirming). | Satisfies AC7 (mechanical EfcViewer3 swap) and removes the AC5 defect mechanism. | EfcViewer.Designer.cs / EfcViewer3.Designer.cs diffs; efcviewer3-mechanical-swap-verification.md. |
| Low | QuickFiler/Controllers/BreadcrumbBridgeRouter.cs | `OnHostMessageReceived` (async void) | The host message handler is `async void` with a boundary catch that suppresses `BreadcrumbMessageException` only (after the codec has logged). This is the correct and conventional pattern for an event handler over a message pump, but an unexpected non-codec exception in `ProcessInboundAsync` would propagate out of the `async void` and could surface on the UI pump. In practice the router's per-branch handlers each guard their own I/O, so no such path was found. | Consider an outer catch that logs-and-swallows unexpected exceptions at this UI-pump boundary, or document that all downstream paths are individually guarded. | Defense-in-depth for an `async void` UI-pump handler; not a current defect because every inner branch already guards its I/O. | BreadcrumbBridgeRouter.cs:187-198, 285-362. |
| Low | UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs | `BuildProbabilityIndex` vs `BreadcrumbBridgeRouter` chain dedup | Probability join keys use `StringComparer.OrdinalIgnoreCase`, while the router's presented-row dedup dictionary also uses `OrdinalIgnoreCase`; the ancestor `joinPath` equality is thus case-insensitive. This is consistent internally and matches the 9101 key's case-insensitive store/path equality, so no mismatch was found. | None; noting the case-sensitivity choice for future maintainers. | Consistency confirmed across builder, router, and the 9101 `FolderTreeNodeKey` equality. | BreadcrumbRowBuilder.cs:208-227; BreadcrumbBridgeRouter.cs:85-107; phase0-9101-provider-gate.md. |

## Design and Policy Observations

- Separation of concerns is clean: no WinForms/COM/WebView2 type appears in any coverage-bearing
  class; the router depends only on `IFolderHierarchyProvider` and `IBreadcrumbWebHost`.
- The message contracts are net48-safe plain classes with explicit constructors and null-guards;
  the outbound hierarchy uses an abstract base with sealed discriminated subclasses.
- Error handling is fail-fast at the codec and log-and-continue at the provider I/O boundaries, both
  consistent with the general and C# error-handling policies.
- csproj wiring adds `<Compile Include>` entries for all 7 new UtilitiesCS sources, 4 new QuickFiler
  sources, and 6 new test files across the four affected projects; no dependency additions.

## Verdict

No blocking or high/medium-severity findings. Two low-severity, non-blocking observations (async-void
boundary hardening; documented case-insensitivity). Code quality is consistent with repository
standards and the feature spec.
