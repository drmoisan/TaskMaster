# Issue #400 Phase 8 Focused Regression Diagnosis

- Timestamp: `2026-07-22T23:45:24.6356437-04:00`
- Input: `issue-400-focused-regression-fail.2026-07-22T23-41.md`
- Combined-gate result: 358 discovered, 353 passed, 5 failed, 0 skipped.
- Diagnostic rerun: the two close/rollback tests were discovered and both reproduced only their recorded assertion failure; no unrelated failure occurred.

## Three controller setup failures

The three `QfcItemControllerBreadcrumbDropDownTests` failures are stale test setup, not a production dependency-injection defect.

- Production `QfcItemController.ViewerSetup.cs` calls `EnsureBreadcrumbPipeline()` before popup configuration.
- `ItemViewer.InitializeBreadcrumbPipeline` captures one `BreadcrumbUiDispatcher`, creates `_breadcrumbPopupUiOperations`, and injects that same dispatcher into the coordinator.
- The first three controller tests call the controller configuration seam without first initializing that pipeline. Later tests in the same class follow the production order.
- The test file remains byte-identical to the P4 version at SHA-256 `9C236CAFDDBD6E2465C7FD6B022817FC5B077B9FD33514515C555826A3A8C3DB`; P5 introduced the shared operations dependency without updating these three setups.
- The production null guard must remain. Adding a local `CaptureCurrentOrTests()` fallback could bind the host and coordinator to different dispatchers and weaken the production UI-thread invariant.

Correction boundary: modify only `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs`; initialize a strict mocked `IFolderHierarchyProvider` through one small helper before the three controller configuration calls, preserving all environment, theme, laziness, reuse, and zero-initializer-call assertions.

## Pending-open close expectation

`BreadcrumbDropDownIntegrationTests.InitializationFailure_CancelsSessionWithoutDuplicateClose` contains a stale pre-P6 expectation.

- P6-T10 requires selector-open-state routes to request a pending close without an `IsOpen` prerequisite.
- The P6 failure-first evidence required the automatic-close host call count to change from zero to one.
- `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` requires one `ExplicitCommit` request. That reason avoids a second cancellation after the selection session has already closed.

Correction boundary: retain the existing test name, callback, and closed-session assertion in `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`; change the total host-close expectation from never to exactly once. The dedicated P6 test remains the reason-specific witness. Current file: 500 lines, SHA-256 `A018588873F04CDA716CB2D37BFEED573EFD3EFE676AEF4C89A4167E20B15B8A`.

## Placement failure message

`BreadcrumbDropDownCoverageThresholdTests.OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` detected a production contract regression.

- The P5-validated message is `The active working area has no space for the folder selector popup.`
- The P6 lifetime extraction changed it to `The popup working area has no available space.` without plan authorization.
- The existing threshold test correctly retains the prior exact contract and must not be changed.

Correction boundary: restore the P5 message in `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`. Current file: 477 lines, SHA-256 `4566CA3383471E2DDC946309125930A8A08C140B924406507E68D10AD80F03E0`. Preserve read-only witnesses `BreadcrumbDropDownCoverageThresholdTests.cs` at SHA-256 `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` and `BreadcrumbDropDownOpenCoordinatorTests.cs` at SHA-256 `989BE280294875DCEFD2E936F6F48D65F3EAFED21B4AE4530D4E6288561AFC59`.

## Required correction tuple

- Production: `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs`.
- Tests: `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` and `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`.
- No coordinator, dispatcher, host, surface-factory, project, package, configuration, filter, threshold, or exclusion file is editable.
- After scoped formatting/analyzer/nullable gates, the five named failures must pass before rerunning the exact 358-test Phase 8 composition.
