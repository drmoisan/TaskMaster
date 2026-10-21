# P6-T5 C# format pass (D12)

## iter1

Timestamp: 2026-09-29T22-17
Command: pwsh -NoProfile -Command '"MERGE-BASE-NOW=" + (git merge-base origin/main HEAD); $f = @(git diff --name-only origin/main...HEAD -- "*.cs"); "FILES=" + $f.Count; foreach ($p in $f) { "HASH| " + $p + " | " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash }' (before); CSHARPIER-FORMAT: pwsh -NoProfile -Command 'dotnet tool run csharpier format . 2>&1 | Tee-Object -FilePath coverage/logs/927-csharpier-format.log; exit $LASTEXITCODE'; the hash payload again (after); git status --porcelain -- "*.cs" "*.xml" "*.config". Each pwsh payload was prefixed only by a Set-Location to the item worktree root.
EXIT_CODE: 0
Output Summary:
- MERGE-BASE-NOW=ddbab26a0149bf2ca5d0256e60686ad79e74d90c (40 characters; recorded, not compared)
- FILES=13 (before and after)
- FORMAT exit code 0; "Formatted 1625 files in 8130ms." (processed count, not a rewrite count)
- REWRITTEN=0 (every one of the thirteen HASH| values is identical before and after)
- Porcelain span over "*.cs" "*.xml" "*.config": no line
- RESTORED-BASELINE-UNFORMATTED: none (P0-T11 BASELINE-UNFORMATTED: none)

HASH| lines (identical before and after):

```text
HASH| QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs | A5C035CDAF27443A07106963E71FB435BA405ADF4C2ECE9828937F1F145814D8
HASH| TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs | B85B0D5D46F432A89C4BE6A4B546AA27B746DC14C033F105C854606B766C1C45
HASH| TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs | 9249D086A6B36D43E172BA9C7D8388CA4FC14E249F30F85C805DDDE698F07CAF
HASH| TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs | 5B47C693669130331D6ADAFD038C0288F2F9875A28213977C57A97370111899F
HASH| ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs | 6D8130C07BBC3C768EACF9F1DFEE8EC5109FFF4D35BBE61EC542DBB81F6F7438
HASH| UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs | 2A83DF9F37B786AB76982EE36921BAF1BA0F1BC991C1A8956A5F385BAFD47342
HASH| UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs | FF8E2F15411BE30FD840CC1D85521D3AEF4106FD8637C345F4EDBC0B4EDA9F67
HASH| UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs | 10A5FCA34897CAC95A85CA324DC00699AABA0F207FEAF6C913D5637106F83CA9
HASH| UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs | 6B602BFCB9EA7941329BBAA0582C8FFA11A8757ABFE85A2A3E05A607F91F659F
HASH| UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs | 850EE73A704C073BD2380730A5121876C8A23B8D9E3823F24BC846CA9F8C60DB
HASH| UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs | 9BBE0E5B3D24F6DEE141AC3BC2A17BD807A45363B6ABFAAFBD7EBDAC0C713AF1
HASH| UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs | 3BE50A2A8B4157A88401B91C4E74CEE8CAE6B766C3BE1F7FE62F31B82D7079CD
HASH| UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs | 94FBCBD158760B07D5D8A7E7D464E3FCD51B832766B7397C2726BCE8B06BEF2D
```
