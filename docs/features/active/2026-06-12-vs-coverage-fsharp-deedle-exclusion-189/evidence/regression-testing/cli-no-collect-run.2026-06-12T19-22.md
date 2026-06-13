# Phase 2 — CLI No-Regression Run (AC5, no /collect)

Timestamp: 2026-06-12T19-22

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" ^
  "c:\Users\DanMoisan\repos\TaskMaster\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" ^
  /Tests:Deedle ^
  /Settings:"c:\Users\DanMoisan\repos\TaskMaster\scripts\vscode\TaskMaster.cli.runsettings" ^
  /InIsolation ^
  /ResultsDirectory:"...\TestResults_p2t2"
```
(NO `/collect` — plain run; `/InIsolation` required for the Moq-referencing assembly per repo test-env quirk;
`/Tests:Deedle` selects the Deedle suite, matching the failing set named in the issue.)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 42, Passed: 42, Failed: 0. Total time ~1.23s.
- Selected Deedle suite includes `DfDeedle_Tests`, `DfDeedle_COM_Tests`, and `DeedleTests.DeedleDoodles`
  cases (e.g., `DeedleDoodles`, `FromArray2D_*`, `FromDefaultFolder_*`, `AddQfcColumns*`, `Email2dArrayToDf*`).
- NO `.coverage` attachment / coverage result was produced. Verified by enumerating the results directory:
  `find TestResults_p2t2 -iname "*.coverage"` returned 0 files (the run produced no results artifacts at all,
  confirming no data collector was active).
- This confirms the CLI runsettings (`TaskMaster.cli.runsettings`, no `<DataCollector>` block) means a plain
  `vstest.console` run never sees the Code Coverage collector and collects no coverage. The temp results
  directory was removed after verification (working tree left clean).

AC5 (CLI no-regression portion): the Deedle tests pass and the run produces no code-coverage attachment.
