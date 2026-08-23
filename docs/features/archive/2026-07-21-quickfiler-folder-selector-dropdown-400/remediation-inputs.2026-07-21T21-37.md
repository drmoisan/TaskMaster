# Remediation Inputs: QuickFiler Folder Selector Drop-Down (#400)

**Timestamp:** 2026-07-21T21-37Z
**Work Mode:** full-bug
**Authoritative Requirements:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md`
**Review Inputs:** `code-review.2026-07-21T21-27.md`, `policy-audit.2026-07-21T21-27.md`, and `feature-audit.2026-07-21T21-27.md`
**Review Verdict:** REMEDIATION_REQUIRED

## Objective

Resolve all seven Major findings from the independent post-remediation feature review without widening issue #400 beyond the breadcrumb selector. Preserve the currently passing probability, issue #398, placement, accessibility, popup ownership, readiness, lifecycle, project-wiring, coverage, and full-suite contracts.

## Required Fixes

### 1. Make logical row identities unique

**Affected files:**

- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- `QuickFiler/Resources/FolderBreadcrumb.html`
- Relevant `UtilitiesCS.Test` and `QuickFiler.Test` selector/projection/asset/integration tests

**Required behavior:**

- A suggestion and recent/search/plain row may expose the same folder output path without sharing the same logical row identity.
- Identities remain stable through synchronous scored fallback and asynchronous resolved-chain replacement.
- `GetSelectedFolder`, `SetFolderSelectedItem`, `FolderContains`, and probability display continue to use/preserve the exact folder output path.
- Closed/open Up/Down navigation reaches each selectable duplicate row once, clamps at boundaries, and does not oscillate.
- Mouse/accessibility activation commits the exact activated logical row.
- Exactly one expanded option is active/`aria-selected=true`; collapsed rendering chooses the actual committed row.

**Failure-first verification:** Add deterministic duplicate suggestion/recent tests covering fallback, resolved upgrade, closed navigation, open pending navigation, activation, collapsed render, and accessible active-option state. Record exact failing commands and intended failures under `evidence/regression-testing/`.

### 2. Enforce the WebView2 UI-thread boundary

**Affected files:**

- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- `QuickFiler/Viewers/WebView2Messenger.cs` or a new focused dispatcher abstraction if required
- Relevant coordinator/integration tests and legacy project includes

**Required behavior:**

- Every production request into `CoreWebView2`, including render, selector state, theme, and routed provider results, occurs on the owning UI/STA thread.
- Suggestion hierarchy upgrades and subfolder-provider completions may perform pure/provider work off-thread but marshal message delivery and host callbacks back to the owning synchronization boundary.
- Invalid or failed dispatch remains observable and cannot escape an `async void` event boundary as an unhandled exception.
- Existing fake messenger tests remain host-neutral; add a deterministic dispatcher/thread-affinity seam rather than live WebView/UI tests.

**Failure-first verification:** Hold provider completion on a worker continuation and prove every resulting message/callback is scheduled through the injected UI dispatcher before production posting.

### 3. Gate the persistent collapsed surface on correlated document readiness

**Affected files:**

- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` or a shared ready-surface helper
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` if attachment semantics require a ready state
- Relevant readiness, integration, hub, and controller tests

**Required behavior:**

- The collapsed surface does not attach to the hub or receive cached render, selector, or theme state until the exact `NavigateToString` navigation is complete and successful.
- An unrelated `NavigationCompleted` event cannot release the surface.
- Failure, reset, and disposal detach handlers, reject late completion, and leave no partial attachment.
- Initial suggestions populated before CoreWebView initialization and the current light/dark theme replay exactly once after readiness.
- Popup readiness behavior remains intact and does not diverge from the shared correlation contract.

**Failure-first verification:** Add deterministic pending, unrelated-completion, success, failure, reset/disposal, and cached render/theme replay tests for the collapsed surface.

### 4. Route every selector/model mutation through one synchronization owner

**Affected files:**

- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- Relevant in-flight router/session/coordinator tests

**Required behavior:**

- Selection-session open/move/activate/commit/cancel and `SelectItem` cannot read or mutate the model outside the router's synchronization/generation contract.
- A user selection or pending move made after an upgrade begins survives the replacement.
- Atomic replacement still exposes no cleared/partial model and stale generations cannot overwrite current state.
- Do not introduce blocking waits or execute external provider/UI work while holding the model lock.

**Failure-first verification:** Use controlled completion sources to interleave an upgrade with closed movement, open pending movement, activation, and item selection. Prove the post-start user state wins.

### 5. Cancel or suppress stale suggestion upgrades across lifecycle transitions

**Affected files:**

- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` as needed
- Relevant lifecycle/concurrency/integration tests

**Required behavior:**

- Coordinator-owned cancellation/generation state invalidates prior suggestion upgrades on later population, clear/reset, pooled reuse, and disposal.
- Stale success/failure completes without posting render/selector state, raising callbacks, or mutating current error/state.
- Coordinator disposal unsubscribes its inbound message handler and prevents any later production messenger call.
- Exactly one render/selector update is delivered for the current population to each attached ready surface.
- Late completion after disposed hub cannot fault through `PostJson`.

**Failure-first verification:** Cover overlapping upgrades, clear/reset then late success/failure, pooled reuse with new data, and disposal with late success/failure. Assert zero stale posts/callbacks and one current update.

### 6. Make pending native open cancelable through Close

**Affected files:**

- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`
- `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` only if the documented close contract must be clarified
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
- Relevant host readiness/lifecycle/concurrency/integration tests

**Required behavior:**

- `Close(Uncommitted)` and the selector toggle/Escape/outside-close path invalidate a pending factory or readiness wait before `_isOpen` becomes true.
- The shared `OpenAsync` task completes deterministically as false; the canceled request cannot later show or focus the popup.
- Selection rollback and anchor focus occur exactly once.
- A fully created reusable surface may be retained only if doing so cannot attach, show, focus, or callback after cancellation; partial/stale resources remain disposed.
- Existing Reset/Dispose/reentrant callback safeguards remain intact.

**Failure-first verification:** Cover close while the factory is pending, close while readiness is pending, repeated close, open-after-canceled-close, and ItemViewer toggle/automatic-close composition.

### 7. Commit expanded subfolder activation into the selector session

**Affected files:**

- `QuickFiler/Resources/FolderBreadcrumb.html`
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`
- Relevant bridge, selector, asset, and integration tests

**Required behavior:**

- Clicking/activating an expanded subfolder commits the subfolder selection as a selector action, publishes `SelectionChanged` exactly once, closes with explicit-commit semantics, renders durable collapsed readback, and returns focus.
- A later Enter, Escape, outside close, or native close cannot silently replace or roll back the committed subfolder.
- Existing legacy selection output mapping to the full subfolder path remains unchanged.
- Invalid subfolder indexes remain explicit deterministic errors/no-ops according to the existing bridge contract.

**Failure-first verification:** Compose open selector + subfolder activation followed by Enter, Escape, and native automatic close; assert durable readback, one event, one close, and no rollback.

## Acceptance-Criteria Reconciliation

Before implementation, reconcile the authoritative `spec.md` checkbox state to the independent review: AC-1, AC-5, AC-6, AC-7, AC-8, AC-10, AC-11, AC-12, AC-13, AC-14, AC-15, AC-16, and AC-19 must be unchecked. Preserve AC wording exactly. AC-2, AC-3, AC-4, AC-9, AC-17, and AC-18 remain supported by current evidence. Re-check a criterion only after the implementation and final evidence directly prove it.

## Required Baseline and Final Verification

Capture one baseline artifact per command step before implementation, then run one uninterrupted final pass after all fixes. Every evidence artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` and reside under this feature's `evidence/baseline/`, `evidence/regression-testing/`, or `evidence/qa-gates/` directory.

Final commands, in order:

1. `csharpier format .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <feature-folder>/evidence/qa-gates/coverage-final.<timestamp>.cobertura.xml`

Required final gates:

- All first-party tests pass with zero failures and zero skips.
- Repository C# line coverage remains at least 80%.
- Every measurable new/changed selector type and member remains at least 90%.
- Modified-line coverage does not regress; numeric baseline, final, and delta are recorded.
- Any direct nonnumeric WebView2/WinForms adapter remains method-bounded, enumerated, and fully protected by deterministic injected seams.
- `coverage.config`, package references, thresholds, and exclusions are unchanged unless the approved plan explicitly requires a narrower policy-compliant change; no weakening is permitted.
- `git diff --check` passes and every added C# file has exactly one legacy project include.
- Every new or modified production/test C# file remains at most 500 lines.
- A fresh independent policy audit, code review, and feature audit validates all 19 ACs after remediation.

## Do Not Do

- Do not widen scope beyond issue #400 breadcrumb selector behavior and the minimum shared seams required by these findings.
- Do not weaken, waive, filter, or reinterpret coverage thresholds.
- Do not add sleeps, temporary files, external services, live Outlook/WebView/UI dependencies, screenshots, or user-operated/manual QA steps.
- Do not deduplicate away a valid recent/search row as a substitute for unique logical row identities unless the authoritative product contract is explicitly changed.
- Do not marshal pure/provider work unnecessarily to the UI thread; marshal only UI/WebView requests and callbacks at the explicit boundary.
- Do not hold the router lock across `await`, provider I/O, WebView posts, or host callbacks.
- Do not silently swallow stale/current failures; suppress only work proven stale by the lifecycle/generation contract.
- Do not modify generated `ItemViewer.Designer.cs`, introduce new external packages, or add persisted configuration.
- Do not remove or relax existing issue #398, probability, popup-readiness, lifecycle, placement, accessibility, or coverage assertions.
- Do not mark any failed AC delivered or skip any planned command task without direct evidence and an explicitly authorized branch in the approved plan.
