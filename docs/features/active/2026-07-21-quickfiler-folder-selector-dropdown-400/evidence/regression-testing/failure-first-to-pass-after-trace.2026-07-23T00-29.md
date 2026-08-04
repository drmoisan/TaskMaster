# Failure-First to Pass-After Trace

Timestamp: `2026-07-23T00:29:06-04:00`

## Result

PASS. Every intended named failure required by P8-T18 has a current passing test or implementation mapping. The trace covers 66 intended failing cases across P1, P5, P7, and P8, including the four data rows of `Parse_InvalidSubfolderActivationPayload_RejectsExplicitly`, plus the P5 numeric-coverage and anti-masking reconciliations. The final unchanged 35-class composition passed 358/358 with zero failures or skips.

All paths below are relative to `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/`.

## P1 Failure Mappings

### P1-T5 — duplicate logical identity and probability

Fail-before: `evidence/regression-testing/duplicate-identity-fail-before.2026-07-21T22-31.md` — 23 discovered, 12 passed, 11 intended failures.

The following failures map to `evidence/regression-testing/duplicate-identity-complete-pass.2026-07-22T02-37.md` — 30/30 passed — and the final 358/358 composition:

- `SetSuggestionFallbacks_DuplicateSuggestionAndRecentPathsHaveDistinctIdentities`
- `SetSuggestionsAsync_ResolvedUpgradePreservesDistinctFallbackIdentities`
- `ClosedMoveNext_DuplicateOutputPathsCommitsSecondLogicalRow`
- `OpenMoveNextThenCommit_DuplicateOutputPathsCommitsSecondLogicalRow`
- `Activate_SecondDuplicateIdentityCommitsExactLogicalRow`
- `OpenCommit_CollapsedReadbackUsesSecondDuplicateSuggestionProbability`
- `ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence`
- `OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence`
- `ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence`
- `CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability`
- `ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption`

The passing evidence proves unique source-qualified logical identities, exact duplicate-row activation/commit, selected-row probability preservation, and one active/selected option.

### P1-T10 — UI dispatch and correlated collapsed readiness

Fail-before: `evidence/regression-testing/ui-readiness-fail-before.2026-07-21T22-44.md` — 13 discovered, four controls passed, nine intended failures.

The UI-dispatch failures below map to `evidence/qa-gates/ui-dispatch-pass-after.2026-07-22T00-06.md` — 21/21 passed — and the later P5 anti-masking closure:

- `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`
- `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext`
- `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink`

The readiness failures map to `evidence/qa-gates/collapsed-ready-core-pass-after.2026-07-22T00-26.md` — 16/16 passed — and `evidence/qa-gates/collapsed-ready-integration-pass-after.2026-07-22T01-26.md` — 30/30 passed. The implementation terminology was refined while preserving one-to-one behavior:

- `AttachAsync_PendingAndUnrelatedNavigation_DefersCachedReplayUntilExactSuccess` maps to current `AttachAsync_PendingAndUnrelatedNavigation_DefersReadyPublicationUntilExactSuccess`.
- `AttachAsync_ExactNavigationFailure_LeavesNoAttachmentOrReplay` maps to current `AttachAsync_ExactNavigationFailure_LeavesNoReadyMessenger`.
- `Reset_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess` maps to current `Reset_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`.
- `Dispose_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess` maps to current `Dispose_PendingNavigation_CancelsDetachesAndRejectsLateSuccess`.
- `LaterNavigation_InvalidatesEarlierGenerationAndAttachesOnlyCurrentSurface` maps to current `LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger`.
- `CollapsedAttachmentContract_IsAwaitableAndControllerOwnedForControllerSetup` maps to current `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`.

These mappings prove exact navigation correlation, late-success rejection, generation ownership, controller-owned attachment, and exactly-once cached replay.

### P1-T15 — selection replacement, coordinator lifetime, and pending open

Fail-before: `evidence/regression-testing/selection-lifecycle-fail-before.2026-07-21T23-04.md` — 14 discovered, one control passed, 13 intended failures.

Selection-replacement failures map to `evidence/qa-gates/router-selection-pass-after.2026-07-21T23-40.md` — 27/27 passed:

- `UpgradeStarted_ClosedMoveToDuplicateRow_RemainsSelectedAfterReplacement`
- `UpgradeStarted_OpenPendingMoveToDuplicateRow_CommitsExactMovedRow`
- `UpgradeStarted_ActivationOfDuplicateRow_CommitsExactActivatedRow`

Coordinator-lifetime failures map to `evidence/regression-testing/coordinator-lifetime-pass-after.2026-07-22T21-12.md` — 32/32 passed:

- `OverlappingUpgrades_CurrentCompletionPostsOnceAndStaleCompletionPostsNothing`
- `Clear_InvalidatesLateSuccessfulUpgradeBeforeAnyPostOrCallback`
- `ViewerResetThenReuse_InvalidatesLateFailureWithoutDuplicatingCurrentState`
- `Dispose_InvalidatesLateSuccessAndUnsubscribesBeforePostOrCallback`
- `Dispose_InvalidatesLateFailureWithoutPostCallbackOrErrorMutation`

Pending-open failures map to `evidence/regression-testing/pending-open-close-pass-after.2026-07-22T22-05.md` — 46/46 passed:

- `CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent`
- `CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus`
- `CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation`
- `ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce`
- `AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce`

These mappings prove stable selection across row replacement, stale upgrade suppression, cancelable pending opens, deterministic false settlement, no late show/focus, exactly-once cleanup, and retry.

### P1-T20 — explicit durable subfolder commit

Fail-before: `evidence/regression-testing/subfolder-commit-fail-before.2026-07-21T23-14.md` — 30 discovered, 18 controls passed, 12 intended failures.

The following session and message failures map to `evidence/regression-testing/subfolder-core-pass-after.2026-07-23T02-21.md` — 53/53 passed — and `evidence/regression-testing/subfolder-composition-pass-after.2026-07-23T02-29.md` — 70/70 passed:

- `OpenSelector_SubfolderActivationThenEnter_PreservesCommittedFullPath`
- `OpenSelector_SubfolderActivationThenEscape_PreservesCommittedFullPath`
- `OpenSelector_SubfolderActivationThenAutomaticClose_PreservesCommittedFullPath`
- `OpenSelector_SubfolderActivationThenEnter_PublishesAndClosesExactlyOnce`
- `OpenSelector_SubfolderActivationThenEscape_PublishesAndClosesExactlyOnce`
- `OpenSelector_SubfolderActivationThenNativeClose_PublishesAndClosesExactlyOnce`
- `SubfolderActivationMessage_RoundTripsUniqueRowIdentityAndSubfolderIndex`
- `SubfolderActivationConstructor_RejectsBlankIdentityAndNegativeIndex`
- four data rows of `Parse_InvalidSubfolderActivationPayload_RejectsExplicitly`

The passing evidence proves one explicit message, one durable commit, one selection event, one explicit close, one focus return, full-path preservation, strict invalid-payload normalization, and invalid-input no-op behavior.

## P5 Failure and Reconciliation Mappings

### P5-T8 — runtime popup UI boundary and mouse retry

Fail-before: `evidence/regression-testing/runtime-ui-boundary-fail-before.2026-07-22T01-43.md` — 10 discovered, eight passed, two intended failures.

- `OpenAsync_AmbientNullWorkerCompletions_KeepEveryPopupOperationOnOwnerBoundary`
- `MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly`

Both map to `evidence/regression-testing/popup-ui-boundary-composition-pass-after.2026-07-22T04-26.md` — 56/56 passed. `evidence/regression-testing/popup-ui-boundary-core-pass-after.2026-07-22T03-35.md` — 31/31 passed — supplies the implementation-level creator-thread dispatch, control-access, error-observation, closed-state, and retry proof.

### P5-T27 — cleanup ownership and Dispose race

Fail-before: `evidence/regression-testing/p5-review-corrections-fail-before.2026-07-22T05-29.md` — 25 discovered, 22 passed, three intended failures.

- `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource`
- `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly`

These map to `evidence/regression-testing/p5-cleanup-ownership-pass-after.2026-07-22T06-11.md` — 13/13 passed — and `evidence/qa-gates/p5-cleanup-ownership-audit.2026-07-22T06-12.md`.

- `Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity`

This maps to `evidence/regression-testing/p5-dispose-race-pass-after.2026-07-22T06-29.md` — 12/12 passed — and `evidence/qa-gates/p5-dispose-race-audit.2026-07-22T06-30.md`.

The implementation attempts host, control, and messenger cleanup independently, preserves the primary error, retries only still-owned resources, prevents wrapper/direct double disposal, invalidates queued work before Dispose, and leaves no late UI/error/focus/close activity.

### P5-T48 — rollback primary-error preservation

Fail-before: `evidence/regression-testing/p5-primary-rollback-fail-before.2026-07-22T06-37.md` — one intended failure and one passing focus/native-close control.

- `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`

This maps to `evidence/regression-testing/p5-primary-rollback-pass-after.2026-07-22T07-31.md` — 7/7 passed — and `evidence/qa-gates/p5-primary-rollback-audit.2026-07-22T07-32.md`. The passing behavior retains the initiating exception, cancels and focuses once, observes rollback secondaries once, applies native close only after show, settles closed, and remains retryable.

### P5-T90 — instrumented creator-thread observation

Fail-before reconciliation: `evidence/regression-testing/p5-selector-toggle-coverage-fail-before.2026-07-22T08-56.md` — instrumented 69/70 with only:

- `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`

The case maps to `evidence/regression-testing/p5-selector-toggle-worker-boundary-nine-class-pass-after.2026-07-22T09-02.md` — 70/70 passed — and `evidence/qa-gates/p5-selector-toggle-worker-boundary-change-ledger.2026-07-22T09-00.md`. The synchronized derived post-count observation preserves every creator-thread, before-drain, callback-context, exception, selector-open, and provider assertion without changing production.

### P5-T100 and P5-T102 — authoritative numeric decision

The nonpassing P5-T100 decision `evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T09-06.md` and the P5-T102 expected-fail reconciliation `evidence/regression-testing/p5-numeric-coverage-remediation-baseline.2026-07-22T09-31.md` map to the superseding P5-T211 decision `evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T19-42.md`.

P5-T211 records `DECISION: PASS`, every applicable measurable new or changed unit at or above 90% line coverage, all nine later shortfall units closed, and `ITEMVIEWER OMISSION: CLEARED`. Its authoritative test input is the P5-T209 170/170 composition, not the superseded P5-T201 coverage values.

### P5-T110 — coordinator extraction

Fail-before: `evidence/regression-testing/p5-open-coordinator-omission-fail-before.2026-07-22T09-37.md` — five discovered, four passed, only:

- `HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator`

The failure maps to `evidence/regression-testing/p5-open-coordinator-pass-after.2026-07-22T10-24.md` — 37/37 passed, including 5/5 contract and 10/10 coordinator cases — and `evidence/qa-gates/p5-open-coordinator-extraction-ledger.2026-07-22T09-45.md`. Host-neutral open/close orchestration is owned by the measured coordinator while ItemViewer retains minimal delegation and direct adapter boundaries.

### P5-T121 — preservation correction and reconciliation

Fail-before: `evidence/regression-testing/p5-open-coordinator-preservation-fail-before.2026-07-22T10-16.md` — 37 discovered, 35 passed, two intended failures:

- `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`
- `NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce`

Both map to the P5-T130 `evidence/regression-testing/p5-open-coordinator-pass-after.2026-07-22T10-24.md` — 37/37 passed — and the P5-T131 `evidence/qa-gates/p5-open-coordinator-preservation-reconciliation.2026-07-22T10-25.md`. The public parameter remains `surfaceFactory`; committed/pending source-qualified identities, null pending state after close, path readback, zero publication, and one focus return are preserved.

### P5-T172 through P5-T184 — UI-dispatch anti-masking closure

P5-T172 `evidence/qa-gates/p5-uidispatch-rootcause-diagnosis.2026-07-22T15-07.md` records `DETERMINATION: B`: the production dispatcher accepted recycled owner-thread identity after `ConfigureAwait(false)` and could complete without posting.

The production correction maps to:

- P5-T181 `evidence/regression-testing/p5-uidispatch-correction-uninstrumented-pass-after.2026-07-22T15-07.md` — 9/9 passed.
- P5-T182 `evidence/regression-testing/p5-uidispatch-correction-instrumented-pass-after.2026-07-22T15-07.md` — two consecutive instrumented 9/9 passes.
- P5-T183 `evidence/qa-gates/p5-numeric-coverage-composition.2026-07-22T16-22.md` — instrumented 160/160 passed.
- P5-T184 `evidence/qa-gates/p5-uidispatch-anti-masking-closure.2026-07-22T16-22.md` — PASS with the test file byte-identical, all nine test names and 33 assertions intact, no timing/retry/skip mechanism, unchanged filter/configuration, and a production-only fix.

### P5-T185 through P5-T211 — nine-unit numeric shortfall

The exact nine-unit baseline is `evidence/qa-gates/p5-numeric-shortfall-target-inventory.2026-07-22T17-01.md`.

P5-T192 `evidence/regression-testing/p5-coordinator-branch-coverage-pass-after.2026-07-22T17-07.md` — 15/15 passed — maps:

- `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` from 6/9 to 9/9.
- `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` from 4/5 to 5/5.
- `BreadcrumbDropDownOpenCoordinator.Reset()` from 4/5 to 5/5.
- `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` from 5/6 to 6/6.
- `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` from 7/8 to 8/8.

P5-T199 `evidence/regression-testing/p5-lifetime-host-branch-coverage-pass-after.2026-07-22T17-24.md` — 23/23 passed — maps:

- `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` from 28/43 to 42/43, 97.67%.
- `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` from 5/6 to 6/6.
- `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` from 8/9 to 9/9.

`BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` maps from 24/28 to 24/24 through removal of the proved-unreachable former lines 153-156.

P5-T201 `evidence/qa-gates/p5-branch-coverage-composition.2026-07-22T18-58.md` passed 170/170 but is superseded for numeric authority because it still reported `<CompleteOpenAsync>d__16` at 24/28. P5-T209 `evidence/qa-gates/p5-deadcode-removal-composition.2026-07-22T19-32.md` is the authoritative 170/170 composition and reports 24/24. P5-T210 `evidence/qa-gates/p5-branch-coverage-nine-unit-closure.2026-07-22T19-39.md` proves all nine units at or above 90% with no regression in the seven protected passing units. P5-T211 records the final passing numeric decision.

## P7 Failure Mappings

### P7-T15 — preserved identity and explicit test UI context

Fail-before: `evidence/regression-testing/preserved-contract-correction-fail-before.2026-07-22T22-52.md` — three discovered, three intended failures:

- `SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives`
- `MalformedInboundMessage_PostsRouterErrorResponse`
- `SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`

These map to P7-T22 `evidence/regression-testing/preserved-contract-correction-pass-after.2026-07-22T23-01.md` — 38/38 passed — and P7-T30 `evidence/regression-testing/preserved-breadcrumb-contracts-pass-after.2026-07-23T03-23.md` — 23/23 passed. The production selection session now preserves identity through plain-to-suggestion migration with bounded selectable-index fallback. The two host-neutral coordinator tests install and restore an explicit owning context without weakening the production UI-affinity constructor or dispatcher.

### P7-T24 — durable selected-child publication and rendering

Fail-before: `evidence/regression-testing/durable-selected-child-render-fail-before.2026-07-23T03-14.md` — 43 discovered, 40 passed, three intended failures:

- `RoundTrip_Render_PreservesSelectedChildStateAndLegacyDefaults`
- `OpenSelector_SubfolderActivation_PublishesOneDurableRenderAndNoLegacySelectionChange`
- `RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection`

These map to P7-T29 `evidence/regression-testing/durable-selected-child-render-pass-after.2026-07-23T03-23.md` — 43/43 passed — and P7-T31 `evidence/qa-gates/subfolder-scope-and-delivery-audit.2026-07-23T03-26.md`. The implementation publishes selected child index and canonical path from one locked snapshot, renders the collapsed child full path with the parent probability, gives exactly one child or pending row active/ARIA ownership and `aria-activedescendant`, preserves Left/Right and parent identity semantics, and emits no legacy `selectionChange` from explicit activation.

## P8 Five-Failure Correction Mapping

The unchanged exact 35-class run in `evidence/regression-testing/issue-400-focused-regression-fail.2026-07-22T23-41.md` discovered 358 tests, passed 353, and produced exactly five intended failures. `evidence/regression-testing/issue-400-focused-regression-diagnosis.2026-07-22T23-45.md` and P8-T2 `evidence/regression-testing/phase8-focused-regression-correction-fail-before.2026-07-22T23-56.md` provide the bounded diagnosis:

- `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
- `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`
- `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`

These three stale setups map to strict mocked `IFolderHierarchyProvider` initialization through the existing production-order pipeline seam.

- `InitializationFailure_CancelsSessionWithoutDuplicateClose`

This stale zero-close expectation maps to exactly one total close while retaining the P6 reason-specific `ExplicitCommit` witness.

- `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`

This production contract regression maps to restoration of the exact message `The active working area has no space for the folder selector popup.`

All five pass in P8-T11 `evidence/regression-testing/phase8-focused-regression-correction-five-test-pass-after.2026-07-23T00-02.md` — 5/5 passed. The complete correction proof is:

- P8-T12 `evidence/regression-testing/runtime-refined-remediation-complete-pass-after.2026-07-23T00-03.md` — unchanged 16-class filter, 149/149 passed.
- P8-T13 `evidence/qa-gates/phase8-focused-regression-correction-scope-audit.2026-07-23T00-05.md` — exact three-file delta, protected hashes, strict helper limited to the three tests, one-close contract, exact message, no fallback/signature/configuration change.
- P8-T14 `evidence/qa-gates/phase8-focused-regression-correction-independent-review.2026-07-23T00-13.md` — PASS with zero Blocker, Major, Medium, or Low findings.
- P8-T15 `evidence/regression-testing/issue-400-focused-regression.2026-07-23T00-15.md` — unchanged 35-class filter, 358/358 passed, zero failed/skipped.

## Verification Command

```powershell
$ErrorActionPreference = 'Stop'
$feature = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400'
$tracePath = Join-Path $feature 'evidence/regression-testing/failure-first-to-pass-after-trace.2026-07-23T00-29.md'
$evidenceChecks = @(
    @{ Path = 'evidence/regression-testing/duplicate-identity-fail-before.2026-07-21T22-31.md'; Tokens = @('Failed: 11', 'Skipped: 0') },
    @{ Path = 'evidence/regression-testing/duplicate-identity-complete-pass.2026-07-22T02-37.md'; Tokens = @('30 of 30 tests', 'zero failures and zero skips') },
    @{ Path = 'evidence/regression-testing/ui-readiness-fail-before.2026-07-21T22-44.md'; Tokens = @('Failed: 9', 'Skipped: 0') },
    @{ Path = 'evidence/qa-gates/ui-dispatch-pass-after.2026-07-22T00-06.md'; Tokens = @('passed all 21', 'Failed: 0') },
    @{ Path = 'evidence/qa-gates/collapsed-ready-core-pass-after.2026-07-22T00-26.md'; Tokens = @('passed all 16', 'Failed: 0') },
    @{ Path = 'evidence/qa-gates/collapsed-ready-integration-pass-after.2026-07-22T01-26.md'; Tokens = @('passed all 30', 'Failed: 0') },
    @{ Path = 'evidence/regression-testing/selection-lifecycle-fail-before.2026-07-21T23-04.md'; Tokens = @('intended failures: 13') },
    @{ Path = 'evidence/qa-gates/router-selection-pass-after.2026-07-21T23-40.md'; Tokens = @('passed all 27', 'Failed: 0') },
    @{ Path = 'evidence/regression-testing/coordinator-lifetime-pass-after.2026-07-22T21-12.md'; Tokens = @('all 32 passed', 'no failures or skips') },
    @{ Path = 'evidence/regression-testing/pending-open-close-pass-after.2026-07-22T22-05.md'; Tokens = @('all 46 selected tests passed', 'no failures or skips') },
    @{ Path = 'evidence/regression-testing/subfolder-commit-fail-before.2026-07-21T23-14.md'; Tokens = @('intended failures: 12') },
    @{ Path = 'evidence/regression-testing/subfolder-core-pass-after.2026-07-23T02-21.md'; Tokens = @('passed 53', '0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/subfolder-composition-pass-after.2026-07-23T02-29.md'; Tokens = @('all 70 cases passed', '0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/runtime-ui-boundary-fail-before.2026-07-22T01-43.md'; Tokens = @('exactly 2 named new regressions failed') },
    @{ Path = 'evidence/regression-testing/popup-ui-boundary-composition-pass-after.2026-07-22T04-26.md'; Tokens = @('56 of 56 tests', '0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/p5-review-corrections-fail-before.2026-07-22T05-29.md'; Tokens = @('exactly 3 named regressions failed') },
    @{ Path = 'evidence/regression-testing/p5-cleanup-ownership-pass-after.2026-07-22T06-11.md'; Tokens = @('all 13', '0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/p5-dispose-race-pass-after.2026-07-22T06-29.md'; Tokens = @('all 12 tests', '0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/p5-primary-rollback-fail-before.2026-07-22T06-37.md'; Tokens = @('1 passed, 1 failed, and 0 skipped') },
    @{ Path = 'evidence/regression-testing/p5-primary-rollback-pass-after.2026-07-22T07-31.md'; Tokens = @('7 discovered, 7 passed, 0 failed, 0 skipped') },
    @{ Path = 'evidence/regression-testing/p5-selector-toggle-coverage-fail-before.2026-07-22T08-56.md'; Tokens = @('69 passed, one failed, and zero skipped') },
    @{ Path = 'evidence/regression-testing/p5-selector-toggle-worker-boundary-nine-class-pass-after.2026-07-22T09-02.md'; Tokens = @('70 passed, zero failed, and zero skipped') },
    @{ Path = 'evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T09-06.md'; Tokens = @('REMEDIATION REQUIRED') },
    @{ Path = 'evidence/regression-testing/p5-numeric-coverage-remediation-baseline.2026-07-22T09-31.md'; Tokens = @('EXPECTED FAIL-BEFORE') },
    @{ Path = 'evidence/regression-testing/p5-open-coordinator-omission-fail-before.2026-07-22T09-37.md'; Tokens = @('5 total; 4 passed; 1 failed; 0 skipped') },
    @{ Path = 'evidence/regression-testing/p5-open-coordinator-preservation-fail-before.2026-07-22T10-16.md'; Tokens = @('35 passed, 2 failed, 0 skipped') },
    @{ Path = 'evidence/regression-testing/p5-open-coordinator-pass-after.2026-07-22T10-24.md'; Tokens = @('37 passed, 0 failed, and 0 skipped') },
    @{ Path = 'evidence/qa-gates/p5-open-coordinator-preservation-reconciliation.2026-07-22T10-25.md'; Tokens = @('P5-T130 is passing replacement evidence with 37 passed') },
    @{ Path = 'evidence/qa-gates/p5-uidispatch-rootcause-diagnosis.2026-07-22T15-07.md'; Tokens = @('DETERMINATION: B') },
    @{ Path = 'evidence/regression-testing/p5-uidispatch-correction-uninstrumented-pass-after.2026-07-22T15-07.md'; Tokens = @('9/9 with zero failed and zero skipped') },
    @{ Path = 'evidence/regression-testing/p5-uidispatch-correction-instrumented-pass-after.2026-07-22T15-07.md'; Tokens = @('| 1 | 0 | 9 | 9 | 0 | 0 | Passed', '| 2 | 0 | 9 | 9 | 0 | 0 | Passed') },
    @{ Path = 'evidence/qa-gates/p5-numeric-coverage-composition.2026-07-22T16-22.md'; Tokens = @('160/160 passed, 0 failed, 0 skipped') },
    @{ Path = 'evidence/qa-gates/p5-uidispatch-anti-masking-closure.2026-07-22T16-22.md'; Tokens = @('Anti-masking closure verified with zero contradictions') },
    @{ Path = 'evidence/qa-gates/p5-numeric-shortfall-target-inventory.2026-07-22T17-01.md'; Tokens = @('count is **nine**') },
    @{ Path = 'evidence/regression-testing/p5-coordinator-branch-coverage-pass-after.2026-07-22T17-07.md'; Tokens = @('15 passed, zero failed, and zero skipped') },
    @{ Path = 'evidence/regression-testing/p5-lifetime-host-branch-coverage-pass-after.2026-07-22T17-24.md'; Tokens = @('23 passed, zero failed, zero skipped') },
    @{ Path = 'evidence/qa-gates/p5-branch-coverage-composition.2026-07-22T18-58.md'; Tokens = @('170 passed, 0 failed, 0 skipped') },
    @{ Path = 'evidence/qa-gates/p5-deadcode-removal-composition.2026-07-22T19-32.md'; Tokens = @('170 passed, 0 failed, 0 skipped', '24/24 = 100%') },
    @{ Path = 'evidence/qa-gates/p5-branch-coverage-nine-unit-closure.2026-07-22T19-39.md'; Tokens = @('All nine units are at or above 90%') },
    @{ Path = 'evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T19-42.md'; Tokens = @('DECISION: PASS', 'ITEMVIEWER OMISSION: CLEARED') },
    @{ Path = 'evidence/regression-testing/preserved-contract-correction-fail-before.2026-07-22T22-52.md'; Tokens = @('All 3 failed for the intended preserved-contract reasons') },
    @{ Path = 'evidence/regression-testing/preserved-contract-correction-pass-after.2026-07-22T23-01.md'; Tokens = @('All 38 passed with 0 failures and 0 skips') },
    @{ Path = 'evidence/regression-testing/preserved-breadcrumb-contracts-pass-after.2026-07-23T03-23.md'; Tokens = @('All 23 passed with zero failures or skips') },
    @{ Path = 'evidence/regression-testing/durable-selected-child-render-fail-before.2026-07-23T03-14.md'; Tokens = @('40 passed and the three named regressions failed') },
    @{ Path = 'evidence/regression-testing/durable-selected-child-render-pass-after.2026-07-23T03-23.md'; Tokens = @('All 43 passed with zero failures or skips') },
    @{ Path = 'evidence/qa-gates/subfolder-scope-and-delivery-audit.2026-07-23T03-26.md'; Tokens = @('The audit reported zero failures') },
    @{ Path = 'evidence/regression-testing/issue-400-focused-regression-fail.2026-07-22T23-41.md'; Tokens = @('353 passed, 5 failed, and zero skipped') },
    @{ Path = 'evidence/regression-testing/issue-400-focused-regression-diagnosis.2026-07-22T23-45.md'; Tokens = @('358 discovered, 353 passed, 5 failed, 0 skipped') },
    @{ Path = 'evidence/regression-testing/phase8-focused-regression-correction-fail-before.2026-07-22T23-56.md'; Tokens = @('intended_failures=5') },
    @{ Path = 'evidence/regression-testing/phase8-focused-regression-correction-five-test-pass-after.2026-07-23T00-02.md'; Tokens = @('all 5 passed, with 0 failed and 0 skipped') },
    @{ Path = 'evidence/regression-testing/runtime-refined-remediation-complete-pass-after.2026-07-23T00-03.md'; Tokens = @('all 149 passed, with 0 failed and 0 skipped') },
    @{ Path = 'evidence/qa-gates/phase8-focused-regression-correction-scope-audit.2026-07-23T00-05.md'; Tokens = @('P8_T13_SCOPE_AUDIT_OK') },
    @{ Path = 'evidence/qa-gates/phase8-focused-regression-correction-independent-review.2026-07-23T00-13.md'; Tokens = @('Result: `PASS`', 'Blocker 0, Major 0, Medium 0, Low 0') },
    @{ Path = 'evidence/regression-testing/issue-400-focused-regression.2026-07-23T00-15.md'; Tokens = @('All 358 passed with 0 failed and 0 skipped') }
)
foreach ($check in $evidenceChecks) {
    $path = Join-Path $feature $check.Path
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing evidence artifact: $path"
    }
    $content = Get-Content -Raw -LiteralPath $path
    foreach ($token in $check.Tokens) {
        if (-not $content.Contains($token)) {
            throw "Missing required evidence token '$token' in $path"
        }
    }
}
$trace = Get-Content -Raw -LiteralPath $tracePath
$ledgerEnd = $trace.IndexOf('## Verification Command', [StringComparison]::Ordinal)
if ($ledgerEnd -lt 0) {
    throw 'Trace has no verification-command boundary.'
}
$ledger = $trace.Substring(0, $ledgerEnd)
$requiredTaskMappings = @(
    'P1-T5', 'P1-T10', 'P1-T15', 'P1-T20',
    'P5-T8', 'P5-T27', 'P5-T48', 'P5-T90', 'P5-T100', 'P5-T102',
    'P5-T110', 'P5-T121', 'P5-T172', 'P5-T184', 'P5-T185',
    'P5-T192', 'P5-T199', 'P5-T201', 'P5-T209', 'P5-T210', 'P5-T211',
    'P7-T15', 'P7-T22', 'P7-T24', 'P7-T29', 'P7-T30', 'P7-T31',
    'P8-T2', 'P8-T11', 'P8-T12', 'P8-T13', 'P8-T14', 'P8-T15'
)
$requiredMechanisms = @(
    'cleanup ownership', 'Dispose race', 'rollback primary-error preservation',
    'creator-thread', 'coordinator extraction', 'preserved identity',
    'explicit test UI context', 'durable selected-child publication',
    'strict mocked `IFolderHierarchyProvider` initialization',
    'exactly one total close', 'exact message'
)
$requiredNamedFailures = @(
    'SetSuggestionFallbacks_DuplicateSuggestionAndRecentPathsHaveDistinctIdentities',
    'SetSuggestionsAsync_ResolvedUpgradePreservesDistinctFallbackIdentities',
    'ClosedMoveNext_DuplicateOutputPathsCommitsSecondLogicalRow',
    'OpenMoveNextThenCommit_DuplicateOutputPathsCommitsSecondLogicalRow',
    'Activate_SecondDuplicateIdentityCommitsExactLogicalRow',
    'OpenCommit_CollapsedReadbackUsesSecondDuplicateSuggestionProbability',
    'ClosedDown_DuplicateSuggestionAndRecentCommitsRecentOccurrence',
    'OpenDownThenEnter_DuplicateSuggestionAndRecentCommitsPendingOccurrence',
    'ActivateSelector_SecondPublishedIdentityCommitsExactDuplicateOccurrence',
    'CollapsedReadback_SecondDuplicateSuggestionRetainsItsProbability',
    'ExpandedDuplicatePathState_YieldsExactlyOneActiveAriaSelectedOption',
    'SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext',
    'InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext',
    'DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink',
    'AttachAsync_PendingAndUnrelatedNavigation_DefersCachedReplayUntilExactSuccess',
    'AttachAsync_ExactNavigationFailure_LeavesNoAttachmentOrReplay',
    'Reset_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess',
    'Dispose_PendingNavigation_CancelsAttachmentAndRejectsLateSuccess',
    'LaterNavigation_InvalidatesEarlierGenerationAndAttachesOnlyCurrentSurface',
    'CollapsedAttachmentContract_IsAwaitableAndControllerOwnedForControllerSetup',
    'UpgradeStarted_ClosedMoveToDuplicateRow_RemainsSelectedAfterReplacement',
    'UpgradeStarted_OpenPendingMoveToDuplicateRow_CommitsExactMovedRow',
    'UpgradeStarted_ActivationOfDuplicateRow_CommitsExactActivatedRow',
    'OverlappingUpgrades_CurrentCompletionPostsOnceAndStaleCompletionPostsNothing',
    'Clear_InvalidatesLateSuccessfulUpgradeBeforeAnyPostOrCallback',
    'ViewerResetThenReuse_InvalidatesLateFailureWithoutDuplicatingCurrentState',
    'Dispose_InvalidatesLateSuccessAndUnsubscribesBeforePostOrCallback',
    'Dispose_InvalidatesLateFailureWithoutPostCallbackOrErrorMutation',
    'CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent',
    'CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus',
    'CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation',
    'ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce',
    'AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce',
    'OpenSelector_SubfolderActivationThenEnter_PreservesCommittedFullPath',
    'OpenSelector_SubfolderActivationThenEscape_PreservesCommittedFullPath',
    'OpenSelector_SubfolderActivationThenAutomaticClose_PreservesCommittedFullPath',
    'OpenSelector_SubfolderActivationThenEnter_PublishesAndClosesExactlyOnce',
    'OpenSelector_SubfolderActivationThenEscape_PublishesAndClosesExactlyOnce',
    'OpenSelector_SubfolderActivationThenNativeClose_PublishesAndClosesExactlyOnce',
    'SubfolderActivationMessage_RoundTripsUniqueRowIdentityAndSubfolderIndex',
    'SubfolderActivationConstructor_RejectsBlankIdentityAndNegativeIndex',
    'Parse_InvalidSubfolderActivationPayload_RejectsExplicitly',
    'OpenAsync_AmbientNullWorkerCompletions_KeepEveryPopupOperationOnOwnerBoundary',
    'MouseToggle_FirstOpenFaultsAfterAwait_SecondClickRetriesCleanly',
    'CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource',
    'CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly',
    'Dispose_WhenResetAndOpenWorkAreQueued_HasNoLateActivity',
    'OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery',
    'WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary',
    'HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator',
    'Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory',
    'NativeAutomaticClose_RestoresOriginalCommittedIdentityWithoutPendingPublicationAndReturnsFocusOnce',
    'SetSuggestionsAsync_WhileUpgradeInFlight_ReadbackStaysConsistentAndSelectionSurvives',
    'MalformedInboundMessage_PostsRouterErrorResponse',
    'SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection',
    'RoundTrip_Render_PreservesSelectedChildStateAndLegacyDefaults',
    'OpenSelector_SubfolderActivation_PublishesOneDurableRenderAndNoLegacySelectionChange',
    'RenderReceiver_OwnsSelectedChildExpandedAndCollapsedProjection',
    'ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily',
    'ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam',
    'ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost',
    'InitializationFailure_CancelsSessionWithoutDuplicateClose'
)
foreach ($token in @($requiredTaskMappings + $requiredMechanisms + $requiredNamedFailures)) {
    if (-not $ledger.Contains($token)) {
        throw "Missing required trace mapping: $token"
    }
}
if (-not $ledger.Contains('four data rows of `Parse_InvalidSubfolderActivationPayload_RejectsExplicitly`')) {
    throw 'The four parameterized P1-T20 failure cases are not explicitly accounted for.'
}
"P8_T18_TRACE_OK artifacts=$($evidenceChecks.Count) task_mappings=$($requiredTaskMappings.Count) unique_named_failures=$($requiredNamedFailures.Count) parameterized_rows=4 final_pass=358/358 missing=0 still_failing=0"
```

`EXIT_CODE: 0`

Output Summary: `P8_T18_TRACE_OK artifacts=54 task_mappings=33 unique_named_failures=62 parameterized_rows=4 final_pass=358/358 missing=0 still_failing=0`
