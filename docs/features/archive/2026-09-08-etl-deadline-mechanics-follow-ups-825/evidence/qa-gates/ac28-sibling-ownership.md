# AC28 — No Sibling-Owned File Appears in This Feature's Diff

Timestamp: 2026-09-09T17-27

Command: git add --intent-to-add -- . ":(exclude).claude"
EXIT_CODE: 0

Command: git diff --name-only $b -- . ":(exclude).claude", with $b re-derived from evidence/baseline/base-commit.md per D3
EXIT_CODE: 0

ReportedPaths: 54
SiblingOwnedMatches: 0

Output Summary: The intent-to-add span ran before the name-listing diff, so a sibling-owned file
created but not yet tracked could not escape the listing. The diff reports 54 paths: the 11 Write Set
paths and 43 paths under this feature's own folder. None matches any entry on the sibling-owned list.

## The ten-entry exclusion list, checked against the reported paths

| Sibling-owned entry | Owning issue | Matches |
| --- | --- | --- |
| QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 813 | 0 |
| QuickFiler/Controllers/QfcHomeController.cs | 821 | 0 |
| UtilitiesCS/Threading/ProgressViewer.cs | 821 | 0 |
| UtilitiesCS/OutlookObjects/Store/StoreWrapperController | 823 | 0 |
| QuickFiler/Viewers/Breadcrumb | 823 | 0 |
| UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ | 824 | 0 |
| UtilitiesCS.Test/Properties/AssemblyInfo.cs | 824 | 0 |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs | 817 | 0 |
| .editorconfig | 826 | 0 |
| BannedSymbols.txt | 826 | 0 |

Four of the ten entries are prefixes rather than full paths, matching a family of files, and each was
checked as a substring so that every member of the family is covered.

## The eleven non-documentation paths

Every non-documentation path the diff reports is one of the eleven Write Set paths in spec.md, and
all eleven appear:

- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs
- UtilitiesCS/Threading/TimeOutTask.cs
- UtilitiesCS/Extensions/DfDeedle.cs
- UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
- UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs
- UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs
- UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs
- UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs
- UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
- UtilitiesCS.Test/UtilitiesCS.Test.csproj

UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs is absent, as AC6 and AC20 require, and
UtilitiesCS/UtilitiesCS.csproj is absent, which is correct because this feature adds no new
production source file and the legacy project's explicit compile-item list therefore needs no new
entry.
