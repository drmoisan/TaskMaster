# Repository-Wide CSharpier Format (P2-T1)

Timestamp: 2026-09-30T08-08
Task: P2-T1
ITERATION: 1
Command: dotnet tool run csharpier format . (inside one payload that also took Get-FileHash -Algorithm SHA256 of the two Write Set files and the SHA-256 of the text printed by `git diff ANCHOR-SHA -- UtilitiesCS.Test` immediately before and after the command, ANCHOR-SHA substituted with 231e1c0b55105aeb626bf5a6e8d0266a567cacad, then ran `git status --porcelain -- UtilitiesCS UtilitiesCS.Test`)
EXIT_CODE: 0
Output Summary: formatter exit 0; neither Write Set file was rewritten and the anchored patch is unchanged; scoped porcelain empty. Fact 22 prediction `REWRITTEN: 0`: met.

- FORMATTER-LINE: `Formatted 1625 files in 8795ms.` (processed-file count, not a rewrite count)
- PFS-HASH-BEFORE (UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs, before command): C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998
- PFS-HASH-AFTER (UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs, after command): C88A785C23D8DB2960E9FAA53DF9EF91F6F00359683485BEC7EDFC44F9A2F998
- DIW-HASH-BEFORE (UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs, before command): 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910
- DIW-HASH-AFTER (UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs, after command): 6650B33204BABCAAB7CD6E97C8B4BA7012ABB1F1320DB6AF7764ECD8714B7910
- PATCH-HASH-BEFORE (SHA-256 of the UTF-8 text of `git diff ANCHOR-SHA -- UtilitiesCS.Test`, before command): 28A94D180B392D995EAFF412BBD3BAA9EF8F01B15003C4DF945698B3CF2963D9
- PATCH-HASH-AFTER (same, after command): 28A94D180B392D995EAFF412BBD3BAA9EF8F01B15003C4DF945698B3CF2963D9
- REWRITTEN: 0
- FORMAT_CHANGED_TREE: False
- PORCELAIN (`git status --porcelain -- UtilitiesCS UtilitiesCS.Test`, verbatim): EMPTY (no output)
- REWRITTEN-PREDICTION (fact 22): MET
