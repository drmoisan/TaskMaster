# Final QC — Tests + Coverage (Issue #185)

Timestamp: 2026-06-12T10-49

Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage

Note: `/InIsolation` required for the Moq-based assembly; invoked with `MSYS_NO_PATHCONV=1`.
The solution was rebuilt with a plain Debug build before this run (the prior P2-T3 forced-
nullable rebuild left the test assembly in a forced-flag state).

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 70, Passed: 70, Failed: 0. Total time: 4.54s.
- This is the post-change count: 68 baseline tests + the 2 new RibbonExplorerXmlTests
  (RibbonExplorerXml_TaskMasterGroupsLiveUnderTaskmasterTab,
  RibbonExplorerXml_TabMailCarriesNoCustomGroup).

Coverage headline (from .coverage, converted via dotnet-coverage merge -f xml):
- Aggregate line coverage across all instrumented modules: covered=7766, partial=443,
  not-covered=84242, total=92451 -> 8.40% (8.88% including partially covered).
- Same single-assembly command scope as the P0-T5 baseline (8.34%); the figure is dominated
  by third-party/other-project DLLs that this run loads but does not exercise.

First-party module breakdown (lines covered / partial / not-covered):
- TaskMaster.Test.dll: 2242 / 48 / 118   (baseline 2206 / 46 / 118; +36 covered from the 2 new tests)
- TaskMaster.dll: 804 / 63 / 2359        (unchanged vs baseline)
- ToDoModel.dll: 45 / 12 / 3580          (unchanged vs baseline)
- UtilitiesCS.dll: 1774 / 176 / 39134    (unchanged vs baseline)
- TaskVisualization.dll: 13 / 0 / 3600   (unchanged vs baseline)

No first-party production module lost coverage. The new test methods added 36 covered lines
in TaskMaster.Test.dll. See coverage-delta.md (P2-T5) for the full comparison.
