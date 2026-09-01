# P0-T14 — Baseline Scoped Run of WpfUiDispatcherTests

Timestamp: 2026-09-01T13-46

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~WpfUiDispatcherTests"
```
Only one `/TestCaseFilter:` switch is accepted, so the class restriction is combined with the
category exclusion in one operand. Only `QuickFiler.Test.dll` is passed; a second class named
`WpfUiDispatcherTests` exists at `UtilitiesCS.Test/Threading/WpfUiDispatcherTests.cs:12`, and the
substring filter would match it if that assembly were included.

EXIT_CODE: 0

Output Summary:

```
VSTest version 18.9.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Test Parallelization enabled for <checkout-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed Construction_YieldsAnIUiDispatcher [31 ms]
  Passed Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread [37 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.2215 Seconds
```

The checkout-root prefix in the parallelization line is elided; the remainder is verbatim.

The run printed `Test Run Successful.` and `Total tests: 2`, which are the two conditions this task's
acceptance names. The two selected members are `Construction_YieldsAnIUiDispatcher` and
`Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`, both passing before any change is
made.
