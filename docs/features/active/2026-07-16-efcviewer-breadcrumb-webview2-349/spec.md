# efcviewer-breadcrumb-webview2 — Spec

- **Issue:** #349
- **Parent (optional):** Epic `folder-tree-breadcrumb-redesign` (integration branch `epic/folder-tree-breadcrumb-redesign-integration`; manifest child 9102, wave 1, band C4, `depends_on: [9101]`)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-16T23-45
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-feature
- **Primary research source:** `research/2026-07-16T22-30-efcviewer-breadcrumb-webview2-research.md`

## Overview

The EfcViewer matching-folders control currently renders folder suggestions as a conventional
indented multi-row tree using `BrightIdeasSoftware.TreeListView` (`QuickFiler/Viewers/EfcViewer.cs`,
`QuickFiler/Viewers/EfcViewer3.cs`, plus their Designer files). The intended design is a single-line
breadcrumb per suggestion anchored at the selected leaf. The current hierarchy is synthesized by
`FolderSuggestionTree.BuildFromRows` via prefix-matching over the top-ranked suggestion rows, so
expanding a folder does not reveal its real Outlook subfolders. The prediction percentage is also
reported as obscured at runtime even though static column/rect math shows no overlap; research §D
identifies unscaled `ColumnHeader` widths under WinForms font autoscaling
(`olvColumnFolder.Width = 3200` / `olvColumnPercent.Width = 500` authored at
`AutoScaleDimensions (12F, 25F)`, `EfcViewer.Designer.cs:915,921,4250`) as the primary candidate
cause: at ordinary runtime DPI the folder column exceeds the visible control width and pushes the
percent column outside the viewport.

`TreeListView` (as currently used) does not naturally support single-line breadcrumb rendering with
per-segment double-click collapse, and it is a VSTO/WinForms-hosting-specific investment that would
not carry forward to the planned VSTO migration. The redesign targets WebView2 (HTML/CSS/JS), which
is largely reusable across a post-VSTO UI stack and reuses a dependency already proven in this
codebase (QuickFiler's WebView2 message-body pane, including the `cid:` fix from feature 326;
Microsoft.Web.WebView2 SDK 1.0.3912.50 is already referenced by `QuickFiler.csproj`).

**Scope correction versus the source objective (research finding 1).** The objective's
parallel-implementation claim — that both `EfcViewer` and `EfcViewer3` require behavioral
conversion — was a starting assessment that the research corrected. `EfcViewer3` is dead code: the
only runtime instantiation of an Efc form viewer is `new EfcViewer()` at
`QuickFiler/Helper Classes/EfcViewerQueue.cs:83`; `EfcFormController` is typed to the concrete
`EfcViewer` (`QuickFiler/Controllers/EfcFormController.cs:34,44-50`); and `EfcViewer3.FolderListBox`
has no controller wiring (its Designer `AspectName` binding is the only binding). The behavioral
WebView2 conversion therefore targets `EfcViewer` + `EfcFormController` only. `EfcViewer3` and its
Designer receive at most a mechanical control swap or removal with no behavioral wiring.

## Behavior

Replace the `EfcViewer` matching-folders `TreeListView` with a WebView2-hosted HTML/CSS/JS
breadcrumb control that:

1. Renders each suggestion as a single-line breadcrumb `Folder -> SubFolder -> Leaf` anchored at
   the selected/predicted leaf.
2. Shows an expand affordance (plus when collapsed, minus when expanded) only on the leaf, and only
   when the leaf has subfolders (`HasSubfolders == true`); leaves without subfolders show no
   affordance.
3. Collapses a row on double-click of a non-leaf segment: everything after that segment
   (arrows, downstream segments, the original leaf) is hidden, and a plus appears to the left of
   the now-terminal segment; activating that plus re-expands the full breadcrumb.
4. On expand of a segment, lists every real immediate Outlook subfolder of that folder via the
   shared 9101 live folder-hierarchy provider (ancestor-chain call plus on-demand
   immediate-subfolders call behind an injectable Outlook seam, research §C.3). It must NOT
   prefix-match over suggestion rows.
5. Keeps the prediction percentage always fully visible: a runtime reproduction of the current
   obscuring defect is captured first (stored under this feature's `evidence/repro/` tree), then a
   CSS-based fix is applied (percent rendered as a trailing fixed, non-shrinking flex item; only
   the breadcrumb path may truncate with ellipsis — research §D.4).
6. Carries double-click and left/right-arrow keyboard interaction, and routes the live subfolder
   query, across the WebView2 boundary via a JS<->.NET event bridge
   (`window.chrome.webview.postMessage` -> `CoreWebView2.WebMessageReceived`;
   `PostWebMessageAsJson` / `NavigateToString` outbound). This bridge is entirely novel in this
   codebase (research finding 2: zero production hits for `WebMessageReceived`,
   `PostWebMessageAsJson`, `ExecuteScriptAsync`, `AddHostObjectToScript`).
7. Introduces no third-party WinForms tree/list control and no WPF/`ElementHost`, and makes no
   change to the scoring/ranking algorithm; the feature-324 percentage plumbing
   (`FolderRow.Score` -> `PercentageFormatter.FormatPercent`) is reused as-is.

`EfcViewer3` handling: mechanical Designer-only control swap (or removal of the dead variant) with
no behavioral wiring. The epic non-goal forbids unifying the two implementations; it does not
forbid removing the dead one.

### Behavior parity to preserve (research §B.4/§B.5)

The following current `EfcFormController` behaviors must survive the control swap:

- Up-arrow at the top row focuses `SearchText` (`FolderListBox_KeyDown`,
  `EfcFormController.cs:416-419`); over the bridge this becomes an outbound `focusSearch` message.
- The `"Trash to Delete"` pseudo-row prepended by `ActionDeleteAsync`
  (`EfcFormController.cs:762-770`) remains a selectable row.
- `"===="`-prefixed banner rows remain non-interactive and are rejected by `IsValidSelection`
  (`EfcFormController.cs:1076-1088`).
- The `'F'` keyboard action jumps focus to the folder list (`EfcFormController.cs:601-605,662-666`)
  — now focusing the WebView2 control.
- Dark-mode re-theming (`DarkMode_Changed`, `EfcFormController.cs:702-716`) — the breadcrumb
  supplies its own dark/light CSS switch (precedent: `NavigateToString(ItemHelper.ToggleDark(...))`
  in `QfcItemController.FocusAndTheme.cs:293`).
- `SearchText` down-arrow moves focus into the list and selects the first row
  (`EfcFormController.cs:406-412`).
- Selection continues to feed `SelectedFolder` (filing, folder creation, find-mode open), now
  derived from bridge-reported selection.

## Inputs / Outputs

- **Inputs**
  - Suggestion rows: `FolderPredictor.FolderArray` (legacy sectioned `string[]`) and the typed
    mirror `FolderRowArray` producing `FolderRow { Text, Kind, Score? }`
    (`UtilitiesCS/OutlookObjects/Folder/FolderRow.cs`), with probabilities from
    `FolderScore.Probability` — unchanged upstream.
  - 9101 provider results: ordered root-to-leaf ancestor chain per suggestion leaf, and on-demand
    immediate subfolders per segment, each segment carrying `FullPath`, `DisplayName`,
    `HasSubfolders` (assumed consumer surface, research §C.3).
  - Inbound bridge messages (JSON) from the hosted document: segment double-click, leaf
    expand/collapse toggle, arrow-key, row-selected.
- **Outputs**
  - The generated HTML document (inline CSS/JS) delivered via `NavigateToString`.
  - Outbound bridge messages (JSON): render/update, subfolder query results (correlated by
    `requestId`), `focusSearch`.
  - `SelectedFolder` (full path) surfaced to the existing filing / folder-creation / find-mode
    paths, unchanged in meaning.
  - Evidence artifacts: runtime reproduction of the percentage defect (screenshot plus geometry
    diagnostic log) under
    `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/repro/`, named
    with ISO-8601 timestamps.
- **Config keys and defaults:** none added. WebView2 environment reuses the existing pattern:
  cache folder `%LocalAppData%\WindowsFormsWebView2`, existing `CoreWebView2EnvironmentOptions`
  usage, initialization through the existing `IWebViewCoreInitializer` seam.
- **Versioning / backward compatibility:** no public API breaks. Microsoft.Web.WebView2
  1.0.3912.50 (net462 assemblies on net481) is already referenced; no new NuGet package is added
  to `QuickFiler`. Newtonsoft.Json 13.0.4 is consumed where it is already referenced
  (`UtilitiesCS`), which is where the shared bridge contracts live.

## API / CLI Surface

No CLI surface. The feature introduces the following internal contracts.

### Bridge message contracts (JSON over the WebView2 message channel)

- Inbound (JS -> .NET), discriminated by `type`:
  `{ type: "segmentDoubleClick" | "leafExpandToggle" | "arrowKey" | "rowSelected", rowId, segmentIndex?, key? }`
- Outbound (.NET -> JS), discriminated by `type`:
  `{ type: "render" | "subfolderResult" | "focusSearch", ..., requestId? }` — `subfolderResult`
  is correlated to its originating expand request by `requestId`.
- Serialization: Newtonsoft.Json 13.0.4; contract types are plain net48-safe classes /
  `readonly struct` with explicit constructors (no `record`, no `init` accessors).
- Validation rules: malformed or unknown-`type` inbound JSON must fail fast with a logged,
  specific error (no silent swallow); negative (malformed-payload) cases are unit-tested.

### Host seam (new, narrow — interface-seam tier per `.claude/rules/csharp.md`)

```csharp
public interface IBreadcrumbWebHost   // implemented by an exempt WebView2 adapter
{
    void NavigateToString(string html);
    void PostMessageJson(string json);
    event EventHandler<string> MessageReceived;   // raises WebMessageAsJson
    bool IsCoreInitialized { get; }
}
```

### Consumed 9101 provider surface (dependency; confirm against the 9101 spec at integration)

```csharp
public interface IFolderHierarchyProvider
{
    Task<IReadOnlyList<FolderSegmentInfo>> GetAncestorChainAsync(
        string leafFolderPath, CancellationToken ct);
    Task<IReadOnlyList<FolderSegmentInfo>> GetImmediateSubfoldersAsync(
        string folderPath, CancellationToken ct);
}

public readonly struct FolderSegmentInfo   // net48-safe: no record/init
{
    public string FullPath { get; }
    public string DisplayName { get; }
    public bool HasSubfolders { get; }
}
```

If 9101 lands a different shape, the single coupling point is the breadcrumb-row builder input;
the re-alignment cost is one mapping/adapter class.

## Data & State

- **Data flow:** `EfcDataModel.FolderHelper` suggestion rows + probabilities -> 9101 ancestor
  chains -> breadcrumb row model -> HTML renderer -> `NavigateToString` / `render` message ->
  user interaction -> inbound bridge message -> bridge router -> state transition and/or provider
  query -> outbound message. Selection flows back to `SelectedFolder` for filing.
- **State machine (pure, host-neutral):** per-row collapse-after-segment state (double-click on
  non-leaf segment *i* hides segments > *i* and shows a plus at *i*; re-expand restores the full
  breadcrumb), leaf expand/collapse state with a children list, left/right-arrow transitions,
  banner and `"Trash to Delete"` pseudo-row kinds. Precedent for shape and tests:
  `FolderSuggestionTree` and its two test files.
- **Invariants:**
  - Percent markup is always emitted as the trailing fixed flex item; the breadcrumb path is the
    only element permitted to truncate.
  - Plus/minus affordance markup is emitted only when the segment's `HasSubfolders` is true.
  - Banner rows are non-interactive; folder names are HTML-encoded by the renderer.
  - Outbound messages issued before `CoreWebView2` initialization completes are queued and
    flushed on `CoreWebView2InitializationCompleted` (no polling, no delays).
- **Caching / persistence:** none introduced. The provider's snapshot caching is 9101's concern.
- **Migration / backfill:** none.

## Constraints & Risks

- **9101 dependency (blocking).** Depends on issue 9101 (live Outlook folder-hierarchy provider),
  merged before this feature during epic execution. This feature consumes 9101's contract
  (ancestor chain + on-demand real immediate subfolders behind an injectable seam) rather than
  re-deriving hierarchy from suggestion rows. 9101 had not started at research time; provider-
  consuming tasks must be sequenced behind 9101's merge, or coded against the assumed §C.3
  interface with a single adapter class as the re-alignment point.
- **Open contract question — dependency note routed to 9101:** the leaf `HasSubfolders` must be
  answerable cheaply (snapshot-backed, not a per-row COM round-trip) for every rendered row,
  because the leaf affordance is gated on it. This requirement must be confirmed in the 9101
  contract review.
- **EfcViewer3 is dead code.** Behavioral conversion is `EfcViewer` + `EfcFormController` only;
  `EfcViewer3` receives a mechanical Designer-only swap or removal (research §B.2).
- **Ruled out:** `BrightIdeasSoftware.TreeListView` (or any third-party WinForms tree/list
  control) and WPF/`ElementHost`. `ObjectListView.Official 2.9.1` stays referenced —
  `BayesianPerformanceViewer` and `ItemViewer.TopicThread` still use it.
- **I/O boundary:** the live Outlook subfolder query is I/O-bound and must stay isolated from
  pure breadcrumb logic per repository policy, so the core is unit-testable without a live
  Outlook process.
- **Coverage exemptions (policy):** Designer-generated code, the `EfcViewer`/`EfcViewer3` forms
  (already `[ExcludeFromCodeCoverage]`), the WebView2 adapter implementing `IBreadcrumbWebHost`
  (1:1 SDK forwarding, in-code justification per the `WebView2CoreInitializer` precedent), and
  the `EnsureCoreWebView2Async` init path bound to the concrete Designer control are exempt.
  `EfcFormController` is already wholly `[ExcludeFromCodeCoverage]`; **new testable logic must
  NOT be added to `EfcFormController`** — it stays wiring-only, with new logic in the non-exempt
  router/model classes.
- **Coverage floors:** new modules target >= 90% line coverage; repository floors apply to the
  testable denominator (line >= 85% / branch >= 75% per `.claude/rules/general-unit-test.md`;
  plan to the stricter of the stated policy bars).
- **net48 constraints:** no `init`-only setters, no `record` / `record struct` (CS0518 without
  `IsExternalInit`); use plain classes or `readonly struct` with explicit constructors
  (precedents: `FolderRow`, `FolderScore`). Both owner projects are legacy non-SDK csproj — every
  new source and test file needs an explicit `<Compile Include>` entry.
- **Banned APIs:** `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
  `Task.Delay` must not appear in touched/new code or tests. Initialization waiting is gated on
  `CoreWebView2InitializationCompleted` plus a pending-message queue, never `Task.Delay`; any
  timing uses injected `TimeProvider`.
- **Threading:** WebView2 controls must be created and touched only on the WinForms UI (STA)
  thread; initialization awaits `UiSyncContext` before `EnsureCoreWebView2Async`, following the
  seam-based `QfcItemController.ViewerSetup` pattern (not the raw `EfcItemController` init).
- **Pooled-viewer lifecycle:** `EfcViewer` instances are pooled via `EfcViewerQueue`
  (`ViewerQueueCore<EfcViewer>`); `CoreWebView2` event subscriptions must be unhooked or
  idempotent across re-initialization (precedent: the `cid:` handler rebuilds state at request
  time).
- **Defect-cause risk:** the percentage-obscuring root cause is a ranked candidate (unscaled
  `ColumnHeader` widths under font autoscaling), not yet proven; the mandatory runtime repro
  capture precedes the fix and validates or corrects the hypothesis.

## Implementation Strategy

- **Recommended approach (research §G.1, option 1):** a C#-generated full HTML document (inline
  `<style>` and `<script>`) delivered via `NavigateToString`, with a
  `postMessage` / `WebMessageReceived` JSON bridge. Initialization goes through the existing
  `IWebViewCoreInitializer` seam (await `UiSyncContext`, then `EnsureCoreWebView2Async`). No
  packaged or on-disk web assets; no `SetVirtualHostNameToFolderMapping`; no
  `AddHostObjectToScript` (rejected: COM marshaling, unmockable, no serializable contract).
- **New classes (pure, coverage-bearing, new-module >= 90%):**
  - `BreadcrumbRow` / `BreadcrumbSegment` model + collapse/expand state machine — `UtilitiesCS`.
  - `BreadcrumbMessage` contracts + JSON codec (Newtonsoft) — `UtilitiesCS` (already references
    Newtonsoft 13.0.4; shared with sibling feature 9103 per the epic).
  - `BreadcrumbHtmlRenderer` (document/fragment generation including the §D.4 CSS) —
    `UtilitiesCS` or a pure QuickFiler class.
  - `BreadcrumbBridgeRouter` (inbound message -> state transition + provider query -> outbound
    messages; init-pending queue) — QuickFiler, tested via `Mock<IFolderHierarchyProvider>` +
    `Mock<IBreadcrumbWebHost>`.
- **New classes (host-bound, exempt with in-code justification):**
  - `WebView2BreadcrumbHost : IBreadcrumbWebHost` (1:1 SDK forwarding adapter).
  - Designer swap in `EfcViewer.Designer.cs` (replace `FolderListBox` `TreeListView` with
    `Microsoft.Web.WebView2.WinForms.WebView2` in the same TLP cell, span 14; delete
    `olvColumnFolder`/`olvColumnPercent`); `EfcViewer3.Designer.cs` mechanical swap or removal;
    `EfcFormController` wiring changes only (touch points per research §B.5).
- **Dependency changes:** none. WebView2 SDK and its `.targets` native-loader import are already
  in place; Newtonsoft contracts live in `UtilitiesCS` to avoid adding a package reference to
  `QuickFiler`.
- **Logging/telemetry:** one temporary log4net diagnostic line on `Form.Shown` (control client
  width, column widths, `CurrentAutoScaleDimensions`, `DeviceDpi`) exists only to produce the
  repro evidence and is removed in the same feature branch. Bridge deserialization failures log
  specific errors via the repo log4net pattern.
- **Rollout:** direct replacement, no feature flag. Manual runtime verification (including the
  AC percent-visibility check at minimum form width) follows the automated toolchain; the
  fallback is branch revert, since the change is confined to the EfcViewer surface.
- **Order of work (research §G.2):** runtime repro capture -> pure model + contracts + renderer
  with tests -> router with mocked provider/host -> host adapter + Designer swap + controller
  wiring -> manual verification including the percent-visibility check -> remove repro
  instrumentation.

## Definition of Done

### Acceptance Criteria

These criteria, together with the identical list in `user-story.md`, are the authoritative
acceptance-criteria source for this full-feature work.

- [ ] Every suggestion row in the live `EfcViewer` renders as a single-line breadcrumb
  `Folder -> SubFolder -> Leaf`, anchored at the selected/predicted leaf, in a WebView2-hosted
  HTML/CSS/JS control replacing the `TreeListView`.
- [x] The expand affordance (plus when collapsed, minus when expanded) appears only on the leaf
  segment, and only when the leaf's `HasSubfolders` is true; leaves without subfolders show no
  affordance.
- [x] Double-clicking a non-leaf segment collapses the row after that segment — downstream
  arrows, segments, and the leaf are hidden — and shows a plus to the left of the now-terminal
  segment; activating that plus re-expands the full breadcrumb.
- [ ] Expanding a segment lists every real immediate Outlook subfolder of that folder via the
  9101 `IFolderHierarchyProvider` seam (ancestor-chain plus on-demand immediate-subfolders
  calls); no hierarchy is derived by prefix-matching over suggestion rows.
- [ ] The prediction percentage is always fully visible: a runtime reproduction of the current
  obscuring defect (screenshot plus geometry diagnostic log) is captured first and stored under
  `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/repro/`, and the
  CSS-based fix (percent as the trailing fixed, non-shrinking flex item) is applied afterward.
- [ ] A JS<->.NET bridge (`window.chrome.webview.postMessage` -> `WebMessageReceived`;
  `PostWebMessageAsJson`/`NavigateToString` outbound) carries double-click and left/right-arrow
  keyboard interaction and routes the live subfolder query across the WebView2 boundary.
- [x] `EfcViewer3` is handled as a mechanical Designer-only control swap or removal with no
  behavioral wiring; the behavioral conversion targets `EfcViewer` + `EfcFormController` only
  (EfcViewer3 is dead code: sole runtime instantiation is `new EfcViewer()` at
  `EfcViewerQueue.cs:83`).
- [x] No third-party WinForms tree/list control and no WPF/`ElementHost` are introduced; the
  control technology is WebView2 (HTML/CSS/JS).
- [x] The scoring/ranking algorithm is unchanged; the feature-324 percentage plumbing
  (`FolderRow.Score` -> `PercentageFormatter.FormatPercent`) is reused as-is.
- [ ] Behavior parity is preserved: Up-at-top focuses `SearchText` (via a `focusSearch` bridge
  message); the `"Trash to Delete"` pseudo-row remains selectable; `"===="` banner rows remain
  non-interactive and rejected by `IsValidSelection`; the `'F'` action focuses the breadcrumb
  control; dark-mode re-theming works.
- [x] The pure breadcrumb row model and collapse/expand state machine, the bridge message
  contracts (JSON round-trip plus malformed-input negatives), the HTML renderer, and the bridge
  router are unit-tested with MSTest + Moq + FluentAssertions (router against
  `Mock<IFolderHierarchyProvider>` and `Mock<IBreadcrumbWebHost>`), meeting repository coverage
  floors with >= 90% on new modules; host/Outlook wiring stays behind coverage-exempt seams with
  in-code justification, and no new testable logic is added to `EfcFormController`.
- [x] The full C# toolchain passes in a single pass: csharpier, msbuild with analyzers, msbuild
  with nullable/TreatWarningsAsErrors, and vstest with coverage; no banned APIs
  (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) in
  touched/new code or tests.

### Process checklist

- [ ] Acceptance criteria mapped to tests or documented manual verification steps
- [ ] Edge cases and error handling covered by tests (malformed bridge JSON, no-subfolder leaf,
  banner rows, pre-initialization message queueing)
- [ ] Docs updated (feature folder documents; 9101 dependency note routed)
- [ ] Temporary repro instrumentation removed in the same branch
- [ ] Toolchain pass completed (format -> lint -> type-check -> test)

## Seeded Test Conditions (from potential)

- [ ] Unit coverage: breadcrumb model construction from an ancestor chain; collapse/expand state
  transitions (collapse-after-segment, plus re-expand, leaf toggle, arrow keys);
  bridge message serialization/deserialization including malformed-input negatives;
  segment-children request shaping and `requestId` correlation; renderer invariants (trailing
  fixed percent item, affordance gated on `HasSubfolders`, HTML-encoding, non-interactive
  banners).
- [ ] Integration scenarios: WebView2 host initialization through `IWebViewCoreInitializer`;
  JS<->.NET bridge round-trip for double-click, arrow keys, and the subfolder query (router
  tested against mocked provider/host; live round-trip verified manually).
- [ ] Runtime reproduction of the percentage-obscuring defect (evidence under `evidence/repro/`)
  and verification of the CSS fix at minimum form width.
