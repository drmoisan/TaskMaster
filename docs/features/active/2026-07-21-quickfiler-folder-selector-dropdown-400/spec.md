# quickfiler-folder-selector-dropdown (Spec)

- **Issue:** #400
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-21T10-41
- **Status:** Draft
- **Version:** 0.1

## Context

QuickFiler's WebView-based folder selector does not preserve the specified interaction contract of the Windows Forms drop-down control it replaced. The closed control can show a row other than the current selection, scored suggestions lose their existing normalized probability on several render paths, the right edge exposes in-place vertical scrolling, and the child WebView cannot expand over neighboring WinForms controls. Issue #400 restores the specified closed, popup, keyboard, mouse, probability, accessibility, placement, focus, theme, and lifecycle behavior while preserving the breadcrumb architecture and issue #398's atomic-upgrade guarantees.

Environment:

- OS/version: Supported Windows desktop environments, including multi-monitor layouts and negative monitor coordinates
- Runtime: .NET Framework 4.8.1 WinForms add-in with Microsoft WebView2
- UI path: QuickFiler `ItemViewer` folder selection widget
- Data source or fixture: `FolderPredictor` suggestions and their existing normalized `FolderRow.Score` values
- Existing browser environment: the `CoreWebView2Environment` initialized by `QfcItemController`

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce:

1. Open QuickFiler for an item that produces multiple folder suggestions with normalized probability scores.
2. Select a suggestion other than the first selectable row and collapse the selector.
3. Observe the rendered row, probability text, and right-side affordances.
4. Press Up and Down while closed; open the selector, press Up or Down, then close with Enter, Escape, and an outside click in separate runs.
5. Position `ItemViewer` near the bottom and top of the active monitor working area and open the selector.
6. Exercise immediate rendering and hierarchy upgrades for resolved, unresolved, empty-chain, and provider-failure suggestions.

Expected:

- The closed selector renders exactly the committed selected folder row and its existing normalized probability, when the selected row is scored.
- The closed selector has no vertical scrollbar or scroll arrows and has exactly one accessible drop-down arrow.
- The arrow opens an owned native popup over sibling controls. Full desired height opens below when it fits; otherwise it opens above, with deterministic clamping when neither side fits.
- Closed Up and Down change the committed selection. Open Up and Down change the pending selection. The keys do not scroll, skip non-selectable rows, and stop at the first and last selectable rows.
- Enter and mouse activation commit. Escape and any uncommitted outside or automatic close restore the selection that was active when the popup opened.
- Left and Right retain their current breadcrumb behavior.
- All immediate and hierarchy-upgrade paths retain the supplied probability without recalculation and preserve issue #398's atomic replacement and selection guarantees.

Actual:

- The selected row can remain outside the visible 25-pixel WebView viewport while another row is displayed.
- Vertical overflow produces unwanted in-place scrolling controls.
- There is no dedicated drop-down button or popup that can cross the native child-control boundary.
- Up, Down, Enter, and Escape do not implement the requested selection-session behavior.
- The synchronous coordinator render and unresolved, empty-chain, and provider-failure fallbacks convert scored suggestions to plain rows, discarding the probability before projection.

Logs / Screenshots:

- [x] Automated code and research evidence captured; screenshots and user-operated validation are not required delivery evidence.
- `QuickFiler/Resources/FolderBreadcrumb.html` renders every row in the fixed-height host, leaves vertical overflow available, and handles only Left and Right.
- `QuickFiler/Viewers/ItemViewer.Designer.cs` places the existing WebView inside a `TableLayoutPanel`; HTML cannot cross that native HWND boundary.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` discard scores when producing plain fallback rows.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` already formats a retained probability through `PercentageFormatter`, so the value must be preserved rather than recomputed.

## Scope & Non-Goals

- In scope:
  - Closed-state projection of one committed selected folder row and its existing normalized percentage.
  - One accessible drop-down button and removal of in-place vertical scrolling behavior.
  - An `ItemViewer`-owned native popup that overlays sibling controls and uses active-monitor working-area placement.
  - Host-neutral committed, original, and pending selection state for closed/open Up, Down, Enter, Escape, mouse activation, and uncommitted outside close.
  - Score-preserving fallback suggestions for immediate, unresolved, empty-chain, and provider-failure paths.
  - Preservation of Left/Right breadcrumb behavior, issue #398 atomic replacement, stable identity, selection preservation, light/dark themes, focus, disposal/reuse, and single event routing.
  - Automated host-neutral, bridge, asset-contract, and integration-seam tests for every specified semantic contract.
- Out of scope / non-goals:
  - Replacing the breadcrumb WebView architecture with an owner-drawn WinForms `ComboBox`.
  - Recomputing, renormalizing, or changing the formatting policy for folder probabilities.
  - A globally topmost window, new persisted settings, a new WebView2 package, or a new browser runtime dependency.
  - Pixel-identical rendering across every Windows, WebView2, display-scale, and theme combination. This limitation is not a blocker because acceptance is defined by automated interaction, geometry, accessibility-attribute, state-transition, and theme-contract verification.
- Explicitly excluded systems, integrations, or datasets:
  - Outlook folder scoring and predictor algorithms beyond preserving the `FolderRow.Score` supplied to the selector.
  - Non-QuickFiler controls and unrelated `ItemViewer` interactions.
  - Manual bootstrap, user-performed QA, screenshot collection, and manual acceptance evidence.

## Root Cause Analysis

The replacement retained a 25-pixel child WebView while rendering the entire row collection within it. The page does not reduce the closed presentation to the selected row, does not suppress vertical overflow, and has no selector state or Up/Down/Enter/Escape messages. Because the WebView is a native child of `ItemViewer`'s `TableLayoutPanel`, CSS positioning and `z-index` cannot render over sibling WinForms controls.

The missing probability has a separate host-side cause. `BreadcrumbBridgeCoordinator.SetSuggestions` immediately converts supplied `FolderRow` suggestions to plain paths, and the router also converts unresolved keys and empty resolved chains to plain rows. `BreadcrumbStateRow` plain rows have no probability, so the existing `PercentageFormatter` and `.pct` binding receive no score. A hierarchy-provider failure can leave that probability-free immediate state in place.

Issue #398 removed the transient clear/partial rebuild by constructing replacement rows and swapping them atomically. Issue #400 must extend the row representation and selector state without reintroducing a transient empty model, allowing a stale async upgrade to overwrite newer state, or losing a host selection made while an upgrade is in flight.

## Proposed Fix

### Design summary (what changes where):

Retain the existing WebView as the closed one-row anchor. Add an `ItemViewer`-owned `ToolStripDropDown` containing a lazily initialized popup WebView2 through `ToolStripControlHost`; initialize it with the existing `CoreWebView2Environment`. Both pages consume the same projected state through a focused multi-surface messenger hub or equivalent relay, while inbound events are processed once.

Add a host-neutral selection session that tracks committed, original-at-open, and pending row identity. Add a pure popup placement calculator that receives anchor screen bounds, active-monitor working area, and desired popup size. Represent unresolved suggestions as scored fallback suggestions with stable identity, fallback display text, and the original probability so an asynchronous hierarchy upgrade changes only the display chain.

### Boundaries and invariants to preserve:

- A scored suggestion retains its original normalized score and stable identity from `FolderRow` input through immediate fallback, resolved projection, unresolved fallback, empty-chain fallback, and provider failure. No selector layer recalculates the score.
- Only genuinely non-scored rows, such as separators or prompts, project an empty percentage.
- Issue #398 remains effective: an async upgrade builds a complete replacement off-model, swaps atomically, preserves a selection made after the upgrade started, and cannot let an older completion overwrite newer state.
- Left and Right keep the current expand, collapse, and unhandled-key routing semantics.
- `IItemViewer.SetFolderDroppedDown(bool)` remains source-compatible and becomes the host open/close seam.
- The popup is above `ItemViewer` sibling controls while open and owned by that `ItemViewer`; it is not system-wide topmost.
- One host render is sent per surface per state update, and one inbound page event causes exactly one state transition and at most one selection notification.
- Explicit commit is the only open-session path that replaces the original selection. Escape, outside click, lost activation, and any other uncommitted automatic close restore the original selection.

### Dependencies or blocked work:

- Use the existing `Microsoft.Web.WebView2.WinForms` reference and the WinForms `ToolStripDropDown`, `ToolStripControlHost`, `Screen`, and coordinate APIs already available to the application.
- No new package, service, runtime, persisted setting, migration, or external data source is required.
- The supplied research is sufficient; there is no research blocker.

### Implementation strategy (what changes, not sequencing):

The following surfaces are grounded in the supplied research. The planner may refine task ordering and split cohesive types or tests to remain under the file-size limit, but it must preserve these contracts and update every affected legacy project include.

#### Files/modules to change:

- Existing folder state and projection: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`, and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs`.
- New focused folder contracts: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectorMessages.cs`, or equivalently cohesive planner-refined files that do not expand the near-limit `BreadcrumbBridgeMessages.cs`.
- Existing host coordination: `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`.
- New focused host types: `QuickFiler/Viewers/BreadcrumbPopupPlacement.cs`, `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, and a focused multi-surface messenger hub such as `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`.
- Shared page asset: `QuickFiler/Resources/FolderBreadcrumb.html`. Avoid hand-written behavior in the already oversized generated `QuickFiler/Viewers/ItemViewer.Designer.cs`.
- Explicit project wiring: `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/QuickFiler.csproj`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and `QuickFiler.Test/QuickFiler.Test.csproj` for every added `.cs` file; update generated resource wiring only if the repository's existing resource workflow requires it after the HTML edit.

#### Functions/classes/CLI commands impacted:

- `BreadcrumbBridgeCoordinator.SetSuggestions` must create scored fallback suggestions synchronously and expose their percentages immediately.
- `FolderBreadcrumbBridgeRouter` upgrade paths must preserve scored fallback identity and probability for unresolved keys, empty chains, provider failures, and successful atomic replacements.
- `BreadcrumbStateModel` and `BreadcrumbRenderProjection` must distinguish scored fallback suggestions from genuinely non-scored plain rows.
- `ItemViewer.AttachBreadcrumbWebView`, `ItemViewer.FocusBreadcrumb`, and `ItemViewer.SetFolderDroppedDown(bool)` must coordinate the anchor, popup, focus return, and lifecycle without changing the public interface.
- The popup host owns `ToolStripDropDown`, `ToolStripControlHost`, lazy WebView initialization, close-reason/commit state, placement, focus, reset, and disposal.
- The page/host bridge adds collapsed/expanded view mode, drop-down toggle, selector key, and row activation messages while leaving existing Left/Right messages intact.

#### Data flow and validation changes:

1. `FolderRow.Score` enters as the already normalized probability.
2. The synchronous coordinator creates a stable scored fallback suggestion containing identity, path/fallback text, and probability.
3. Projection sends existing `PercentageFormatter` output as `percentText`; the page displays it without recalculation.
4. A successful hierarchy resolution atomically substitutes breadcrumb segments for fallback text while retaining identity, probability, and current selection. Unresolved, empty-chain, and failed resolutions leave the scored fallback unchanged.
5. Opening snapshots the committed row identity as `original`, sets `pending` to it, chooses placement from the active monitor working area, and focuses the pending popup option.
6. Closed Up/Down commits the adjacent selectable identity immediately. Open Up/Down changes only `pending`. Enter or mouse activation commits; Escape or uncommitted close restores `original`.
7. The coordinator publishes the resulting state to the closed and expanded surfaces once each; the collapsed page filters to the committed row, while the expanded page renders the selectable list and keeps the pending option in view.

#### Error handling and logging updates:

- Treat invalid selector messages and unknown selector keys consistently with the existing bridge parser: reject them without changing selection or opening state, and use the existing logging boundary where bridge parse failures are currently reported.
- If lazy popup WebView initialization fails, close or keep the selector closed, restore the pre-open selection, dispose any partially created host, and report through the existing QuickFiler logging pattern. Do not leave an orphan popup or duplicate subscription.
- Hierarchy provider exceptions, unresolved keys, and empty chains retain the scored fallback row and percentage. They must not clear the current model or surface an exception solely because hierarchy decoration was unavailable.
- Empty/no-selectable-row state and Up/Down at either boundary are deterministic no-ops and must not throw.

#### Rollback/feature-flag considerations (if applicable):

- No feature flag or data rollback is required because the change adds no persisted state or schema.
- Reverting the implementation restores the prior anchor-only behavior. The new popup host must dispose independently so rollback leaves no resource or event-registration migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- Host-to-page view message: collapsed or expanded mode, open state, and current committed/pending stable row identity.
- Page-to-host toggle message: activation of the single drop-down button.
- Page-to-host selector-key message: only `up`, `down`, `enter`, or `escape`.
- Page-to-host activation message: stable row identity selected by mouse or equivalent accessible activation.
- Existing render rows continue to include breadcrumb segments and `percentText`; scored fallback rows additionally expose fallback display text or an equivalent discriminated representation without changing the meaning of plain non-scored rows.
- The closed button exposes an accessible name, `aria-haspopup="listbox"`, and accurate `aria-expanded`. The expanded list exposes listbox/option semantics, one active selected option, and a deterministic active-row relationship.

#### Required configuration keys and defaults:

- None. Reuse the existing `CoreWebView2Environment`.
- `ToolStripDropDown.AutoClose` remains enabled.
- Popup padding and margins must not create unintended scrollbars or alter placement calculations.

#### Backward-compatibility expectations:

- Preserve the `IItemViewer` method signatures and current controller call sites.
- Preserve Left/Right breadcrumb behavior, path readback, selection notification semantics, percentage formatting, and all issue #398 concurrency guarantees.
- Preserve non-scored rows as non-scored and do not convert labels/separators into selectable suggestions.
- Existing QuickFiler and UtilitiesCS tests remain green.

#### Performance constraints (latency/throughput/memory):

- Create the popup WebView lazily on first open, not during normal `ItemViewer` construction.
- Reuse one popup WebView and the existing `CoreWebView2Environment` per active/reused viewer lifecycle; do not allocate a new browser environment or popup WebView on every open.
- Dispose the popup WebView, host, event handlers, and relay attachments with `ItemViewer`; reset pooled/reused viewers so subsequent use has one active subscription per surface.
- Selection movement and placement calculation remain synchronous host-neutral operations with no network, Outlook, filesystem, or provider calls.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): `FolderRow.Score` is already normalized by the existing scoring pipeline; the current HTML resource and WebView2 environment remain available to both surfaces; `Screen.FromControl(anchor).WorkingArea` identifies the active monitor working area.
- Constraints (budget, performance, compatibility): all new and modified production and test source files must remain below 500 lines; generated `ItemViewer.Designer.cs` receives no hand-written runtime behavior; bridge contracts, host-neutral state, WinForms hosting, and tests remain separated by responsibility.
- External dependencies (services, libraries, releases): existing .NET Framework 4.8.1 WinForms and Microsoft WebView2 only; no external service, package, temporary file, manual bootstrap, or user-supplied runtime evidence.
- Validation limitation: pixel-identical output across all Windows/WebView2/rendering combinations is outside the specified contract and is not a completion blocker. Automated tests must verify the required interaction, placement geometry, accessibility attributes, theme-state propagation, focus/lifecycle seams, and state transitions.

## Data / API / Config Impact

- User-facing or API changes: the folder selector gains one accessible drop-down button, a native expanded list, closed/open keyboard behavior, and visible normalized probability text. Public controller/viewer method signatures remain unchanged.
- Data or migration considerations: no persistence or migration. The in-memory row representation gains a scored fallback form or equivalent invariant that preserves stable identity, fallback text, and probability.
- Logging/telemetry updates (if any): use existing logging for invalid bridge messages and popup initialization failures; no new telemetry is required for normal selection movement.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI flags, configuration keys, schemas, or versioned external APIs change.

## Test Strategy

Seeded from issue:

- [ ] Add fail-before host-neutral MSTest coverage for collapsed-row projection, scored fallback probability retention, committed/original/pending selection transitions, selectable-row boundaries, and deterministic popup placement.
- [ ] Add fail-before bridge and asset-contract tests proving the closed asset has hidden vertical overflow, exactly one accessible drop-down button, accurate expanded state, selector-key posts, expanded active-row visibility, and unchanged Left/Right routing.
- [ ] Add fail-before integration-seam tests for one render per surface, one transition per inbound event, native popup ownership/overlay configuration, focus return, lazy initialization, disposal/reuse, and existing `SetFolderDroppedDown(bool)` compatibility.
- [ ] Add fail-before concurrency/fallback tests proving immediate, successful, unresolved, empty-chain, and provider-failure states retain the original score and that issue #398 atomic replacement and late-upgrade protections remain intact.

- Regression tests to add or update:
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs`: closed Up/Down commits; open Up/Down changes pending only; non-selectable rows are skipped; first/last bounds do not wrap; Enter commits; Escape and uncommitted auto-close restore the original; Left/Right do not alter the session.
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs`: round-trip view, toggle, selector-key, and activation messages; reject missing identity and unknown selector keys without state changes.
  - Split or extend `BreadcrumbRenderProjectionTests` in an under-500-line file: scored fallback renders fallback text and the same formatted percentage; resolved suggestions retain that percentage; non-scored rows remain blank; collapsed projection contains exactly the selected data row.
  - Split or extend `FolderBreadcrumbBridgeRouterEdgeTests` and `FolderBreadcrumbBridgeRouterInFlightTests`: unresolved key, empty chain, and provider failure retain kind, identity, path, and probability; a completed upgrade remains atomic, preserves a host selection made in flight, and cannot let an older completion overwrite newer state.
  - `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs`: full fit below; below does not fit/full fit above; neither fits with greater-side choice; equal-space tie favors below; vertical and horizontal clamp; non-primary/negative-coordinate monitor bounds; zero available space.
  - `QuickFiler.Test/Viewers/BreadcrumbDropDownCoordinatorTests.cs`: open snapshots selection and broadcasts expanded mode; closed/open arrow semantics; Enter/mouse commit; Escape/outside/lost-activation cancellation; focus enters the pending option and returns on close; two messengers receive one render each while one inbound event causes one transition; lazy host reuse and deterministic disposal; `SetFolderDroppedDown` compatibility.
  - A new coordinator probability test file rather than expanding the approximately 460-line existing test: synchronous `SetSuggestions` exposes the supplied percentage before resolution; successful upgrade retains the same score and selection; failed upgrade retains the fallback score.
  - The established HTML resource-contract test home, split if needed: collapsed one-row filtering, `overflow: hidden`, exactly one button, accessible name, `aria-haspopup="listbox"`, accurate `aria-expanded`, listbox/option state, Up/Down/Enter/Escape messages with browser scrolling prevented, Left/Right compatibility, active-row visibility, and light/dark theme hooks.
- Unit tests (MSTest) for the fixed behavior and boundaries: use MSTest with FluentAssertions and Moq only at external/host boundaries; tests must follow Arrange-Act-Assert, use no network, external process, mutable machine state, temporary file, wall-clock sleep, or user interaction.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): empty/no-selectable collection, selection `-1`, first/last selectable row, separators between suggestions, invalid selector message, unknown key, unresolved key, empty chain, provider exception, stale upgrade completion, zero/limited working-area space, negative monitor coordinates, popup initialization failure, repeat open/close, disposed host, and reused `ItemViewer`.
- Error handling and logging verification: assert invalid messages and initialization failure preserve the original selection, close cleanly, log through the existing seam, dispose partial resources, and do not duplicate handlers; assert provider failure retains scored fallback state without selector-layer recomputation.
- Coverage impact and targets for changed lines/modules: repository-wide line coverage remains at least 80%; every new class or method and the new/changed selector types target at least 90%; changed-line coverage does not regress. Numeric baseline, post-change, and changed/new-code values are required in canonical feature evidence.
- Toolchain commands to run (format → lint → type-check → test), in one uninterrupted final pass and restarted from formatting after any failure or file change:
  1. `csharpier format .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
- Automated validation requirement: no manual bootstrap, screenshot, user-operated validation, or manual QA step is required or accepted as a substitute for automated host-neutral, bridge, asset-contract, integration-seam, and coverage evidence.

## Acceptance Criteria

- [ ] AC-1: With a scored folder selected, the collapsed selector renders exactly one data row, that row is the committed selected folder, and its displayed `percentText` equals the existing `PercentageFormatter` output for the supplied normalized `FolderRow.Score`; the selector performs no probability recomputation or renormalization.
- [x] AC-2: The collapsed page exposes no vertical scrollbar, spinner, or scroll arrows and contains exactly one drop-down button with an accessible name, `aria-haspopup="listbox"`, and `aria-expanded` matching the host open state.
- [ ] AC-3: Button activation and `SetFolderDroppedDown(true)` open a native `ToolStripDropDown`/`ToolStripControlHost` popup over `ItemViewer` sibling controls; while open it remains owned by and above that `ItemViewer` and is never configured as a global/system-wide topmost window.
- [x] AC-4: Placement uses the anchor's active monitor working area: the full desired height opens below when it fits; otherwise a full-height popup opens above when it fits; when neither fits, the side with more available space is chosen, an equal-space tie opens below, and size/location are clamped horizontally and vertically for primary, non-primary, and negative-coordinate monitor rectangles.
- [ ] AC-5: While closed, Up and Down immediately commit the previous or next selectable folder, skip non-selectable rows, stop without wrapping at the first/last selectable row, publish at most one selection change, and never scroll the page.
- [ ] AC-6: Opening snapshots the committed identity as `original` and initializes `pending` to it. While open, Up and Down change only `pending`, skip/clamp using the same selectable-row rules, keep the active option visible, and do not change the committed selection before commit.
- [ ] AC-7: Enter and accessible/mouse row activation commit the pending row, publish the selection exactly once, close the popup, render the committed row in the collapsed control, and return focus to the collapsed selector or owning `ItemViewer` focus target.
- [ ] AC-8: Escape, outside click, lost activation, and every other uncommitted automatic close restore the identity selected when the popup opened, publish no pending selection as committed, close cleanly, and return focus. A close after an explicit commit does not roll back that commit.
- [x] AC-9: Left and Right preserve the existing breadcrumb expand, collapse, and unhandled-key behavior in both view modes and do not mutate the committed/original/pending selector session.
- [ ] AC-10: Immediate synchronous render, successful hierarchy resolution, unresolved key, empty resolved chain, and hierarchy-provider failure all retain the supplied score, stable row identity, and selection; only genuinely non-scored rows display no percentage.
- [ ] AC-11: Issue #398 guarantees remain intact: no transient cleared or partially rebuilt model is observable, row replacement is atomic, readback remains pre-upgrade consistent while an upgrade is in flight, a host selection made after upgrade start survives replacement, and a stale completion cannot overwrite newer state.
- [ ] AC-12: Both closed and popup WebView surfaces receive the same logical selector state with their respective view modes; each state update renders once per attached surface, and each event from either surface is routed once with no duplicate selection, open/close, or breadcrumb transition.
- [ ] AC-13: Automated asset and host-seam tests prove light and dark theme state reaches both surfaces, the expanded list exposes listbox/option selection semantics, focus enters the pending option on open, and focus returns predictably on commit, cancellation, and initialization failure.
- [ ] AC-14: The popup WebView is created lazily with the existing `CoreWebView2Environment`, reused rather than recreated for each open, and disposed/reset with `ItemViewer`; repeated pooled viewer reuse leaves one live subscription per surface, no orphan popup, and no callback after disposal.
- [ ] AC-15: Empty/no-selectable state, selection `-1`, invalid selector messages, unknown keys, popup initialization failure, zero available placement space, repeated open/close, and provider failure are deterministic, preserve the last valid committed selection and any supplied scores, and do not throw or leak resources at the selector boundary.
- [ ] AC-16: Deterministic failure-first MSTest evidence exists for the pre-fix defects and covers selection sessions, probability fallbacks, issue #398 concurrency, bridge serialization/routing, placement geometry, HTML/accessibility/theme contracts, popup ownership/focus, and lifecycle/reuse; each regression fails for the intended reason before implementation and passes afterward without sleeps, temporary files, external services, screenshots, or user interaction.
- [x] AC-17: Every added production and test `.cs` file is explicitly included in the applicable legacy `.csproj`; no new or modified production/test source file exceeds 500 lines; no hand-written runtime behavior is added to the already oversized generated `ItemViewer.Designer.cs`; no new external package or persisted configuration is introduced.
- [ ] AC-18: One final uninterrupted C# toolchain pass succeeds in this exact order: `csharpier format .`; analyzer-enabled `msbuild`; nullable warnings-as-errors `msbuild`; and coverage-enabled `vstest.console.exe` for `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`. Repository-wide line coverage is at least 80%, every measurable new or changed selector type and member reaches at least 90%, and changed-line coverage does not regress, with numeric baseline/post-change/delta evidence. Only direct WebView2/WinForms adapter calls and unavoidable navigation-readiness coordination and cleanup may be classified as bounded nonnumeric surfaces, and every such surface must be enumerated and verified through deterministic injected seams; no numeric threshold, filter, or exclusion is waived or widened.
- [ ] AC-19: All existing breadcrumb, QuickFiler controller, UtilitiesCS, and issue #398 regression tests pass, and the full specified semantic contract is verified through automated host-neutral, bridge, asset-contract, and integration-seam tests. Pixel-identical cross-environment rendering is not required and is not treated as a blocker.

## Risks & Mitigations

- Technical or operational risks:
  - A second WebView can increase resource use or create initialization/disposal races.
  - Two page messengers can duplicate events or render stale state if attachment ownership is unclear.
  - Native popup placement can be wrong on non-primary monitors, negative coordinates, taskbar-constrained working areas, or limited vertical space.
  - Async hierarchy upgrades can regress issue #398 by losing score, stable identity, or a newer selection.
  - WebView/native focus and theme state can diverge between the anchor and popup.
- Mitigations and rollbacks:
  - Create the popup lazily, reuse the existing environment, own subscriptions in one host, and verify disposal/reuse through deterministic seams.
  - Broadcast outbound state through one hub and route each inbound event through one coordinator; test exact call counts.
  - Keep placement pure and cover all fit, clamp, tie, and coordinate boundaries without requiring a live display.
  - Preserve score and identity in a discriminated fallback row and reuse the issue #398 atomic swap/generation protections.
  - Share the same asset/theme contract, explicitly model focus transitions, and close/restore safely after initialization failure.
  - Revert the host/state additions if necessary; no data, configuration, or package rollback is required.

## Rollout & Follow-up

- Release/rollout steps: deliver through the normal TaskMaster build and deployment path after the automated acceptance criteria, exact C# toolchain, coverage thresholds, feature review, and CI gates pass. No manual bootstrap or migration is required.
- Post-fix monitoring or clean-up tasks: use automated regression and CI results to detect selector, issue #398, coverage, file-size, or project-wiring regressions; no user-operated monitoring or screenshot collection is required.
- Links: issue #400; predecessor issue #398; implementation PR and final audit links to be recorded by the orchestration workflow.
