Timestamp: 2026-09-09T11-57
Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /ListTests /TestCaseFilter:"FullyQualifiedName~FolderPredictorTests" (same corrected methodology as [P0-T13]/pre-split-listtests-evidence.2026-09-09T11-43.md, for the same environmental reason: this vstest build's /ListTests prints short DisplayNames only)
EXIT_CODE: 0
Output Summary: Post-split discovery returned exactly 39 tests. Compare-Object between the pre-split filtered set and this post-split filtered set returned zero rows: identical fully-qualified name sets before and after the split. Because the filter operates on FullyQualifiedName metadata from the compiled assembly, this proves the Phase 2 <Compile Include> wiring is correct and all 4 new files are compiled into UtilitiesCS.Test.dll (the exact failure mode this check guards against, per Note 5/binding warning: filtering on the type name FolderPredictorTests, never a file-name fragment).

FILTERED_COUNT: 39
COMPARE_ROWS: 0
