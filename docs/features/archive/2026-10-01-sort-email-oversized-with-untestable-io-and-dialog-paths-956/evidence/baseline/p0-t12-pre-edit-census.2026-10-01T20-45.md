# P0-T12 Pre-edit census of SortEmail.cs and merge-base backup

Timestamp: 2026-10-01T20-45
Command: CMD-CENSUS with PATHS-SRC ("UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs") and TOKENS-SORTEMAIL (29 tokens); then CMD-BACKUP-MERGEBASE (copies the file to coverage\control-956\SortEmail.mergebase.bak, git-ignored)
EXIT_CODE: 0
Output Summary:
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 28
TOKEN YesNoToAll.ShowDialog( @ TOTAL = 9
TOKEN YesNoToAll.ShowDialog @ TOTAL = 9
TOKEN _removeReadOnly @ TOTAL = 13
TOKEN TrySaveAttachmentAsync( @ TOTAL = 7
TOKEN File.Delete( @ TOTAL = 5
TOKEN File.Exists( @ TOTAL = 6
TOKEN File. @ TOTAL = 11
TOKEN Directory. @ TOTAL = 1
TOKEN DirectoryInfo @ TOTAL = 1
TOKEN FileAttributes @ TOTAL = 1
TOKEN FileIO2.WriteTextFile( @ TOTAL = 1
TOKEN newFileInfo( @ TOTAL = 0
TOKEN publicstaticclassSortEmail @ TOTAL = 1
TOKEN publicstaticpartialclassSortEmail @ TOTAL = 0
TOKEN #region @ TOTAL = 5
TOKEN #endregion @ TOTAL = 5
TOKEN #nullableenable @ TOTAL = 1
TOKEN staticAction< @ TOTAL = 0
TOKEN staticFunc< @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt @ TOTAL = 0
TOKEN YesNoToAllPromptSession @ TOTAL = 0
TOKEN RemoveReadOnlyPrompt.Reset(); @ TOTAL = 0
TOKEN case(YesNoToAllResponse.NoToAll|YesNoToAllResponse.No): @ TOTAL = 1
TOKEN case(YesNoToAllResponse.Yes|YesNoToAllResponse.YesToAll): @ TOTAL = 1
TOKEN File.Exists(Path.Combine(strFileName,strFileLocation)) @ TOTAL = 1
TOKEN catch(System.UnauthorizedAccessException @ TOTAL = 1
TOKEN internalstaticvoidSaveAttachmentsOld( @ TOTAL = 1
TOKEN internalstaticboolIsPicture( @ TOTAL = 1
(Each per-file count equals its TOTAL because PATHS-SRC holds one file.)
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 1454
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
SRC-HASH: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
BACKUP-HASH: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
BACKUP-LINES: 1454
PRE-EDIT-HASH-SRC (P0-T4): 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
Acceptance: every TOTAL equals the PRE column of TOKENS-SORTEMAIL; LINES = 1454; SRC-HASH, BACKUP-HASH and the census SHA256 equal PRE-EDIT-HASH-SRC; BACKUP-LINES 1454 (all hold).
