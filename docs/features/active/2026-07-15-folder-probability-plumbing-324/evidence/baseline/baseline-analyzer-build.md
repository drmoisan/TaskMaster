# Baseline — Analyzer Build

Timestamp: 2026-07-16T03-32

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
Actual invocation (this host): "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (git-bash with MSYS_NO_PATHCONV=1)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Error(s), 76 Warning(s). Time Elapsed ~11s.
- Warnings are pre-existing baseline diagnostics, predominantly CS8632 ("The annotation for nullable reference types should only be used in code within a '#nullable' annotations context") in TaskMaster.Test files, plus MSB3245 reference-resolution warnings for VSTO Office.Tools reference assemblies. None originate in the Folder scoring code targeted by this feature.

Environment note:
- The canonical build tool is full-framework MSBuild.exe from Visual Studio 18 (Community, 18.7.8). NuGet packages.config dependencies were restored once via nuget.exe restore TaskMaster.sln (169 packages). The repo-local .NET SDK 8.0.205 (.dotnet-sdk/) is used only for the csharpier dotnet tool.
