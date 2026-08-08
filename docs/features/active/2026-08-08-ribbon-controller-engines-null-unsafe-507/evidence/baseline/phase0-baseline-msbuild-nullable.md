# Phase 0 — Baseline msbuild (nullable)

Timestamp: 2026-08-08T16-14

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
Invocation used (git-bash, dash switches + MSYS_NO_PATHCONV):
`MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" TaskMaster.sln -t:Build -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -nologo -v:minimal`

EXIT_CODE: 1

Output Summary: Build failed with 195 errors, 0 warnings. All 195 errors are confined to
`UtilitiesCS.csproj` (verified: `grep "error" | grep -c "UtilitiesCS.csproj"` = 195, matching the
total error count; a separate grep for `TaskMaster.csproj`/`TaskMaster.Test.csproj` errors returns
zero matches). The errors are pre-existing nullable-reference diagnostics (CS8618, CS8601, CS8602,
CS8603, CS8604, CS8625, CS8766) in UtilitiesCS source files unrelated to this feature's two
in-scope files (`TaskMaster/Ribbon/RibbonController.Intelligence.cs`,
`TaskMaster.Test/Ribbon/RibbonControllerTests.cs`). Passing `/p:Nullable=enable` at the solution
level overrides UtilitiesCS.csproj's own (non-nullable-enabled) per-project setting, surfacing
long-standing nullable debt in a project this feature does not touch and, per the plan's Hard
Scope Boundary, may not modify. This baseline establishes that the solution-wide nullable gate
was already failing (195 pre-existing UtilitiesCS errors, 0 in TaskMaster/TaskMaster.Test) before
any change in this feature; this is recorded as-is per the baseline-capture task's acceptance
criteria (artifact populated), with no expectation that baseline itself is green. This condition
is carried forward and re-examined at Phase 2 (P2-T3) to confirm no new errors are introduced by
this feature's change.
