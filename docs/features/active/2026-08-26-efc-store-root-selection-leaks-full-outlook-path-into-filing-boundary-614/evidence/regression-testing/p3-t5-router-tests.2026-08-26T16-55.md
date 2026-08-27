# P3-T5 — Router test set after the D1/D2/D3 fixes (#614)

Timestamp: 2026-08-26T16-55

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests|FullyQualifiedName~BreadcrumbBridgeRouterTests" "/Logger:trx;LogFileName=p3-t5.trx" "/ResultsDirectory:coverage\trx\p3-t5"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 0

## Output Summary

- `Test Run Successful.` Total tests: 35; Passed: 35; Failed: 0; Skipped: 0.
- AC18 pass-after: `Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath` PASSED,
  against the P1-T4 fail-before artifact (EXIT_CODE 1, ExpectedExitCode 1).
- All eight new `BreadcrumbBridgeRouterIssue614Tests` PASSED:
  `SegmentActivate_StoreRootAncestor_LeavesSelectionUnchangedAndDiagnoses`,
  `SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses`,
  `SegmentActivate_ArchiveRootExactly_IsTreatedAsNonSelection`,
  `SegmentActivate_UnderRootAncestor_SetsTheRelativeStem`,
  `RenderedChildActivate_UnderRootChild_SetsTheRelativeStem`,
  `SegmentActivate_LeafSegment_RemainsNonActivatable`,
  `RowSelected_OutOfRootFilingTarget_DoesNotStoreAFullOutlookPath`,
  `SegmentActivate_WithNoBoundArchiveRoot_PreservesThePassThroughMode`.
- All seven `BreadcrumbBridgeRouterIssue439Tests` PASSED, including
  `Issue439ArchiveRootBoundarySelectionAndHostEventRemainDeterministic` in its P3-T4-corrected
  form (the single documented exception to unchanged behaviour in that class) and the four
  adjacent tests the plan required to stay green UNEDITED:
  `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`,
  `Issue439SlashOnlyArchiveRootPreservesFullHierarchySelection`,
  `Issue439ArchiveRelativeRowsRenderLineagePreserveFilingTargetAndProbability`,
  `Issue439InvalidTypedNavigationDoesNotSelectBannerOrPseudoRows`, plus
  `Issue439AncestorActivationQueriesAncestorKeyAndSelectsArchiveRelativeChild` and
  `Issue439UnresolvedChainsUseSelectableFallbackForEveryDiagnosableProviderOutcome`.
- All three `Issue609_*` router tests PASSED unedited:
  `Issue609_AncestorActivation_EmitsArchiveRelativeFilingTarget`,
  `Issue609_DirectRowSelection_UsesFullLookupAndRelativeFilingTarget`,
  `Issue609_ImmediateChildActivation_EmitsArchiveRelativeFilingTarget`.
- Every pre-existing `BreadcrumbBridgeRouterTests` (#349) test PASSED unedited, confirming the
  preserved no-archive-root binding mode.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p3-t5\` tree.
