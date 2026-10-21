# P0-T9 — MSBuild Resolution Through vswhere

Timestamp: 2026-09-01T13-35

Command:
```
$vswherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuildPath = & $vswherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
& $msbuildPath -version
```
run through `pwsh -NoProfile -File`, using the same discovery
`scripts/vscode/Invoke-Restore.ps1:22-30` performs.

EXIT_CODE: 0

Output Summary:

Resolved MSBuild path (non-empty):

```
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
```

Version string printed by `MSBuild.exe -version`:

```
MSBuild version 18.9.1+a81b43525 for .NET Framework
18.9.1.35102
```

The resolved path is under the machine-wide Program Files tree and carries no account name, so it is
recorded verbatim. It is not copied into any other plan task; every task that needs MSBuild
re-resolves it through the same vswhere discovery.
