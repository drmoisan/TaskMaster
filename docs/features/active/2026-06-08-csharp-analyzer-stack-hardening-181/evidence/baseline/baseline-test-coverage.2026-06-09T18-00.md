# Baseline — Full First-Party Suite with Coverage (Cycle 7)

Timestamp: 2026-06-09T18-00
Command: vstest.console.exe QuickFiler.Test.dll Tags.Test.dll TaskMaster.Test.dll TaskVisualization.Test.dll ToDoModel.Test.dll UtilitiesCS.Test.dll VBFunctions.Test.dll /EnableCodeCoverage /InIsolation
EXIT_CODE: 0 (stable green run)

Resolved vstest.console: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
First-party Test.dll paths (7): QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test (vendored SVGControl/Swordfish test assemblies excluded).

## Output Summary

Numeric baseline test counts (stable run):
- Total tests: 4065
- Passed: 4065
- Failed: 0
- Skipped: 0

Flakiness note: the first of three baseline runs reported 1 failure (a pre-existing
flaky test unrelated to this cycle's in-scope conversions; a subsequent run was fully
green 4065/4065, and a third confirmed 4065/4065). This matches the cycle-6 baseline,
which documented run-to-run flakiness in tests OUTSIDE this cycle's conversion scope.
The stable expectation is 4065/4065 green, consistent with the cycle-7 inputs.

Numeric baseline coverage (line coverage from the merged coverage XML):
- Primary in-scope first-party production assembly carrying all changed lines:
  - UtilitiesCS.dll: line_coverage = 85.46%
    (lines_covered=35064, lines_partially_covered=886, lines_not_covered=5082;
     block_coverage=86.48%, blocks_covered=40335, blocks_not_covered=6304)
- All three changed production files this cycle live in UtilitiesCS.dll
  (TimeOutTask.cs, OlTableExtensions.TableAccess.cs, TimerWrapper.cs).

Coverage source: TestResults/beb50a72-d70f-4108-80ce-51a85208d897/DanMoisan_MEGALODON4_2026-06-09.18_24_57.coverage
Merged to: evidence/baseline/baseline-coverage.2026-06-09T18-00.xml (via dotnet-coverage merge -f xml)

Coverage headline for no-regression comparison: UtilitiesCS.dll = 85.46% line coverage at baseline.
