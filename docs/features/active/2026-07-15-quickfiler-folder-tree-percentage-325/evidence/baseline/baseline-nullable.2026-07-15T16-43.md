# Baseline — Nullable / TreatWarningsAsErrors (P0-T4)

Timestamp: 2026-07-16T09-10
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build SUCCEEDED. 0 Warning(s), 0 Error(s). Nullable warnings treated as errors did not break the baseline build. This ran incrementally after the P0-T3 analyzer build (most projects up-to-date; test DLLs re-copied). The same command is re-run at final QC (P6-T3) for a consistent comparison.
