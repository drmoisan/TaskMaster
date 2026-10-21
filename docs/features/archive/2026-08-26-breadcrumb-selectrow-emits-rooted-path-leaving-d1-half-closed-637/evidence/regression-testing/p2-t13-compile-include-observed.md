Timestamp: 2026-08-31T10:41:32-04:00
Command: Extract `UnitTestResult` attributes from `<trx-file>` after the P2-T11 run.
EXIT_CODE: 0
Output Summary: Exactly 10 fixed-method results were observed. The explicit compile item is present; removing it would reduce the observed count to 0 in this non-SDK project.

Compile Include: `<Compile Include="Controllers\BreadcrumbBridgeRouterIssue637Tests.cs" />`

Observed result attributes:

- `testName="RowSelected_OutOfRootRootedTarget_IsStillRejected" outcome="Passed"`
- `testName="RowSelected_RootedTargetWithNoBoundArchiveRoot_PassesThroughVerbatim" outcome="Passed"`
- `testName="RowSelected_RootedTargetUnderArchiveRoot_CaseInsensitiveAndTrailingSeparatorRoot_CommitsTheStem" outcome="Failed"`
- `testName="RowSelected_TrashPseudoRow_CommitsTheSentinelVerbatim" outcome="Passed"`
- `testName="RowSelected_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem" outcome="Failed"`
- `testName="RowSelected_RelativeFilingTarget_CommitsTheValueVerbatim" outcome="Passed"`
- `testName="SelectFirstRow_RootedTargetUnderArchiveRoot_CommitsTheArchiveRelativeStem" outcome="Failed"`
- `testName="RowSelected_ArchiveRootExactFilingTarget_PreservesAPriorValidSelection" outcome="Failed"`
- `testName="RowSelected_SeparatorBoundaryNearMissTarget_IsStillRejected" outcome="Passed"`
- `testName="RowSelected_ArchiveRootExactFilingTarget_IsNotSelected" outcome="Failed"`
