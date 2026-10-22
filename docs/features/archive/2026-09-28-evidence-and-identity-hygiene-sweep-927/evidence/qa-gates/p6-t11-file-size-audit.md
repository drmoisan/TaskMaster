# P6-T11 Post-format file-size audit (AC5)

Timestamp: 2026-09-29T22-24
Command: pwsh -NoProfile -Command '$f = @(git ls-files -- scripts/hygiene tests/scripts/hygiene ".github/workflows/_hygiene.yml" "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1") + @(git diff --name-only origin/main...HEAD -- "*.cs"); "MERGE-BASE-NOW=" + (git merge-base origin/main HEAD); $over = 0; foreach ($p in $f) { $n = @(Get-Content -LiteralPath $p).Count; "SIZE| " + $p + " | " + $n; if ($n -gt 500) { $over++ } }; "OVER-500=" + $over' (run from the item worktree root)
EXIT_CODE: 0
Output Summary:
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c
- SIZE| lines: 21
- OVER-500=0 (largest: tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 at 494)

```text
SIZE| .github/workflows/_hygiene.yml | 24
SIZE| scripts/hygiene/Test-RepositoryHygiene.Git.ps1 | 161
SIZE| scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | 123
SIZE| scripts/hygiene/Test-RepositoryHygiene.ps1 | 87
SIZE| tests/scripts/hygiene/Test-RepositoryHygiene.Git.Tests.ps1 | 90
SIZE| tests/scripts/hygiene/Test-RepositoryHygiene.Rules.Tests.ps1 | 222
SIZE| tests/scripts/hygiene/Test-RepositoryHygiene.Tests.ps1 | 159
SIZE| tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1 | 494
SIZE| QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs | 318
SIZE| TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs | 161
SIZE| TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs | 195
SIZE| TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs | 128
SIZE| ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs | 284
SIZE| UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs | 473
SIZE| UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs | 101
SIZE| UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs | 305
SIZE| UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs | 335
SIZE| UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs | 335
SIZE| UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs | 486
SIZE| UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs | 369
SIZE| UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs | 431
```
