# Baseline — Analyzer Build

Timestamp: 2026-06-22T22-10
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

Output Summary:
- Build SUCCEEDED. EXIT_CODE 0.
- Errors: 0.
- Warnings: pre-existing only, all in test projects (TaskMaster.Test, UtilitiesCS.Test): CS8632 ("nullable annotation outside #nullable context") and CS0067 ("event never used"). These are baseline noise present before this change and are not promoted to errors under the analyzer gate (TreatWarningsAsErrors not set in this step).
- MSBuild resolved at: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe (not on PATH; invoked by absolute path).
