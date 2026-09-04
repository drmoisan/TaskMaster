# P6-T11 — Frozen-contract regression set, post-change

Timestamp: 2026-09-04T02-19

Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~AppOlObjectsArchiveRootValidationTests&TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p6-t11-taskmaster.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\qa-gates\p6-t11
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcDataModelArchiveRootTests.MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates" "/Logger:trx;LogFileName=p6-t11-quickfiler.trx" /ResultsDirectory:docs\features\active\2026-09-02-efc-archiveroot-boundary-sink-defects-736\evidence\qa-gates\p6-t11
```

EXIT_CODE: 0 for both runs.

## TaskMaster.Test — `AppOlObjectsArchiveRootValidationTests`

Total **6**, passed **6**, failed **0**. Total time 1.2169 seconds.

| Method | Outcome |
|---|---|
| `RequireResolvedArchiveRoot_ResolvedRootMatchesComposedPath_ReturnsIt` | Passed |
| `RequireResolvedArchiveRoot_CaseDifferingResolvedRoot_ReturnsComposedPath` | Passed |
| `RequireResolvedArchiveRoot_UnresolvableRoot_ThrowsAndDiagnosesWithoutTheValue` | Passed |
| `RequireResolvedArchiveRoot_CrossStoreRoot_ThrowsAndDiagnosesWithoutTheValue` | Passed |
| `RequireResolvedArchiveRoot_EmptyComposedPath_Throws` | Passed |
| `ConsumerSeam_ArchiveRootPath_IsReadThroughTheMockableInterface` | Passed |

The 6/6/0 triple matches the P0-T7 baseline exactly, so the frozen guard `ArchiveRootPathGuard` and
its consumer seam behave identically after this item's edits. That class is the contract this item
reuses without modifying: D3 records that ArchiveRootPathGuard.cs is not edited and that the new core
calls its existing method and reuses its rule constant.

## QuickFiler.Test — the COM-propagation contract

Total **1**, passed **1**, failed **0**. Total time 1.3144 seconds.

| Method | Outcome | Duration |
|---|---|---|
| `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` | **Passed** | 164 ms |

This is the contract the finding-6 rewrite had to leave intact: a `COMException` raised by the
archive-root read still propagates out of `MoveToFolderAsync` rather than being absorbed by the new
`InvokeFilerAsync` seam. P5-T6 separately proved that no diff hunk intersects this method's source
span.

## Results directory

Exactly **two** TRX files exist under this task's results directory and no others:

```
p6-t11-quickfiler.trx
p6-t11-taskmaster.trx
```

No MSTest deployment directory was created beside them.

Output Summary: both runs exited 0. TaskMaster.Test's `AppOlObjectsArchiveRootValidationTests`
recorded total 6, passed 6, failed 0 — identical to the P0-T7 baseline. QuickFiler.Test recorded
`MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` as passed. Exactly two TRX
files exist under the results directory and no others.
