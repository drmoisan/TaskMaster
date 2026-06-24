# Baseline — Nullable / TreatWarningsAsErrors (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(msbuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`; run with `-m -v:m`)
EXIT_CODE: 0

Output Summary:
- Build succeeded with `Nullable=enable` and `TreatWarningsAsErrors=true`. All 19 projects compiled with no nullable warnings-as-errors and no compiler warnings promoted to errors.
- This is the type-check baseline; the Phase 5 final-QC nullable run is compared against it for "no new nullable warnings-as-errors".
