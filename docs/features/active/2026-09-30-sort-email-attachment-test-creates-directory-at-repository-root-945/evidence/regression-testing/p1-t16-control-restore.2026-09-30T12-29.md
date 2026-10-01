# P1-T16 restore of the production file after the control (AC5)

Timestamp: 2026-09-30T12-29
Command: CMD-RESTORE (copy the backup coverage\control-945\SortEmail.fixed.bak over UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs), then CMD-CENSUS on the file with TOKENS-SRC, then CMD-DIFFHASH with MERGE-BASE 039cf779110df3313b3324299d019cabfccce980
EXIT_CODE: 0

Output Summary:
SRC-HASH-RESTORED: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
FIX-HASH-SORTEMAIL (P1-T7): 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B (equal: byte-identical restore)
BACKUP-HASH-NOW: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
DIFF-LINES = 56 (FIX-DIFF-LINES: 56)
DIFF-SHA256 = 9B58FAA848DADF26DEAAFEC963DC05F1243CFAA0FDD4217914F6D66DC5BA1FE0 (FIX-DIFF-HASH-SORTEMAIL: 9B58FAA848DADF26DEAAFEC963DC05F1243CFAA0FDD4217914F6D66DC5BA1FE0, equal)

Census after restore (equals the `post (fixed state)` column):
TOKEN [ExcludeFromCodeCoverage] = 28
TOKEN TrySaveAttachmentAsync( = 7
TOKEN System.IO.Directory.CreateDirectory( = 1
TOKEN System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave)) = 0
TOKEN createDirectory = 4
TOKEN Action<string>createDirectory = 1
TOKEN staticAction< = 0
TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); = 1
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave); = 0
TOKEN returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory); = 1
TOKEN returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path)); = 1
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 0
TOKEN internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave) = 1
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory) = 1
TOKEN [ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync( = 1
TOKEN [ExcludeFromCodeCoverage]internalstaticasyncTask<bool>TrySaveAttachmentAsync( = 1
TOKEN awaitattachmentHelper.Attachment.TrySaveAttachmentAsync( = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSaveAlt); = 1
TOKEN awaitattachment.TrySaveAttachmentAsync(filePathSave); = 1
TOKEN YesNoToAll.ShowDialog( = 9
TOKEN catch(System.UnauthorizedAccessException = 1
LINES = 1454
SHA256 = 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B (equals SRC-HASH-RESTORED)
