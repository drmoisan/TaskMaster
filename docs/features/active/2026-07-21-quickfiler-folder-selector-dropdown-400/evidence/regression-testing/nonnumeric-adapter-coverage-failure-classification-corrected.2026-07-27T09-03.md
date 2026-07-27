# P9-T25 Corrected non-numeric adapter failure classification

Timestamp: 2026-07-27T09-03Z

Command:

```text
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\bin\Debug\Tags.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\bin\Debug\TaskTree.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:Console;Verbosity=Detailed /ResultsDirectory:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing /Logger:trx;LogFileName=nonnumeric-adapter-coverage-failure-classification-corrected.2026-07-27T09-03.trx
```

EXIT_CODE: 124 (external runner timeout)

Output Summary: The single corrected direct VSTest process remained active past the external ten-minute host limit. The host terminated the owning PowerShell runner before it could persist VSTest console output, a TRX, or a process-tree receipt; the verified remaining direct VSTest process tree was then terminated without retry.

## Ordered assemblies

1. `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
2. `Tags.Test/bin/Debug/Tags.Test.dll`
3. `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
4. `TaskTree.Test/bin/Debug/TaskTree.Test.dll`
5. `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll`
6. `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll`
7. `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
8. `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll`

## Missing result data

No canonical TRX was created. The captured console stream was unavailable because the external host terminated the owning runner before it could flush redirected output. Therefore, the discovered count, failed `UnitTestResult` identities, failure messages, stack details, and TRX SHA-256 are unavailable.

The required 6,066-test count and the complete classification of the eight P9-T19 failures cannot be established from this run.

## Verified process cleanup

After the owning runner was externally terminated, the remaining worktree-owned process tree was read and verified before termination:

- `vstest.console.exe` PID `228736`, parent PID `241724`, command line scoped to this worktree and the eight required test assemblies.
- `testhost.exe` PID `236628`, parent PID `228736`, with `--parentprocessid 228736`.
- `conhost.exe` PID `279020`, parent PID `236628`.

The verified descendants were terminated in child-first order: `279020`, `236628`, then `228736`. A post-cleanup process query found zero worktree-related `vstest.console.exe`, `testhost.exe`, or `dotnet.exe` processes.

## Classification outcome

P9-T25 remains unchecked. This one-shot command produced missing mandatory result data and no test-behavior classification. Per the plan, an in-place revision is required before any further diagnostic or coverage command.
