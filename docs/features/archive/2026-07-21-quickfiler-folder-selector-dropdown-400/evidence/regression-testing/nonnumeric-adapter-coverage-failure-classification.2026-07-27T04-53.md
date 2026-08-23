# P9-T24 Non-numeric adapter coverage failure classification

Timestamp: 2026-07-27T08:53:18.9554772Z

Command:

```text
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\bin\Debug\Tags.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\bin\Debug\TaskTree.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ConsoleLogger:verbosity=detailed /ResultsDirectory:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing /Logger:trx;LogFileName=nonnumeric-adapter-coverage-failure-classification.2026-07-27T04-53.trx
```

EXIT_CODE: 1

## Ordered assemblies

1. `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
2. `Tags.Test/bin/Debug/Tags.Test.dll`
3. `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
4. `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
5. `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
6. `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
7. `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
8. `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

## Result

The one permitted direct VSTest invocation exited before test discovery. VSTest reported:

```text
The argument /ConsoleLogger:verbosity=detailed is invalid. Please use the /help option to check the list of valid arguments.
```

Consequently, no tests were discovered, no `UnitTestResult` records or failed-test identities/messages/stacks were produced, and no canonical TRX was created. The planned expected count of 6,066 was not reached. TRX SHA-256: not available because the file does not exist.

## Process ownership and cleanup

- Direct VSTest parent process ID: `278488`
- Start: `2026-07-27T08:53:17.0246650Z`
- Completion: `2026-07-27T08:53:20.7207539Z`
- Timed out: `False`
- Observed descendant processes: none; VSTest exited before creating a testhost.
- Pre-run related VSTest/testhost/dotnet processes: 0
- Post-run related VSTest/testhost/dotnet processes: 0
- No process termination was required.

Captured console: `nonnumeric-adapter-coverage-failure-classification.2026-07-27T04-53.console.txt`.

Process-tree receipt: `nonnumeric-adapter-coverage-failure-classification.2026-07-27T04-53.process-tree.json`.

## Classification outcome

The eight P9-T19 failures cannot be classified because the only permitted direct diagnostic did not execute test discovery. P9-T24 remains unchecked. The plan requires an in-place revision before any further diagnostic or coverage command.
