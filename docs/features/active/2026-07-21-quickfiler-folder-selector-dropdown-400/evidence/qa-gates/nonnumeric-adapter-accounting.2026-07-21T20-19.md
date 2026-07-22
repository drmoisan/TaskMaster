# Nonnumeric Adapter Accounting

Timestamp: 2026-07-21T20-19Z
Command: Direct source inspection of the complete excluded `BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` and `BreadcrumbDropDownHost.ShowOwnedPopup` method bodies; identifier and responsibility comparison against the included host lifecycle/state-machine code; mapping of each adapter branch to the exact deterministic factory/display-seam regression tests
EXIT_CODE: 0
Output Summary: Both exclusions are bounded direct adapters. The helper exclusion contains only WebView2 construction, core initialization, correlated navigation readiness, third-party event cleanup, messenger construction, and partial-control disposal. `ShowOwnedPopup` contains only the direct WinForms display call. Neither exclusion contains selection, replay, host lifecycle, generation, stale-completion, popup-state, or error-state ownership.

## Excluded Helper Boundary

`BreadcrumbWebViewSurfaceFactory.CreateSurfaceAsync` contains only:

- `WebView2` construction and `DockStyle.Fill` assignment.
- `IWebViewCoreInitializer.EnsureCoreWebView2Async` and validation of the third-party `CoreWebView2` result.
- A `RunContinuationsAsynchronously` readiness completion source.
- Pre-navigation `NavigationStarting`, `NavigationCompleted`, and `Disposed` event subscription.
- Capture of the first requested navigation ID and rejection of unrelated completion IDs.
- Success/failure completion of the readiness task for the correlated document navigation.
- Handler detachment on success, failure, disposal, and synchronous navigation exception.
- Direct `NavigateToString`, `WebView2Messenger` construction, and partial `WebView2` disposal.

The method contains no selector model, committed/original/pending identity, cached render/theme/selector replay, `ToolStripDropDown` lifecycle, host generation, shared-open ownership, `LastInitializationException`, focus callback, selection callback, or stale-completion state. All such host-neutral logic remains in the instrumented `BreadcrumbDropDownHost` and coordinator/hub types.

## Helper Branch-to-Seam Mapping

| Nonnumeric adapter branch | Deterministic seam evidence |
|---|---|
| Core/factory remains incomplete | `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`; `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup` |
| Only the designated correlated readiness completion permits exposure | Pending-readiness assertions keep attach/replay/show/focus at zero until the injected readiness TCS completes; unrelated/absent completion is modeled by leaving that TCS incomplete |
| Successful readiness completion | `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess` |
| Navigation/readiness failure | `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce` |
| Core/factory or synchronous navigation exception | `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce`; `FailedFactoryTask_ClosesWithoutLeavingAHostOrCallbackSubscription` |
| Partial surface cleanup | `OpenAsync_PartialInitializationFailureDisposesAndRestoresFocusAndSelection`; `Reset_DisposesAnOrphanedPartialSurface` |
| Disposal/cancellation before readiness completes | `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation`; `Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation` |
| Handler detachment and no duplicate ready-surface subscription/replay | `ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions`; `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`; `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` |

The deterministic seam tests do not claim to execute WebView2 itself. They verify every observable host outcome for pending, successful, failed, canceled, stale, reset, disposed, and reused readiness states without a live browser or message loop.

## Excluded WinForms Display Boundary

`BreadcrumbDropDownHost.ShowOwnedPopup` is a single expression calling `ToolStripDropDown.Show(anchor, anchor.PointToClient(screenLocation))`. It contains no branch and no host-neutral state.

- Display request, owner, calculated location, and size are verified through `OpenAsync_CreatesToolStripControlHostAndUsesCalculatedScreenPlacement` and the injected display callback.
- Direct display failure behavior is verified through `OpenAsync_ShowFailure_ClosesUncommittedAndRetainsTheFailure`.
- Placement branch behavior remains numeric in `BreadcrumbPopupPlacement` tests and is not excluded.

P4-T3 result: PASS. No plan revision is required because excluded code contains no host-neutral lifecycle or selector behavior.
