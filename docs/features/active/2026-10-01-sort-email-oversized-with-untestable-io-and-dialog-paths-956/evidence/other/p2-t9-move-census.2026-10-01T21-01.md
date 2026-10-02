# P2-T9 Verbatim move census over the six SortEmail files (split state)

Timestamp: 2026-10-01T21-01
Command: CMD-MOVE-CENSUS with STATE split and ONLY ALL; then CMD-CENSUS with PATHS-SIX and TOKENS-SORTEMAIL (two pwsh -NoProfile -Command invocations, each beginning Set-Location to the item worktree; paths below are repository-relative; the directory prefix `UtilitiesCS\EmailIntelligence\EmailParsingSorting\` is abbreviated as `EPS\` in the token lines only)
EXIT_CODE: 0
Output Summary:

CMD-MOVE-CENSUS (STATE split, ONLY ALL):
FILE-EXACT SortEmail.cs = True
HEADER-PREFIX SortEmail.cs = True
CLOSING-BRACES SortEmail.cs = True
FIRST-LINE SortEmail.cs = True
LINES SortEmail.cs = 277
FILE-EXACT SortEmail.MailItemSort.cs = True
HEADER-PREFIX SortEmail.MailItemSort.cs = True
CLOSING-BRACES SortEmail.MailItemSort.cs = True
FIRST-LINE SortEmail.MailItemSort.cs = True
LINES SortEmail.MailItemSort.cs = 388
FILE-EXACT SortEmail.AttachmentSaving.cs = True
HEADER-PREFIX SortEmail.AttachmentSaving.cs = True
CLOSING-BRACES SortEmail.AttachmentSaving.cs = True
FIRST-LINE SortEmail.AttachmentSaving.cs = True
LINES SortEmail.AttachmentSaving.cs = 342
FILE-EXACT SortEmail.TrySaveAttachment.cs = True
HEADER-PREFIX SortEmail.TrySaveAttachment.cs = True
CLOSING-BRACES SortEmail.TrySaveAttachment.cs = True
FIRST-LINE SortEmail.TrySaveAttachment.cs = True
LINES SortEmail.TrySaveAttachment.cs = 125
FILE-EXACT SortEmail.LegacyAttachmentSaving.cs = True
HEADER-PREFIX SortEmail.LegacyAttachmentSaving.cs = True
CLOSING-BRACES SortEmail.LegacyAttachmentSaving.cs = True
FIRST-LINE SortEmail.LegacyAttachmentSaving.cs = True
LINES SortEmail.LegacyAttachmentSaving.cs = 240
FILE-EXACT SortEmail.UndoAndMoveLog.cs = True
HEADER-PREFIX SortEmail.UndoAndMoveLog.cs = True
CLOSING-BRACES SortEmail.UndoAndMoveLog.cs = True
FIRST-LINE SortEmail.UndoAndMoveLog.cs = True
LINES SortEmail.UndoAndMoveLog.cs = 195
SEG S01 TOTAL=1 IN=SortEmail.cs=1
SEG S02 TOTAL=1 IN=SortEmail.cs=1
SEG S03 TOTAL=1 IN=SortEmail.MailItemSort.cs=1
SEG S04 TOTAL=1 IN=SortEmail.MailItemSort.cs=1
SEG S05 TOTAL=1 IN=SortEmail.cs=1
SEG S06 TOTAL=1 IN=SortEmail.cs=1
SEG S07 TOTAL=1 IN=SortEmail.cs=1
SEG S08 TOTAL=1 IN=SortEmail.MailItemSort.cs=1
SEG S09 TOTAL=1 IN=SortEmail.MailItemSort.cs=1
SEG S10 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S11 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S12 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S13 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S14 TOTAL=1 IN=SortEmail.LegacyAttachmentSaving.cs=1
SEG S15 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S16 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S17 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S18 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S19 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S20 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S21 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S22 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S23 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S24 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S25 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S26 TOTAL=1 IN=SortEmail.MailItemSort.cs=1
SEG S27 TOTAL=1 IN=SortEmail.cs=1
SEG S28 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S29 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S30 TOTAL=1 IN=SortEmail.LegacyAttachmentSaving.cs=1
SEG S31 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S32 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S33 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S34 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S35 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S36 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S10P TOTAL=0 IN=

CMD-CENSUS (PATHS-SIX, TOKENS-SORTEMAIL), per file in the order SortEmail.cs, SortEmail.MailItemSort.cs, SortEmail.AttachmentSaving.cs, SortEmail.TrySaveAttachment.cs, SortEmail.LegacyAttachmentSaving.cs, SortEmail.UndoAndMoveLog.cs:
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.cs = 4
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.MailItemSort.cs = 5
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.AttachmentSaving.cs = 10
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.TrySaveAttachment.cs = 2
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.LegacyAttachmentSaving.cs = 1
TOKEN [ExcludeFromCodeCoverage] @ EPS\SortEmail.UndoAndMoveLog.cs = 6
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 28
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.cs = 0
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.AttachmentSaving.cs = 6
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAll.ShowDialog( @ TOTAL = 9
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.cs = 0
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.AttachmentSaving.cs = 6
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 9
TOKEN _removeReadOnly @ EPS\SortEmail.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN _removeReadOnly @ EPS\SortEmail.TrySaveAttachment.cs = 12
TOKEN _removeReadOnly @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN _removeReadOnly @ TOTAL = 13
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.AttachmentSaving.cs = 3
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.TrySaveAttachment.cs = 4
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN TrySaveAttachmentAsync( @ TOTAL = 7
TOKEN File.Delete( @ EPS\SortEmail.cs = 1
TOKEN File.Delete( @ EPS\SortEmail.MailItemSort.cs = 2
TOKEN File.Delete( @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN File.Delete( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN File.Delete( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN File.Delete( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN File.Delete( @ TOTAL = 5
TOKEN File.Exists( @ EPS\SortEmail.cs = 0
TOKEN File.Exists( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN File.Exists( @ EPS\SortEmail.AttachmentSaving.cs = 2
TOKEN File.Exists( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN File.Exists( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 3
TOKEN File.Exists( @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN File.Exists( @ TOTAL = 6
TOKEN File. @ EPS\SortEmail.cs = 1
TOKEN File. @ EPS\SortEmail.MailItemSort.cs = 2
TOKEN File. @ EPS\SortEmail.AttachmentSaving.cs = 2
TOKEN File. @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN File. @ EPS\SortEmail.LegacyAttachmentSaving.cs = 5
TOKEN File. @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN File. @ TOTAL = 11
TOKEN Directory. @ EPS\SortEmail.cs = 0
TOKEN Directory. @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN Directory. @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN Directory. @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN Directory. @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN Directory. @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN Directory. @ TOTAL = 1
TOKEN DirectoryInfo @ EPS\SortEmail.cs = 0
TOKEN DirectoryInfo @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN DirectoryInfo @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN DirectoryInfo @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN DirectoryInfo @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN DirectoryInfo @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN DirectoryInfo @ TOTAL = 1
TOKEN FileAttributes @ EPS\SortEmail.cs = 0
TOKEN FileAttributes @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN FileAttributes @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN FileAttributes @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN FileAttributes @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN FileAttributes @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN FileAttributes @ TOTAL = 1
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.cs = 0
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN FileIO2.WriteTextFile( @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN FileIO2.WriteTextFile( @ TOTAL = 1
TOKEN newFileInfo( @ EPS\SortEmail.cs = 0
TOKEN newFileInfo( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN newFileInfo( @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN newFileInfo( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN newFileInfo( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN newFileInfo( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN newFileInfo( @ TOTAL = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.cs = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN publicstaticclassSortEmail @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN publicstaticclassSortEmail @ TOTAL = 0
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.cs = 1
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.MailItemSort.cs = 1
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.LegacyAttachmentSaving.cs = 1
TOKEN publicstaticpartialclassSortEmail @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN publicstaticpartialclassSortEmail @ TOTAL = 6
TOKEN #region @ EPS\SortEmail.cs = 0
TOKEN #region @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN #region @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN #region @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN #region @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN #region @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN #region @ TOTAL = 2
TOKEN #endregion @ EPS\SortEmail.cs = 0
TOKEN #endregion @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN #endregion @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN #endregion @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN #endregion @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN #endregion @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN #endregion @ TOTAL = 2
TOKEN #nullableenable @ EPS\SortEmail.cs = 1
TOKEN #nullableenable @ EPS\SortEmail.MailItemSort.cs = 1
TOKEN #nullableenable @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN #nullableenable @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN #nullableenable @ EPS\SortEmail.LegacyAttachmentSaving.cs = 1
TOKEN #nullableenable @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN #nullableenable @ TOTAL = 6
TOKEN staticAction< @ EPS\SortEmail.cs = 0
TOKEN staticAction< @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN staticAction< @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN staticAction< @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN staticAction< @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN staticAction< @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN staticAction< @ TOTAL = 0
TOKEN staticFunc< @ EPS\SortEmail.cs = 0
TOKEN staticFunc< @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN staticFunc< @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN staticFunc< @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN staticFunc< @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN staticFunc< @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN staticFunc< @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN RemoveReadOnlyPrompt @ TOTAL = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAllPromptSession @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ TOTAL = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.cs = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ TOTAL = 1
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.cs = 0
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ TOTAL = 1
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.cs = 0
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ EPS\SortEmail.UndoAndMoveLog.cs = 1
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ TOTAL = 1
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.cs = 0
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN catch(System.UnauthorizedAccessException @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN catch(System.UnauthorizedAccessException @ TOTAL = 1
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.cs = 0
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 1
TOKEN internalstaticvoidSaveAttachmentsOld( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN internalstaticvoidSaveAttachmentsOld( @ TOTAL = 1
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.cs = 0
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN internalstaticboolIsPicture( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN internalstaticboolIsPicture( @ TOTAL = 1
LINES EPS\SortEmail.cs = 277
SHA256 EPS\SortEmail.cs = D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61
LINES EPS\SortEmail.MailItemSort.cs = 388
SHA256 EPS\SortEmail.MailItemSort.cs = 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C
LINES EPS\SortEmail.AttachmentSaving.cs = 342
SHA256 EPS\SortEmail.AttachmentSaving.cs = 03928C9F5C2102BEEADBCC703FB66D84C19EDB013CFE459A5B6C1C42714050B6
LINES EPS\SortEmail.TrySaveAttachment.cs = 125
SHA256 EPS\SortEmail.TrySaveAttachment.cs = 23050A30E8F66A7B18E73CEB9774D7969781EFA4AA89D6CD0E8AE976A0BCFEA1
LINES EPS\SortEmail.LegacyAttachmentSaving.cs = 240
SHA256 EPS\SortEmail.LegacyAttachmentSaving.cs = 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
LINES EPS\SortEmail.UndoAndMoveLog.cs = 195
SHA256 EPS\SortEmail.UndoAndMoveLog.cs = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634

Comparison with the SPLIT columns of TOKENS-SORTEMAIL: rows 1 to 29 match per file and in TOTAL (28; AS 6, TS 1, LAS 2 = 9; 9; AS 1, TS 12 = 13; AS 3, TS 4 = 7; SE 1, MIS 2, LAS 2 = 5; AS 2, LAS 3, UML 1 = 6; SE 1, MIS 2, AS 2, LAS 5, UML 1 = 11; TS 1; TS 1; TS 1; UML 1; 0; 0; 6; LAS 2; LAS 2; 6; 0; 0; 0; 0; 0; AS 1; AS 1; UML 1; TS 1; LAS 1; AS 1).
Acceptance: FILE-EXACT, HEADER-PREFIX, CLOSING-BRACES and FIRST-LINE True for all six; SEG S01 to S36 each TOTAL=1 in the Segment Table destination and SEG S10P TOTAL=0; every TOKENS-SORTEMAIL per-file count and TOTAL equals the SPLIT columns; every LINES value at most 499 (largest 388) (all hold).
