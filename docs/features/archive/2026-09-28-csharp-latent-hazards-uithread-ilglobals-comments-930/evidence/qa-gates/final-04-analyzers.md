# Final 04: analyzer Rebuild ([P2-T4])

Timestamp: 2026-09-29T09-19
Command: CMD-ANALYZE with STAGE = final: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/930-final-analyzers.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: CMD-MSBUILD-SUMMARY with LOG = 930-final-analyzers.detailed.log
EXIT_CODE: 0
Iteration: 1
Output Summary:
- Outlook re-checked before the Rebuild: OUTLOOK_STATE=CLOSED.
- MSBUILD_EXIT=0
- CSC_TASK_LINES=18
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- WARNING_SUMMARY_LINE=0 Warning(s) (BASELINE-ANALYZER-WARNINGS: 0 Warning(s); not above baseline)
- CS86_ERROR_LINES=0
