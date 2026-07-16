# QC — Analyzer Build

Timestamp: 2026-07-16T03-32

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
Actual invocation: "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (git-bash, MSYS_NO_PATHCONV=1)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Error(s), 0 Warning(s). EXIT 0.
- The immediately-preceding full build of the two touched first-party projects (UtilitiesCS, UtilitiesCS.Test) with the new/changed Folder files compiled with 0 analyzer errors (no new analyzer diagnostics escalated). No files were changed by this step; the QC loop does not restart.
