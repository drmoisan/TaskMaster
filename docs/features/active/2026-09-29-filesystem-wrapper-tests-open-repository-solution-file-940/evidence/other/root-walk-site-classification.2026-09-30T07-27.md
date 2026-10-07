# Root-Walk Site Classification (P1-T1, AC7)

Timestamp: 2026-09-30T07-27
Task: P1-T1
Command: Read tool over the four files; no command executed
EXIT_CODE: 0 (scoped to the read-only classification)
Output Summary: four sections over the three files AC7 names; RibbonControllerTests and FSharpCoreHintPathAlignmentTests classified `legitimate repository-file read`; the four string-only SortEmail_Tests consumers classified `layout dependence without file-system I/O`; the SortEmail_Tests `TrySaveAttachmentAsync` site classified `same defect class`. Every classification matches the plan's expected classification after the re-read; none was corrected. No file was modified; no potential entry and no issue was created by this task (D-11).

Ranges re-read: TaskMaster.Test/Ribbon/RibbonControllerTests.cs 296 to 311 and 422 to 437; TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs 11 to 107 with its consumers at 156 and 181; UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs 186 to 296 and 393 to 415; UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs 888 to 897. Supporting read (to confirm the string-only claim of section 3): the production methods `GetAttachmentsInfo` (SortEmail.cs 637 to 659), `SaveMessageAsMsgAsync` (1081 to 1089), `SaveMessageAsMSG` (1092 to 1098) and `AttachmentHelper.AdjustForMaxPath` (UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs 216 to 234).

## Section 1

- SITE: TaskMaster.Test/Ribbon/RibbonControllerTests.cs lines 296 to 311 (test `RibbonFolderOperations_DoNotConstructThrowawayFolderTrees`) and 422 to 437 (`FindRepositoryRoot()`)
- ROOT-USE: the located root is combined with TaskMaster/Ribbon/RibbonController.FolderTree.cs and read with `File.ReadAllText`, and the assertions check the checked-in source text for three forbidden substrings.
- CLASS: legitimate repository-file read
- RECOMMENDATION: out of scope for #940; no change
- MODIFIED: NONE

## Section 2

- SITE: TaskMaster.Test/Bootstrap/FSharpCoreHintPathAlignmentTests.cs lines 11 to 107 (class remarks 15 to 21, `FindRepositoryRoot()` 47 to 67, `DiscoverFSharpCoreHintPaths()` 83 onward) with consumers at 156 and 181
- ROOT-USE: the located root is the starting directory of a read-only enumeration of every project file (skipping dot-prefixed, packages, bin, obj and node_modules directories), whose FSharp.Core HintPath values are asserted, which the class remarks state is the design ("These assertions read the project files directly").
- CLASS: legitimate repository-file read
- RECOMMENDATION: out of scope for #940; no change
- MODIFIED: NONE

## Section 3

- SITE: UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs lines 186 to 296 (consumers at 196, 223, 255 and 280) and 393 to 415 (`GetRepositoryRoot()`)
- ROOT-USE: the located root's `FullName` is passed as a folder-path string to `GetAttachmentsInfo`, `GetAttachmentsInfoAsync`, `SaveMessageAsMsgAsync` and `SaveMessageAsMSG`, which combine strings on it; the only save calls reached are `SaveAs` on a strict Moq `MailItem` mock, so no file-system entry is read or written through the path.
- CLASS: layout dependence without file-system I/O
- RECOMMENDATION: out of scope for #940; no change
- MODIFIED: NONE

## Section 4

- SITE: UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs lines 237 to 249 (`TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile`, root use at 241) with UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs lines 888 to 897
- ROOT-USE: the located root is combined with `saved.txt` and passed to the production method `TrySaveAttachmentAsync`, which at SortEmail.cs line 896 calls `System.IO.Directory.CreateDirectory` on the directory part of that path, the repository root; the call is a real creation API that is a no-op only because the root exists (the method carries `ExcludeFromCodeCoverage` at line 888; `SaveAsFile` itself is a Moq mock).
- CLASS: same defect class
- RECOMMENDATION: out of scope for #940; follow-up handed to the orchestrator
- MODIFIED: NONE
