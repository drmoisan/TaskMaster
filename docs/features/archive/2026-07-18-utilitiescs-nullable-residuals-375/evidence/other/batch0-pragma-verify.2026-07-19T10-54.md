# Batch 0 Pragma Verification (P1-T10)

Timestamp: 2026-07-19T10-54

Batch 0 opted-in files (9):
1. UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs
2. UtilitiesCS/EmailIntelligence/Evaluation/EvaluationResult.cs
3. UtilitiesCS/OutlookObjects/Fields/MAPIFields.cs
4. UtilitiesCS/EmailIntelligence/FolderConverter.cs
5. UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/IFilterOlFoldersViewer.cs
6. UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/IFolderRemapViewer.cs
7. UtilitiesCS/EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs
8. UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/OSFolder.cs
9. UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings
(CS0618/CS0168). All nine Batch 0 files were verify-only clean under `#nullable enable`: no
annotations were required (empty/DTO/interface/static-const/pure-logic files; SmithWaterman's
reflection calls resolve against the nullable-oblivious net481 BCL; DASLFilterParser consumes the
oblivious `ReusableTypeClasses.TreeNode<string>.Value`, so `CombineTree` returns it with no CS8603).
No new runtime guard was added.

## Note on the mandated full-solution command

The plan-of-record command `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`
cannot produce an informative per-batch CS86xx signal: `UtilitiesCS.csproj` has a `<ProjectReference>`
to `SVGControl.csproj`, whose pre-existing out-of-scope CS0649 (owned by #368) fails first under TWAE
and blocks UtilitiesCS from compiling at all. The isolated UtilitiesCS build above is therefore the
trustworthy per-file CS86xx gate (it compiles UtilitiesCS with the pragmas and excludes only the
three pre-existing NON-nullable warning classes CS0649/CS0618/CS0168; no CS86xx is ever excluded, and
no `/p:Nullable=enable` is used). The mandated full-solution command is recorded once at final QC
(P12-T3), confirming the only full-solution blocker is the out-of-scope SVGControl CS0649 and that no
CS86xx is emitted.
