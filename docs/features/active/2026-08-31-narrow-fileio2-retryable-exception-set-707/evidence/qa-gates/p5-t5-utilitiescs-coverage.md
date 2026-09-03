Timestamp: 2026-09-03T13-55
Iteration: 1

KNOWN_ENVIRONMENT_DEFECT: issue #752 (same substitution as evidence/baseline/p0-t17-utilitiescs-coverage.md).

Command (substituted): resolved $vstest via vswhere, located the freshly rebuilt UtilitiesCS.Test.dll (workspace-root-prefix checked), then:
dotnet-coverage collect "<vstest>" "<dll>" /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook "/Logger:trx;LogFileName=p5-t5.trx" /ResultsDirectory:"coverage\testresults\p5-t5" --output "coverage\coverage.cobertura.xml" --output-format cobertura

DISCOVERED_ASSEMBLY: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a1cd2e1147794981e\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll (begins with the workspace root)
EXIT_CODE: 1 (vstest reports non-zero because of the 17 pre-existing Deedle/F# failures, not a tooling error)

TOTAL_TESTS: 4786
PASSED: 4769
FAILED: 17
SKIPPED: 0

Failed-name set (identical to evidence/baseline/p0-t20-baseline-failure-set.md, a subset of BASELINE_FAILURE_SET): DeedleDoodles, GetColumnEid_WithStringValues_ReturnsOrdinalSeries, GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields, FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows, GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame, FromArray2D_EmailLikeArray_ReturnsExpectedRowCountAndColumnLayout, Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame, GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform, FromDefaultFolder_EmptyStores_ReturnsEmptyFrame, FromDefaultFolder_StoresWithOneStoreThatHasNoData_ReturnsEmptyFrame, PrintToLog_WithPopulatedFrame_LogsWithoutThrowing, DropFirstN_DropsFirstNRows, Exclude_EmptyOtherFrame_ReturnsSameRowCount, Exclude_NonEmptyOtherFrame_RemovesMatchingRows, GetDuplicateEntriesByColumn_ReturnsDuplicateValues, FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame, FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame.

WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying: Passed.

Output Summary: Total tests 4786 (baseline 4785 + 1 new test), Passed 4769, Failed 17 (identical set to baseline, a subset of BASELINE_FAILURE_SET as required). Total >= 12 (satisfied at 4786). New test WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying Passed.
