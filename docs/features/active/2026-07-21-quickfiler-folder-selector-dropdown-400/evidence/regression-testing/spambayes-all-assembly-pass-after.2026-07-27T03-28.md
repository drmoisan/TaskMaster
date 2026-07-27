# SpamBayes all-assembly pass-after

Timestamp: 2026-07-27T03-28
Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll Tags.Test\\bin\\Debug\\Tags.Test.dll TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll "/ResultsDirectory:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:console;verbosity=detailed "/Logger:trx;LogFileName=spambayes-all-assembly-pass-after.2026-07-27T03-28.trx"`.
EXIT_CODE: 0
Output Summary: VSTest directly executed the ordered eight Debug non-obj/non-ref test assemblies. Exactly 6,049 tests were discovered and passed; zero failed and zero were skipped. The run includes `UtilitiesCS.Test.EmailIntelligence.SpamBayesActionsRegressionTests.GetDestinationFolder_WhenSpamTrueAndJunkCertainExists_ReturnsConfiguredJunkCertain` and `UtilitiesCS.Test.EmailIntelligence.SpamBayesActionsRegressionTests.GetDestinationFolder_WhenSpamTrueAndJunkCertainIsNull_ReturnsCurrentParent`.

## Results-directory and TRX proof

- Resolved absolute results directory: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing`
- Canonical TRX: `evidence/regression-testing/spambayes-all-assembly-pass-after.2026-07-27T03-28.trx`
- Canonical TRX exists: `true`
- Canonical TRX SHA-256: `FCB6E8E9D733B75B26D0E457EFA1C372A01C6431B460102BCD674B5BBBA914C6`

## Ordered assemblies

1. `QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll`
2. `Tags.Test\\bin\\Debug\\Tags.Test.dll`
3. `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
4. `TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll`
5. `TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll`
6. `ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll`

## Parsed totals

| Discovered | Passed | Failed | Skipped |
| ---: | ---: | ---: | ---: |
| 6049 | 6049 | 0 | 0 |
