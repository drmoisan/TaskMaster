# Acceptance Verification

Timestamp: 2026-07-21T21-19Z
Run Identity: `final-pass-2026-07-21T21-07Z`
Command: Complete source inspection of both method-level coverage exclusions; Roslyn source-span attribution of final Cobertura sequence points; protected readiness/lifecycle test mapping; and coverage configuration, threshold, and exclusion comparison against base `df5ad49c909f6b739edef45d0336151f44e827a6`
EXIT_CODE: 0
Output Summary: Numeric and nonnumeric coverage surfaces are fully accounted for. The two exclusions remain bounded direct adapters, every excluded readiness/cleanup outcome is verified through deterministic injected seams, no host-neutral lifecycle behavior is excluded, and no threshold, filter, or exclusion was weakened.

## Numeric Coverage Reference

`evidence/qa-gates/coverage-delta.2026-07-21T21-18.md` records:

- Repository coverage: 89,255/106,048 = 84.1647%.
- Changed/new measurable production lines: 1,141/1,143 = 99.8250%.
- Minimum measurable selector type: 98.2270%.
- Minimum measurable host/helper member: 97.5000%.
- Measurable helper `Create`: 7/7 = 100.0000%.

## Complete Nonnumeric Surface Inventory

| Surface | Current source span | Baseline/current status | Classification |
|---|---|---|---|
| `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` | Attribute line 30; method lines 31-116 | The direct-WebView2 exclusion moved from the former host adapter to this helper exactly once | Bounded nonnumeric direct WebView2/WinForms construction, initialization, correlated readiness, handler cleanup, messenger creation, and partial-control disposal |
| `BreadcrumbDropDownHost.ShowOwnedPopup` | Attribute line 477; method lines 478-482 | Unchanged method-level exclusion | Bounded nonnumeric single-expression `ToolStripDropDown.Show` adapter |

There is no class-level exclusion on `BreadcrumbWebViewSurfaceFactory`, `BreadcrumbDropDownHost`, or any other new dedicated selector type. The helper's measurable `Create` member remains instrumented and is 7/7.

## Why Direct Numeric Execution Is Unavailable

`CreateSurfaceAsync` directly constructs `WebView2`, requires a live `CoreWebView2Environment`, initializes the third-party browser core, subscribes to navigation events, navigates a document, and depends on native WebView2/WinForms event delivery. `ShowOwnedPopup` directly invokes native `ToolStripDropDown.Show` against a real control/owner handle. Executing those calls as unit tests would require live UI/browser resources and an external message-pump environment, contrary to the repository's deterministic, isolated unit-test policy.

The boundary does not make lifecycle behavior unverified. Production delegates expose a factory/display seam, and deterministic in-memory controls, messengers, completion sources, and callbacks exercise every observable pending, success, failure, cancellation, reset, disposal, reuse, focus, and cleanup result. The tests do not claim to execute WebView2 or native display directly.

## Excluded Helper Branch-to-Protected-Seam Mapping

| Excluded readiness/cleanup behavior | Deterministic protected factory-seam evidence | Verified outcome |
|---|---|---|
| Lazy construction with the existing environment | `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`; `ProductionConstructor_RejectsMissingInitializerOrHtml` | No factory call before open; supplied environment reaches one factory; invalid construction arguments fail explicitly |
| Core/factory remains pending | `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`; `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup` | No attach, replay, show, or focus before readiness; concurrent opens share one attempt |
| Correlated navigation has not completed or an unrelated completion must not expose the surface | `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`; `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface` | Incomplete designated readiness remains pending; reset/cancellation wins without exposing the surface |
| Correlated navigation succeeds | `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess` | Exactly one ready event, attachment, replay, show, and pending focus occur after success |
| Navigation/readiness fails | `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`; `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce` | Failure remains observable; partial resources are disposed; selection/focus restoration is bounded |
| Synchronous initialization/navigation exception or faulted factory | `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce`; `FailedFactoryTask_ClosesWithoutLeavingAHostOrCallbackSubscription`; `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` | Outer open pipeline resolves false, preserves the current error, and cannot leave a host or subscription |
| Partial surface cleanup after failure | `OpenAsync_PartialInitializationFailureDisposesAndRestoresFocusAndSelection`; `Reset_DisposesAnOrphanedPartialSurface` | Partial control/messenger ownership is released once and state is restored |
| Disposal or reset before readiness completes | `Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen`; `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation`; `Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation`; `OpenAsync_ResetWhileReadinessPending_CancellationRejectsSurface` | Late success/failure cannot mutate the current lifecycle; created resources are rejected/disposed |
| Handler detachment and no duplicate callback after reuse | `ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions`; `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`; `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | Reopen/reuse maintains one live attachment and no orphan or duplicate callback |
| Surface tuple and messenger creation become visible only after readiness | `OpenAsync_ReadyHandlerResetsLifecycle_RejectsInstalledSurface`; `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess` | A ready-handler reset invalidates and disposes the installed surface; current success exposes exactly one messenger |

The four P0-T9 protected assertion inventories remain unchanged as proved by `coverage-threshold-scope-integrity.2026-07-21T21-06.md`. The complete final repository run passed 5,849/5,849 tests.

## Excluded Native Display Mapping

`ShowOwnedPopup` contains one expression: `dropDown.Show(anchor, anchor.PointToClient(screenLocation))`. It has no branch, selection state, readiness state, or cleanup ownership.

- `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement` verifies owner, calculated location, and size through the injected display callback.
- `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure` verifies direct-display failure handling.
- `OpenAsync_ShowCallbackResetsLifecycle_StopsBeforeFocus`, `OpenAsync_FocusCallbackResetsLifecycle_StopsBeforeSuccess`, and `OpenAsync_ShowCallbackResetsThenThrows_DoesNotOverwriteCurrentLifecycle` verify reentrant display/focus lifecycle guards.
- `BreadcrumbPopupPlacement` remains numeric at 44/44; no placement logic is inside the exclusion.

## Host-Neutral Logic and Integrity Checks

The excluded helper method contains no committed/original/pending identity, selector model, replay cache, `ToolStripDropDown` open/close ownership, host generation, shared-open task, `LastInitializationException` ownership, focus callback, selection callback, or stale-completion state. Those behaviors remain in measurable host, hub, coordinator, session, and router members.

- Base exclusions: two method-level direct adapters.
- Current exclusions: the same two method-level direct adapters; one moved to the helper and one remained in the host.
- Class-level exclusion added: No.
- `coverage.config` working Git blob and `HEAD` blob: `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`.
- `git diff --exit-code -- coverage.config`: 0.
- Coverage filter or package setting change: None.
- Numeric threshold lowered or waived: No.
- Numeric omission hidden as zero or silently ignored: No. The raw helper 7/32 residue is explicitly attributed to the excluded adapter; measurable helper coverage is separately reported as 7/7.

P5-T7 result: PASS.

## Final AC-by-AC Reconciliation

Evidence paths are relative to `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/`. The current final repository run is `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md`; it discovered all eight first-party assemblies and passed 5,849/5,849 tests.

| Criterion | Direct current evidence | Verified behavior or gate | Result |
|---|---|---|---|
| AC-1 | `evidence/regression-testing/pass-after-probability-upgrade.2026-07-21T16-19.md`; `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Scored fallback and resolved rows retain the supplied score; collapsed projection returns the committed row and existing formatter output without recomputation. | PASS |
| AC-2 | `evidence/regression-testing/pass-after-html-asset.2026-07-21T17-04.md`; `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Asset-contract tests verify hidden overflow/scroll controls, exactly one accessible button, listbox semantics, and accurate `aria-expanded`. | PASS |
| AC-3 | `evidence/regression-testing/pass-after-popup-host.2026-07-21T16-37.md`; `evidence/regression-testing/pass-after-itemviewer-integration.2026-07-21T16-49.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Native `ToolStripDropDown`/`ToolStripControlHost` ownership, anchor-relative display, and absence of global topmost behavior are covered by host and ItemViewer seams. | PASS |
| AC-4 | `evidence/regression-testing/pass-after-popup-host.2026-07-21T16-37.md`; `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md`; `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md` | Placement tests cover below, above, more-space, equal-space, primary/non-primary, negative-coordinate, and zero-space clamping; `BreadcrumbPopupPlacement` is 44/44. | PASS |
| AC-5 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/pass-after-selector-domain.2026-07-21T16-14.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Closed Up/Down commits at most once, skips separators, clamps without wrapping, and remains a browser-scroll no-op. | PASS |
| AC-6 | `evidence/regression-testing/semantic-composition.2026-07-21T20-14.md`; `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/pass-after-html-asset.2026-07-21T17-04.md` | Inbound/open Up and Down change pending only, skip separators, clamp at boundaries, preserve committed identity, publish no committed selection, and keep the active expanded option visible. | PASS |
| AC-7 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/pass-after-coordinator-surfaces.2026-07-21T16-30.md`; `evidence/regression-testing/pass-after-popup-host.2026-07-21T16-37.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Enter and mouse/accessibility activation commit one stable pending identity, close, render committed state, publish once, and return focus through the host seam. | PASS |
| AC-8 | `evidence/regression-testing/semantic-composition.2026-07-21T20-14.md`; `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/closed-surface-regression.2026-07-21T20-03.md` | Native automatic close and Escape restore the opening identity, publish no pending selection, return focus once, and keep explicit commits distinct from rollback. | PASS |
| AC-9 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Left/Right continue routing once and preserve existing breadcrumb behavior without mutating selector-session identities. | PASS |
| AC-10 | `evidence/regression-testing/pass-after-probability-upgrade.2026-07-21T16-19.md`; `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md` | Immediate, resolved, unresolved, empty, and failed provider paths retain supplied scores, identity, and selection; only plain rows have no percentage. | PASS |
| AC-11 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Issue #398 tests verify atomic replacement, stable in-flight readback, post-start selection survival, and stale-generation rejection. | PASS |
| AC-12 | `evidence/regression-testing/closed-surface-regression.2026-07-21T20-03.md`; `evidence/regression-testing/host-readiness-lifecycle-pass.2026-07-21T19-55.md`; `evidence/regression-testing/pass-after-coordinator-surfaces.2026-07-21T16-30.md` | Closed and popup surfaces receive mode-specific views from one logical state, popup replay waits for readiness, and attach/detach/inbound routing remain exactly once without duplicates. | PASS |
| AC-13 | `evidence/regression-testing/host-readiness-lifecycle-pass.2026-07-21T19-55.md`; `evidence/regression-testing/pass-after-html-asset.2026-07-21T17-04.md`; `evidence/regression-testing/semantic-composition.2026-07-21T20-14.md`; `evidence/qa-gates/acceptance-verification.2026-07-21T21-19.md` | Theme and accessibility contracts pass; readiness gates pending focus; commit/cancel/failure paths return focus predictably through injected seams. | PASS |
| AC-14 | `evidence/regression-testing/host-readiness-lifecycle-pass.2026-07-21T19-55.md`; `evidence/regression-testing/closed-surface-regression.2026-07-21T20-03.md`; `evidence/regression-testing/coverage-threshold-focused-pass.2026-07-21T21-04.md`; `evidence/qa-gates/acceptance-verification.2026-07-21T21-19.md` | Lazy existing-environment creation, shared initialization, reuse, reset/disposal, stale completion rejection, one live attachment, and no post-disposal callback all pass. | PASS |
| AC-15 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/host-readiness-lifecycle-pass.2026-07-21T19-55.md`; `evidence/regression-testing/coverage-threshold-focused-pass.2026-07-21T21-04.md`; `evidence/regression-testing/pass-after-selector-domain.2026-07-21T16-14.md` | Empty/no-selectable, `-1`, invalid/unknown messages, initialization/readiness failure, zero placement, repeated lifecycle, and provider failure are deterministic and resource-safe while preserving committed selection/scores. | PASS |
| AC-16 | `evidence/regression-testing/readiness-fail-before.2026-07-21T19-04.md`; `evidence/regression-testing/lifecycle-fail-before.2026-07-21T19-13.md`; `evidence/regression-testing/failure-first-test-policy.2026-07-21T19-14.md`; original six `evidence/regression-testing/fail-before-*` artifacts and their `pass-after-*` counterparts; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | Original selector, probability, concurrency, bridge, placement, asset, ownership, readiness, and lifecycle defects have intended fail-before/pass-after evidence. Deterministic tests use no sleeps, temporary files, live services/UI, screenshots, or user interaction. | PASS |
| AC-17 | `evidence/qa-gates/file-size-and-project-includes.2026-07-21T20-19.md`; `evidence/qa-gates/coverage-threshold-scope-integrity.2026-07-21T21-06.md`; `evidence/qa-gates/final-diff-integrity.2026-07-21T21-21.md`; `evidence/qa-gates/semantic-test-policy.2026-07-21T20-17.md` | Every added source has one legacy-project include; final host is 484 lines, helper 118, threshold test 395, integration test 500, and all other changed sources are at most 500. Designer, package, and persisted configuration boundaries are unchanged. | PASS |
| AC-18 | `evidence/qa-gates/final-csharpier.2026-07-21T21-07.md`; `evidence/qa-gates/final-analyzers.2026-07-21T21-07.md`; `evidence/qa-gates/final-nullable.2026-07-21T21-07.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md`; `evidence/qa-gates/final-pass-integrity.2026-07-21T21-11.md`; `evidence/qa-gates/coverage-delta.2026-07-21T21-18.md`; `evidence/qa-gates/acceptance-verification.2026-07-21T21-19.md` | One uninterrupted sequence passed. Repository coverage is 84.1647%; modified hunks improve to 100%; changed/new measurable production is 99.8250%; every measurable selector type/member exceeds 90%; both bounded nonnumeric adapters are enumerated without threshold/filter/exclusion weakening. | PASS |
| AC-19 | `evidence/regression-testing/preserved-selector-contracts.2026-07-21T20-15.md`; `evidence/regression-testing/semantic-composition.2026-07-21T20-14.md`; `evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md`; `evidence/qa-gates/final-mstest-coverage.2026-07-21T21-09.md` | All eight first-party test assemblies and 5,849 tests pass, including breadcrumb, controller, UtilitiesCS, issue #398, host-neutral, bridge, asset-contract, and integration-seam coverage. Pixel-identical rendering remains outside the required semantic contract. | PASS |

P6-T1 result: 19 PASS, 0 FAIL. No criterion has missing or contradictory final evidence.
