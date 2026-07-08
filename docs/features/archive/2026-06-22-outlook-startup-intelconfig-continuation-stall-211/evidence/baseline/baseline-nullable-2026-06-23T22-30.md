# Baseline — Nullable / TreatWarningsAsErrors (#211 Phase 3.2)

Timestamp: 2026-06-23T22-30
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(MSBuild 18.7.8 VS18 Community; run with `-m -v:m`)
EXIT_CODE: 0

Output Summary:
- Build succeeded with `Nullable=enable` and `TreatWarningsAsErrors=true`. All 19 projects compiled with no nullable warnings-as-errors and no compiler warnings promoted to errors.
- Type-check baseline for the Phase 5 final-QC nullable comparison.
