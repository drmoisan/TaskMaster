# Baseline — MSTest Coverage (TaskMaster.Test, LiveOutlook excluded)

Timestamp: 2026-06-22T22-10
Command: vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 117. Passed: 117. Failed: 0. Skipped/excluded: LiveOutlook category filtered out.
- Total time: ~4.86 s.
- Coverage headline (from the .coverage file merged to Cobertura via dotnet-coverage 18.5.2):
  - line-rate = 0.11144686 (11.14%)
  - lines-covered = 9036
  - lines-valid = 81079
- Interpretation: this is the raw `/EnableCodeCoverage` figure for the single TaskMaster.Test assembly run as specified by the plan (`<TaskMaster.Test assembly>`). The instrumentation denominator includes every loaded solution assembly while only TaskMaster.Test code paths are exercised in this run, so the headline percentage is low by construction. It serves as the deterministic baseline for the P3-T5 delta because P0-T6 and P3-T4 use the identical command and denominator.
- `/InIsolation` is required for this Moq-based assembly (otherwise vstest STTE setup fails with FileNotFound).
- Notes: MSYS_NO_PATHCONV=1 used for the vstest filter and the dotnet-coverage Windows output path.
