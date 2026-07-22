# Code Review: QuickFiler Folder Selector Drop-Down (#400)

**Review Date:** 2026-07-21
**Reviewer:** Codex feature-review
**Feature Folder:** docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400
**Feature Folder Selection Rule:** The branch, issue metadata, checkpoint, and full-bug specification all identify issue #400.
**Base Branch:** main at df5ad49c909f6b739edef45d0336151f44e827a6
**Head Branch:** bug/quickfiler-folder-selector-dropdown-400 at b38a87751669f3522928dd01ac0f4f97b82572ed
**Review Type:** Initial review

## Executive Summary

The full merge-base diff was reviewed, including 13 production C# files, 16 C# test files, four project files, the shared HTML asset, and all feature evidence. The design separates selector state, message serialization, popup placement, multi-surface messaging, and WinForms hosting. Exact-head evidence records clean formatting, zero new analyzer/nullable diagnostics, 5,830 passing tests, and repository line coverage of 84.1610%.

The branch needs revision. Initial popup state is not gated on document readiness, so the first cached render, theme, selector state, and focus request can be lost. The asynchronous surface factory is also not serialized or invalidated when Reset or Dispose occurs, allowing duplicate or stale surfaces and callbacks.

**What changed:** The branch adds scored fallback projection, committed/original/pending selection state, selector messages, an owned ToolStripDropDown WebView host, a two-surface messenger hub, deterministic placement, ItemViewer/controller wiring, expanded/collapsed HTML behavior, and broad MSTest coverage.

**Top 3 risks:**

1. The first popup can be blank or have default state because messages are posted before the document listener is ready.
2. Concurrent open, reset, or disposal during factory completion can attach multiple or stale surfaces.
3. Existing tests do not exercise these timing boundaries or several full composition paths.

**PR readiness recommendation:** **Needs Revision** — two major production correctness findings violate the feature's state-delivery and lifecycle guarantees.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | QuickFiler/Viewers/BreadcrumbDropDownHost.cs; QuickFiler/Viewers/BreadcrumbMessengerHub.cs; QuickFiler/Resources/FolderBreadcrumb.html | Host 359-378; hub 62-88 and 168-173; HTML 391-423 | CreateProductionSurfaceAsync calls NavigateToString and immediately returns a messenger. The host then raises PopupMessengerReady, and hub attachment immediately replays cached render, theme, and selector messages. The page registers its message listener only when its script executes. There is no NavigationCompleted, DOM-ready, or page-ready handshake. | Do not expose or attach the popup messenger until the target document is ready. Add a deterministic readiness seam or explicit page-ready protocol, replay cached state exactly once after readiness, and handle navigation failure with cleanup and selection rollback. | WebView2 posting is asynchronous, and navigation before a posted message is delivered can prevent that message from reaching the target document. A first popup can open without rows, theme, pending option, or focus state. | Source inspection; FolderBreadcrumbAssetContractTests only inspect strings. Microsoft PostWebMessageAsJson and NavigationCompleted documentation linked in Research Log. |
| Major | QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 117-165, 190-260, 311-330 | EnsureSurfaceAsync has no shared in-flight task, generation token, cancellation, or post-await disposed/reset validation. Two overlapping opens can both create and attach a surface. Reset or Dispose while the factory is pending does not invalidate its completion. | Serialize lazy initialization through one in-flight operation. Use lifecycle generation/cancellation state so stale completion is disposed and cannot attach, raise PopupMessengerReady, show, focus, or call selection callbacks after reset/disposal. | The feature requires one reused surface, no orphan popup, one live subscription, and no callback after disposal. The current field check occurs only before awaiting the factory. | BreadcrumbDropDownLifecycleTests uses completed Task factories and has no TaskCompletionSource case for concurrent open, reset, or disposal. |
| Minor | QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs; QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs; QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs | Asset tests 12-206; integration 67-84; coordinator 176-236 | The HTML tests are source-string/regex contracts rather than executed message/DOM behavior. Integration tests do not move pending selection before an automatic close, and no populated open-session Up message covers separator skipping and boundary clamping. | Add deterministic behavior tests through a page-ready/message seam, an integrated pending-move then uncommitted-close scenario, and an inbound selectorKey up scenario with nonselectable rows and boundaries. | The current suite can pass while runtime listener ordering is wrong, and it does not fully prove the AC-8 and AC-16 composition contracts. | Full test-file inspection and the passing 115-test issue-specific inventory. |
| Minor | QuickFiler/Viewers/BreadcrumbDropDownHost.cs; spec.md | Host 356-397; AC-18 | Two new production methods are excluded from numeric coverage. The recorded scope change is narrow and coverage.config is unchanged, but literal AC-18 and the plan require numeric coverage for every new method. CreateProductionSurfaceAsync also contains the readiness defect above. | Keep direct UI adapters minimal, cover all host-neutral behavior through deterministic seams, and reconcile the literal coverage requirement through the authorized requirements workflow or provide qualifying numeric evidence. Do not present the existing nonnumeric methods as satisfying the original criterion. | Measurable coverage passes, but the criterion cannot be marked fully delivered when two new methods have no numeric result. | coverage-delta.2026-07-21T17-49.md; coverage-accounting-scope-change.2026-07-21T18-01.md. |

## Implementation Audit

### C# implementation audit

#### What changed well

- BreadcrumbSelectionSession keeps committed, original-at-open, and pending identities separate.
- BreadcrumbPopupPlacement is a pure geometry component with primary, non-primary, negative-coordinate, tie, clamp, and zero-space coverage.
- BreadcrumbMessengerHub centralizes cached state and one inbound subscription per surface.
- Score-preserving fallback rows and router changes retain probability and issue #398 generation/atomic-swap behavior.
- New files are explicitly included in legacy project files, and all changed production/test C# files remain within the 500-line limit.

#### Type safety and API notes

The final nullable and analyzer evidence reports no new diagnostics. Existing IItemViewer signatures remain source-compatible. The added host interface and injected factories/delegates are appropriately narrow. The lifecycle contract is incomplete because an asynchronous factory completion is not bound to the current host generation.

#### Error handling and logging

Completed initialization failures dispose partial resources and restore selection/focus. Show failures are caught and converted to a clean close. The missing stale-completion checks create an uncovered error boundary: a reset or disposed host can still be mutated after an await.

## Test Quality Audit

The exact-head repository run reports 5,830 passed, zero failed, zero skipped in 53.4409 seconds. Issue-specific evidence reports 115 tests across 15 test families. Tests use MSTest and FluentAssertions, avoid sleeps and temporary files, and isolate live WebView/WinForms behavior through injected seams.

The suite is not scenario-complete. All surface factories used in lifecycle tests complete synchronously. The HTML contract suite loads source text but does not execute JavaScript or verify listener readiness. The automatic-close and Up-key semantics are proven in separate components but not in the missing composed scenarios listed above.

### Reviewed test and QA artifacts

- evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md — 115 issue-specific tests pass.
- evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md — predecessor concurrency regressions pass.
- evidence/qa-gates/final-mstest-coverage.2026-07-21T17-44.md — eight assemblies and 5,830 tests pass.
- evidence/qa-gates/coverage-delta.2026-07-21T17-49.md — numeric baseline/final/change accounting.
- QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs — covers completed lazy initialization, reuse, reset, failure, and disposal but not pending factory state.
- QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs — verifies static HTML contract tokens without executing page behavior.

### Quality assessment

- **Determinism:** PASS for existing tests; no live UI, sleep, temporary file, network, or external service.
- **Isolation:** PASS for host-neutral units; PARTIAL for runtime WebView integration.
- **Speed:** PASS; the complete suite finishes in 53.4409 seconds.
- **Diagnostics:** PASS; scenario-specific names and FluentAssertions provide useful failures.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Full diff inspection found no secret material. |
| No unsafe subprocess or command construction | N/A | Feature code adds no process execution. |
| Input validation at boundaries | PASS | Selector serializers reject missing identity and unknown key values; placement handles zero/limited bounds. |
| Error handling remains explicit | PARTIAL | Immediate failures are handled, but stale async completion after reset/disposal is not. |
| Configuration and path handling is safe | PASS | No new setting, persisted path, package, or coverage-filter change. |
| Resource lifecycle is deterministic | FAIL | Pending initialization is not serialized or invalidated. |

## Research Log

External research was used only to verify WebView2 message/navigation ordering:

- [CoreWebView2.PostWebMessageAsJson](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.postwebmessageasjson?view=webview2-dotnet-1.0.4022.49) documents asynchronous posting and that a navigation can prevent delivery to the prior page.
- [CoreWebView2.NavigationCompleted](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.navigationcompleted?view=webview2-dotnet-1.0.3856.49) identifies the navigation completion event available after the top-level document is loaded.

## Verdict

The branch is not ready for normal PR flow. The host-neutral design, broad tests, numeric coverage, and exact-head toolchain evidence are substantive, but they do not offset the two major lifecycle defects. Remediation must gate initial state on page readiness, serialize and invalidate pending surface creation, add deterministic regression tests for those boundaries and the missing composition scenarios, rerun the ordered C# toolchain, and repeat feature review.
