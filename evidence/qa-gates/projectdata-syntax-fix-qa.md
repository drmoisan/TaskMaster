Timestamp: 2026-03-04T10:29:37.6816103-05:00
Command: QA loop (pass 1 + required restart pass 2): dotnet format TaskMaster.sln; msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug
EXIT_CODE: 1
Output Summary:
- Format: PASS (workspace warnings only; no formatting edits reported).
- Analyzer build: PASS (Build succeeded; 0 errors in final restart pass).
- Nullable/type build: PASS (Build succeeded; 0 warnings, 0 errors in final restart pass).
- Tests: FAIL (exit code 1; Total=438, Passed=384, Failed=51, Skipped=3 in restart pass).
