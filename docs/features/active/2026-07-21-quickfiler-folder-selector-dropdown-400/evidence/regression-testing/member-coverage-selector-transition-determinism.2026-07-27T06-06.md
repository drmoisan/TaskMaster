# P8-T82 selector transition all-eight determinism

Both direct VSTest runs used the same ordered assemblies:

1. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
2. `Tags.Test\bin\Debug\Tags.Test.dll`
3. `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`
4. `TaskTree.Test\bin\Debug\TaskTree.Test.dll`
5. `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
6. `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`

Each run used the resolved CommonExtensions VSTest executable with `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, and `/Logger:console;verbosity=detailed`. Each was owned by a process-tree runner that captured the VSTest parent and descendants and verified no residual tracked process after completion.

| Run | Watchdog / VSTest PID | Observed descendants | Result | TRX SHA-256 |
| --- | --- | --- | --- | --- |
| 1 | 243580 / 277944 | testhost 266616, conhost 278880 | 6,056 total; 6,056 passed; 0 failed; 0 skipped; exit 0; no timeout or cleanup | `7318DFD9CAB2DDAF4692E0283D258C7B2BEAFC7BF8AC085A2181EFBC4ED00178` |
| 2 | 243632 / 254240 | testhost 271040, conhost 124592 | 6,056 total; 6,056 passed; 0 failed; 0 skipped; exit 0; no timeout or cleanup | `42D0A3B7BD66885FFD838B6234DE82A3E26522C58E9E713D8513B70238E00756` |

Canonical TRXs:

- `member-coverage-selector-transition-determinism-run-1.2026-07-27T06-04.trx`
- `member-coverage-selector-transition-determinism-run-2.2026-07-27T06-05.trx`

The parsed TRX totals and process-tree cleanup checks satisfy the sole P8-T73 reauthorization condition. Phase 9 may begin.
