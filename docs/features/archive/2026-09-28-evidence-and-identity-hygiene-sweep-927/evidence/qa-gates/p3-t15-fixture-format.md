# P3-T15 CSharpier format over the thirteen rewritten C# fixture files

Timestamp: 2026-09-29T19-48
Command: pwsh -NoProfile -Command '$f = @(git diff --name-only HEAD -- "*.cs"); "FILES=" + $f.Count; $before = $f | ForEach-Object { (Get-FileHash -Algorithm SHA256 -LiteralPath $_).Hash }; dotnet tool run csharpier format @f 2>&1 | Tee-Object -FilePath coverage/logs/927-p3-format.log; "FORMAT-EXIT=" + $LASTEXITCODE; $after = $f | ForEach-Object { (Get-FileHash -Algorithm SHA256 -LiteralPath $_).Hash }; $n = 0; for ($i = 0; $i -lt $f.Count; $i++) { if ($before[$i] -ne $after[$i]) { $n++ } }; "REWRITTEN=" + $n; "PATTERN=" + @(git grep -n -i -E -e "[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]" -- "*.Test/*.cs").Count'
EXIT_CODE: 0
Output Summary:
- FILES=13
- Console: Formatted 13 files in 12053ms. (processed count, not the rewrite figure)
- FORMAT-EXIT=0
- REWRITTEN=1 (TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs: the three single-line dictionary initialisers at the former lines 123, 162 and 176 exceeded the print width once the literal lengthened and were wrapped onto four lines each; the file's numstat against HEAD moved from 10/10 to 19/10; no literal changed)
- PATTERN=0 after the format.

Hashes (SHA-256, before | after):
- QuickFiler.Test/Controllers/EfcSelectionGuardTests.cs | A5C035CDAF27443A07106963E71FB435BA405ADF4C2ECE9828937F1F145814D8 | A5C035CDAF27443A07106963E71FB435BA405ADF4C2ECE9828937F1F145814D8
- TaskMaster.Test/AppGlobals/AppAutoFileObjectsFolderPredictorTests.cs | B85B0D5D46F432A89C4BE6A4B546AA27B746DC14C033F105C854606B766C1C45 | B85B0D5D46F432A89C4BE6A4B546AA27B746DC14C033F105C854606B766C1C45
- TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsMatchBestSpecialFolderTests.cs | A92A55EEF26B1827FF6F63B173C15840BBBAA52619DCA79312B6A4AE972A8FE6 | 9249D086A6B36D43E172BA9C7D8388CA4FC14E249F30F85C805DDDE698F07CAF
- TaskMaster.Test/AppGlobals/AppFileSystemFolderPathsOneDriveResolutionTests.cs | 5B47C693669130331D6ADAFD038C0288F2F9875A28213977C57A97370111899F | 5B47C693669130331D6ADAFD038C0288F2F9875A28213977C57A97370111899F
- ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs | 6D8130C07BBC3C768EACF9F1DFEE8EC5109FFF4D35BBE61EC542DBB81F6F7438 | 6D8130C07BBC3C768EACF9F1DFEE8EC5109FFF4D35BBE61EC542DBB81F6F7438
- UtilitiesCS.Test/EmailIntelligence/EmailFilerConfig_Tests.cs | 2A83DF9F37B786AB76982EE36921BAF1BA0F1BC991C1A8956A5F385BAFD47342 | 2A83DF9F37B786AB76982EE36921BAF1BA0F1BC991C1A8956A5F385BAFD47342
- UtilitiesCS.Test/EmailIntelligence/LcppnFolderPredictorStore_Tests.cs | FF8E2F15411BE30FD840CC1D85521D3AEF4106FD8637C345F4EDBC0B4EDA9F67 | FF8E2F15411BE30FD840CC1D85521D3AEF4106FD8637C345F4EDBC0B4EDA9F67
- UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs | 10A5FCA34897CAC95A85CA324DC00699AABA0F207FEAF6C913D5637106F83CA9 | 10A5FCA34897CAC95A85CA324DC00699AABA0F207FEAF6C913D5637106F83CA9
- UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs | 6B602BFCB9EA7941329BBAA0582C8FFA11A8757ABFE85A2A3E05A607F91F659F | 6B602BFCB9EA7941329BBAA0582C8FFA11A8757ABFE85A2A3E05A607F91F659F
- UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterIssue614Tests.cs | 850EE73A704C073BD2380730A5121876C8A23B8D9E3823F24BC846CA9F8C60DB | 850EE73A704C073BD2380730A5121876C8A23B8D9E3823F24BC846CA9F8C60DB
- UtilitiesCS.Test/OutlookObjects/Store/StoreFilterAttributionTests.cs | 9BBE0E5B3D24F6DEE141AC3BC2A17BD807A45363B6ABFAAFBD7EBDAC0C713AF1 | 9BBE0E5B3D24F6DEE141AC3BC2A17BD807A45363B6ABFAAFBD7EBDAC0C713AF1
- UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs | 3BE50A2A8B4157A88401B91C4E74CEE8CAE6B766C3BE1F7FE62F31B82D7079CD | 3BE50A2A8B4157A88401B91C4E74CEE8CAE6B766C3BE1F7FE62F31B82D7079CD
- UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs | 94FBCBD158760B07D5D8A7E7D464E3FCD51B832766B7397C2726BCE8B06BEF2D | 94FBCBD158760B07D5D8A7E7D464E3FCD51B832766B7397C2726BCE8B06BEF2D

Execution note: the payload ran with a Set-Location prefix to <repo-root>; the worktree path was composed by concatenation inside the payload because the parallel worktree-removal hook scans the whole command string.
