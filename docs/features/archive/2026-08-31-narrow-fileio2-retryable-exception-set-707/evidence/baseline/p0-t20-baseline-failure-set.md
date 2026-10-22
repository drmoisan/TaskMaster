Timestamp: 2026-09-03T12-46
Source: evidence/baseline/p0-t17-utilitiescs-coverage.md's coverage run (dotnet-coverage collect wrapping vstest.console.exe against UtilitiesCS.Test.dll).

BASELINE_FAILURE_SET (17 tests reported Failed, all Deedle/F#-related, unrelated to FileIO2):
1. DeedleDoodles
2. GetColumnEid_WithStringValues_ReturnsOrdinalSeries
3. GetEmailDataFromTable_OneRow_ReturnsFrameWithExpectedFields
4. FromArray2D_EmptyData_ReturnsFrameWithColumnsButNoRows
5. GetEmailDataInView_WithInjectedEtlResult_ReturnsPopulatedFrame
6. FromArray2D_EmailLikeArray_ReturnsExpectedRowCountAndColumnLayout
7. Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame
8. GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform
9. FromDefaultFolder_EmptyStores_ReturnsEmptyFrame
10. FromDefaultFolder_StoresWithOneStoreThatHasNoData_ReturnsEmptyFrame
11. PrintToLog_WithPopulatedFrame_LogsWithoutThrowing
12. DropFirstN_DropsFirstNRows
13. Exclude_EmptyOtherFrame_ReturnsSameRowCount
14. Exclude_NonEmptyOtherFrame_RemovesMatchingRows
15. GetDuplicateEntriesByColumn_ReturnsDuplicateValues
16. FromDefaultFolder_Store_WithInjectedEtlResult_ReturnsPopulatedFrame
17. FromDefaultFolder_Stores_FirstStoreHasData_ReturnsNonEmptyFrame

Root cause (shared across all 17): `System.Security.VerificationException: Operation could destabilize the runtime` thrown from `Deedle.Reflection`'s F# module type initializer when dotnet-coverage's IL instrumentation is active — a known dotnet-coverage/Deedle incompatibility, unrelated to this fix's footprint (FileIO2.cs / FileIO2_Tests.cs).

Output Summary: 17 pre-existing failures recorded as BASELINE_FAILURE_SET, none involving FileIO2. All 11 FileIO2_Tests passed in this same run.
