# Code Review: QuickFiler Folder Selector Drop-Down (#400)

**Review Date:** 2026-07-21
**Reviewer:** Codex feature-review (independent post-remediation review)
**Feature Folder:** `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`
**Feature Folder Selection Rule:** Branch name, issue metadata, plan of record, and full-bug specification all identify issue #400.
**Base Branch:** `origin/main` at `fd9fb5ee1ca0c044b8dd0e02a81a22f58c6f3f68`; merge base `df5ad49c909f6b739edef45d0336151f44e827a6`
**Head Branch:** `bug/quickfiler-folder-selector-dropdown-400` at `b38a87751669f3522928dd01ac0f4f97b82572ed`, plus the current tracked and untracked remediation worktree
**Review Type:** Post-remediation re-review

## Executive Summary

The complete merge-base feature range and current remediation worktree were reviewed, including selector state, projection, routing, WebView messaging, native popup lifecycle, ItemViewer/controller integration, the HTML/JavaScript/CSS resource, 19 issue-related test classes, and final QA/coverage evidence. The ordered C# toolchain is green: 5,849/5,849 tests pass and repository line coverage is 84.1647%.

The change is not ready for PR flow. Seven runtime correctness defects remain despite the green tests. Duplicate folder paths are used as row identities; WebView2 calls can occur off the UI thread; the collapsed surface replays before document readiness; selector-session writes bypass the router lock; hierarchy upgrades remain active after reset/disposal; a close cannot cancel a pending native open; and subfolder activation is not reconciled with the open selector session. These defects affect selection, accessibility, lifecycle, and exactly-once delivery requirements.

**What changed:** The branch adds score-preserving fallback projection, committed/original/pending selector state, two-surface message replay, a lazy owned `ToolStripDropDown` WebView host, deterministic placement, ItemViewer/controller wiring, expanded/collapsed resource behavior, and broad MSTest evidence. The remediation adds popup navigation readiness and host lifecycle-generation safeguards, but does not address the seven findings below.

**Top 3 risks:**

1. A path present in both suggestions and recents produces duplicate active options and commits the wrong row.
2. Normal asynchronous hierarchy work posts into WebView2 from a worker thread, contrary to the control's STA/UI-thread contract.
3. Reset, disposal, or close does not fully invalidate coordinator upgrades or pending host open work, allowing late messages, exceptions, or transient popup/focus behavior.

**PR readiness recommendation:** **Needs Revision** — seven Major findings leave AC-1, AC-5 through AC-8, and AC-10 through AC-16/AC-19 incomplete.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `FolderPredictor.cs`; `FolderBreadcrumbBridgeRouter.cs`; `BreadcrumbSelectionSession.cs`; `FolderBreadcrumb.html` | predictor 243-256, 832-859; router 64-67, 94-97, 123-132; session 183-199; asset 221-224, 258-273 | Suggestion and recent rows can carry the same folder path, and that path is used as both rows' identity. Identity lookup selects the first match, while the page marks every matching row active. Down can repeatedly resolve from the first duplicate, activation of the recent row commits the suggestion row, and multiple options receive `aria-selected=true`. | Generate a unique stable row identity per logical row while preserving the folder path separately as the output value; preserve identities through fallback-to-resolved upgrades; add duplicate suggestion/recent navigation, activation, collapsed-render, and accessibility regressions. | Identity is used for pending navigation, commit, collapsed selection, and active-option accessibility and therefore must be unique within one model. | Direct source trace; `FolderRowArray` performs no suggestion/recent deduplication; no changed test covers duplicate identities. |
| Major | `BreadcrumbBridgeCoordinator.cs`; `WebView2Messenger.cs` | coordinator 81-86, 112-119, 330-361; messenger 39-45 | `ConfigureAwait(false)` resumes hierarchy/subfolder continuations on a worker thread and then calls `_messenger.PostJson`; the production messenger calls `CoreWebView2.PostWebMessageAsJson` directly. | Capture/use the owning UI synchronization boundary for every production WebView post and host callback, or retain the UI context across these awaits; add a deterministic thread-affinity seam test for suggestion upgrades and inbound provider completion. | Microsoft requires every request into WebView2 to occur on its UI thread; fake messengers cannot reveal the current violation. | Source inspection and Microsoft WebView2 threading model: https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model |
| Major | `ItemViewer.Breadcrumb.cs`; `BreadcrumbMessengerHub.cs`; `QfcItemController.ViewerSetup.cs` | ItemViewer 62-70; hub 59-88; setup 107-113, 156-163 | The collapsed surface calls `NavigateToString` and immediately attaches, which immediately replays cached render/selector state; theme is then posted without a navigation-ready handshake. Initial state can target the prior/incomplete document and be lost. | Give the collapsed surface the same correlated navigation-readiness contract as the popup and attach/replay only after the target document is ready; test pending, unrelated completion, success, failure, reset, and initial cached theme/render replay. | `PostWebMessageAsJson` is asynchronous and is not delivered if navigation intervenes. AC-13 requires theme/state to reach both surfaces. | Source inspection and Microsoft API remarks: https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.postwebmessageasjson |
| Major | `FolderBreadcrumbBridgeRouter.cs`; `BreadcrumbSelectionSession.cs`; `BreadcrumbBridgeCoordinator.cs` | router 20-22, 34-35, 100-107, 196-213, 427-448; session 33-40, 63-164, 183-199; coordinator 145-165, 203-255, 266-299 | New selector-session reads/writes and direct item selection mutate the public model outside the router's `_sync` lock. An async upgrade can capture the old identity, a user move can select a new row, and replacement can reselect the old identity. | Move all selector transitions and identity-based selection behind router methods using the same synchronization/generation boundary; add a controlled in-flight upgrade regression for closed and open selector movement/activation. | AC-11 explicitly requires a host selection made after upgrade start to survive replacement. Existing in-flight tests cover only `router.SelectRow`, not the new selector session. | Source interleaving inspection; `FolderBreadcrumbBridgeRouterInFlightTests` lines 229-254 use the locked route only. |
| Major | `BreadcrumbBridgeCoordinator.cs`; `FolderBreadcrumbBridgeRouter.cs`; `ItemViewer.Breadcrumb.cs`; `BreadcrumbMessengerHub.cs` | coordinator 96-119; router 55, 100-108, 121, 174, 188; ItemViewer 212-226, 349-369; hub 121-124 | Suggestion upgrades use `CancellationToken.None`, retain only the latest task reference, and always post after completion. A stale generation returns the current render and is still posted. Reset/disposal neither cancels nor awaits upgrades; a late completion can duplicate a new state or call the disposed hub and fault. | Add coordinator lifecycle cancellation/generation ownership, suppress stale completion before any post/callback, detach/dispose the coordinator, and test reset, pooled reuse, disposal, overlapping upgrades, late success, and late failure. | AC-12 requires once-per-update delivery and AC-14 requires no callback after disposal. Router generation protects model replacement but not coordinator posts. | Direct source trace; existing readiness/lifecycle tests constrain the popup host, not coordinator suggestion upgrades. |
| Major | `BreadcrumbDropDownHost.cs`; `ItemViewer.Breadcrumb.cs` | host 137-166, 209-251, 263-269; ItemViewer 177-208, 261-285 | `Close` returns false while `_openTask` is pending because `_isOpen` is still false. The selector session is canceled, but the pending host can later show and focus the popup before ItemViewer notices the closed session and dismisses it. | Treat a pending open as closeable: invalidate/cancel its lifecycle without disposing the reusable ready surface unnecessarily, complete the shared task deterministically, and add close/toggle/Escape/outside-close tests while factory and readiness tasks are pending. | A close request must not be followed by a popup/focus transfer, and pending work must not remain indefinitely. | Direct state-machine trace; current concurrency tests cover `Reset` and `Dispose`, not `Close` during pending initialization/readiness. |
| Major | `FolderBreadcrumb.html`; `BreadcrumbBridgeCoordinator.cs`; `BreadcrumbSelectionSession.cs`; `FolderBreadcrumbBridgeRouter.cs` | asset 287-305; coordinator 330-367; session 33-40, 113-133; router 256-273 | Expanded subfolder click posts legacy `selectionChange`, which updates the model and raises `SelectionChanged` but leaves the selector session open and committed identity unchanged. Later Enter commits the pending parent; Escape/outside close reselects the opening row and clears the subfolder selection. | Define subfolder activation as an explicit committed selector transition: synchronize committed row/subfolder state, close once with explicit-commit semantics, publish once, and preserve readback; add composition tests for click followed by Enter, Escape, and native automatic close. | A selection notification must correspond to durable committed readback. The current paths can announce one subfolder and later silently roll it back. | Direct message/session trace; no test combines an open selector with subfolder activation. |

No Blocker finding was identified. All seven Major findings require remediation.

## Implementation Audit

### C# implementation audit

#### What changed well

- `BreadcrumbSelectionSession` clearly separates committed, original, and pending state for unique-row scenarios.
- `BreadcrumbPopupPlacement` remains a pure, fully measured geometry component.
- Popup readiness correlates `NavigationStarting` and `NavigationCompleted` by navigation ID and detaches handlers on all completion paths.
- `BreadcrumbDropDownHost` now shares concurrent open work and rejects late factory/readiness completion after reset or disposal.
- Score fallback and issue #398 generation logic avoid transient empty/partially built models on the locked router path.

#### Type safety and API notes

Analyzer and nullable builds are clean for compiler/analyzer diagnostics. The primary API defect is ownership: the router exposes a mutable model, while the new selection session mutates it outside the router synchronization contract. Row identity is stable as a string across upgrades but not unique across logical rows, which makes it unsuitable as the selector's key.

#### Error handling and logging

Popup factory/readiness/show failures are bounded and dispose partial resources. Coordinator background work has no lifecycle cancellation or current-generation post guard, and the `async void` inbound boundary can observe WebView thread-affinity failures. These are correctness/lifecycle defects rather than logging gaps.

### HTML/Embedded JavaScript/CSS implementation audit

The resource has clear collapsed/expanded rendering, accessible roles, keyboard handling, and theme state for unique identities after ready delivery. Duplicate identity comparison makes multiple options active, and subfolder activation remains on the legacy message path without an explicit selector-session commit.

## Test Quality Audit

The final run reports 5,849 passed, zero failed/skipped, in 52.4323 seconds. C# repository coverage is 84.1647%; changed/new measurable production coverage is 99.8250%; 12/12 compiled-resource asset contracts pass. Tests are deterministic, isolated from live Outlook/WebView/display resources, and use MSTest/FluentAssertions.

Coverage is high but scenario completeness is not sufficient. The suite lacks duplicate path identities; WebView UI-thread enforcement; collapsed-document readiness; selector-session versus upgrade interleaving; stale coordinator upgrades after reset/disposal; pending-open close; and open-selector subfolder activation/rollback.

### Reviewed test and QA artifacts

- `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` — 5,849/5,849 tests and 84.1647% repository coverage.
- `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md` — 99.8250% changed/new measurable production coverage; all measurable selector types/members exceed 90%.
- `evidence/regression-testing/host-readiness-lifecycle-pass.2026-07-21T19-55.md` — verifies popup readiness and host reset/disposal, not the persistent collapsed surface or coordinator upgrades.
- `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md` — verifies unique-identity selector scenarios, not duplicate paths or subfolder/session composition.
- `FolderBreadcrumbBridgeRouterInFlightTests.cs` — verifies the locked host selection path but not unlocked selection-session mutations.

### Quality assessment prompts

- **Determinism:** PASS for implemented tests; no sleep, temp file, external service, screenshot, or user interaction was found.
- **Isolation:** PASS for covered seams; missing thread/readiness/lifecycle compositions create untested production boundaries.
- **Speed:** PASS; the complete repository run is 52.4323 seconds.
- **Diagnostics:** PASS; scenario names and FluentAssertions are clear.
- **Scenario completeness:** FAIL; seven material runtime paths lack regression coverage.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Full diff inspection found no credentials or secret material. |
| No unsafe subprocess or command construction | N/A | Feature code adds no process execution. |
| Input validation at boundaries | PARTIAL | Message parsing and index validation are explicit, but logical row identity uniqueness is not enforced. |
| Error handling remains explicit | FAIL | Late coordinator tasks and off-thread WebView calls are not bounded by the current lifecycle. |
| Configuration/path handling is safe | PASS | No persisted setting, package, coverage filter, or threshold change remains. |
| Resource lifecycle is deterministic | FAIL | Coordinator upgrades and pending close are not canceled/suppressed across lifecycle transitions. |
| Accessibility state is singular and stable | FAIL | Duplicate identities can mark multiple options active/selected. |

## Research Log

External research was required only for WebView2 runtime contracts:

- Microsoft WebView2 threading model confirms that the control is STA-based and every request into WebView2 must occur on its UI thread: https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/threading-model
- `CoreWebView2.PostWebMessageAsJson` remarks confirm asynchronous delivery and that a navigation occurring before posting prevents delivery: https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.postwebmessageasjson

## Verdict

The branch is not ready for normal PR flow. The remediation resolves the previously identified popup factory/readiness races and the final QA/coverage gates pass, but seven Major correctness findings remain. The change requires new failure-first regressions, targeted production fixes, a fresh ordered C# toolchain/coverage pass, AC reconciliation, and another independent feature review.
