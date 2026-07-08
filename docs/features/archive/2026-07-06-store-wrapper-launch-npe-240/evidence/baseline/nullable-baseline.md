# Nullable / TreatWarningsAsErrors Baseline (Issue #240)

Timestamp: 2026-07-06T07-15

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(Invoked as `-t:Rebuild` because an incremental `-t:Build` skipped `CoreCompile` for up-to-date outputs and did not exercise the forced-nullable flags; `-t:Rebuild` forces recompilation. Dash-switch form required by the git-bash shell.)

EXIT_CODE: 1

Output Summary: Build FAILED with 84 pre-existing error(s), 0 warning(s). All 84 errors are confined to two vendored/legacy projects that are not nullable-annotated: `SVGControl.csproj` (CS8600/CS8601/CS8602/CS8603/CS8618/CS8625/CS0649) and `UtilitiesSwordfish.NET.General.csproj` (CS8600/CS8601/CS8602/CS8603/CS8604/CS8618/CS8619/CS8625). `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` (the in-scope projects for this fix) contribute zero errors. This is a pre-existing baseline condition when `Nullable=enable` is forced globally across the solution; it is not caused by this issue's change and is unaffected by the planned `StoreWrapperController.cs` fix. Final QA (Phase 3) will confirm zero new warnings/errors on the touched files.
