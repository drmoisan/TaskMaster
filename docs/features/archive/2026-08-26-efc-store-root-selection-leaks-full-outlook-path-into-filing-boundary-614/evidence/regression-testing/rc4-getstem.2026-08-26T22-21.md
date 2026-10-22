# RC-4 GetStem regression evidence

Timestamp: 2026-08-26T22-21

## Commands

1. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
2. `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailFilerConfig_Tests|FullyQualifiedName~ArchiveStemContractTests" "/Logger:trx;LogFileName=p3-t3.trx" "/ResultsDirectory:coverage\trx\p3-t3"`

EXIT_CODE: 0

## Output Summary

The build passed with 0 errors and the five previously recorded `System.Reactive` `packages.config` warnings. The filtered test run passed 42 of 42 tests with 0 failures.

The RC-4 test passed:

- `GetStem_FolderPathOutsideAncestor_ReturnsInputTrimmedOfLeadingSeparators`

The four Issue 614 `ResolvePaths` boundary tests passed:

- `Issue614_ResolvePaths_WithStoreRootStem_RejectsNonRelativeStemWithoutLeakingIdentifiers`
- `Issue614_ResolvePathsWithFolder_RejectsStoreRootStemThroughTheFolderOverload`
- `Issue614_ResolvePaths_RejectsSingleSeparatorLeadingStem`
- `Issue614_ResolvePaths_RejectsEmptyStem`
