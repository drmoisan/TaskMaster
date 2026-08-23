# Dispose during in-flight traversal: failing regression evidence

Timestamp: 2026-08-04T21:24:00-04:00

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants=REMEDIATION_P1_T14 /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `vstest.console.exe UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll /Tests:Dispose_CancelsInFlightTraversalBeforeItCanPublish`
EXIT_CODE: 1
Output Summary: Expected-red result: the controlled disposal test proved the service did not own or cancel the in-flight traversal token before the repair.

Commands:

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:DefineConstants=REMEDIATION_P1_T14 /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /Tests:Dispose_CancelsInFlightTraversalBeforeItCanPublish
```

Result: the dedicated test compiled and failed as expected before the disposal cancellation repair.

The test holds a traversal at a controlled `IDispatcherYield` boundary, disposes the service, and checks whether the active cancellation token is signalled before release. The current service does not own or cancel an in-flight traversal token, so the assertion failed:

```text
Expected cancellationWasObservedAtDispose to be True because disposing the service must cancel the active traversal before it can publish, but found False.
```

The yield was released after observation and the pending traversal exception was awaited, so the test leaves no incomplete task. It uses no timer, polling, temporary file, live Outlook object, or real UI.
