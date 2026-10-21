# P0-T13 — Baseline Full QuickFiler.Test.dll Run

Timestamp: 2026-09-01T13-44

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook"
```
`vstest.console.exe` was re-resolved through `vswhere.exe` with
`-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'` and the command
was issued through `pwsh` from the checkout root.

EXIT_CODE: 0

Output Summary:

The run printed `Test Run Successful.`, so no `BASELINE_GATE_RED:` line is recorded and execution
continues to P0-T14. The summary block, recorded verbatim:

```
Test Run Successful.
Total tests: 1285
     Passed: 1285
 Total time: 10.9182 Seconds
```

No `Failed:` line was printed, which is the expected shape of a green `vstest.console.exe` run.

- Assembly path used: `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
- Resolved runsettings path used: `scripts\vscode\TaskMaster.cli.runsettings`

That runsettings file carries the `Workers=0` and `Scope=ClassLevel` parallelization block, so this
run is simultaneously the class-level parallel-scope run the issue's validation notes request.

Baseline `Passed:` count for the P2-T6 comparison: **1285**.
