Timestamp: 2026-10-06T23-29
Command: "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-final-utilities
EXIT_CODE: 0
Output Summary:
- The first full run discovered 5,017 tests: 5,016 passed and 1 failed because the preexisting strict-mock projection test did not configure the newly read `IItemInfo.Triage` property.
- The directly affected projection test was moved from the oversized aggregate into the focused mapping companion, where its original assertions were preserved and Triage setup/assertion were added. Phase 3 then restarted at P3-T1.
- Final run: 5,017 passed, 0 failed, 0 skipped in 13.2213 seconds.
- TRX: `TestResults\issue-979-file-size-final-utilities\DanMoisan_MEGALODON4_2026-10-06_23_29_30_net481.trx`.
- Coverage collection succeeded and produced `TestResults\issue-979-file-size-final-utilities\bdd03955-070c-4154-ae42-acc84656b497\DanMoisan_MEGALODON4_2026-10-06.23_29_39.coverage`.
- VSTest emitted no numeric coverage percentage. The user-authorized issue #979 exception waives only coverage requirements; no functional test failure was waived.
