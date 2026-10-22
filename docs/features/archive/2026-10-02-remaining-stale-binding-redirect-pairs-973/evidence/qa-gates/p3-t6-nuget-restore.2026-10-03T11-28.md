# P3-T6 NuGet restore with the new entries (issue #973)

Timestamp: 2026-10-03T11-28
Command: msbuild TaskMaster.sln /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m via scripts/vscode/Invoke-Restore.ps1 (pwsh -NoProfile -File <execution-worktree-root>/scripts/vscode/Invoke-Restore.ps1); Glob packages/System.Linq.AsyncEnumerable.10.0.12/lib/*/*.dll; git -C <execution-worktree-root> status --porcelain -- '*.csproj'; git -C <execution-worktree-root> status --porcelain -- '*packages.config'
EXIT_CODE: 0
Output Summary: restore succeeded (0 warnings, 0 errors) and installed System.Linq.AsyncEnumerable 10.0.12 into the packages folder; lib/net462 DLL present; no csproj written; exactly the five manifests are modified.

Output excerpts (C4):
Using MSBuild: <Visual Studio 18 Community install>\MSBuild\Current\Bin\MSBuild.exe
Restoring NuGet package System.Linq.AsyncEnumerable.10.0.12.
Added package 'System.Linq.AsyncEnumerable.10.0.12' to folder '<execution-worktree-root>\packages'
Installed: 1 package(s) to packages.config projects
Build succeeded.
    0 Warning(s)
    0 Error(s)

GLOB packages/System.Linq.AsyncEnumerable.10.0.12/lib/net462/System.Linq.AsyncEnumerable.dll: found
GLOB lib/*/*.dll parent folders: net10.0, net9.0, net8.0, net462, netstandard2.0 (net462 among them)
PORCELAIN '*.csproj': (empty)
PORCELAIN '*packages.config':
 M QuickFiler/packages.config
 M TaskMaster/packages.config
 M ToDoModel/packages.config
 M UtilitiesCS.Test/packages.config
 M UtilitiesCS/packages.config
