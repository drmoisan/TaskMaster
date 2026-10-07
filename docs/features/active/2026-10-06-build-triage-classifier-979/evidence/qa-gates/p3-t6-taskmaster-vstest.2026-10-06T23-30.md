Timestamp: 2026-10-06T23-30
Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-final-taskmaster
EXIT_CODE: 0
Output Summary:
- Final run: 478 passed, 0 failed, 0 skipped in 6.5489 seconds.
- The standard-QC `TestCategory!=LiveOutlook` exclusion was retained.
- TRX: `TestResults\issue-979-file-size-final-taskmaster\DanMoisan_MEGALODON4_2026-10-06_23_29_55_net481.trx`.
- Coverage collection succeeded and produced `TestResults\issue-979-file-size-final-taskmaster\9da38f49-f810-471f-a1c2-54fc03dbb8bf\DanMoisan_MEGALODON4_2026-10-06.23_29_58.coverage`.
- VSTest emitted no numeric coverage percentage. The user-authorized issue #979 exception waives only coverage requirements; no functional test failure was waived.
