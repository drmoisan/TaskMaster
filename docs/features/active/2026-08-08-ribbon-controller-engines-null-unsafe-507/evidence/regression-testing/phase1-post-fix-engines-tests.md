# Phase 1 — Post-fix Engines tests (both new tests)

Timestamp: 2026-08-08T16-52

Command: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing|FullyQualifiedName~Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines"`
Invocation used:
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage "/TestCaseFilter:FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing|FullyQualifiedName~Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines" /InIsolation`

MSTest Discovery Caveat: only `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` is relevant to this
filtered run; no path under `.claude` was included.

Precondition: run post-fix, after `TaskMaster/Ribbon/RibbonController.Intelligence.cs` line 204
was changed to `internal IAppItemEngines Engines => Globals?.Engines;` (P1-T4) and the solution
rebuilt successfully (exit 0, confirmed in P1-T4).

EXIT_CODE: 0

Output Summary: `Total tests: 2`, `Passed: 2`, `Failed: 0`. Both
`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing` (357 ms) and
`Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines` (80 ms) passed: `Test Run Successful.` The
first test confirms `Engines` no longer throws and returns `null` when `Globals` is unassigned
(AC1); the second confirms `Engines` continues to forward the exact assigned
`Globals.Engines` instance by reference when `Globals` is assigned (AC4).
