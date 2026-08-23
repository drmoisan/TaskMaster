# Phase 1 — [expect-fail] Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing (pre-fix)

Timestamp: 2026-08-08T16-50

Command: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing"`
Invocation used:
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /EnableCodeCoverage "/TestCaseFilter:FullyQualifiedName~Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing" /InIsolation`

MSTest Discovery Caveat: only `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` is relevant to this
filtered run (the new test lives in that assembly); no path under `.claude` was included.

Precondition: this run targets the current pre-fix source (`RibbonController.Intelligence.cs`
line 204 still reads `Globals.Engines` with no null-conditional). The production fix (P1-T4) has
not yet been applied.

EXIT_CODE: 1

Output Summary: `Total tests: 1`, `Failed: 1`. The test failed as expected with:
`Error Message: Did not expect any exception, but found System.NullReferenceException: Object
reference not set to an instance of an object.` — the FluentAssertions
`act.Should().NotThrow()` assertion caught the pre-fix `NullReferenceException` thrown from
`RibbonController.get_Engines()` when `Globals` is unassigned, matching the issue's documented
observed failure signature. This confirms the regression test correctly fails against the pre-fix
source, satisfying the `[expect-fail]` requirement.
