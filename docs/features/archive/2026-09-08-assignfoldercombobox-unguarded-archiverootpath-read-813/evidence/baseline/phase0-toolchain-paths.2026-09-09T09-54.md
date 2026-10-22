Timestamp: 2026-09-09T09-54
Command: vswhere.exe -latest -find **\vstest.console.exe
EXIT_CODE: 0
Output Summary: two candidates returned; resolved vstest.console.exe path used by this plan:
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
(secondary candidate not used: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe)
Confirmed via Test-Path: True

Command: vswhere.exe -latest -find **\MSBuild.exe
EXIT_CODE: 0
Output Summary: two candidates returned; resolved MSBuild.exe path used by this plan:
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe
(secondary candidate not used: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe)
Confirmed via Test-Path: True

Resolved paths for later phases:
- VSTEST_EXE = C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe
- MSBUILD_EXE = C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe
