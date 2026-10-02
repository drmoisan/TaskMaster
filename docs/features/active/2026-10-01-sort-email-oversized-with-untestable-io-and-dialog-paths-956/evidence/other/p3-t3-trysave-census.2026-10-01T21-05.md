# P3-T3 SortEmail.TrySaveAttachment.cs rewrite to Listing L-TRYSAVE and census

Timestamp: 2026-10-01T21-05
Command: Write tool rewrote UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.TrySaveAttachment.cs (read first) with the content of Listing L-TRYSAVE (four leading spaces stripped); then CMD-CENSUS with PATHS-TRYSAVE and TOKENS-TRYSAVE (one trailing `Get-Date -Format "yyyy-MM-ddTHH-mm"` statement appended to read the write time). The census covers one file, so each per-file count equals its TOTAL; the TOTAL lines are listed by token ID.
EXIT_CODE: 0
Output Summary:
A1 `[ExcludeFromCodeCoverage]internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave)` TOTAL = 1
A2 `</summary>internalstaticTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory)` TOTAL = 1
A3 `</returns>internalstaticasyncTask<bool>TrySaveAttachmentAsync(thisAttachmentattachment,stringfilePathSave,Action<string>createDirectory,Action<string>clearReadOnly,YesNoToAllPromptSessionremoveReadOnlyPrompt)` TOTAL = 1
A4 `[ExcludeFromCodeCoverage]privatestaticvoidClearReadOnlyAttributeOnDisk(stringdirectoryPath)` TOTAL = 1
A5 `privatestaticreadonlyYesNoToAllPromptSessionRemoveReadOnlyPrompt=new(YesNoToAll.ShowDialog);` TOTAL = 1
A6 `vardi=newDirectoryInfo(directoryPath);di.Attributes&=~System.IO.FileAttributes.ReadOnly;` TOTAL = 1
A7 `returnTrySaveAttachmentAsync(attachment,filePathSave,path=>System.IO.Directory.CreateDirectory(path));` TOTAL = 1
A8 `returnTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,ClearReadOnlyAttributeOnDisk,RemoveReadOnlyPrompt);` TOTAL = 1
A9 `returnawaitTrySaveAttachmentAsync(attachment,filePathSave,createDirectory,clearReadOnly,removeReadOnlyPrompt);` TOTAL = 1
A10 `try{createDirectory(Path.GetDirectoryName(filePathSave));awaitTask.Run(()=>attachment.SaveAsFile(filePathSave));returntrue;}catch(System.UnauthorizedAccessExceptione){Debug.WriteLine(e.Message);` TOTAL = 1
A11 `if(removeReadOnlyPrompt.Response==YesNoToAllResponse.Empty){varmessage=` TOTAL = 1
A12 `isread-only.Doyouwanttoremovethereadonlyattribute?` TOTAL = 1
A13 `removeReadOnlyPrompt.Ask(message);}if((removeReadOnlyPrompt.Response==YesNoToAllResponse.Yes)||(removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll)){vardirectory=Path.GetDirectoryName(filePathSave);try{clearReadOnly(directory);}catch(System.Exceptioninner){Debug.WriteLine(inner.Message);returnfalse;}finally{removeReadOnlyPrompt.ReleaseSingleAnswer();}` TOTAL = 1
A14 `}elseif((removeReadOnlyPrompt.Response==YesNoToAllResponse.No)||(removeReadOnlyPrompt.Response==YesNoToAllResponse.NoToAll)){Debug.WriteLine(` TOTAL = 1
A15 `removeReadOnlyPrompt.ReleaseSingleAnswer();returnfalse;}else{throw;}}catch(System.Exception){throw;}}` TOTAL = 1
A16 `catch(` TOTAL = 3
A17 `throw;` TOTAL = 2
A18 `clearReadOnly(directory);` TOTAL = 1
A19 `//NEGATIVE-CONTROL-956` TOTAL = 0
A20 `Excludedfromcoverage:` TOTAL = 2
A21 `(UT4)` TOTAL = 2
A22 `asyncTask<bool>` TOTAL = 1
A23 `Path.GetDirectoryName(filePathSave)` TOTAL = 3
A24 `ShowDialog(` TOTAL = 0
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 172
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 0492724AC8BDF089BFC25FF28773E42A7EF894F166648C5FEF4EE136286E6AC3
Acceptance: every TOKENS-TRYSAVE total equals the SEAM column (A1 to A15, A18, A22 1; A16 3; A17 2; A19 0; A20 2; A21 2; A23 3; A24 0) and LINES = 172 (all hold).
