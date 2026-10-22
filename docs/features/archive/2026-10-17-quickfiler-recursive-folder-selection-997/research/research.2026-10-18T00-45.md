<!-- markdownlint-disable-file -->

# Task Research Notes: QuickFiler descendant selection and recursive expansion (#997)

Research author: `/root/quickfiler_fix/quickfiler_research` (`task-researcher-c3`).

This report records the initial investigation, not the final implementation. Findings and physical line counts below describe the source inspected during that investigation. This formatting update adds no source research or measurements.

The orchestrator persists this attributed report at `docs/features/active/2026-10-17-quickfiler-recursive-folder-selection-997/research/research.2026-10-18T00-45.md`. The researcher profile permits writes only to `artifacts/research/`, but repository enforcement rejected that path with `EVIDENCE_LOCATION_BLOCKED`. A shell write was also rejected with `PROMOTION_MCP_ONLY`. The parent coordinator directed canonical feature-associated research storage without changing enforcement. The researcher made no source or configuration changes and executed no tests.

## Research Executed

### File Analysis

- `QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` and `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
  - `EnsureBreadcrumbPipeline` initializes the QuickFiler item viewer’s embedded selector pipeline. Both collapsed and popup surfaces consume `Properties.Resources.FolderBreadcrumb`.
- `QuickFiler/Resources/FolderBreadcrumb.html`
  - Immediate-child activation already posts `selectorSubfolderActivate`. `makeSubfolders` does not use `hasChildren` to render descendant expansion controls.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
  - The coordinator dispatches selector messages to the host-owned router. Expansion attaches one immediate-child list; activation addresses its containing row and child index.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, `BreadcrumbStateModel.Row.cs`, `BreadcrumbSelectionMap.cs`, and `BreadcrumbSelectionSession.cs`
  - Selection uses one `SelectedSubfolderIndex`. Child selection returns the provider’s full path. Collapsing clears the indexed child collection. Open/cancel snapshots retain the containing row identity rather than the complete descendant selection.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs`, `BreadcrumbBridgeMessages.cs`, and `BreadcrumbSelectorMessages.cs`
  - The child render contract contains display name, folder path, and `hasChildren`, without recursive expansion state. Existing selector activation uses stable row identity plus immediate-child index.
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, and `QfcItemController.MailActions.cs`
  - Viewer readback becomes the controller’s selected destination and is assigned to `EmailFilerConfig.DestinationOlStem`.
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFilerConfig.cs` and `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs`
  - Filing rejects rooted destinations before composing the archive-relative stem with the archive root.
- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, `BreadcrumbBridgeRouter.Arrows.cs`, `BreadcrumbBridgeRouter.Selection.cs`, and `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` and `BreadcrumbDocumentAssets.cs`
  - EFC uses a separate generated-document implementation. Its child activation already passes through archive-relative selection validation.
- Existing selector, session, projection, message, duplicate-identity, and in-flight tests
  - Immediate-child activation and durable popup commit are already tested. Existing child-path tests assert full-path compatibility without the production combination of relative suggestion target and rooted child hierarchy.

### Code Search Results

- `InitializeBreadcrumbPipeline` and `new BreadcrumbBridgeRouter`
  - Distinguished QuickFiler’s embedded selector from EFC’s generated-document surface.
- `selectorSubfolderActivate`
  - Located the embedded click handlers, typed selector message, coordinator dispatch, router/session activation, and existing activation tests.
- `GetSelectedFolder` and `DestinationOlStem`
  - Traced the child’s rooted provider path from selector readback to the filing boundary.
- `TryCollapseLeaf`, `SelectedSubfolderIndex`, and `Subfolders`
  - Identified collection clearing while the selected child index remains populated.
- `hasChildren`
  - Confirmed the child render DTO carries this property, while embedded child rendering does not create a corresponding expansion control.
- `Jint`, `ClearScript`, `Chakra`, and `jsdom`
  - No JavaScript engine dependency was identified in the investigated test patterns. Existing embedded asset contracts use text and regular-expression assertions.

### External Research

- #githubRepo:"MicrosoftEdge/WebView2Samples web message"
  - The [official WebView2 sample](https://github.com/MicrosoftEdge/WebView2Samples/blob/main/SampleApps/WebView2WpfBrowser/assets/webMessages.html) confirms the existing native/web messaging pattern.
- #fetch:https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/communicate-btwn-web-native
  - [Microsoft’s WebView2 interop documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/communicate-btwn-web-native) confirms JSON message exchange between the page and native host. Repository implementations and tests remain authoritative for this defect.

### Project Conventions

- Standards referenced: `AGENTS.md`, `.agents/skills/execution-environment/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.vscode/tasks.json`, actual TaskMaster workflows, and existing MSTest/Moq/FluentAssertions tests.
- Instructions followed: research-only investigation; deterministic in-memory regression design; no dependency additions; preserve the ordered formatting → analysis → type-checking → testing loop, coverage requirements, and 500 physical line ceiling.
- The actual consuming TaskMaster workflows were checked rather than adopting the inherited execution-environment skill’s drm-copilot CI inventory.
- Memory search returned no QuickFiler-specific facts; this report relies on inspected source and official documentation.

## Key Discoveries

### Project Structure

The active QuickFiler item-selector path is:

`QfcItemController.EnsureBreadcrumbPipeline` → `ItemViewer.InitializeBreadcrumbPipeline` → embedded `FolderBreadcrumb.html` → `BreadcrumbBridgeCoordinator` → `UtilitiesCS.FolderBreadcrumbBridgeRouter`.

EFC follows a separate path:

`EfcFormController.ConfigureBreadcrumbControl` → `QuickFiler.Controllers.BreadcrumbBridgeRouter` → `BreadcrumbHtmlRenderer` and `BreadcrumbDocumentAssets`.

The recommended fix targets the QuickFiler embedded selector. EFC remains compatibility regression scope unless the implementation scope is explicitly enlarged.

### Implementation Patterns

Three defects were established from the initial source:

1. **Child filing value differs from the top-level contract.** `BreadcrumbStateRow.WithFilingTarget` preserves the relative presented suggestion target in the leaf’s `FolderPath`. `BreadcrumbSelectionMap.GetSelectedFolder` instead returns a selected child’s rooted provider `FolderPath`. The controller forwards that value to `DestinationOlStem`, which `EmailFilerConfig.ResolvePaths` rejects through `ArchiveStemContract.RequireArchiveRelativeStem`. Existing click handlers and explicit child commit already exist; this establishes an unusable filing destination, not an absence of click handlers.
2. **Recursive state and addressing are absent.** The state model stores one immediate-child list and one selected child index. The bridge and renderer cannot independently address, expand, or render deeper descendants.
3. **Selection depends on visible expansion storage.** `TryCollapseLeaf` clears `Subfolders`, while `SelectedSubfolderIndex` can still reference that collection. Subsequent selection mapping indexes the emptied list. Separately, opening after a child commit and cancelling can restore the containing row instead of the child because the session snapshots only row identity.

EFC qualification: its `subfolderResult` JavaScript handler creates `.child` elements without `data-child-index`, while activation requires that attribute. However, `ExpandLeafAsync` posts the result followed by a correct indexed row render. That mismatch alone did not establish a persistent EFC selection failure.

### Complete Examples

```csharp
// Initial BreadcrumbSelectionMap.GetSelectedFolder child-selection branch.
if (model.SelectedSubfolderIndex >= 0)
{
    return row.Subfolders[model.SelectedSubfolderIndex].FolderPath;
}
```

This branch returns the hierarchy value directly. The downstream filing boundary requires an archive-relative stem.

### API and Schema Documentation

`IFolderHierarchyProvider.GetImmediateSubfoldersAsync(FolderTreeNodeKey, CancellationToken)` already supplies the lazy traversal seam. Returned `FolderBreadcrumbSegment` objects contain `Key`, `DisplayName`, `FolderPath`, and `HasChildren`. No provider dependency addition is required.

The existing QuickFiler selector contract uses `rowIdentity` and `subfolderIndex`. Preserve first-level index messages as compatibility aliases while introducing stable descendant addressing. Resolve client-supplied identities against host-owned nodes; do not accept client-supplied filing paths.

Retain distinct raw hierarchy paths and filing values. Explicit bound-root normalization can use `ArchiveStemContract`; existing constructors without root context must preserve their tested full-path behavior.

### Configuration Examples

```text
Format:
dotnet tool run csharpier format .

Analyze:
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild

Type check:
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -TreatWarningsAsErrors

Test with coverage:
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

These commands were discovered in the actual workspace configuration and scripts, not executed by the researcher. The coverage wrapper supports `-CoverageOutput` for canonical feature evidence storage.

The nullable workflow uses per-file `#nullable enable` and a warnings-as-errors rebuild without globally enabling nullable. CI checks formatting with `dotnet csharpier check .`.

TaskMaster CI contains seven jobs: actionlint, format-check, build-analyzers, build-nullable, mstest-coverage, pester, and hygiene. Pull-request triggers target `main` and `development`. The coverage wrapper runs vstest through dotnet-coverage and excludes LiveOutlook tests and test-assembly instrumentation.

### Technical Requirements

- Every displayed folder, including intermediate folders, must be selectable.
- Plus/minus activation changes expansion only.
- Every descendant with children receives its own expansion control; leaves receive none.
- Selection survives visual collapse and cancellation of a subsequently reopened selector.
- Preserve archive-relative validation, explicit popup commit/close behavior, provider ordering, duplicate row occurrences, and no-root compatibility.
- Reject unknown, unloaded, cross-row, and invalid archive destinations.
- Provider completions must not restore collapsed nodes or attach data to replaced populations.
- Capture a new baseline, fail-before/pass-after regressions, final ordered QA, and coverage comparison under canonical feature evidence paths.
- Repository requirements remain at least 80% repository line coverage and at least 90% coverage for new code; do not expand coverage exclusions.

Initial physical line counts were HTML 490, router 489, session 474, bridge messages 463, coordinator 437, and child activation tests 480. These are investigation-time measurements, not final implementation counts.

No objective was established as unachievable.

## Recommended Approach

Implement a lazy host-owned tree beneath each unique suggestion-row occurrence. Each node retains its stable provider key, display name, raw hierarchy path, expansion state, children, and request generation.

Activate only a node resolved from the current host-owned tree. Preserve the containing row identity for duplicate-occurrence and probability contracts, but retain the committed node and filing value independently of expansion visibility. Continue emitting one selection event and the existing explicit popup close on activation. Toggle operations publish expansion/render changes without modifying the destination.

Use explicit optional archive-root context for filing normalization, preserving constructor defaults and existing no-root full-path contracts. Keep the raw hierarchy path available for provider queries and display.

Perform provider awaits outside the state lock. Attach results only when the same row and node remain current, the request generation matches, expansion is still intended, and the population generation has not changed. Clear, replacement, and collapse invalidate pending work.

Rejected alternatives: a JavaScript-only tree leaves host selection and filing validation incomplete; eager subtree loading performs unnecessary work and creates more asynchronous state than the existing immediate-child provider requires.

## Implementation Guidance

- **Objectives**: Deliver usable descendant filing destinations and recursive expansion through the active QuickFiler embedded selector, preserving established compatibility boundaries.
- **Key Tasks**:
  - Add failing regressions before production changes.
  - Introduce host-owned recursive node state and stable descendant addressing.
  - Separate committed node/value from visible expansion storage and preserve complete selection snapshots.
  - Apply archive-relative normalization with explicit root context.
  - Add lazy expansion with stale-completion rejection.
  - Preserve legacy first-level messages and no-root behavior.
  - Complete the ordered toolchain, coverage comparison, acceptance-criteria tracking, and evidence-backed handoff.
- **Dependencies**: Existing hierarchy provider, bridge serializers, render projection, coordinator, archive-stem contract, and MSTest/Moq/FluentAssertions. No new runtime dependency is recommended.
- **Success Criteria**: Three or more levels support intermediate and leaf selection; toggles leave selection unchanged; leaves have no controls; duplicate identities remain distinct; invalid addresses are deterministic no-ops; stale asynchronous work cannot overwrite current state; required compatibility tests and final QA pass.

Proposed production inventory:

- Modify `BreadcrumbStateModel.cs`, `BreadcrumbStateModel.Row.cs`, `BreadcrumbSelectionMap.cs`, `BreadcrumbRenderProjection.cs`, and `BreadcrumbSelectorMessages.cs`.
- Split near-limit files before extending them: `BreadcrumbBridgeMessages.cs`, `BreadcrumbSelectionSession.cs`, `FolderBreadcrumbBridgeRouter.cs`, and, if necessary, `BreadcrumbBridgeCoordinator.cs`. Proposed new parts are `BreadcrumbSelectionSession.Descendants.cs`, `FolderBreadcrumbBridgeRouter.Descendants.cs`, and `BreadcrumbBridgeCoordinator.Descendants.cs`.
- Add focused `BreadcrumbDescendantNode.cs`.
- Update `FolderBreadcrumb.html` while respecting its initial 490-line size. Extract an embedded script with shared collapsed/popup resource composition if needed, or reduce existing HTML sufficiently.
- Pass explicit archive-root context through `QfcItemController.BreadcrumbWiring.cs` and `ItemViewer.Breadcrumb.cs`.
- Update resource declarations only if assets are split, and add explicit source/resource entries to affected legacy project files.

Added-file names above are proposed research inventory, not claims about final implementation artifacts.

Proposed regression files and scenarios:

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbDescendantSelectionIssue997Tests.cs`: relative presented `Projects\Apollo`, rooted provider leaf under the archive, and rooted child `Alpha`; activate through the existing first-level contract and assert `Projects\Apollo\Alpha` satisfies `RequireArchiveRelativeStem`. Also cover child commit → reopen → parent collapse without changing readback, and reopen → Escape preserving the descendant. These scenarios expose defects through existing APIs.
- `FolderBreadcrumbRecursiveAssetIssue997Tests.cs`: verify an expandable child receives an independent control and a leaf does not. Recursive contract tests should report missing APIs/contracts through assertions rather than uncompilable test code.
- `FolderBreadcrumbDescendantOrderingIssue997Tests.cs`: use `TaskCompletionSource` barriers for collapse during fetch, clear/rebind during fetch, and reversed completion. Avoid sleeps, filesystem access, Outlook, and external processes.
- `QuickFiler.Test/Viewers/BreadcrumbDescendantActivationIssue997Tests.cs`: verify one selection event, explicit close/focus once, collapsed display, and reopened highlight. Add a separate file rather than extending the initial 480-line activation suite.

Preserve or update these existing contracts as appropriate:

- `BreadcrumbSelectionMapTests.GetSelectedFolder_ExpandedSubfolderSelection_YieldsTheSubfolderFullPath` for no-root compatibility.
- `BreadcrumbSubfolderSelectorSessionTests` for durable commit and invalid activation.
- `BreadcrumbSubfolderActivationTests` for readback, render, event, and close ordering.
- `FolderBreadcrumbAssetContractTests` for immediate activation messages, accessible child state, identifiers, and function-body contracts.
- `BreadcrumbSelectorMessagesTests` for strict JSON field types and integer validation.
- `BreadcrumbRenderProjectionSelectorTests` for collapsed projection and probabilities.
- `BreadcrumbDuplicateIdentityIntegrationTests` for distinct logical row occurrences.
- `FolderBreadcrumbBridgeRouterInFlightTests` for population generations and reconciliation.

Use existing asset-contract testing alongside host behavioral tests. The research did not identify an existing JavaScript engine dependency that would justify introducing another runtime.
