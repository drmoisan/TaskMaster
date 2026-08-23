# P8-T44 TRX command-shape defect

Timestamp: 2026-07-27T02-48Z

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe <eight Debug test assemblies> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /Logger:console;verbosity=detailed /Logger:trx;LogFileName=docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-48.trx`

EXIT_CODE: 1

Output Summary: The expected fail-before test result matched P8-T44's required totals (6,047 discovered, 6,043 passed, four failed, zero skipped) and the four required SpamBayes failures. VSTest interpreted the relative `LogFileName` below its default `TestResults` directory, creating the TRX outside the canonical feature evidence path. P8-T44 is therefore not complete and requires an in-place plan revision.

## Ordered test assemblies

1. `QuickFiler.Test\\bin\\Debug\\QuickFiler.Test.dll`
2. `Tags.Test\\bin\\Debug\\Tags.Test.dll`
3. `TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll`
4. `TaskTree.Test\\bin\\Debug\\TaskTree.Test.dll`
5. `TaskVisualization.Test\\bin\\Debug\\TaskVisualization.Test.dll`
6. `ToDoModel.Test\\bin\\Debug\\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\\bin\\Debug\\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\\bin\\Debug\\VBFunctions.Test.dll`

## Required failure signatures verified

- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.GetDestinationFolder_WhenSpamTrue_ReturnsJunkCertain`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.MoveSpamOrHam_WithMailItemAndDestination_MovesMail`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Selection_WhenInputContainsMailItem_ProcessesMessage`
- `UtilitiesCS.Test.EmailIntelligence.ClassifierGroups.SpamBayes_Additional_Tests.TestAsync_Object_WhenInputIsMailItem_ProcessesMessage`

## Artifact integrity

- Preserved final-coverage report SHA-256: `21DF0583C534689A4685575718768D15BE87F59AFBEBA9B551A4CDDEDAF0C43D`
- Preserved Cobertura file SHA-256: `F9EF1C829E4B820C80EF6E4FFDA3D9DCBDCB7933B589EBFFA6F0F5255DF3A20E`
- Noncanonical VSTest TRX path: `TestResults/docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/p9-t4-all-assembly-spambayes-diagnostic.2026-07-27T02-48.trx`
- Noncanonical VSTest TRX SHA-256: `1B92CBD73B3C79B9AC2FE4C5D0F34B5C7A0AF404B2A0725BD268564354507FCC`

## Required plan delta

Replace the relative TRX logger argument in P8-T44 with a canonical results directory plus basename-only file name, such as `/ResultsDirectory:docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing /Logger:trx;LogFileName=p9-t4-all-assembly-spambayes-diagnostic.<timestamp>.trx`, and require the generated TRX to exist under the feature evidence directory before the task may be checked off.
