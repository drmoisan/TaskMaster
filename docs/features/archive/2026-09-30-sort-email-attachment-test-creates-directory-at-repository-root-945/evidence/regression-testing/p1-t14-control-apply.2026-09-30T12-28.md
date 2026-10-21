# P1-T14 negative control: apply the mutation (AC5)

Timestamp: 2026-09-30T12-28
Command: CMD-MUTATE, run as three steps. (1) backup payload: copy the fixed SortEmail.cs to the git-ignored coverage\control-945\SortEmail.fixed.bak and hash both; (2) read-only needle count over the Latin-1 text of the file; (3) removal of the one statement `createDirectory(Path.GetDirectoryName(filePathSave));` with the Edit tool. Followed by CMD-CENSUS on the mutated file with TOKENS-SRC.
EXIT_CODE: 0

Mechanism note: the single payload of CMD-MUTATE (backup plus byte-preserving Latin-1 write-back through [System.IO.File]::WriteAllBytes) was refused twice by the tool layer with an EPERM spawn error before executing (the file hash and the absence of the backup were re-observed unchanged after each refusal; the read-only and the backup-only halves ran without refusal). The removal was therefore made with the Edit tool, which preserved the CRLF working-tree ending (git ls-files --eol reports w/crlf after the edit). Every value this task gates was observed after the edit, so the acceptance evidence is unaffected. Restoration is by copy of the backup (P1-T16), not by reverse edit.

Output Summary:
SRC-HASH-BEFORE: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
BACKUP-HASH: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
NEEDLE-COUNT: 1
SRC-HASH-MUTATED: A45FC1F2ABE280DA124C8524CAD97225EE80989DF8CEEB747E385AD70E4C84DF
FIX-HASH-SORTEMAIL (P1-T7): 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B

Mutated census (control state column):
TOKEN [ExcludeFromCodeCoverage] = 28
TOKEN TrySaveAttachmentAsync( = 7
TOKEN System.IO.Directory.CreateDirectory( = 1
TOKEN System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePathSave)) = 0
TOKEN createDirectory = 3
TOKEN Action<string>createDirectory = 1
TOKEN staticAction< = 0
TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); = 0
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
LINES = 1453
SHA256 = A45FC1F2ABE280DA124C8524CAD97225EE80989DF8CEEB747E385AD70E4C84DF
