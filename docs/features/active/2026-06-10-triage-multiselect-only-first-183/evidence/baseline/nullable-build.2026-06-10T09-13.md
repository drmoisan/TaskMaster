# Baseline — Nullable / TreatWarningsAsErrors Build (Issue #183)

Timestamp: 2026-06-10T09-13

Command (canonical): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
Command (executed, git-bash dash-switch form): `"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true -m`

EXIT_CODE: 0

Output Summary: Build PASS (incremental `-t:Build`). 0 Warnings, 0 Errors. The first-party assemblies (including UtilitiesCS and UtilitiesCS.Test) compile clean under nullable/TreatWarningsAsErrors. Note: a forced `-t:Rebuild` would surface ~84 pre-existing nullable errors confined to the vendored SVGControl and UtilitiesSwordfish projects (out of scope for issue #183, which touches only first-party UtilitiesCS). The canonical command form (`/t:Build`, incremental) is green at baseline. Test DLLs remain present after this incremental build.
