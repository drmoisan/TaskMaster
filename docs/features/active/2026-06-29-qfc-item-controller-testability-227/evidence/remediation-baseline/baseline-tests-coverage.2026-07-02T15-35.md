# Baseline — Tests + Coverage (Cycle 4, Issue #227)

Timestamp: 2026-07-02T15-35
Command: `vstest.console.exe QuickFiler.Test.dll UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
EXIT_CODE: 0
Output Summary:
- `UtilitiesCS.Test.dll (net481)`: Passed 4093, Failed 0, Skipped 0, Total 4093.
- `QuickFiler.Test.dll (net481)`: Passed 347, Failed 0, Skipped 0, Total 347.
- Combined: **4440/4440 passed, 0 failed, 0 skipped** — matches the expected cycle-3 exit state.
- Coverage (converted via `Microsoft.CodeCoverage.Console.exe merge <file> -f xml`, summed across all 18 loaded modules in the merged report): repo-wide (whole-process, includes vendored/third-party loaded assemblies) line coverage = **63.21%** (109,235 covered + 2,689 partially covered of 177,062 total lines).
- Per-module first-party line coverage from the same report (informational, matches policy's first-party-denominator framing): `UtilitiesCS.dll` 85.86%, `QuickFiler.dll` 47.69%, `QuickFiler.Test.dll` 94.31% (test file, excluded from application-code metric per policy), `UtilitiesCS.Test.dll` 95.77% (test file, excluded), `TaskMaster.dll` 8.58%, `ToDoModel.dll` 0.00%, `Tags.dll` 0.00%, `TaskVisualization.dll` 0.00%.
- The temporary `.coverage`/converted-XML artifacts were not retained (large binary/XML, not committed); the numeric summary above is the durable evidence record.
