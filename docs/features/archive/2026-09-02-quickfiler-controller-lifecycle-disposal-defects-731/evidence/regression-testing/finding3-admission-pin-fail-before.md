# Finding 3 — admission scoring-delegate pin, failing run before the fix

Timestamp: 2026-09-03T14-22

Task: [P3-T2] [expect-fail]
Issue: #731

## Command

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcRemainingQueueAdmission_DeclaresNoScoringDelegate"
```

vstest console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64).

EXIT_CODE: 1

ExpectedExitCode: 1

The build exited 0 and the test run exited 1, which is the expected outcome for this task: the dead constructor parameters have not been removed yet.

## Output Summary

```
  Failed QfcRemainingQueueAdmission_DeclaresNoScoringDelegate [171 ms]

Total tests: 1
Test Run Failed.
     Failed: 1
```

- Total tests: **1**
- Passed: **0**
- Failed: **1**

Observed failure message, with absolute paths rewritten to their repository-relative remainder under the Evidence path-hygiene rule:

```
Expected constructors[0].GetParameters() {UtilitiesCS.IApplicationGlobals globals,
System.Func`3[Microsoft.Office.Interop.Outlook.MailItem,System.Threading.CancellationToken,
System.Threading.Tasks.Task`1[System.Int64]] scoreLoader,
System.Action`1[Microsoft.Office.Interop.Outlook.MailItem] addToQueue,
System.Action`2[Microsoft.Office.Interop.Outlook.MailItem,
System.Action`1[Microsoft.Office.Interop.Outlook.MailItem]] hookItem,
System.Action`1[Microsoft.Office.Interop.Outlook.MailItem] removeFromQueue} to not have any items
matching (parameter.ParameterType ==
System.Func`3[Microsoft.Office.Interop.Outlook.MailItem,System.Threading.CancellationToken,
System.Threading.Tasks.Task`1[System.Int64]]) because issue #233: Threshold scoring belongs to
dequeue-time enforcement., but found
{System.Func`3[Microsoft.Office.Interop.Outlook.MailItem,System.Threading.CancellationToken,
System.Threading.Tasks.Task`1[System.Int64]] scoreLoader}.
```

The test failed for exactly the reason this task requires: the sole constructor of `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` still declares the scoring-delegate parameter `Func<MailItem, CancellationToken, Task<long>> scoreLoader` at `:17`, and the assertion enumerates the observed parameter list and names that parameter as the offending item. The reproduction is genuine rather than fabricated: the assertion reads the real constructor signature, and it will pass only once [P3-T3] removes that parameter.
