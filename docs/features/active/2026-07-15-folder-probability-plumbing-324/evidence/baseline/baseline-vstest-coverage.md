# Baseline — Tests + Code Coverage

Timestamp: 2026-07-16T03-32

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage
Actual invocation (this host; readable Cobertura via dotnet-coverage 18.5.2 wrapping VS18 vstest.console.exe):
  dotnet-coverage collect --output <cobertura.xml> --output-format cobertura -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation

EXIT_CODE: 0 (non-instrumented run) ; 1 (coverage-instrumented run — pre-existing Deedle flakiness)

Output Summary:
Test counts:
- Non-instrumented vstest run (authoritative pass/fail): Total 4213, Passed 4213, Failed 0, EXIT 0.
- Coverage-instrumented run (dotnet-coverage): Total 4213, Passed 4196, Failed 17, EXIT 1.
  - The 17 failures under instrumentation are ALL Deedle/DataFrame/ETL tests (DeedleDoodles, GetColumnEid_WithStringValues_ReturnsOrdinalSeries, GetEmailDataFromTable_OneRow_..., FromArray2D_* (3), GetEmailDataInView*/Async, Email2dArrayToDf_ViaReflection_*, FromDefaultFolder_* (5), PrintToLog_WithPopulatedFrame_..., DropFirstN_DropsFirstNRows, Exclude_* (2), GetDuplicateEntriesByColumn_...).
  - These are pre-existing timing/reflection-sensitive flakes induced by coverage instrumentation (they pass with 0 failures when instrumentation is off). NONE are FolderScorer/FolderPredictor/Folder-scoring tests. Out of scope for this feature (#324).

Coverage headline values (from the readable Cobertura report; whole merged report across all assemblies including vendored Swordfish/SVGControl):
- Repository/merged line-rate: 0.5935 (59.35%); lines-covered 95116 / lines-valid 160276.
- Repository/merged branch-rate: 0.3028 (30.28%); branches-covered 12298 / branches-valid 40611.

Baseline per-module line % (target modules; primary class, compiler-generated closures excluded):
- FolderScorer (UtilitiesCS.FolderScorer): line-rate 0.9775 (97.75%), branch-rate 0.9420 (94.20%).
- FolderPredictor (UtilitiesCS.FolderPredictor): line-rate 0.8671 (86.71%), branch-rate 0.8627 (86.27%).
- FolderScore: not present at baseline (new file created in Phase 1).
- FolderRow: not present at baseline (new file created in Phase 2).

Note: AddBayesianSuggestionsAsync / RefreshSuggestions async state machines in FolderScorer show 0% at baseline (COM/model-bound, not exercised) — unchanged by this feature per plan constraint.
