# Baseline Nullable/Warnings-as-Errors msbuild Pass (Issue #267)

- Timestamp: 2026-07-07T21-04
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Execution note: Invoked in the git-bash shell as `msbuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true` (dash switches; see rationale in `csharp-analyzers-baseline.2026-07-07T20-45.md`). Properties and flags are identical to the plan's stated command.
- EXIT_CODE: 0
- Output Summary: Build succeeded in 1.28s. 0 Warning(s), 0 Error(s). The run completed in 1.28 seconds with 68 "Skipping target" lines, indicating MSBuild's incremental up-to-date check short-circuited recompilation for most/all projects (this local workstation already had Debug binaries from the immediately preceding P0-T5 pass). This is a known local-build characteristic (an incremental `/t:Build` after a prior pass does not always force recompilation under changed properties, unlike a fresh CI checkout with no prior `obj`/`bin` state). This baseline value (0/0) is recorded as observed; the diagnostic-parity comparison in P2-T3 accounts for this incremental-skip caveat explicitly rather than treating 0/0 as a meaningful upper bound on nullable diagnostics.
