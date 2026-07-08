# Baseline Test Run with Coverage (Cycle 2)

Timestamp: 2026-06-12T16:58Z

Command: vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage
(VS18 Community vstest.console.exe. /InIsolation is required for the Moq-using
UtilitiesCS.Test assembly. .coverage attachment merged to artifacts/csharp/coverage.xml
via dotnet-coverage v18.5.2 `merge -f xml`.)

EXIT_CODE: 0

Output Summary:
- Total tests: 3904; Passed: 3904; Failed: 0 (full UtilitiesCS.Test assembly).
- The 21 LcppnFolderPredictor_Tests cases (14 config/validation/training/untrain/build +
  9 Classify_*) all pass.
- LcppnFolderPredictor strict coverage (baseline): line_coverage = 97.71%
  (lines_covered=171, lines_partially_covered=4, lines_not_covered=0 across 12 function
  elements with type_name="LcppnFolderPredictor"); block_coverage = 97.58%
  (blocks_covered=242, blocks_not_covered=6). The plan-cited 97.71% strict figure is the
  line-coverage value; recorded numerically here for the no-regression check.
- Canonical coverage XML written to artifacts/csharp/coverage.xml.
