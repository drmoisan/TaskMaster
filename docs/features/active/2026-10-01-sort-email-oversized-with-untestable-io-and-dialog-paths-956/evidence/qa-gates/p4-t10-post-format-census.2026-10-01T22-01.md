# P4-T10 Post-format census

Timestamp: 2026-10-01T22-01
ITERATION: 1
Command: CMD-MOVE-CENSUS with STATE seam and ONLY ALL; CMD-CENSUS with PATHS-SIX and TOKENS-SORTEMAIL; with PATHS-TRYSAVE and TOKENS-TRYSAVE; with PATHS-SESSION and TOKENS-SESSION; with PATHS-TEST-T and TOKENS-TEST-T; with PATHS-TEST-S and TOKENS-TEST-S; CMD-CSPROJ (seven pwsh -NoProfile -Command invocations, each beginning Set-Location to the item worktree, payloads exactly as in the Command Reference); plus a Glob of the pattern `**/SortEmail*.cs` rooted at UtilitiesCS/EmailIntelligence/EmailParsingSorting
EXIT_CODE: 0
Output Summary:
Representation notes (no value changed): the directory prefix `UtilitiesCS\EmailIntelligence\EmailParsingSorting\` printed by the six-file census is abbreviated `EPS\` below; for the four single-file censuses the per-file line equals the `@ TOTAL` line and only the TOTAL line is reproduced; the TOKENS-TRYSAVE lines are labelled by their table ID (A1 to A24, payload order) instead of the printed literal. Every invocation exited 0. The values are identical to the P3-T6 post-edit census (FEATURE/evidence/other/p3-t6-post-edit-census.2026-10-01T21-08.md), including every SHA-256.

CMD-MOVE-CENSUS (STATE seam, ONLY ALL):
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
FILE-EXACT SortEmail.TrySaveAttachment.cs = NOT-APPLICABLE
HEADER-PREFIX SortEmail.TrySaveAttachment.cs = True
CLOSING-BRACES SortEmail.TrySaveAttachment.cs = True
FIRST-LINE SortEmail.TrySaveAttachment.cs = True
LINES SortEmail.TrySaveAttachment.cs = 172
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
SEG S10 TOTAL=0 IN=
SEG S11 TOTAL=1 IN=SortEmail.UndoAndMoveLog.cs=1
SEG S12 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S13 TOTAL=0 IN=
SEG S14 TOTAL=1 IN=SortEmail.LegacyAttachmentSaving.cs=1
SEG S15 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S16 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S17 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S18 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S19 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S20 TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1
SEG S21 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S22 TOTAL=1 IN=SortEmail.TrySaveAttachment.cs=1
SEG S23 TOTAL=0 IN=
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
SEG S10P TOTAL=1 IN=SortEmail.AttachmentSaving.cs=1

CMD-CENSUS (PATHS-SIX, TOKENS-SORTEMAIL):
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
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN YesNoToAll.ShowDialog( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAll.ShowDialog( @ TOTAL = 8
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.cs = 0
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.AttachmentSaving.cs = 6
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.TrySaveAttachment.cs = 1
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.LegacyAttachmentSaving.cs = 2
TOKEN YesNoToAll.ShowDialog @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 9
TOKEN _removeReadOnly @ EPS\SortEmail.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN _removeReadOnly @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN _removeReadOnly @ TOTAL = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.AttachmentSaving.cs = 3
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.TrySaveAttachment.cs = 6
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN TrySaveAttachmentAsync( @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN TrySaveAttachmentAsync( @ TOTAL = 9
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
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.TrySaveAttachment.cs = 2
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN RemoveReadOnlyPrompt @ TOTAL = 3
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.AttachmentSaving.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.TrySaveAttachment.cs = 2
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN YesNoToAllPromptSession @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN YesNoToAllPromptSession @ TOTAL = 2
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.MailItemSort.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.AttachmentSaving.cs = 1
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.TrySaveAttachment.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.LegacyAttachmentSaving.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ EPS\SortEmail.UndoAndMoveLog.cs = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ TOTAL = 1
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
SHA256 EPS\SortEmail.AttachmentSaving.cs = A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B
LINES EPS\SortEmail.TrySaveAttachment.cs = 172
SHA256 EPS\SortEmail.TrySaveAttachment.cs = B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E
LINES EPS\SortEmail.LegacyAttachmentSaving.cs = 240
SHA256 EPS\SortEmail.LegacyAttachmentSaving.cs = 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
LINES EPS\SortEmail.UndoAndMoveLog.cs = 195
SHA256 EPS\SortEmail.UndoAndMoveLog.cs = E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634

CMD-CENSUS (PATHS-TRYSAVE, TOKENS-TRYSAVE; IDs in payload order):
TOKEN A1 @ TOTAL = 1
TOKEN A2 @ TOTAL = 1
TOKEN A3 @ TOTAL = 1
TOKEN A4 @ TOTAL = 1
TOKEN A5 @ TOTAL = 1
TOKEN A6 @ TOTAL = 1
TOKEN A7 @ TOTAL = 1
TOKEN A8 @ TOTAL = 1
TOKEN A9 @ TOTAL = 1
TOKEN A10 @ TOTAL = 1
TOKEN A11 @ TOTAL = 1
TOKEN A12 @ TOTAL = 1
TOKEN A13 @ TOTAL = 1
TOKEN A14 @ TOTAL = 1
TOKEN A15 @ TOTAL = 1
TOKEN A16 @ TOTAL = 3
TOKEN A17 @ TOTAL = 2
TOKEN A18 @ TOTAL = 1
TOKEN A19 @ TOTAL = 0
TOKEN A20 @ TOTAL = 2
TOKEN A21 @ TOTAL = 2
TOKEN A22 @ TOTAL = 1
TOKEN A23 @ TOTAL = 3
TOKEN A24 @ TOTAL = 0
LINES EPS\SortEmail.TrySaveAttachment.cs = 172
SHA256 EPS\SortEmail.TrySaveAttachment.cs = B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E

CMD-CENSUS (PATHS-SESSION, TOKENS-SESSION):
TOKEN #nullableenable @ TOTAL = 1
TOKEN namespaceUtilitiesCS{ @ TOTAL = 1
TOKEN internalsealedclassYesNoToAllPromptSession @ TOTAL = 1
TOKEN privatereadonlyFunc<string,YesNoToAllResponse>_showDialog; @ TOTAL = 1
TOKEN internalYesNoToAllPromptSession(Func<string,YesNoToAllResponse>showDialog) @ TOTAL = 1
TOKEN _showDialog=showDialog??thrownewArgumentNullException(nameof(showDialog)); @ TOTAL = 1
TOKEN internalYesNoToAllResponseResponse{get;privateset;} @ TOTAL = 1
TOKEN internalYesNoToAllResponseAsk(stringmessage){if(Response==YesNoToAllResponse.Empty){Response=_showDialog(message);}returnResponse;} @ TOTAL = 1
TOKEN internalvoidReleaseSingleAnswer(){if(Response==YesNoToAllResponse.Yes||Response==YesNoToAllResponse.No){Response=YesNoToAllResponse.Empty;}} @ TOTAL = 1
TOKEN internalvoidReset(){Response=YesNoToAllResponse.Empty;} @ TOTAL = 1
TOKEN static @ TOTAL = 0
TOKEN ShowDialog @ TOTAL = 0
TOKEN ExcludeFromCodeCoverage @ TOTAL = 0
LINES UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs = 70
SHA256 UtilitiesCS\Dialogs\YesNoToAllPromptSession.cs = 2F18C839968EB3752FBB4A15289A991109ED9DAD6358CF0D5790FA75D797F565

CMD-CENSUS (PATHS-TEST-T, TOKENS-TEST-T):
TOKEN [TestMethod] @ TOTAL = 11
TOKEN [TestClass] @ TOTAL = 1
TOKEN namespaceUtilitiesCS.Test.EmailIntelligence{ @ TOTAL = 1
TOKEN publicclassSortEmail_TrySaveAttachment_Tests @ TOTAL = 1
TOKEN C:\Sortemail956Sandbox\attachments @ TOTAL = 3
TOKEN TrySaveAttachmentAsync( @ TOTAL = 1
TOKEN SaveAsync(attachment,seams) @ TOTAL = 13
TOKEN newSeams( @ TOTAL = 11
TOKEN newSeams(YesNoToAllResponse.YesToAll) @ TOTAL = 3
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN .Throws(newUnauthorizedAccessException( @ TOTAL = 12
TOKEN .Throws(newIOException( @ TOTAL = 1
TOKEN ThrowAsync<UnauthorizedAccessException>() @ TOTAL = 1
TOKEN ThrowAsync<IOException>() @ TOTAL = 1
TOKEN seams.Session.Response.Should().Be( @ TOTAL = 11
TOKEN newMock<Attachment>(MockBehavior.Loose) @ TOTAL = 11
TOKEN System.Exception @ TOTAL = 1
TOKEN SortEmail. @ TOTAL = 0
TOKEN typeof( @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt @ TOTAL = 0
TOKEN Cleanup_Files @ TOTAL = 0
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN [DataRow @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN Directory. @ TOTAL = 0
TOKEN Path. @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 0
TOKEN GetTemp @ TOTAL = 0
TOKEN Xunit @ TOTAL = 0
TOKEN NUnit @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 375
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645

CMD-CENSUS (PATHS-TEST-S, TOKENS-TEST-S):
TOKEN [TestMethod] @ TOTAL = 7
TOKEN namespaceUtilitiesCS.Test.Dialogs{ @ TOTAL = 1
TOKEN publicclassYesNoToAllPromptSession_Tests @ TOTAL = 1
TOKEN newYesNoToAllPromptSession( @ TOTAL = 9
TOKEN WithParameterName( @ TOTAL = 1
TOKEN .ReleaseSingleAnswer(); @ TOTAL = 4
TOKEN .Reset(); @ TOTAL = 1
TOKEN [DataRow @ TOTAL = 0
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN SortEmail @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Xunit @ TOTAL = 0
TOKEN NUnit @ TOTAL = 0
LINES UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 180
SHA256 UtilitiesCS.Test\Dialogs\YesNoToAllPromptSession_Tests.cs = 0A0FBBD8F51CFEE972E60718EA619F9A0FFB1670FA5A76B7635FC74AC4F40C8B

CMD-CSPROJ:
UCS Dialogs\NotImplementedDialog.cs COUNT=1 LINE=573
UCS Dialogs\YesNoToAll.cs COUNT=1 LINE=574
UCS Dialogs\YesNoToAllPromptSession.cs COUNT=1 LINE=575
UCS EmailIntelligence\Bayesian\Obsolete\BayesianClassifier.cs COUNT=1 LINE=576
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=818
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=819
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=1 LINE=820
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=821
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=822
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=823
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=824
UCT EmailIntelligence\Triage_OlLogic_Tests.cs COUNT=1 LINE=97
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=100
UCT Dialogs\YesNoToAll_Test.cs COUNT=1 LINE=442
UCT Dialogs\YesNoToAll_Tests.cs COUNT=1 LINE=443
UCT Dialogs\YesNoToAllPromptSession_Tests.cs COUNT=1 LINE=444
UCT ReusableTypeClasses\AsyncLazy_Tests.cs COUNT=1 LINE=445

Glob `**/SortEmail*.cs` rooted at UtilitiesCS/EmailIntelligence/EmailParsingSorting (file names only):
SORTEMAIL-FILES: SortEmail.cs, SortEmail.MailItemSort.cs, SortEmail.LegacyAttachmentSaving.cs, SortEmail.UndoAndMoveLog.cs, SortEmail.AttachmentSaving.cs, SortEmail.TrySaveAttachment.cs

LINE-COUNTS:
SortEmail.cs = 277
SortEmail.MailItemSort.cs = 388
SortEmail.AttachmentSaving.cs = 342
SortEmail.TrySaveAttachment.cs = 172
SortEmail.LegacyAttachmentSaving.cs = 240
SortEmail.UndoAndMoveLog.cs = 195
YesNoToAllPromptSession.cs = 70
SortEmail_TrySaveAttachment_Tests.cs = 375
YesNoToAllPromptSession_Tests.cs = 180

Acceptance evaluation (P4-T10, all four required):
1. Every P3-T6 acceptance condition holds on the post-format state: seam-state CMD-MOVE-CENSUS expectations (five FILE-EXACT True, TrySave NOT-APPLICABLE, every HEADER-PREFIX, CLOSING-BRACES and FIRST-LINE True, S10, S13 and S23 TOTAL=0, S10P TOTAL=1 in SortEmail.AttachmentSaving.cs, every other segment TOTAL=1 in its destination); every TOKENS-SORTEMAIL per-file count and TOTAL equals the SEAM columns; TOKENS-TRYSAVE equals the SEAM column; TOKENS-SESSION (ten 1s, three 0s), TOKENS-TEST-T (11, 1, 1, 1, 3, 1, 13, 11, 3, 1, 12, 1, 1, 1, 11, 11, 1, fifteen 0s) and TOKENS-TEST-S (7, 1, 1, 9, 1, 4, 1, nine 0s) equal their expectations; every UCS and UCT line matches the final column; every LINES value at most 499: HOLDS.
2. `SORTEMAIL-FILES:` lists exactly SortEmail.cs, SortEmail.AttachmentSaving.cs, SortEmail.LegacyAttachmentSaving.cs, SortEmail.MailItemSort.cs, SortEmail.TrySaveAttachment.cs and SortEmail.UndoAndMoveLog.cs (six files, order as returned by the Glob tool): HOLDS.
3. Every LINES value of the nine files is at most 499 (largest 388), recorded in the LINE-COUNTS block: HOLDS.
4. The SortEmail.TrySaveAttachment.cs SHA-256 B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E equals `RESTORED-HASH-TRYSAVE:` of P3-T12 (FEATURE/evidence/regression-testing/p3-t12-control-restored.2026-10-01T21-14.md); P4-T1 reported `WRITESET-CHANGED-COUNT: 0`: HOLDS.
