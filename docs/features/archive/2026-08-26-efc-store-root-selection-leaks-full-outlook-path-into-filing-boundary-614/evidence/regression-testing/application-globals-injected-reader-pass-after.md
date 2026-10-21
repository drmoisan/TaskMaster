Timestamp: 2026-08-27T03-21-25Z
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0
Output Summary: The project compiled with 0 errors and 5 existing compatibility warnings. The scoped SmartSerializableLoader test run passed 10/10, including the unchanged P1-T1 regression.

VSTest resolver: `$vstest = Join-Path (& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath) "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`
VSTest command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests" "/Logger:trx;LogFileName=p2-t2.trx" "/ResultsDirectory:coverage\trx\p2-t2"`
VSTest exit code: 0
Test result: 10 total, 10 passed, 0 failed.

The P1-T1 regression body was not modified between red and green runs.
