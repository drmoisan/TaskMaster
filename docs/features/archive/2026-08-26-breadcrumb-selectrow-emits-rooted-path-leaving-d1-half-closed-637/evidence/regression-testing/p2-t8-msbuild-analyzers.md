Timestamp: 2026-08-31T10:34:53-04:00
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\\**\\Bin\\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; "EXIT_CODE=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary: Build succeeded after the partial-class seam, project registration, and regression-test additions. MSBuild executed Rebuild target(s), reported 5 Warning(s), and 0 Error(s).
