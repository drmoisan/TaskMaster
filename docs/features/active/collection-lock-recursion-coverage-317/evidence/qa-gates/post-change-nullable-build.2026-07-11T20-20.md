# Post-Change Nullable/TreatWarningsAsErrors Build (#317) — Phase 3, P3-T3

Timestamp: 2026-07-11T20-20

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`

Note: `-t:Rebuild` used (not `-t:Build`) to force genuine recompilation, per the same false-cache-skip
finding recorded in `baseline-nullable-build.2026-07-11T19-52.md` (P0-T8).

EXIT_CODE: 1

Output Summary: Build FAILED. 34 Error(s), 0 Warning(s) — identical count, identical project
(`SVGControl\SVGControl.csproj`), and identical diagnostic codes (CS8618/CS8600/CS8601/CS8602/
CS8603/CS8625/CS0649) as the pre-restoration baseline
(`baseline-nullable-build.2026-07-11T19-52.md`). Zero errors reference `UtilitiesCS.Test` or either
of the two files touched by this plan (confirmed via grep: 0 matches). No new nullable warnings or
errors were introduced by the restoration. This is the identical pre-existing vendored-project debt,
unaffected by this plan's scope. A normal `-t:Build` pass (Debug, no special properties) was run
immediately afterward to restore the Debug binaries wiped by `-t:Rebuild`, in preparation for P3-T4
(0 Error(s), 76 Warning(s), matching the P0-T7 baseline analyzer-build warning profile).
