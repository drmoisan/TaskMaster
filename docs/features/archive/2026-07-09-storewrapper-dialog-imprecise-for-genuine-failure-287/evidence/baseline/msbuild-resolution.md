Timestamp: 2026-09-01T00-19
Command: pwsh -NoProfile -Command 'msbuild -version'; pwsh -NoProfile -Command '(Get-Command msbuild).Source'
EXIT_CODE: 0
Output Summary: `msbuild -version` prints "MSBuild version 18.9.1+a81b43525 for .NET Framework" / "18.9.1.35102". `msbuild` resolves on PATH to the absolute executable C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe (VS18 Community). All later msbuild tasks in this plan invoke this same executable via the bare `msbuild` command on PATH.
