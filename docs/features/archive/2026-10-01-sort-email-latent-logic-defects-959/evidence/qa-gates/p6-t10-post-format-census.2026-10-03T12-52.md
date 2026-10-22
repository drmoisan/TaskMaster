# P6-T10 Post-Format Census of the Final Tree

Timestamp: 2026-10-03T12-52
ITERATION: 1
Command: CMD-CENSUS for PATHS-A/TOKENS-A, PATHS-T/TOKENS-T, PATHS-U/TOKENS-U, PATHS-S/TOKENS-S, PATHS-M/TOKENS-M, PATHS-E/TOKENS-E, PATHS-TST1/TOKENS-TST1, PATHS-TST2/TOKENS-TST2, PATHS-TSC/TOKENS-TSC, PATHS-TAS/TOKENS-TAS, PATHS-TUL/TOKENS-TUL, PATHS-TEF/TOKENS-TEF; CMD-USINGS; CMD-CSPROJ; CMD-GREP-FACTS (STAGE final); CMD-LINES with PATHS-CSHARP-FINAL (each run as pwsh -NoProfile -Command with Set-Location to the item worktree); then a Grep-tool search of UtilitiesCS/EmailIntelligence/EmailParsingSorting (glob SortEmail*.cs, pattern ExcludeFromCodeCoverage, one trailing context line); then a Read of each of the six test files
EXIT_CODE: 0 (scoped to the CMD-LINES payload, the last pwsh invocation)
Output Summary: every TOKENS total equals its FINAL column or listed value; USINGS-EXACT-FILES 5; every CMD-CSPROJ row matches the final column; every CMD-GREP-FACTS line matches its final expectation; MAX-LINES 488 with twelve LINES rows; EFCC-MEMBERS lists the eighteen expected attribute-and-member pairs; RETRY-READ: NONE FOUND.

Each census ran over a single path, so each per-path TOKEN line equals its TOTAL line; the TOTAL lines are transcribed below together with LINES and SHA256.

## CMD-CENSUS PATHS-A (TOKENS-A, FINAL column)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 3
TOKEN caseYesNoToAllResponse.NoToAll: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.No: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.Yes: @ TOTAL = 1
TOKEN caseYesNoToAllResponse.YesToAll: @ TOTAL = 1
TOKEN |YesNoToAllResponse. @ TOTAL = 0
TOKEN HasFlag @ TOTAL = 0
TOKEN _attachmentsAltName=YesNoToAllResponse.Empty; @ TOTAL = 0
TOKEN YesNoToAllResponse_ @ TOTAL = 0
TOKEN YesNoToAll.ShowDialog( @ TOTAL = 0
TOKEN new(YesNoToAll.ShowDialog) @ TOTAL = 3
TOKEN AllPromptSessions @ TOTAL = 2
TOKEN RedirectSaveFolder( @ TOTAL = 2
TOKEN FolderPathSave=destinationPath; @ TOTAL = 1
TOKEN FilePathHelperSaveAlt.FolderPath=destinationPath; @ TOTAL = 1
TOKEN IsPicture @ TOTAL = 0
TOKEN _responseSaveFile @ TOTAL = 0
TOKEN Func<Attachment,string,Task<bool>>trySave @ TOTAL = 0
TOKEN TrySaveAttachmentDelegatetrySave @ TOTAL = 2
TOKEN delegateTask<bool>TrySaveAttachmentDelegate( @ TOTAL = 1
TOKEN Func<string,bool>fileExists @ TOTAL = 2
TOKEN File.Exists @ TOTAL = 2
TOKEN TrySaveAttachmentAsync @ TOTAL = 1
TOKEN SaveCaseAsync( @ TOTAL = 2
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 329
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 9F1A8D46B77388DE545B8A9D611A431C13749205C6CF5B7A3B078E57D72418C2
```

## CMD-CENSUS PATHS-T (TOKENS-T, FINAL column)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 2
TOKEN Debug.WriteLine( @ TOTAL = 0
TOKEN catch(System.Exception){throw;} @ TOTAL = 0
TOKEN catch( @ TOTAL = 2
TOKEN catch(System.UnauthorizedAccessExceptione) @ TOTAL = 1
TOKEN catch(System.Exceptioninner) @ TOTAL = 1
TOKEN throw; @ TOTAL = 2
TOKEN TrySaveAttachmentCoreAsync( @ TOTAL = 3
TOKEN privatestaticasyncTask<bool>TrySaveAttachmentCoreAsync( @ TOTAL = 1
TOKEN internalstaticasyncTask<bool>TrySaveAttachmentAsync( @ TOTAL = 0
TOKEN internalstaticTask<bool>TrySaveAttachmentAsync( @ TOTAL = 3
TOKEN boolisRetryAfterClear @ TOTAL = 1
TOKEN isRetryAfterClear:false @ TOTAL = 1
TOKEN isRetryAfterClear:true @ TOTAL = 1
TOKEN isRetryAfterClear&&removeReadOnlyPrompt.Response==YesNoToAllResponse.YesToAll @ TOTAL = 1
TOKEN logger.Warn( @ TOTAL = 2
TOKEN logger.Error( @ TOTAL = 2
TOKEN createDirectory(Path.GetDirectoryName(filePathSave)); @ TOTAL = 1
TOKEN System.IO.Directory.CreateDirectory(path) @ TOTAL = 1
TOKEN removeReadOnlyPrompt.ReleaseSingleAnswer(); @ TOTAL = 2
TOKEN RemoveReadOnlyPrompt=new( @ TOTAL = 1
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 204
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = AA6C5D7347969E4729692C479EC81B8889FDAFA98B58C50DC21CDB8ADB135C12
```

## CMD-CENSUS PATHS-U (TOKENS-U, FINAL column)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 4
TOKEN Path.Combine(strFileName,strFileLocation) @ TOTAL = 0
TOKEN fileExists(Path.Combine(strFileLocation,strFileName)) @ TOTAL = 1
TOKEN SanitizeArray( @ TOTAL = 0
TOKEN MovedMailsHeader @ TOTAL = 2
TOKEN string.Join("\t",MovedMailsHeader) @ TOTAL = 1
TOKEN "Triage" @ TOTAL = 1
TOKEN "FlaggedAsTask" @ TOTAL = 1
TOKEN string[14,2] @ TOTAL = 0
TOKEN Func<string,bool>fileExists @ TOTAL = 1
TOKEN Action<string,string[],string>writeTextFile @ TOTAL = 1
TOKEN writeTextFile( @ TOTAL = 1
TOKEN File.Exists @ TOTAL = 1
TOKEN FileIO2.WriteTextFile @ TOTAL = 1
TOKEN publicstaticvoidWriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 1
TOKEN internalstaticvoidWriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 1
TOKEN Debug.WriteLine( @ TOTAL = 0
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingSystem.Collections.Generic; @ TOTAL = 0
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 0
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 190
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 228B900DD4A7FAE71DE1A08F081AC72DC37265C32B853D9487637860287562B4
```

## CMD-CENSUS PATHS-S (TOKENS-S, S FINAL column)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 4
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingSystem.Text.RegularExpressions; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
TOKEN usingUtilitiesCS.ReusableTypeClasses @ TOTAL = 0
TOKEN usingSystem.Windows.Forms; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; @ TOTAL = 1
TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 1
TOKEN usingSystem; @ TOTAL = 1
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 268
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = D08A06D2921F987BB6D0FCFDDA6CED5DAF88801C56A30E7D780234A8B71A2161
```

## CMD-CENSUS PATHS-M (TOKENS-M, M FINAL column)

```
TOKEN [ExcludeFromCodeCoverage] @ TOTAL = 5
TOKEN usingDeedle; @ TOTAL = 0
TOKEN usingSDILReader; @ TOTAL = 0
TOKEN usingOutlook= @ TOTAL = 0
TOKEN usingUtilitiesCS; @ TOTAL = 0
TOKEN usingSystem.Diagnostics; @ TOTAL = 0
TOKEN usingSystem.Text.RegularExpressions; @ TOTAL = 0
TOKEN usingUtilitiesCS.EmailIntelligence; @ TOTAL = 0
TOKEN usingUtilitiesCS.ReusableTypeClasses @ TOTAL = 0
TOKEN usingSystem.Windows.Forms; @ TOTAL = 1
TOKEN usingUtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder; @ TOTAL = 0
TOKEN usingUtilitiesCS.OutlookExtensions; @ TOTAL = 1
TOKEN usingSystem; @ TOTAL = 1
TOKEN #nullableenable @ TOTAL = 1
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 379
SHA256 UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 61FED1959AFE0C8D322ADDC88405E347E21CE44F038018E578D1FEBB7CB388AB
```

## CMD-CENSUS PATHS-E (TOKENS-E, FINAL column)

```
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 1
TOKEN ResetFilerPromptState(); @ TOTAL = 1
TOKEN protectedinternalvirtualvoidResetFilerPromptState() @ TOTAL = 1
TOKEN varresult=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 0
TOKEN result=awaitInvokeFilerAsync(config,mailHelpers); @ TOTAL = 1
TOKEN boolresult; @ TOTAL = 1
TOKEN finally{ @ TOTAL = 1
TOKEN returnresult; @ TOTAL = 1
LINES QuickFiler\Controllers\EfcDataModel.cs = 485
SHA256 QuickFiler\Controllers\EfcDataModel.cs = 772262874673028C7D109263F82A55B6C6B4D2641FED3B186166F30DA9F15FA2
```

## CMD-CENSUS PATHS-TST1 (TOKENS-TST1, FINAL column)

```
TOKEN [TestMethod] @ TOTAL = 12
TOKEN [DataTestMethod] @ TOTAL = 2
TOKEN [DataRow( @ TOTAL = 6
TOKEN DisplayName= @ TOTAL = 6
TOKEN SanitizeArray_WhenOutputArrayIsInitialized_WritesSanitizedRows @ TOTAL = 0
TOKEN "SanitizeArray" @ TOTAL = 0
TOKEN "SanitizeArrayLineTSV" @ TOTAL = 1
TOKEN saveAttachments:false,savePictures:true @ TOTAL = 0
TOKEN saveAttachments:saveAttachments,savePictures:savePictures @ TOTAL = 2
TOKEN "photo.jpg,report.pdf" @ TOTAL = 2
TOKEN TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile @ TOTAL = 1
TOKEN TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave @ TOTAL = 1
TOKEN C:\Sortemail945Sandbox @ TOTAL = 1
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 900B58CF0CB9C457EBAF62375D6087452844D880EBF395A1EC259544FA158A7F
```

## CMD-CENSUS PATHS-TST2 (TOKENS-TST2, FINAL column)

```
TOKEN [TestMethod] @ TOTAL = 12
TOKEN CreateDirectoryLimit @ TOTAL = 3
TOKEN TrySaveAttachmentAsync_WhenYesToAllIsHeldAndRetryIsStillDenied_RethrowsAfterOneClear @ TOTAL = 1
TOKEN "retryboundexceeded" @ TOTAL = 1
TOKEN .Throws(denied) @ TOTAL = 1
TOKEN BeSameAs(denied) @ TOTAL = 1
TOKEN SetupSequence @ TOTAL = 11
TOKEN Times.Exactly(2) @ TOTAL = 5
TOKEN newSeams( @ TOTAL = 12
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 412
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 0C2A1463F2810F281A33190059A71260B513FF178FF57BB12E67E3B53C9E5E8D
```

## CMD-CENSUS PATHS-TSC (TOKENS-TSC, FINAL column)

```
TOKEN [TestMethod] @ TOTAL = 6
TOKEN [DataTestMethod] @ TOTAL = 3
TOKEN [DataRow( @ TOTAL = 6
TOKEN DisplayName= @ TOTAL = 6
TOKEN SortEmail.SaveCase( @ TOTAL = 3
TOKEN SortEmail.SaveCaseAsync( @ TOTAL = 9
TOKEN newScriptedPrompt( @ TOTAL = 6
TOKEN RecordingSave(saves) @ TOTAL = 9
TOKEN SortEmail.TrySaveAttachmentDelegate @ TOTAL = 1
TOKEN Func<Attachment,string,Task<bool>> @ TOTAL = 0
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN Times.Once @ TOTAL = 2
TOKEN Times.Never @ TOTAL = 3
TOKEN C:\Sortemail959Sandbox @ TOTAL = 2
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 343
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 915560E406A9CD4C14D6FE06360902D7C0030BE079E0C300EFADCB511E9DB247
```

## CMD-CENSUS PATHS-TAS (TOKENS-TAS, FINAL column)

```
TOKEN [TestMethod] @ TOTAL = 11
TOKEN [DataTestMethod] @ TOTAL = 0
TOKEN [DataRow( @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptAnswerField @ TOTAL = 0
TOKEN Cleanup_Files_ResetsEveryPromptSession @ TOTAL = 1
TOKEN SortEmail.Cleanup_Files(); @ TOTAL = 0
TOKEN field.SetValue(null,YesNoToAllResponse.YesToAll); @ TOTAL = 0
TOKEN SetValue( @ TOTAL = 0
TOKEN SortEmail.SaveAttachmentAsync( @ TOTAL = 7
TOKEN SortEmail.SaveAttachment( @ TOTAL = 3
TOKEN SortEmail.RedirectSaveFolder( @ TOTAL = 1
TOKEN SortEmail.TrySaveAttachmentDelegate @ TOTAL = 1
TOKEN Func<Attachment,string,Task<bool>> @ TOTAL = 0
TOKEN "AllPromptSessions" @ TOTAL = 1
TOKEN typeof(YesNoToAllPromptSession) @ TOTAL = 1
TOKEN HaveCount(4) @ TOTAL = 2
TOKEN OnlyHaveUniqueItems() @ TOTAL = 1
TOKEN newYesNoToAllPromptSession(Prompt) @ TOTAL = 1
TOKEN C:\Sortemail959Sandbox @ TOTAL = 3
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 437
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = D1D485E11108E56879CE6E031CBC2E5007AAF5E2E28F59688716F4A602A9A046
```

## CMD-CENSUS PATHS-TUL (TOKENS-TUL, listed values)

```
TOKEN [TestMethod] @ TOTAL = 2
TOKEN SortEmail.WriteCSV_StartNewFileIfDoesNotExist( @ TOTAL = 2
TOKEN Path.Combine(LogFolder,LogFileName) @ TOTAL = 1
TOKEN HaveCount(13) @ TOTAL = 1
TOKEN "MovedMails.txt" @ TOTAL = 1
TOKEN Triage\tFolderName\tSent_On @ TOTAL = 1
TOKEN C:\Sortemail959Sandbox @ TOTAL = 1
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 82
SHA256 UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = CAE210E9A9983A8C8BB843A3E80789248D2BF3EBB6A81E097482AE71C7F06C59
```

## CMD-CENSUS PATHS-TEF (TOKENS-TEF, listed values)

```
TOKEN [TestMethod] @ TOTAL = 3
TOKEN MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates @ TOTAL = 1
TOKEN MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce @ TOTAL = 1
TOKEN MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState @ TOTAL = 1
TOKEN overrideTask<bool>InvokeFilerAsync( @ TOTAL = 1
TOKEN overridevoidResetFilerPromptState() @ TOTAL = 1
TOKEN ResetCalls.Should().Be(1) @ TOTAL = 2
TOKEN ResetCalls.Should().Be(0) @ TOTAL = 1
TOKEN Task.FromException<bool>( @ TOTAL = 1
TOKEN SpecialFoldersWithoutOneDrive() @ TOTAL = 2
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 192
SHA256 QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = F5E5FB79715C806B8D98216E4FDD32335D60593D1EE025DD69A3A0510ABFDD04
```

## CMD-USINGS

```
USINGS A count=8 exact=True firstline=True blankafter=True
USINGS T count=5 exact=True firstline=True blankafter=True
USINGS U count=10 exact=True firstline=True blankafter=True
USINGS S count=9 exact=True firstline=True blankafter=True
USINGS M count=9 exact=True firstline=True blankafter=True
USINGS-EXACT-FILES: 5
```

## CMD-CSPROJ

```
UCS EmailIntelligence\EmailParsingSorting\MovedMailInfo.cs COUNT=1 LINE=817
UCS EmailIntelligence\EmailParsingSorting\SortEmail.cs COUNT=1 LINE=818
UCS EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs COUNT=1 LINE=819
UCS EmailIntelligence\EmailParsingSorting\SortEmail.LegacyAttachmentSaving.cs COUNT=0 LINE=
UCS EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs COUNT=1 LINE=820
UCS EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs COUNT=1 LINE=821
UCS EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs COUNT=1 LINE=822
UCS OutlookObjects\Folder\FolderPredictor.cs COUNT=1 LINE=823
UCT EmailIntelligence\SortEmail_Tests.cs COUNT=1 LINE=98
UCT EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs COUNT=1 LINE=99
UCT EmailIntelligence\SortEmail_SaveCase_Tests.cs COUNT=1 LINE=100
UCT EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs COUNT=1 LINE=101
UCT EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs COUNT=1 LINE=102
UCT EmailIntelligence\FilterOlFoldersController_Tests.cs COUNT=1 LINE=103
QFT Controllers\EfcDataModelArchiveRootTests.cs COUNT=1 LINE=127
QFT Controllers\EfcDataModelFilerCleanupTests.cs COUNT=1 LINE=128
QFT Controllers\EfcDataModelIssue792CarryTests.cs COUNT=1 LINE=129
```

## CMD-GREP-FACTS (STAGE final)

```
CS-FILES: 1708
DEAD-MEMBERS-CS: 0
TODOMODEL-CSPROJ-MATCHES: 2
TODOMODEL-CSPROJ-FILES: ToDoModel.Test\ToDoModel.Test.csproj
NEW-IDENTIFIERS: 18
SORTEMAIL-FILES: SortEmail.AttachmentSaving.cs,SortEmail.cs,SortEmail.MailItemSort.cs,SortEmail.TrySaveAttachment.cs,SortEmail.UndoAndMoveLog.cs
SORTEMAIL-FILE-COUNT: 5
SHOWDIALOG-CALLS-PARTIALS: 0
ENUM-FIELDS-A: 0
DEAD-TOKENS-PARTIALS: 0
BANNED-USINGS-PARTIALS: 0
USING-SYSTEM-PARTIAL-FILES: 5
DEBUG-WRITELINE-T: 0
EFCC-PARTIALS: 18
TESTS6-PRESENT: 6
BANNED-TEST-APIS: 0
NON-APPROVED-FRAMEWORKS: 0
LEGACY-FILE-EXISTS: False
TODOMODEL-FILE-EXISTS: False
```

## CMD-LINES (PATHS-CSHARP-FINAL)

```
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 329
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 204
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 190
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs = 268
LINES UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs = 379
LINES QuickFiler\Controllers\EfcDataModel.cs = 485
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 412
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_SaveCase_Tests.cs = 343
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 437
LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_UndoAndMoveLog_Tests.cs = 82
LINES QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 192
MAX-LINES: 488
```

- AC23-CLOSEST:
  - LINES QuickFiler\Controllers\EfcDataModel.cs = 485
  - LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_AttachmentSaving_Tests.cs = 437
  - LINES UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 488

## EFCC-MEMBERS (Grep tool, glob SortEmail*.cs, pattern ExcludeFromCodeCoverage, one trailing context line; paths repository-relative)

- SortEmail.UndoAndMoveLog.cs:37 `[ExcludeFromCodeCoverage]` / 38 `public static async Task UndoAsync(`
- SortEmail.UndoAndMoveLog.cs:93 `[ExcludeFromCodeCoverage]` / 94 `private static void PushToUndoStack(`
- SortEmail.UndoAndMoveLog.cs:105 `[ExcludeFromCodeCoverage]` / 106 `private static void CaptureMoveDetails(`
- SortEmail.UndoAndMoveLog.cs:151 `[ExcludeFromCodeCoverage]` / 152 `public static void WriteCSV_StartNewFileIfDoesNotExist(` (the two-parameter overload: strFileName, strFileLocation; lines 153 to 155 read)
- SortEmail.TrySaveAttachment.cs:27 `[ExcludeFromCodeCoverage]` / 28 `internal static Task<bool> TrySaveAttachmentAsync(` (the two-argument overload: this Attachment attachment, string filePathSave; lines 29 to 31 read)
- SortEmail.TrySaveAttachment.cs:197 `[ExcludeFromCodeCoverage]` / 198 `private static void ClearReadOnlyAttributeOnDisk(string directoryPath)`
- SortEmail.MailItemSort.cs:16 `[ExcludeFromCodeCoverage]` / 17 `public static async Task SortAsync(`
- SortEmail.MailItemSort.cs:50 `[ExcludeFromCodeCoverage]` / 51 `public static async Task SortAsync(`
- SortEmail.MailItemSort.cs:86 `[ExcludeFromCodeCoverage]` / 87 `public static async Task SortAsync(`
- SortEmail.MailItemSort.cs:238 `[ExcludeFromCodeCoverage]` / 239 `public static void Sort(`
- SortEmail.MailItemSort.cs:344 `[ExcludeFromCodeCoverage]` / 345 `private static void ResolvePaths(`
- SortEmail.cs:31 `[ExcludeFromCodeCoverage]` / 32 `public static async Task SortAsync(`
- SortEmail.cs:100 `[ExcludeFromCodeCoverage]` / 101 `public static async Task UpdatePredictiveEngineAsync(`
- SortEmail.cs:129 `[ExcludeFromCodeCoverage]` / 130 `public static async Task ProcessMailItemAsync(`
- SortEmail.cs:222 `[ExcludeFromCodeCoverage]` / 223 `private static void ResolvePaths(`
- SortEmail.AttachmentSaving.cs:113 `[ExcludeFromCodeCoverage]` / 114 `public static void SaveAttachment(this AttachmentHelper attachmentHelper)`
- SortEmail.AttachmentSaving.cs:161 `[ExcludeFromCodeCoverage]` / 162 `public static Task SaveAttachmentAsync(this AttachmentHelper attachmentHelper)`
- SortEmail.AttachmentSaving.cs:228 `[ExcludeFromCodeCoverage]` / 229 `public static Task SaveAttachmentAsync(` (the destination overload: this AttachmentHelper attachmentHelper, string destinationPath; lines 230 to 232 read)

Eighteen pairs: A 3, T 2, U 4, S 4, M 5.

## RETRY-READ

RETRY-READ: NONE FOUND (the six test files were read in full; none carries a retry attribute or a retry loop. The TST2 T12 test exercises the production retry bound and uses the CreateDirectoryLimit tripwire, which is not a test retry).

## Acceptance (P6-T10, all seven required)

1. Every TOKENS total equals its FINAL column (TOKENS-S and TOKENS-M their FINAL columns; TOKENS-TSC, TOKENS-TAS, TOKENS-TUL and TOKENS-TEF their listed values): met.
2. USINGS-EXACT-FILES: 5: met.
3. Every CMD-CSPROJ row matches the final column: met.
4. Every CMD-GREP-FACTS line matches its final expectation: met.
5. MAX-LINES 488 (at most 499) with the twelve LINES rows present and the three AC23-CLOSEST rows quoted individually: met.
6. EFCC-MEMBERS lists eighteen attribute-and-member pairs whose member lines are exactly the expected set: met.
7. RETRY-READ: NONE FOUND: met.
