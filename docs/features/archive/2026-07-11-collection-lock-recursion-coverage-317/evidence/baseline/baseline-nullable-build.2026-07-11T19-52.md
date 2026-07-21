# Baseline Nullable/TreatWarningsAsErrors Build (#317)

Timestamp: 2026-07-11T19-52

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln -t:Rebuild -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`

Note: `-t:Build` alone reported a false-clean pass because MSBuild's per-project incremental cache
skipped `CoreCompile` for projects already up to date from the prior baseline-analyzer build pass, so
`-t:Rebuild` was used to force a genuine recompilation under the `Nullable=enable` /
`TreatWarningsAsErrors=true` property set, per prior-session guidance (repo-local SDK + nullable
Rebuild note).

EXIT_CODE: 1

Output Summary: Build FAILED. 34 Error(s), 0 Warning(s), all 34 in the vendored
`SVGControl\SVGControl.csproj` project (CS8618/CS8600/CS8601/CS8602/CS8603/CS8625/CS0649 nullable-flow
and unassigned-field diagnostics). This is pre-existing nullable debt in a vendored, non-first-party
project, confirmed unrelated to this plan's scope (no error references `UtilitiesCS.Test` or either of
the two files this plan touches — confirmed via grep for `UtilitiesCS.Test` in the build output:
0 matches). This baseline failure is the pre-existing repo state and is unaffected by the planned
test-only restoration.
