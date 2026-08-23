# P8-T44 canonical SpamBayes fail-before diagnostic

Timestamp: 2026-07-27T02-59Z

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll Tags.Test\\bin\\Debug\\Tags.Test.dll TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll /ResultsDirectory:C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:console;verbosity=detailed /Logger:trx;LogFileName=p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-59.trx`

EXIT_CODE: 1

Output Summary: Expected fail-before result verified. VSTest discovered 6,047 tests; 6,043 passed, four required SpamBayes tests failed, and zero were skipped. The canonical TRX was written directly under feature evidence. No coverage command or configuration change was performed.

## Results-directory and TRX proof

- Resolved results directory: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\regression-testing`
- Canonical TRX: `evidence/regression-testing/p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-59.trx`
- Canonical TRX exists: `true`
- Canonical TRX SHA-256: `C073B9E35134FECFB64EB015D11F475B7FEB70FD3823F684058B611AC358E235`

## Ordered test assemblies

1. `QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll`
2. `Tags.Test\\bin\\Debug\\Tags.Test.dll`
3. `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
4. `TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll`
5. `TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll`
6. `ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll`

## Parsed failed UnitTestResult identities

- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.GetDestinationFolder_WhenSpamTrue_ReturnsJunkCertain`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.MoveSpamOrHam_WithMailItemAndDestination_MovesMail`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Selection_WhenInputContainsMailItem_ProcessesMessage`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Object_WhenInputIsMailItem_ProcessesMessage`

## Totals

| Discovered | Passed | Failed | Skipped |
| ---: | ---: | ---: | ---: |
| 6047 | 6043 | 4 | 0 |

## Preservation and noncanonical cleanup proof

- Preserved `final-remediation-mstest-coverage.2026-07-27T02-10.md` SHA-256: `21DF0583C534689A4685575718768D15BE87F59AFBEBA9B551A4CDDEDAF0C43D`
- Preserved `coverage-final-remediation.2026-07-27T02-10.cobertura.xml` SHA-256: `F9EF1C829E4B820C80EF6E4FFDA3D9DCBDCB7933B589EBFFA6F0F5255DF3A20E`
- Sole noncanonical TRX verified before deletion: `TestResults/docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-48.trx`
- Noncanonical TRX SHA-256 before deletion: `1B92CBD73B3C79B9AC2FE4C5D0F34B5C7A0AF404B2A0725BD268564354507FCC`
- Deletion proof: exact file deleted after canonical rerun and hash verification; its parent directory and all other `TestResults` items were preserved.
