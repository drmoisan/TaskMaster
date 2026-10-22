# P0-T13 Nullable msbuild baseline (Rebuild with TreatWarningsAsErrors)

Timestamp: 2026-09-29T08-57
Command: pwsh -NoProfile -Command '$vw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vw -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe" | Select-Object -First 1; "MSBUILD-LEAF=" + (Split-Path -Leaf $mb); & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath coverage/logs/927-nullable.log; exit $LASTEXITCODE'; then MSBUILD-OBSERVE over coverage/logs/927-nullable.log
EXIT_CODE: 0
Output Summary:
- MSBUILD-LEAF=MSBuild.exe
- OUT-LINES=36
- SKIP-CORECOMPILE=0
- ZERO-ERRORS=1
- SUCCEEDED=1
- Warning summary line transcribed: "    0 Warning(s)".
- No solution-wide nullable property was added.
- The two analyzer package folders provisioned in P0-T12 (deviation D-P0-T12-1) were present for this run.

BASELINE-WARNINGS: 0
