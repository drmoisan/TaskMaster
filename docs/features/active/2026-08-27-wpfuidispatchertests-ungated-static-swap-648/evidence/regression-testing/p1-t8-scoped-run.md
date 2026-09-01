# P1-T8 — Post-Change Scoped Class Run

Timestamp: 2026-09-01T14-14

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~WpfUiDispatcherTests"
```
`vstest.console.exe` was re-resolved through `vswhere.exe` as in P0-T14 and the command was issued
through `pwsh` from the checkout root.

EXIT_CODE: 0

Output Summary:

```
VSTest version 18.9.0 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
Test Parallelization enabled for <checkout-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)
  Passed Construction_YieldsAnIUiDispatcher [31 ms]
  Passed Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread [40 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.2535 Seconds
```

The checkout-root prefix is elided; the remainder is verbatim.

The output contains `Test Run Successful.`, `Total tests: 2` is recorded, and the exit code is 0.
Both members of the class pass after the rewrite, including the rewritten
`Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`, which now acquires its gate from
`UiThreadDispatcherFixture` rather than mutating the static by reflection.
