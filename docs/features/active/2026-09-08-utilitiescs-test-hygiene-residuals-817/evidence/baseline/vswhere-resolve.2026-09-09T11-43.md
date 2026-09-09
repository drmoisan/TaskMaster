Timestamp: 2026-09-09T11-43
Command: vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' ; vswhere -latest -products * -find 'MSBuild\Current\Bin\MSBuild.exe'
EXIT_CODE: 0
Output Summary: Both vstest.console.exe and MSBuild.exe resolved and confirmed to exist (Test-Path True for both).

$vstest = C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
$msbuild = C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
