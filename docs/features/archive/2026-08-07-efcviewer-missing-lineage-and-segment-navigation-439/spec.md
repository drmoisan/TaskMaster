# 2026-08-07-efcviewer-missing-lineage-and-segment-navigation (Spec)

- **Issue:** #439
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T17-30
- **Status:** Draft
- **Version:** 0.1

## Context

Issue #439 restores EfcViewer folder lineage and mouse navigation for suggested and searched folders. The Efc path is `EfcFormController` -> `BreadcrumbBridgeRouter` -> generated `BreadcrumbHtmlRenderer` document; it does not use the ItemViewer `FolderBreadcrumb.html` resource. Presented Efc folder values are archive-relative filing targets, while `IFolderHierarchyProvider` resolves exact full Outlook paths, which currently forces ordinary rows into a single-segment fallback.

Environment:

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in with Microsoft WebView2
- UI path: EfcViewer folder list (`EfcViewer.FolderListBox`, exposed as `BreadcrumbWebView`), driven by `EfcFormController`, `BreadcrumbBridgeRouter`, `BreadcrumbDocumentAssets`, and `BreadcrumbHtmlRenderer`
- Data source or fixture: `EfcDataModel.FindMatches` search results and `FolderPredictor.Suggestions` suggestion rows under an `ArchiveRootPath`-rooted search

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Without lineage, same-named folders cannot be disambiguated by parent path. Without ancestor selection and expansion, a user cannot select a sibling of a suggested folder without searching again.

## Repro & Evidence

Steps to Reproduce:

1. Open EfcViewer on a mail item with suggestion rows.
2. Observe that each suggestion shows only its leaf name.
3. Enter a search that returns folder rows and observe the same leaf-only rendering.
4. On a multi-segment row, activate a non-leaf segment.
5. Observe that the ancestor is not selected and cannot be expanded to choose one of its children.

Expected:

- Each resolved suggestion and search row displays its complete root-to-leaf lineage in the Efc-generated document, with `→` between adjacent visible segments.
- Activating a non-leaf segment selects that ancestor and enables expansion of that ancestor's immediate children.
- Activating a rendered child selects that child, including a sibling of the original leaf.
- A genuinely unresolved row remains selectable as one segment; it is an exception rather than the normal result for archive-relative targets.

Actual:

- The path-form mismatch makes ordinary archive-relative suggestion and search targets fail exact hierarchy resolution, so they render as one leaf-only segment.
- The active Efc document emits no typed ancestor or child activation message. Existing segment double-click only collapses trailing segments; expansion is leaf-only.

Logs / Screenshots:

- [ ] Attached minimal logs or screenshot
- The 2026-08-24 research document supplies code-read evidence. No runtime capture exists yet.

## Scope & Non-Goals

- In scope:
  - Translate archive-relative Efc filing targets to full hierarchy paths only at the Efc-to-`IFolderHierarchyProvider` boundary.
  - Preserve the original filing target and its existing `FolderScore.Probability` semantics while using resolved full paths for hierarchy only.
  - Render root-to-leaf lineages and the Unicode `→` separator in the Efc-generated renderer.
  - Support typed non-leaf ancestor activation, ancestor expansion, and rendered child or sibling activation.
  - Preserve a selectable single-segment fallback only for null resolution, an empty chain, or a provider failure, and make that fallback diagnosable.
- Out of scope / non-goals:
  - Keyboard Left/Right navigation changes, including Issue #440 keyboard-navigation work.
  - Issue #400 behavior or any score-model recalibration.
  - Replacing EfcViewer's generated-document bridge with ItemViewer's resource-page breadcrumb implementation.
  - Fuzzy or prefix matching in `OutlookFolderHierarchyProvider`, live Outlook COM tests, configuration changes, or new packages.
- Explicitly excluded systems, integrations, or datasets:
  - `QuickFiler/Resources/FolderBreadcrumb.html` and the ItemViewer breadcrumb popup.
  - Non-folder banner rows (`====`) and the `Trash to Delete` pseudo-row, except to preserve their current behavior.

## Root Cause Analysis

`FolderPredictor.GetOlSubpath` produces archive-root-relative search stems, and research confirmed suggestions and score keys use the same relative form. `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` compares its input exactly to a full Outlook `FolderPath`; it therefore returns no key for the normal Efc presentation form. `BreadcrumbBridgeRouter.FetchChainAsync` then reaches `BreadcrumbRowBuilder`'s intentional one-leaf fallback.

The active Efc renderer is separate from `FolderBreadcrumb.html`. `BreadcrumbHtmlRenderer` writes `&gt;`, and `BreadcrumbDocumentAssets` supports double-click collapse and whole-row selection but no typed segment activation. `BreadcrumbBridgeRouter` expands only `row.LeafSegment`, so an ancestor cannot become the active expandable node.

## Proposed Fix

### Design summary (what changes where):

Introduce an explicit Efc breadcrumb presentation boundary with two values per selectable folder row: the original `FilingTarget` and a full `HierarchyPath`. `EfcFormController` and `BreadcrumbBridgeRouter` create the hierarchy path using `IApplicationGlobals.Ol.ArchiveRootPath` only when the input is not already rooted; `IFolderHierarchyProvider` continues exact full-path resolution. `BreadcrumbRow` and `BreadcrumbRowBuilder` retain the filing target independently of the resolved full path so normal selection and probability lookup remain archive-relative.

Extend `BreadcrumbMessages`, `BreadcrumbMessageCodec`, `BreadcrumbDocumentAssets`, `BreadcrumbBridgeRouter`, and `BreadcrumbHtmlRenderer` with typed segment and child activation. An activated non-leaf becomes the active expandable segment; its stable `FolderTreeNodeKey` is passed to `GetImmediateSubfoldersAsync`. A returned child is rendered beneath that active segment and, when activated, becomes the selected archive-relative filing target.

### Boundaries and invariants to preserve:

- `IFolderHierarchyProvider.ResolveLeafKeyAsync` remains exact and case-insensitive against full Outlook paths; no fuzzy or prefix matching is introduced.
- `FilingTarget` remains byte-for-byte equivalent to the originally presented archive-relative target for normal row selection and `FolderScore.Probability` lookup.
- An already full input is not archive-root-prefixed a second time.
- A resolved hierarchy chain is root-first. The one-segment fallback occurs only after null resolution, an empty chain, or provider failure and remains selectable.
- Ancestor and child selection targets are derived by removing the same archive root from verified hierarchy paths; action callers continue to receive their current archive-relative values with `ArchiveRootPath`.
- `segmentActivate` stops event propagation so the row-level handler cannot reselect the leaf. `segmentDoubleClick` continues to collapse trailing segments.
- Banner rows, `Trash to Delete`, keyboard Left/Right behavior, and Issue #400 behavior are unchanged.

### Dependencies or blocked work:

No external dependency, configuration, package, migration, or feature flag is required. The supplied research is sufficient for implementation planning. Runtime Outlook validation remains required after automated tests but does not block authoring this specification.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `QuickFiler/Controllers/EfcFormController.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`: supply archive-root-aware presentation inputs, resolve hierarchy paths, preserve filing targets, manage active segment state, and handle validated bridge messages.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` and `BreadcrumbRowBuilder.cs`: represent selection independently of full hierarchy identity and join probability by the original filing target.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`, `BreadcrumbMessageCodec.cs`, and `BreadcrumbDocumentAssets.cs`: define, encode, validate, and emit typed segment and child activation messages.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`: use `→` between visible segments and render the active ancestor's child affordance and child rows.
- Existing tests under `QuickFiler.Test/Controllers`, `UtilitiesCS.Test/OutlookObjects/Folder`, and `QuickFiler.Test/Viewers`: add the required automated regression coverage.

#### Functions/classes/CLI commands impacted:

- `EfcFormController.ConfigureBreadcrumbControl` and `BindBreadcrumbRowsAsync` must pass `ArchiveRootPath` to the Efc breadcrumb boundary.
- `BreadcrumbBridgeRouter.BindRowsAsync`, hierarchy-chain fetching, selection, expansion, and message dispatch must operate on the active segment rather than assuming `LeafSegment`.
- `BreadcrumbRowBuilder.BuildRow` and `BuildProbabilityIndex` must accept or retain the filing-target key independently from hierarchy leaf full paths.
- `BreadcrumbHtmlRenderer.AppendSuggestionRow` must render `→` in the active Efc document.
- The existing C# toolchain remains `csharpier .`, analyzer build, nullable build, and `vstest.console.exe` with coverage.

#### Data flow and validation changes:

1. Search or suggestion input arrives as an archive-relative filing target.
2. The Efc boundary preserves that value and derives a full hierarchy path only when needed for exact provider resolution.
3. The provider key and ancestor chain build the root-first visual segments; score lookup and normal selection remain keyed by the original target.
4. The generated document posts typed JSON messages with `type`, `rowId`, and required segment or child index fields.
5. The codec rejects missing, malformed, out-of-range, banner, or pseudo-row activation input before state changes.
6. A valid non-leaf activation selects that segment. Expanding it requests immediate children using its key. A valid rendered-child activation selects the child target after archive-root removal.

#### Error handling and logging updates:

Provider resolution failure, a null key, an empty ancestor chain, and provider exceptions retain the selectable fallback but are recorded through the repository's existing logging pattern with the resolution outcome and non-sensitive target context. Invalid bridge payloads are rejected by the typed codec without changing selection or expansion state. No exception may convert an ordinary archive-relative target into a silent normal fallback.

#### Rollback/feature-flag considerations (if applicable):

No feature flag is planned. Reverting the implementation restores the current Efc behavior; the change must remain isolated from ItemViewer, keyboard navigation, and Issue #400 paths.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Input row: archive-relative folder string or an already full Outlook path, optional `FolderScore` keyed by the original input, banner, or pseudo-row.
- Hierarchy input: full Outlook path only. If the input already begins with `ArchiveRootPath` under ordinal-ignore-case comparison, use it unchanged; otherwise prepend the archive root according to the existing folder-path separator convention.
- Normal output: `SelectedFolder` equals the original archive-relative filing target; visible segments are root-first and separated by `→`.
- Ancestor/child output: `SelectedFolder` equals the verified selected segment or child full path after the archive-root prefix is removed.
- Bridge messages: typed JSON with a `type` and `rowId`; segment activation additionally has a valid segment index, and child activation identifies a valid child of the currently expanded segment.

#### Required configuration keys and defaults:

Use the existing `IApplicationGlobals.Ol.ArchiveRootPath`. No new app setting, command-line option, environment variable, default, or persisted state is permitted.

#### Backward-compatibility expectations:

Existing Efc actions (`CreateFolder`, move, and open-folder callers) continue receiving archive-relative `SelectedFolder` values. Existing `segmentDoubleClick` collapse stays available. Existing banner and `Trash to Delete` rendering and selection rules, ItemViewer breadcrumbs, keyboard Left/Right navigation, and Issue #400 behavior do not change.

#### Performance constraints (latency/throughput/memory):

Use the existing asynchronous hierarchy-provider contract. Do not add synchronous Outlook or WebView calls to UI event handlers, duplicate hierarchy snapshots, or perform additional provider lookups for invalid messages. The change must keep existing bounded Efc row binding behavior.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): `ArchiveRootPath` identifies the prefix of full archive folder paths; provider nodes contain stable keys and full paths; score keys use the original archive-relative target.
- Constraints (budget, performance, compatibility): .NET Framework 4.8.1 WinForms/VSTO and WebView2 remain supported. Tests must be deterministic MSTest tests using Moq and FluentAssertions, without live Outlook or temporary files.
- External dependencies (services, libraries, releases): Existing WebView2 typed-message boundary, `IFolderHierarchyProvider`, and no new packages.

## Data / API / Config Impact

- User-facing or API changes: Efc rows gain visible `→` lineage, mouse ancestor activation, ancestor child expansion, and child/sibling activation. No public CLI or external API changes.
- Data or migration considerations: No persisted-data or schema migration. The in-memory row presentation carries filing and hierarchy forms separately.
- Logging/telemetry updates (if any): Emit diagnosable resolution-fallback outcomes through the existing logger; preserve existing malformed-message rejection behavior.
- Compatibility notes (CLI flags, config schemas, versioning): No flags, config schema, or version changes. Existing action callers receive their current archive-relative target form.

## Test Strategy

Research-established coverage must be added before the implementation is considered complete.

- Regression tests to add or update:
  - In `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`, add a fail-before/pass-after test binding mixed archive-relative suggestion and search rows with a three-segment chain. Assert the provider receives the root-expanded full path, visible segments are root-first, selection remains the original filing target, and probability remains present.
  - Add router cases for an already full target (no double root), null resolution key, empty chain, provider exception or cancellation, and valid versus invalid non-leaf and child message indices. Assert no ancestor-chain query follows a null key.
  - Add router cases proving a non-leaf activation selects the relative ancestor, expansion queries that ancestor key, and a returned child activation selects the relative sibling target.
  - In `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowBuilderTests.cs`, assert filing-target score joining is independent of a resolved full hierarchy leaf.
  - In `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs`, assert a three-segment Efc row has exactly two `→` separators, retains output encoding, and renders the active segment's children.
  - In codec/document-asset tests, assert typed ancestor and child messages round-trip, malformed or missing indexes are rejected, and segment activation stops propagation. Retain the double-click-collapse test.
  - Retest mixed rows containing a search result, suggestion, `====` banner, and `Trash to Delete`; lineage applies only to resolved folder rows.
- Unit tests (pytest) for the fixed behavior and boundaries:
  - Not applicable. This is C# code; use MSTest, Moq, and FluentAssertions.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - Case-insensitive already-full path; relative target; null resolution; empty chain; provider failure or cancellation; invalid row, segment, and child indexes; banner and pseudo-row clicks; and a child whose full path cannot be converted relative to the configured archive root.
- Error handling and logging verification:
  - Verify invalid payloads leave state unchanged and provider fallback causes are passed to the existing logging boundary. Verify normal archive-relative rows resolve without fallback logging.
- Coverage impact and targets for changed lines/modules:
  - Capture the required baseline and final coverage comparison. New path translation, message validation, fallback, renderer separator, ancestor expansion, and child activation branches require direct deterministic coverage; repository-wide coverage must remain at least 80 percent and new or changed behavior must target at least 90 percent coverage.
- Toolchain commands to run (format → lint → type-check → test):
  1. `csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <relevant test assembly paths> /EnableCodeCoverage`
  - Restart this ordered pass if any command changes files or fails. Store baseline, regression, QA, and coverage evidence only under `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/evidence/<kind>/`.
- Manual validation steps (if required):
  1. In EfcViewer, open a mail item with a suggestion and execute a folder search.
  2. Verify each resolved folder row displays its root-to-leaf lineage with `→` and retains its expected score.
  3. Activate a middle ancestor, expand it, then activate a rendered child or sibling; verify selection uses the archive-relative filing target.
  4. Verify unresolved, banner, and `Trash to Delete` rows retain their specified behavior, and verify keyboard Left/Right behavior is unchanged.

### Definition of Done

- Automated regression coverage passes for every acceptance criterion below.
- The final C# toolchain pass is clean and its evidence is stored in the canonical Issue #439 evidence folders.
- The manual EfcViewer scenario is recorded as passed or as an explicitly documented environment-blocked item; an environment block does not permit acceptance-criteria check-off.

## Acceptance Criteria

- [x] Given an archive-relative suggestion or search target and `ArchiveRootPath`, the Efc boundary sends the correctly root-expanded full path to `IFolderHierarchyProvider.ResolveLeafKeyAsync` while retaining the original target as the row's filing target.
- [x] Given a target already rooted at `ArchiveRootPath` under ordinal-ignore-case comparison, the boundary sends that full path unchanged and does not duplicate the root.
- [x] Given a resolved three-node ancestor chain for a suggestion or search row, the Efc-generated renderer displays those nodes in root-to-leaf order with exactly one `→` between each adjacent pair.
- [x] Given a resolved row with a `FolderScore` keyed by its original archive-relative target, the row displays that score after hierarchy resolution and normal row selection returns that original target.
- [x] Given a null resolution key, empty ancestor chain, or hierarchy-provider failure, the row remains selectable as one segment and the fallback cause is sent to the existing logging boundary.
- [x] Given an ordinary archive-relative suggestion or search target that resolves after root expansion, the row does not use the one-segment fallback.
- [x] Given a malformed, missing, banner, pseudo-row, out-of-range row, out-of-range segment, or invalid child activation message, the codec/router rejects it without changing selected or expanded state.
- [x] Given activation of a valid non-leaf segment, the router selects that ancestor's archive-relative target and prevents the row-level handler from reselecting the original leaf.
- [x] Given a valid activated non-leaf ancestor, expansion requests its immediate children using that ancestor's stable `FolderTreeNodeKey`, not the original leaf key.
- [x] Given rendered immediate children for an expanded ancestor, activation of a valid child selects that child or sibling's archive-relative target.
- [x] Existing segment double-click collapses trailing segments, while keyboard Left/Right behavior remains unchanged.
- [x] `====` banner rows and `Trash to Delete` retain their existing behavior and do not gain lineage, hierarchy resolution, or child activation.
- [x] No ItemViewer `FolderBreadcrumb.html` behavior, Issue #400 behavior, score-model calculation, public configuration, or external API changes are included.
- [ ] The required C# formatter, analyzer, nullable, MSTest, and coverage comparison pass in one final ordered toolchain pass, with canonical Issue #439 evidence artifacts present.

## Risks & Mitigations

- Technical or operational risks:
  - Combining hierarchy full paths with filing targets could cause a move or open action to receive the wrong path form.
  - An untyped or bubbling WebView message could overwrite the ancestor selection with the leaf.
  - Fuzzy provider matching could choose a duplicate folder in a different branch or store.
  - Modifying the ItemViewer resource would leave EfcViewer unchanged and expand scope.
- Mitigations and rollbacks:
  - Keep the two path forms explicit and test normal, ancestor, and child output values.
  - Use the typed codec, index validation, and stopped propagation; test rejected messages leave state unchanged.
  - Retain exact provider equality and test duplicate-safe full-path input.
  - Change the generated Efc renderer only; revert the focused change set if a rollout regression is found.

## Rollout & Follow-up

- Release/rollout steps:
  - Complete automated evidence and the manual EfcViewer verification before promotion. Include the Issue #439 evidence paths in review material.
- Post-fix monitoring or clean-up tasks:
  - Review fallback logging after release for unexpected archive-relative resolution failures. Treat any systematic fallback as a defect rather than a visual-only degradation.
- Links: issue [#439](https://github.com/drmoisan/TaskMaster/issues/439); research `artifacts/research/2026-08-24T17-34-issue-439-efc-lineage-research.md`; active feature folder `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/`.
