# P2-T4/P2-T6 regression pass: notification cleanup affinity

Timestamp: 2026-08-04T20:28:00-04:00

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~NotificationRefreshAndDispose_RunOnTheCapturedDispatcher'`

EXIT_CODE: 0

Output Summary: The solution build passed with the existing six warnings and the dedicated-STA regression passed. It verified notification-triggered refresh execution, notification unsubscription, and sink disposal all used the captured dispatcher; worker-initiated disposal did not perform notification cleanup on the worker.
