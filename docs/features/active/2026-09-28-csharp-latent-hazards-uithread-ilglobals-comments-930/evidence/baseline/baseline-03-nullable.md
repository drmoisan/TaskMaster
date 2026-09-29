# Baseline 03: nullable Rebuild ([P0-T11])

Timestamp: 2026-09-29T08-56
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/930-baseline-nullable.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: CMD-MSBUILD-SUMMARY with LOG = 930-baseline-nullable.detailed.log
EXIT_CODE: 0
Output Summary:
- MSBUILD_EXIT=0; all 18 projects built.
- CSC_TASK_LINES=18
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- WARNING_SUMMARY_LINE=0 Warning(s)
- CS86_ERROR_LINES=0
- UiThread.cs and ILGlobals.cs both carry `#nullable enable` at line 1, so this gate covers both production edits.
