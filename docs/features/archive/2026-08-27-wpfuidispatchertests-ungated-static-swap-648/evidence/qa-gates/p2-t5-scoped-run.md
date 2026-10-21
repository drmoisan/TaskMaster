# P2-T5 — Scoped Class Test (Phase 2)

Timestamp: 2026-09-01T14-33

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
  Passed Construction_YieldsAnIUiDispatcher [30 ms]
  Passed Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread [40 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.1568 Seconds
```

The checkout-root prefix is elided; the remainder is verbatim.

The output contains `Test Run Successful.`, `Total tests: 2` is recorded, and the exit code is 0. The
two members selected by the filter are named explicitly by the run and are
`Construction_YieldsAnIUiDispatcher` and
`Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread`, both passing.

This run is executed after P2-T1's formatting pass and after the P2-T3 and P2-T4 solution rebuilds, so
it exercises the assembly those rebuilds produced.
