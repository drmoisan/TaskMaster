# #889 red-state compile ([P1-T2])

Timestamp: 2026-09-29T09-11
Command: CMD-NULLABLE with STAGE = 889-red: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/930-889-red-nullable.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: CMD-MSBUILD-SUMMARY with LOG = 930-889-red-nullable.detailed.log
EXIT_CODE: 0
Output Summary:
- MSBUILD_EXIT=0
- CSC_TASK_LINES=18
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- WARNING_SUMMARY_LINE=0 Warning(s)
- CS86_ERROR_LINES=0
- TEST_DLL_WRITTEN: 2026-09-29T09:10:29 (UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll)
- TEST_SOURCE_WRITTEN: 2026-09-29T09:09:46 (UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs)
- The test assembly was written after the test source, so it contains the new test.
