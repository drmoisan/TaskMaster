# P1-T3 fail-before: notification cleanup affinity

Timestamp: 2026-08-04T20:27:00-04:00

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~NotificationRefreshAndDispose_RunOnTheCapturedDispatcher'`

EXIT_CODE: 1

Output Summary: After temporarily reconstructing only the reviewed defect by replacing dispatcher-marshalled cleanup with direct caller-thread cleanup, the solution build passed with the existing six warnings and the dedicated-STA test failed. Recorded subscription and cleanup thread IDs included worker thread `31` rather than the captured dispatcher thread. The temporary hunk was immediately restored.
