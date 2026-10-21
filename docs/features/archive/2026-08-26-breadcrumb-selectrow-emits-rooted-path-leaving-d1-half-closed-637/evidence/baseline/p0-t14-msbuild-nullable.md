Timestamp: 2026-08-31T10:01:51-04:00
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\\**\\Bin\\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; "EXIT_CODE=$LASTEXITCODE"'
EXIT_CODE: 0
NULLABLE_OPT_IN_PROPERTY: absent
Output Summary: Build succeeded. MSBuild executed Rebuild target(s), with 5 warnings and 0 errors.

MSBuild final status line: Build succeeded.
Warning(s): 5
Error(s): 0
(Rebuild target(s)): observed.
