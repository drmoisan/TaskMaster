# P0-T13 NuGet restore of the unchanged tree (issue #973)

Timestamp: 2026-10-03T10-49
Command: msbuild TaskMaster.sln /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m via scripts/vscode/Invoke-Restore.ps1 (pwsh -NoProfile -File <execution-worktree-root>/scripts/vscode/Invoke-Restore.ps1)
EXIT_CODE: 0
Output Summary: restore succeeded (Build succeeded, 0 Warning(s), 0 Error(s), elapsed 00:00:02.29); System.Linq.Async 7.0.1 lib/net48 DLL present; no System.Linq.AsyncEnumerable package folder (negative control); no tracked csproj or packages.config written.

Output excerpts (C4):
Using MSBuild: <Visual Studio 18 Community install>\MSBuild\Current\Bin\MSBuild.exe
MSBuild version 18.10.1-1.26427.6+3cd27c13e for .NET Framework
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.29

GLOB packages/System.Linq.Async.7.0.1/lib/net48/System.Linq.Async.dll: found (1 file)
GLOB packages/System.Linq.AsyncEnumerable.*/**: no files found
git -C <execution-worktree-root> status --porcelain -- '*.csproj' '*packages.config': (empty)
