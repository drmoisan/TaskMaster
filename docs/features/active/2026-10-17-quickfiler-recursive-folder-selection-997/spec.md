# 2026-10-17-quickfiler-recursive-folder-selection (Spec)

- **Issue:** #997
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-18T01-31
- **Status:** Implemented and locally verified; independent review and final PR-head CI pending
- **Version:** 0.2

## Context
- QuickFiler expands top-level folders, but revealed descendants cannot be used reliably as filing destinations and descendants with children lack expansion controls. Every displayed folder must be selectable, while its plus/minus control changes expansion only.
- Observed environment(s): TaskMaster QuickFiler QFC embedded WebView2 breadcrumb selector on Windows. The separate EFC renderer is compatibility regression scope.
- Customer impact and severity (who is affected, how often, how bad): Users navigating an archive hierarchy cannot file to revealed descendants or navigate below the first revealed level. No production frequency measurement is available.
- First observed date and version(s) impacted: Reported 2026-10-17; exact first affected release is unknown.
- Research: [research/research.2026-10-18T00-45.md](research/research.2026-10-18T00-45.md). The supplied research is sufficient to complete this spec; implementation and local tests are now verified by the delivery evidence below; independent review and final PR-head CI remain pending.

## Repro & Evidence
- Steps to reproduce (with data/flags/inputs): Provide a rooted hierarchy `\\mailbox\Archive\Projects\Apollo\Alpha\Review`, with `Alpha` containing `Review` and `Review` containing a leaf. Bind archive root `\\mailbox\Archive`, present the top-level target as `Projects\Apollo`, open the QFC selector, and expand Apollo. Select Alpha, then reopen and expand Alpha and Review. Separately select Alpha, reopen, collapse Apollo, read the committed destination, and reopen/Escape.
- Expected vs actual behavior: Alpha must commit `Projects\Apollo\Alpha`, and intermediate and leaf descendants must have the same selection behavior. Current selection returns the provider's rooted child path; downstream `ArchiveStemContract.RequireArchiveRelativeStem` rejects that filing destination. Current embedded children do not render recursive controls. Collapsing a selected child's parent clears its indexed list and can invalidate readback; reopen/Escape snapshots only the containing row.
- Logs/screenshots/error snippets: Source-level diagnosis is recorded in the research report. The four original failures and a fifth legacy indexed root-boundary failure have recorded fail-before/pass-after evidence under `evidence/regression-testing/`. No live Outlook screenshot or browser UI session is claimed.
- Frequency / determinism (always, intermittent, data-dependent): Rooted-provider/relative-presented-path selection and missing recursive rendering are deterministic with the in-memory hierarchy above. Async ordering requires controlled provider completion tests.

## Scope & Non-Goals
- In scope: QFC descendant destination mapping, lazy recursive expansion, selection/expansion independence, durable committed descendant identity, and async request validity needed for these behaviors. Existing bridge and EFC behavior receive compatibility regression coverage.
- Out of scope / non-goals: EFC production changes, eager archive traversal, general breadcrumb redesign, new search or keyboard features, and unrelated folder/classifier behavior.
- Explicitly excluded systems, integrations, or datasets: Live Outlook and external services in unit tests; new packages, JavaScript engines, persistence schemas, and coverage exclusions.

## Root Cause Analysis
- Confirmed source causes: `BreadcrumbSelectionMap.GetSelectedFolder` returns a selected child provider path without the archive-relative mapping applied to the presented top-level target. `BreadcrumbStateRow` has one immediate-child list/index, and embedded `makeSubfolders` ignores child `hasChildren`; host and wire state lack recursive identity/expansion. `TryCollapseLeaf` clears the list underlying the selected index, while selection session open/cancel snapshots retain row identity only.
- Signals/evidence supporting it: The researched call chain is `QfcItemController.BreadcrumbWiring.EnsureBreadcrumbPipeline` -> `ItemViewer.Breadcrumb.InitializeBreadcrumbPipeline/CreateCollapsedBreadcrumbCandidate` -> embedded `Resources.FolderBreadcrumb` -> `BreadcrumbBridgeCoordinator` -> `UtilitiesCS.FolderBreadcrumbBridgeRouter`. Selection passes through `ItemViewer.FolderSearch`, `QfcItemController.EventHandlers.CboFolders_SelectedIndexChanged`, and `QfcItemController.MailActions.MoveMailAsync` to `EmailFilerConfig.ResolvePaths`, which enforces an archive-relative stem. Immediate-child click and commit handlers already exist.
- Affected components/modules (paths, services, pipelines): `UtilitiesCS/OutlookObjects/Folder/` host state, selection, projection, and message contracts; `QuickFiler/Viewers/` coordinator and root binding; `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs`; `QuickFiler/Resources/FolderBreadcrumb.html`. Exact files and symbols follow.

## Delivered Fix

### Design summary (what changes where):

Use a lazy host-owned descendant tree per unique row occurrence. Each known node retains its provider `FolderTreeNodeKey`, hierarchy path, archive-relative filing value when a root is bound, children, expansion state, and request validity. Fetch immediate children through the existing provider seam on expansion; serialize recursive render state and accept only host-known descendant identities. Bind an optional archive root from QFC and derive descendant filing values through `ArchiveStemContract` at the host selection boundary; existing no-root callers retain full-path behavior.

### Boundaries and invariants to preserve:

- Clicking a folder, including an intermediate descendant, commits that folder through existing selection notification and popup close/focus behavior. Clicking plus/minus changes expansion only and emits no selection event.
- Keep committed node/value independent of visible child lists. Collapsing an ancestor, reopening, and Escape retain the committed descendant; Escape restores the complete selection snapshot.
- Separate raw provider hierarchy paths from filing values. Preserve unique containing row identity, duplicate-name/store isolation, score/probability contracts, provider ordering, and existing top-level selection behavior.
- Reject client-supplied paths and unknown, unloaded, malformed, or cross-row identities. Await providers outside the state lock; apply results only while population generation, row occurrence, node request generation, and intended expansion remain current.

### Dependencies or blocked work:

No missing research, external dependency, or manual user action blocks planning. Required work uses existing hierarchy-provider and in-memory bridge seams.

### Implementation strategy (what changes, not sequencing):

Replace single-level child selection assumptions with recursive host-owned identity and projection. Preserve first-level indexed messages as compatibility aliases and add a typed descendant activation/toggle address. Use optional archive-root binding solely where QFC supplies it. Keep changes confined to the confirmed defects and tests below.

#### Files/modules to change:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, `BreadcrumbStateModel.Row.cs`, `BreadcrumbSelectionMap.cs`, `BreadcrumbRenderProjection.cs`, `BreadcrumbBridgeMessages.cs`, `BreadcrumbSelectorMessages.cs`, `BreadcrumbSelectionSession.cs`, and `FolderBreadcrumbBridgeRouter.cs`: recursive state, host identity validation, durable selection, root mapping, and render/wire behavior.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs`, and `QuickFiler/Resources/FolderBreadcrumb.html`: descendant routing/rendering and QFC archive-root context.
- Add a focused descendant model/helper or partial file only where cohesive recursive logic would otherwise extend a file near the 500-physical-line ceiling. Research measures HTML at 490, router at 489, session at 474, bridge messages at 463, and coordinator at 437 lines. If HTML script extraction is needed, share the existing resource-composition path across collapsed and popup hosts; do not create competing documents.
- Register only necessary new source/resource files in the affected legacy `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/QuickFiler.csproj`, and test projects. Update `QuickFiler/Properties/Resources.resx` and its generated designer only if required by an actual embedded asset split.
- New focused regression test files are proposed under the existing directories: `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDescendantSelectionIssue997Tests.cs`, `FolderBreadcrumbDescendantOrderingIssue997Tests.cs`, and `QuickFiler.Test/Viewers/BreadcrumbDescendantActivationIssue997Tests.cs` and `FolderBreadcrumbRecursiveAssetIssue997Tests.cs`. These files now exist, together with focused descendant message/validation tests and the necessary host/render/coordinator partials and compiled script resource. All new sources/resources are registered exactly once.

#### Functions/classes/CLI commands impacted:

`BreadcrumbStateRow.WithFilingTarget/TryCollapseLeaf`, `BreadcrumbSelectionMap.GetSelectedFolder`, `BreadcrumbSelectionSession` open/cancel/commit methods, `FolderBreadcrumbBridgeRouter.ToggleAsync`, recursive render/message types, embedded `makeSubfolders` and click dispatch, coordinator message handling, and QFC breadcrumb initialization. No CLI surface changes.

#### Data flow and validation changes:

Bound root + known provider key -> immediate children -> host-owned descendant identities and validated relative filing values -> recursive render DTO -> activation/toggle identity -> host validation -> commit or expansion. A descendant activation emits exactly one existing selection notification and explicit popup close; a toggle preserves selection. Root-bound selection must satisfy `RequireArchiveRelativeStem`, including archive boundary checks, before it becomes a filing destination.

#### Error handling and logging updates:

Retain existing error reporting and no-op handling for invalid wire requests. Provider failures must not commit a destination or attach stale children; permit a subsequent valid retry through the existing error behavior. Use existing logging only when required for a new actionable failure; no new telemetry project is required.

#### Rollback/feature-flag considerations (if applicable):

No feature flag or migration is required. Reverting the scoped implementation/resource registration restores prior behavior; no persisted data is changed.

### Technical specifications (interfaces/contracts):

The host owns node identity and filing values. Descendant wire addresses identify a known node in its containing row and never authorize arbitrary paths. Recursive render children carry identity, display/path information, `hasChildren`, expansion, selection, and loaded children sufficient to render the current visible hierarchy. Preserve existing strict JSON type validation and immediate-child message compatibility.

#### Inputs/outputs and formats:

Inputs are existing candidate rows, optional bound archive-root context, immediate-child provider results, and typed JSON bridge messages. Outputs are recursive JSON render state and the existing selected-folder string/event. With root `\\mailbox\Archive`, a selected rooted Alpha node yields `Projects\Apollo\Alpha`; without root binding, legacy full-path output remains unchanged.

#### Required configuration keys and defaults:

No new persisted configuration key. Optional archive-root constructor/binding context defaults to absent, preserving existing callers; QFC supplies its configured archive root.

#### Backward-compatibility expectations:

Preserve no-root `BreadcrumbSelectionMapTests.GetSelectedFolder_ExpandedSubfolderSelection_YieldsTheSubfolderFullPath`, immediate-child activation semantics, existing top-level score/duplicate contracts, and EFC renderer behavior. Update contract assertions only where recursion intentionally extends QFC output; do not weaken their validation.

#### Performance constraints (latency/throughput/memory):

Opening/rendering must not eagerly enumerate the archive subtree. Fetch only requested immediate children and retain bounded session tree state; collapse removes descendants from visible projection without invalidating a committed destination. Controlled tests verify superseded completions cannot repopulate collapsed/replaced rows. No new numerical latency target is asserted without measurement.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): QFC can bind its configured archive root, and hierarchy provider keys identify nodes across duplicate names/stores.
- Constraints (budget, performance, compatibility): Existing dependencies, UI-thread/host boundaries, strict JSON contracts, 500 physical lines per production/test/script file, and deterministic in-memory tests without temporary files.
- External dependencies (services, libraries, releases): Existing WebView2 and hierarchy-provider interfaces only; no dependency upgrade.

## Data / API / Config Impact
- User-facing or API changes: Descendants become valid filing destinations and recursively expandable. Internal bridge contracts gain stable descendant identity while preserving first-level aliases.
- Data or migration considerations: Session state only; no migration or archive mutations during browsing.
- Logging/telemetry updates (if any): Existing diagnostic conventions apply; no additional telemetry requirement.
- Compatibility notes (CLI flags, config schemas, versioning): No CLI/config schema change; optional binding defaults preserve unbound full-path semantics and EFC production behavior.

## Test Strategy
- Regression tests to add or update: Add fail-first `ActivateImmediateChild_BoundArchiveRoot_CommitsArchiveRelativeDestination`, `CollapseAncestor_SelectedChild_PreservesCommittedDestination`, and `ReopenThenEscape_SelectedChild_RetainsCommittedDestination` in the proposed selection test file. Use existing APIs for the rooted-provider/relative-presented-target repro so it fails behaviorally before the fix. Add `RenderDescendantWithChildren_OffersIndependentExpansionControl` in the proposed asset test file against the actual QFC embedded resource. Capture deterministic failing output before production changes and passing output afterward.
- Unit tests (pytest) for the fixed behavior and boundaries: This C# bug uses MSTest, Moq, and FluentAssertions, not pytest. Add `ActivateIntermediateAndLeaf_ThreeLevels_CommitsKnownRelativeTargets`, `ToggleDescendant_DoesNotRaiseSelectionChanged`, `ActivateDescendant_NotifiesAndClosesPopupOnce`, and `CollapseAndReopen_SelectedDescendant_RendersCommittedSelection`. Asset contracts use the repository's text/regex style plus behavioral host tests; do not add a JavaScript runtime package.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): Leaves lack controls; duplicate names/stores remain distinct; unknown/unloaded/cross-row IDs, malformed JSON types, and out-of-root targets cannot commit. Add barrier-driven `CollapseDuringFetch_DiscardsLateChildren`, `ReplacePopulationDuringFetch_DiscardsOldChildren`, and `ReverseCompletion_OnlyCurrentNodeRequestApplies` using `TaskCompletionSource`, without sleeps or external processes.
- Error handling and logging verification: Provider failure preserves selection and permits retry; invalid requests retain existing no-op/error reporting and do not raise selection/close events. Preserve existing `BreadcrumbSubfolderSelectorSessionTests`, `BreadcrumbSubfolderActivationTests`, `FolderBreadcrumbAssetContractTests`, `BreadcrumbSelectorMessagesTests`, `BreadcrumbRenderProjectionSelectorTests`, `BreadcrumbDuplicateIdentityIntegrationTests`, and `FolderBreadcrumbBridgeRouterInFlightTests`; include EFC `Controllers/BreadcrumbBridgeRouter*Tests` compatibility suites.
- Coverage impact and targets for changed lines/modules: Capture baseline and final production coverage using the existing wrapper; repository line coverage remains at least 80%, new code targets at least 90%, and changed-line coverage does not decrease. Do not expand exclusions. Store baseline, regression, and QA/coverage evidence in `evidence/baseline/`, `evidence/regression-testing/`, and `evidence/qa-gates/` with current UTC write timestamps and command/exit/output summaries.
- Toolchain commands to run (format → lint → type-check → test): Use the current TaskMaster task commands below in one successful ordered pass, restarting at formatting after a failure or file-changing repair. Capture baseline and final comparisons; per-file nullable enable and warnings-as-errors follow the repository task rather than globally enabling nullable in legacy projects.
  1. `dotnet tool run csharpier format .`
  2. `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  3. `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -TreatWarningsAsErrors`
  4. `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` with a unique issue-997-prefixed `-CoverageOutput` directly in the existing ignored `coverage/` directory so detailed source-line output survives the existing retention rule. Execution dot-sources the unchanged wrapper and calls `Invoke-MSTestWithCoverageMain` to place test results under `SCRATCH`; it copies hash-verified detailed baseline/final snapshots into `SCRATCH` and publishes sanitized numeric summaries in canonical feature evidence.
- Manual validation steps (if required): None required from the user. Deterministic host/asset/activation tests must verify the acceptance criteria; an available UI smoke test may supplement them. Repository hygiene and all seven current CI jobs (actionlint, format-check, build-analyzers, build-nullable, mstest-coverage, pester, hygiene) remain required delivery gates on the final PR head.

## Acceptance Criteria
- [x] Every displayed folder, including intermediate descendants and leaves at three or more levels, commits through the existing top-level selection interaction; the bound-root repro yields `Projects\Apollo\Alpha` accepted by `ArchiveStemContract.RequireArchiveRelativeStem` and exactly one selection notification/popup close.
- [x] Clicking any plus/minus only expands/collapses that node, preserves the committed destination even when its ancestor collapses, and emits no selection notification; reopen/Escape retains the committed descendant.
- [x] Every displayed folder with children has an independent expansion control at every depth, expanding reveals its immediate children, and leaves have no expansion control.
- [x] Rooted-provider/relative-presented-target selection, selected-descendant collapse/readback, reopen/Escape, and recursive asset regressions have recorded fail-before/pass-after evidence and passing named MSTest coverage.
- [x] Unknown, unloaded, cross-row, malformed, and out-of-root descendant requests cannot commit; controlled late/reversed fetches after collapse or population replacement cannot attach stale children or change selection.
- [x] Existing no-root full-path selection, duplicate identity/score contracts, immediate-child message compatibility, and EFC compatibility suites pass; expansion performs no eager whole-subtree enumeration.
- [x] The full ordered CSharpier, analyzer rebuild, warnings-as-errors rebuild, and MSTest/coverage pass succeeds with baseline comparisons, at least 80% repository line coverage, at least 90% new-code coverage, no reduced changed-line coverage, and no added exclusions; source/test/script files remain within 500 physical lines.
- [ ] This spec and implementation evidence describe delivered behavior, and all required CI checks succeed against the final PR head.

## Risks & Mitigations
- Technical or operational risks: Confusing provider full paths with filing stems can target invalid destinations; recursive messages can select the wrong duplicate row; late provider results can reinsert collapsed/replaced nodes; near-limit files can breach repository size constraints.
- Mitigations and rollbacks: Explicit root binding and `ArchiveStemContract` validation, host-known row/node identity, request/population checks around awaits, and focused files only where required. Preserve legacy no-root semantics and compatibility suites; revert the scoped change if rollout exposes a regression.

## Rollout & Follow-up
- Release/rollout steps: Complete regression-first implementation, ordered baseline/final QA, acceptance tracking, required review/remediation, and final-head CI through the orchestrator. Ship through the existing TaskMaster release process; no migration.
- Post-fix monitoring or clean-up tasks: Check existing diagnostics for descendant resolution/provider failures; no unrelated cleanup is included.
- Links: [Issue #997](https://github.com/drmoisan/TaskMaster/issues/997), [issue.md](issue.md), and [research report](research/research.2026-10-18T00-45.md). PR link, independent review and final-head CI are pending the orchestrator delivery gates.

## Delivered Behavior and Local Verification

QFC binds its configured archive root. Known descendant identities authorize filing; both typed descendant activation and the legacy indexed selection route reject invalid/out-of-root children before selection changes. Unbound callers preserve full-path values. Each row owns a lazy descendant tree; every nonleaf receives its own plus/minus, and folder names commit independently. Collapse, reopen and Escape preserve the committed descendant. Population, row, node request and expansion checks discard obsolete provider results.

The embedded script is extracted into FolderBreadcrumb.js and inserted once by FolderBreadcrumbDocument for both collapsed and popup navigation. Compiled-resource tests prove complete script insertion and no unresolved marker; Node syntax passes. No package, persistence setting, eager subtree traversal or coverage exclusion was added.

Final ordered local QA passed: manifest-pinned CSharpier format/check, full analyzer Rebuild, full warnings-as-errors Rebuild, and full repository MSTest/coverage. Both rebuilds have zero warnings/errors. Baseline tests: 7,471 passed; final tests: 7,504 passed, zero failures. Repository production line coverage: 56,931/66,178 (86.03%) to 57,268/66,515 (86.10%); branches: 80.21% to 80.31%. Changed-line coverage: 201/206 (97.57%) to 547/552 (99.09%). New measured production files: 379/384 (98.70%); all 45 measured new methods meet 90%. Existing instrumentation/exclusions are unchanged. Embedded JavaScript is verified through syntax/composition/wiring contracts and host behavior; no JavaScript line coverage or live Outlook verification is claimed. Every applicable changed source/test/script file is at most 474 lines. Hygiene and exact project/resource registration passed.

The final coverage comparison includes tracked working changes and new untracked production files relative to the origin/main merge base, rather than only committed HEAD. Current evidence is selected by the greatest timestamp under evidence/baseline, evidence/regression-testing and evidence/qa-gates. Five individual fail-before records, combined original pass-after, the legacy-boundary pass-after, compatibility/isolation, full QA, numeric comparison, hygiene, registration and template-based self-review are retained.

## Usage and Remaining Delivery Gates

Click a folder name to select that destination. Click its plus/minus to reveal or hide immediate children; child folders with children have their own controls. No new setting or migration is required. The separate EFC production renderer remains unchanged and its compatibility tests pass. Ship through the existing TaskMaster release process after independent review, canonical PR context, final PR-head required CI and completion validation. AC8 remains unchecked until the orchestrator verifies those CI results.
